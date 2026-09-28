using System;

public interface ICardContainer
{
    bool TryGetCardAtIndex(int cardIndex, out CardInstance card);
    int Capacity { get; }

    event Action<int, CardInstance> OnCardAdded;
    event Action<int, CardInstance> OnCardRemoved;
}