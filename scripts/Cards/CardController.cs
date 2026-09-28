using System.Threading;
using System.Threading.Tasks;

[GlobalClass]
public partial class CardController : Node, IAbilitySystem
{

    public ICardContainer CardProvider => _cardManager;
    public ICardPiles CardPiles => _cardManager;

    public event Action<bool, IReadOnlyList<CardDefinition>> OnPileReceived;

    private CardRuntime _activeCard;
    private int _activeCardSlotIndex = -1;
    private bool _isInputHeld = false;

    private PlayerMovementController _playerMovement;
    private bool _isCasting = false;
    private ModifierHandle _movementLockHandle;
    private IStatContainer _stats;
    private CardManager _cardManager;
    private IEntity _owner;

    private (CardRuntime Card, int HandIndex, CancellationTokenSource CancelToken)? _pendingCast;

    private Action _onActiveCardCompleted;
    private Action _onActiveCardInterrupted;
    private IAnimationHandler _animationHandler;
    private CardRegistry _cardRegistry;

    public override void _Ready()
    {
        if (Multiplayer.IsServer())
        {
            _playerMovement = this.GetComponentInOwner<PlayerMovementController>();
            Debug.Assert(_playerMovement != null, "CardController requires a PlayerMovementController in parent");

            _stats = this.GetComponentInOwner<IStatContainer>();
            Debug.Assert(_stats != null, "CardController requires an IStatContainer in parent");
        }
    }

    public void InitializeClientObservers(IAnimationHandler animationHandler)
    {
        _animationHandler = animationHandler;
    }

    public void InitializeServer(IEntity owner)
    {
        if (!Multiplayer.IsServer()) return;

        _owner = owner;
        _cardRegistry = GameData.Instance.Cards;
        List<CardInstance> cards = new();

        foreach (CardDefinition definition in _cardRegistry.Cards)
        {
            cards.Add(new CardInstance(Guid.NewGuid(), definition, _owner));
        }

        _cardManager = new CardManager(cards);
        _cardManager.OnCardAdded += OnCardAdded;
        _cardManager.OnCardRemoved += OnCardRemoved;
    }

    private void OnCardRemoved(int index, CardInstance card)
    {
        RpcId(GetMultiplayerAuthority(), nameof(RemoveCardClientRpc), index);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void RemoveCardClientRpc(int index)
    {
        ClientBridge.Instance.CardHandController.OnCardRemoved(index);
    }

    private void OnCardAdded(int index, CardInstance card)
    {
        RpcId(GetMultiplayerAuthority(), nameof(AddCardClientRpc), index, card.Definition.Id.ToString());
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void AddCardClientRpc(int index, string cardDefinitionIdStr)
    {
        if (Guid.TryParse(cardDefinitionIdStr, out Guid cardDefinitionId))
        {
            ClientBridge.Instance.CardHandController.OnCardAdded(index, cardDefinitionId);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void CardStartedObserversRpc(string cardIdStr, float animationDuration)
    {
        if (!Guid.TryParse(cardIdStr, out Guid cardId)) return;
        if (!GameData.Instance.Cards.TryGet(cardId, out var definition)) return;

        _animationHandler?.PlayAnimation(definition.Visuals.AnimationOverride, duration: animationDuration);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void RequestUseAbility(int cardIndex)
    {
        if (!Multiplayer.IsServer()) return;
        if (_isCasting || _activeCard != null) return;

        if (_cardManager.TryGetCardAtIndex(cardIndex, out CardInstance card))
        {
            _activeCardSlotIndex = cardIndex;
            _isInputHeld = true;
            Server_StartCast(card, cardIndex);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void RequestCancelAbility(int cardIndex)
    {
        if (!Multiplayer.IsServer()) return;
        if (_activeCardSlotIndex != cardIndex) return;

        _isInputHeld = false;

        if (_activeCard != null)
        {
            var channel = _activeCard.GetCardComponent<ChannelingComponent>();
            channel?.SetInputHeld(false);
        }
    }

    private void Server_StartCast(CardInstance card, int handIndex)
    {
        _isCasting = true;

        _movementLockHandle = _stats.AddModifier(
            new StatModifier(GameTags.ModStatMovement, MathOp.Multiplicative, 0));

        CardRuntime runtime = CardFactory.CreateRuntime(card, this);

        runtime.SetTargetLocation(
            runtime.SpawnAtCursor
                ? _owner.GetCursorPosition()
                : _owner.Node.Position);

        var castTime = _stats.GetStat(
            GameTags.ModOffenseCastSpeed,
            runtime.Tags,
            runtime.CastTime);

        Rpc(nameof(CardStartedObserversRpc), runtime.Definition.Id.ToString(), castTime);

        var cts = new CancellationTokenSource();
        _pendingCast = (runtime, handIndex, cts);
        _ = Server_CastTimeRoutineAsync(runtime, castTime, cts.Token);
    }

    private async Task Server_CastTimeRoutineAsync(CardRuntime card, float castTime, CancellationToken token)
    {
        try
        {
            await Task.Delay((int)(castTime * 1000), token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        _cardManager.DiscardPlayedCard();

        _pendingCast = null;
        _isCasting = false;

        Server_ExecuteCard(card);
    }

    private void Server_ExecuteCard(CardRuntime card)
    {
        var channel = card.GetCardComponent<ChannelingComponent>();

        if (channel != null)
        {
            _activeCard = card;

            _onActiveCardCompleted = () => Server_EndChannel(card);
            _onActiveCardInterrupted = () => Server_EndChannel(card);

            channel.OnCompleted += _onActiveCardCompleted;
            channel.OnInterrupted += _onActiveCardInterrupted;
        }

        card.ExecuteCastTimeDone();

        if (channel != null)
            channel.SetInputHeld(_isInputHeld);
        else
            Server_EndCard(card);
    }

    private void Server_EndChannel(CardRuntime card)
    {
        if (_activeCard != card) return;

        var channel = card.GetCardComponent<ChannelingComponent>();

        if (channel != null)
        {
            channel.OnCompleted -= _onActiveCardCompleted;
            channel.OnInterrupted -= _onActiveCardInterrupted;
            _onActiveCardCompleted = null;
            _onActiveCardInterrupted = null;
        }

        _activeCard = null;
        _activeCardSlotIndex = -1;
        _isInputHeld = false;

        Server_EndCard(card);
    }

    private void Server_EndCard(CardRuntime card)
    {
        _owner.Stats.RemoveModifier(_movementLockHandle);
        _movementLockHandle = default;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void RequestPile(bool drawPile)
    {
        if (!Multiplayer.IsServer()) return;

        var pile = drawPile
            ? _cardManager.DrawPile
            : _cardManager.DiscardPile;

        string[] ids = pile.Select(card => card.Id.ToString()).ToArray();

        RpcId(GetMultiplayerAuthority(), nameof(SendPileClientRpc), drawPile, ids);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void SendPileClientRpc(bool drawPile, string[] ids)
    {
        var cards = new List<CardDefinition>();

        foreach (var idStr in ids)
        {
            if (Guid.TryParse(idStr, out var id))
            {
                _cardRegistry = GameData.Instance.Cards;
                if (_cardRegistry.TryGet(id, out var definition))
                    cards.Add(definition);
                else
                    GD.PrintErr($"Unknown CardDefinition ID '{id}'.");
            }
        }

        OnPileReceived?.Invoke(drawPile, cards);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void NotifyClientReadyServerRpc()
    {
        if (!Multiplayer.IsServer()) return;
        _cardManager.DrawHand();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void RequestCancelCurrentCast()
    {
        if (!Multiplayer.IsServer()) return;
        if (_pendingCast == null) return;

        var pending = _pendingCast.Value;
        pending.CancelToken.Cancel();

        pending.Card.ExecuteCancelled();

        _pendingCast = null;
        _isCasting = false;
        _activeCardSlotIndex = -1;
        _isInputHeld = false;

        Server_EndCard(pending.Card);
    }
}