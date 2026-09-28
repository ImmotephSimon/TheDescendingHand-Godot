public partial class PlayerAnimationController : Node, IAnimationHandler
{
    [Export] private AnimationTree _animationTree;
    [Export] private AnimationPlayer _animationPlayer;
    [Export] private Animation _dodgeAnimation;

    // StateMachine node lives nested inside the root BlendTree, so its
    // own playback/state paths need the "StateMachine/" prefix.
    private const string PlaybackParam = "parameters/StateMachine/playback";

    // Overlay/Clip/ClipSpeed/ClipSeek live at the BlendTree root level
    // (siblings of StateMachine), so these stay unprefixed.
    private const string ClipAnimationParam = "parameters/Clip/animation";
    private const string ClipSpeedParam = "parameters/ClipSpeed/scale";
    private const string ClipSeekParam = "parameters/ClipSeek/seek_request";
    private const string OverlayRequestParam = "parameters/Overlay/request"; // OneShot request enum

    public string RunBlendPosParam => "parameters/StateMachine/Locomotion/Run/blend_position";
    public string SprintBlendPosParam => "parameters/StateMachine/Locomotion/Sprint/blend_position";
    public string RunSprintBlendParam => "parameters/StateMachine/Locomotion/Blend2/blend_amount";

    private const string RuntimeLibrary = "runtime";

    private static readonly StringName Locomotion = "Locomotion";
    private static readonly StringName Immobilized = "Immobilized";
    private static readonly StringName Dead = "Dead";

    private readonly HashSet<GameTag> _activeStatuses = new();

    private AnimationLibrary _runtimeAnimations;
    private AnimationNodeStateMachinePlayback _playback;
    private float _clipLength;
    private Action _unsubscribeStats;
    private SceneTreeTimer _clipTimer;

    public AnimationTree AnimationTree => _animationTree;

    public override void _Ready()
    {
        if (_animationTree == null || _animationPlayer == null)
        {
            GD.PushError(
                "PlayerAnimationController: AnimationTree and AnimationPlayer must be assigned.");
            return;
        }

        _animationTree.AnimPlayer = _animationTree.GetPathTo(_animationPlayer);
        _animationTree.Active = true;

        // Getting the playback object immediately after Active = true can
        // return null because the tree hasn't done its first internal
        // setup pass yet. Defer by one frame so the parameter exists.
        CallDeferred(nameof(SetupPlayback));

        if (ClientBridge.Instance != null)
            ClientBridge.Instance.OnClientPlayerReady += OnClientPlayerReady;

        foreach (Godot.Collections.Dictionary p in _animationTree.GetPropertyList())
        {
            string name = p["name"].AsString();

            if (name.StartsWith("parameters/"))
                GD.Print(name);
        }
    }

    public override void _ExitTree()
    {
        if (ClientBridge.Instance != null)
            ClientBridge.Instance.OnClientPlayerReady -= OnClientPlayerReady;

        _unsubscribeStats?.Invoke();
        _unsubscribeStats = null;

        StopCurrentAnimation();
    }

    private void OnClientPlayerReady(ClientPlayer _)
    {
        _unsubscribeStats?.Invoke();

        var stats = ClientBridge.Instance.Stats;
        stats.StatusChanged += HandleStatusChanged;
        _unsubscribeStats = () => stats.StatusChanged -= HandleStatusChanged;
    }

    private void HandleStatusChanged(GameTag status, bool isActive)
    {
        if (isActive)
            _activeStatuses.Add(status);
        else
            _activeStatuses.Remove(status);

        CrossFadeTo(
            _activeStatuses.Contains(GameTags.StatusStun)
                ? Immobilized
                : Locomotion);
    }

    public void SetAnimationState(CharacterAnimationState state)
    {
        if (state == CharacterAnimationState.Unset)
            return;

        StringName target = state switch
        {
            CharacterAnimationState.Locomotion => Locomotion,
            CharacterAnimationState.Immobilized => Immobilized,
            CharacterAnimationState.Dead => Dead,
            _ => null
        };

        if (target == null)
        {
            GD.PushWarning($"Unhandled animation state: {state}");
            return;
        }

        CrossFadeTo(target);
    }

    public void PlayAttackAnimation(
        AttackAnimation attackAnimation,
        Action onFinished)
    {
        throw new NotImplementedException();
    }

    public void SetSpeed(float value)
    {
        throw new NotImplementedException();
    }

    public float PlayDodgeRoll(float duration)
    {
        PlayClip(_dodgeAnimation, duration);
        return _dodgeAnimation != null
            ? (float)_dodgeAnimation.Length
            : 0f;
    }

    public Action PlayAnimation(Animation clip, float duration)
    {
        PlayClip(clip, duration);
        return StopCurrentAnimation;
    }

    public void StopCurrentAnimation()
    {
        _clipTimer = null;
        // OneShot.Abort forces it back to the base input immediately.
        _animationTree.Set(OverlayRequestParam, (int)AnimationNodeOneShot.OneShotRequest.Abort);
    }

    private void SetupPlayback()
    {
        _playback = _animationTree.Get(PlaybackParam)
            .As<AnimationNodeStateMachinePlayback>();

        StartLocomotion();
    }

    private void StartLocomotion()
    {
        if (_playback != null && !_playback.IsPlaying())
            _playback.Start(Locomotion);
    }

    private void CrossFadeTo(StringName state)
    {
        _playback?.Travel(state);
    }

    private void PlayClip(Animation clip, float duration)
    {
        if (clip == null)
            return;

        duration = Mathf.Max(duration, 0.0001f);
        _clipLength = (float)clip.Length;

        _animationTree.Set(ClipAnimationParam, RegisterClip(clip));
        _animationTree.Set(ClipSpeedParam, _clipLength / duration);
        _animationTree.Set(ClipSeekParam, 0f);

        // OneShot handles its own fade in/out (set via the node's
        // Fade In/Fade Out times in the editor) — no manual tweening.
        _animationTree.Set(OverlayRequestParam, (int)AnimationNodeOneShot.OneShotRequest.Fire);

        _clipTimer = GetTree().CreateTimer(duration);
        _clipTimer.Timeout += FinishClip;
    }

    private void FinishClip()
    {
        _clipTimer = null;

        // Request the fade-out first, while the clip is still playing
        // normally. Freezing ClipSpeed to 0 here would leave the OneShot
        // blending toward a paused/frozen frame during its fade, instead
        // of cleanly releasing back to the StateMachine output.
        _animationTree.Set(OverlayRequestParam, (int)AnimationNodeOneShot.OneShotRequest.FadeOut);
    }

    private StringName RegisterClip(Animation clip)
    {
        _runtimeAnimations ??= GetRuntimeLibrary();

        string name = $"clip_{clip.GetInstanceId()}";

        if (!_runtimeAnimations.HasAnimation(name))
            _runtimeAnimations.AddAnimation(name, clip);

        return $"{RuntimeLibrary}/{name}";
    }

    private AnimationLibrary GetRuntimeLibrary()
    {
        if (_animationPlayer.HasAnimationLibrary(RuntimeLibrary))
            _animationPlayer.RemoveAnimationLibrary(RuntimeLibrary);

        var library = new AnimationLibrary();
        _animationPlayer.AddAnimationLibrary(RuntimeLibrary, library);

        return library;
    }
}