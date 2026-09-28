using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class WaterPuddleDefinition : CardDefinition
{
    [Export] private float radius = 3f;
    [Export] private float effectiveness;
    [Export] private GameTag damageConversion;
    [Export] private float duration = 8f;

    private Action _cancelVisuals;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var overlap = card.AddSphereOverlap(radius);
        var status = card.AddCardComponent<StatusEffectComponent>();
        status.Configure(GameTags.StatusFreeze, 4f);

        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage.Configure(effectiveness, damageConversion, triggerOnHit: false);

        var listeners = new Dictionary<IEntity, Action<float>>();
        var electrified = false;

        void SetElectrified(bool isElectrified)
        {
            if (electrified == isElectrified) return;
            electrified = isElectrified;
            _cancelVisuals?.Invoke();
            _cancelVisuals = context.ClientSpawn?.Invoke(this, new VfxSpawnParams(card.GlobalPosition, 1, duration));
        }

        card.OnActivated += () =>
        {
            card.GlobalPosition = card.TargetLocation;
            _cancelVisuals = context.ClientSpawn?.Invoke(this, new VfxSpawnParams(card.TargetLocation, 0, duration)
            {
                Scale = Vector3.One * radius
            });
        };

        overlap.OnEntityEntered += entity =>
        {
            Action<float> listener = newValue => { if (newValue > 0f) SetElectrified(true); };
            listeners[entity] = listener;
            entity.Stats.Listen(GameTags.StatusElectrified, listener);
            listener?.Invoke(entity.Stats.GetStat(GameTags.StatusElectrified));
        };

        overlap.OnEntityExited += entity =>
        {
            if (listeners.TryGetValue(entity, out var listener))
            {
                entity.Stats.StopListening(GameTags.StatusElectrified, listener);
                listeners.Remove(entity);
            }
        };
    }
}