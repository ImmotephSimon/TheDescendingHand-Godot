using System;
using Godot;

[GlobalClass]
public partial class HingeController : Node3D
{
    public enum PropType { Door, Chest }

    [Export] private Node3D hinge;
    [Export] private Vector3 openAngle = new Vector3(0, -150f, 0);
    [Export] private float openSpeed = 1f;
    [Export] private PropType propType;
    [Export] private Area3D interactionArea;

    public event Action PlayerInteracted;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;

    public override void _Ready()
    {
        if (hinge == null || interactionArea == null)
        {
            SetProcess(false);
            return;
        }

        closedRotation = hinge.Transform.Basis.GetRotationQuaternion();
        Vector3 radAngle = new Vector3(Mathf.DegToRad(openAngle.X), Mathf.DegToRad(openAngle.Y), Mathf.DegToRad(openAngle.Z));
        openRotation = closedRotation * Quaternion.FromEuler(radAngle);
        targetRotation = closedRotation;

        if (propType == PropType.Door)
        {
            interactionArea.BodyEntered += body => { if (body.IsInGroup("Player")) Open(); };
            interactionArea.BodyExited += body => { if (body.IsInGroup("Player")) Close(); };
        }
        else if (propType == PropType.Chest)
        {
            interactionArea.InputEvent += (camera, @event, position, normal, shapeIdx) =>
            {
                if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
                    Open();
            };
        }
    }

    public override void _Process(double delta)
    {
        Quaternion currentQuat = hinge.Transform.Basis.GetRotationQuaternion();
        Quaternion nextQuat = currentQuat.Slerp(targetRotation, openSpeed * (float)delta);

        Transform3D tf = hinge.Transform;
        tf.Basis = new Basis(nextQuat);
        hinge.Transform = tf;
    }

    public void Open()
    {
        PlayerInteracted?.Invoke();
        targetRotation = openRotation;
    }

    public void Close()
    {
        targetRotation = closedRotation;
    }
}