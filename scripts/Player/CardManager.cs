using System;
using System.Collections.Generic;
using System.Linq;

public class CardManager : ICardContainer, ICardPiles
{
    private readonly CardInstance[] _hand;
    private readonly List<CardInstance> _drawPile = new();
    private readonly List<CardInstance> _discard = new();
    private int _playedIndex = -1;

    public event Action<int, CardInstance> OnCardAdded;
    public event Action<int, CardInstance> OnCardRemoved;

    public CardManager(IEnumerable<CardInstance> startingCards, int handSize = 5)
    {
        _hand = new CardInstance[handSize];

        _drawPile.AddRange(startingCards);
        ShuffleDrawPile();
    }

    public IReadOnlyList<CardInstance> Hand =>
        Array.AsReadOnly(_hand);

    public bool IsHandFull =>
        _hand.All(card => card != null);

    public bool IsHandEmpty =>
        _hand.All(card => card == null);

    public int Capacity =>
        _hand.Length;

    public void DrawHand()
    {
        for (int i = 0; i < _hand.Length; i++)
        {
            if (_drawPile.Count == 0)
                break;

            DrawCard();
        }
    }

    public void AddToDrawPile(CardInstance card) =>
        _drawPile.Add(card);

    public bool AddToHand(CardInstance card)
    {
        for (int i = 0; i < _hand.Length; i++)
        {
            if (_hand[i] == null)
            {
                _hand[i] = card;
                return true;
            }
        }

        return false;
    }

    public CardInstance DrawCard()
    {
        int emptySlot = FindEmptyHandSlot();

        if (emptySlot == -1)
            throw new InvalidOperationException("Cannot draw card: hand is full.");

        if (_drawPile.Count == 0)
            throw new InvalidOperationException("Cannot draw card: no cards available.");

        CardInstance drawnCard = _drawPile[0];
        _drawPile.RemoveAt(0);

        _hand[emptySlot] = drawnCard;
        OnCardAdded?.Invoke(emptySlot, drawnCard);

        return drawnCard;
    }

    public IReadOnlyList<CardDefinition> DrawPile =>
        _drawPile.Select(card => card.Definition).ToArray();

    public IReadOnlyList<CardDefinition> DiscardPile =>
        _discard.Select(card => card.Definition).ToArray();

    private int FindEmptyHandSlot() =>
        Array.FindIndex(_hand, card => card == null);

    private void ResetHand()
    {
        _drawPile.AddRange(_discard);
        _discard.Clear();

        ShuffleDrawPile();
        DrawHand();
    }

    public void DiscardPlayedCard()
    {
        if (_playedIndex < 0)
        {
            GD.PushError($"[CardManager] Invalid discard call with index {_playedIndex}");
            return;
        }

        if (_playedIndex >= _hand.Length)
        {
            GD.PushError($"[CardManager] Invalid discard index {_playedIndex}, hand length {_hand.Length}");
            return;
        }

        CardInstance card = _hand[_playedIndex];

        if (card == null)
        {
            GD.PushError($"[CardManager] Discard target slot {_playedIndex} is already empty");
            return;
        }

        _discard.Add(card);
        _hand[_playedIndex] = null;

        OnCardRemoved?.Invoke(_playedIndex, card);

        if (IsHandEmpty)
            ResetHand();
    }

    public bool TryGetCardAtIndex(int index, out CardInstance card)
    {
        if (index >= 0 && index < _hand.Length)
        {
            card = _hand[index];

            if (card != null)
                _playedIndex = index;

            return card != null;
        }

        card = null;
        return false;
    }

    private void ShuffleDrawPile()
    {
        for (int i = _drawPile.Count - 1; i > 0; i--)
        {
            int j = GD.RandRange(0, i);
            (_drawPile[i], _drawPile[j]) = (_drawPile[j], _drawPile[i]);
        }
    }
}