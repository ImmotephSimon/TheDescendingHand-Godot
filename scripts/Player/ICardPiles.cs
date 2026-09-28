using System.Collections.Generic;

public interface ICardPiles
{
    IReadOnlyList<CardDefinition> DrawPile { get; }
    IReadOnlyList<CardDefinition> DiscardPile { get; }
}