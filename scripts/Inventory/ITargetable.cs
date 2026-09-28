using System;

public enum InventoryResponse
{
    Failed,
    Consumed
}

public interface ITargetable
{
    TagContainer GetTargetingRequirements();
    InventoryResponse ApplyTargetedEffect(ItemDropInstance targetItem);
    void StartTargeting(ItemDropInstance orbInstance, TagContainer requirements, Action<ItemDropInstance> onTargetSelected);
}