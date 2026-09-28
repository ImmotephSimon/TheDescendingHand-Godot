using System;
using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class PileHUDController : Node3D
{
    [Export] private Button drawButton;
    [Export] private Button discardButton;
    [Export] private PileView pileView;

    private Label drawText;
    private Label discardText;
    private CardController _cardController;

    private enum ActivePile { None, Draw, Discard }
    private ActivePile active = ActivePile.None;

    public override void _Ready()
    {
        drawText = drawButton.GetNodeOrNull<Label>("Label") ?? drawButton.GetComponentInChildren<Label>(true);
        discardText = discardButton.GetNodeOrNull<Label>("Label") ?? discardButton.GetComponentInChildren<Label>(true);

        if (drawText == null || discardText == null)
            GD.PrintErr($"Missing text: drawText: {drawText}, discardText: {discardText}");

        _cardController = ClientBridge.Instance.ClientPlayer.GetComponentInOwner<CardController>();
        _cardController.OnPileReceived += OnPileReceived;

        drawButton.Pressed += OnDrawClicked;
        discardButton.Pressed += OnDiscardClicked;
    }

    private void OnDrawClicked()
    {
        Toggle(ActivePile.Draw);
    }

    private void OnDiscardClicked()
    {
        Toggle(ActivePile.Discard);
    }

    private void Toggle(ActivePile target)
    {
        bool isSame = active == target;
        active = isSame ? ActivePile.None : target;

        if (isSame)
        {
            pileView.Clear();
            return;
        }

        _cardController.RequestPile(target == ActivePile.Draw);
    }

    private void OnPileReceived(
        bool isDrawPile,
        IReadOnlyList<CardDefinition> cards)
    {
        bool isActive =
            (isDrawPile && active == ActivePile.Draw) ||
            (!isDrawPile && active == ActivePile.Discard);

        if (!isActive)
            return;

        pileView.Show(cards);

        if (isDrawPile)
            drawText.Text = cards.Count.ToString();
        else
            discardText.Text = cards.Count.ToString();
    }

    public override void _ExitTree()
    {
        if (_cardController != null)
            _cardController.OnPileReceived -= OnPileReceived;
    }
}