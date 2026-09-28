public interface ICardServerComponent
{
    void ExecuteBegin();
    void ExecuteCastTimeDone();
    void ExecuteCancelled();
}

public interface ICardClientComponent
{
    void ExecuteClientVisuals();
}