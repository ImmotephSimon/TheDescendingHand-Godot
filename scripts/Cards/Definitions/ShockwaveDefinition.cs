[GlobalClass]
public partial class ShockwaveDefinition : CardDefinition
{
    [Export] private float radius = 8f;
    [Export] private float force = 20f;
    [Export] private float upwardModifier = 0f;
    [Export] private float effectiveness;
    [Export] private GameTag damageConversion;
    [Export] private float delayDuration = 1f;
    [Export(PropertyHint.Layers3DPhysics)] private uint debrisLayerMask = 1;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        card.AddSphereOverlap(radius);
        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage.Configure(effectiveness, damageConversion, triggerOnHit: false);

        var delay = card.AddCardComponent<DelayedComponent>();
        delay.Configure(delayDuration);

        delay.OnCompleted += () =>
        {
            Vector3 epicentre = card.TargetLocation;

            context.ClientSpawn?.Invoke(this, new VfxSpawnParams
            {
                Position = epicentre,
                Scale = Vector3.One * radius
            });

            // Layer mask bitwise OR directly (AttackLayer is already a shifted mask, not an index)
            uint targetMask = debrisLayerMask | card.CardOwner.AttackLayer;
            var spaceState = card.GetWorld3D().DirectSpaceState;
            var query = new PhysicsShapeQueryParameters3D
            {
                Shape = new SphereShape3D { Radius = radius },
                Transform = new Transform3D(Basis.Identity, epicentre),
                CollisionMask = targetMask
            };

            var hits = spaceState.IntersectShape(query);

            foreach (var hit in hits)
            {
                var col = hit["collider"].As<Node3D>();
                if (col == null) continue;

                var fracture = col.GetComponentInParent<FractureComponent>();
                fracture?.Break(epicentre, force, radius, upwardModifier);

                var collisionNotifier = col.GetComponentInParent<DebrisCollisionNotifier>();
                collisionNotifier?.Initialize((targetNode, hitPoint) =>
                {
                    if (targetNode is Node node && node.TryGetComponentInParent<IEntity>(out var targetEntity))
                    {
                        damage.TriggerDamage(new HitInfo(targetEntity, card.CardOwner, hitPoint));
                    }
                });
            }

            card.GlobalPosition = epicentre;
        };
    }
}