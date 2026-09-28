[GlobalClass]
public partial class HUD : Control
{
    [Export] private ProgressBar experienceBar;
    [Export] private RichTextLabel healthText;
    [Export] private RichTextLabel manaText;
    [Export] private Control speechBubble;
    [Export] private RichTextLabel zoneText;

    private const float DisplayDuration = 0.2f;
    private float _displayedHealth, _targetHealth, _maxHealth, _healthTimer;
    private float _displayedMana, _targetMana, _maxMana, _manaTimer;

    public Action<int> GetZoneUpdateHandler() => UpdateZoneText;

    public override void _Ready()
    {
        DungeonManager.Instance.OnZoneLevelChanged += UpdateZoneText;
        UpdateZoneText(DungeonManager.Instance.ZoneLevel);
    }

    public void Bind(LevelComponent comp)
    {
        if (comp == null || experienceBar == null) return;

        comp.OnExperienceChanged += (_, progress) => experienceBar.Value = progress;
        experienceBar.Value = comp.Progress;
    }


    public void Bind(IHealth health)
    {
        health.OnHealthChanged += UpdateHealth;
        UpdateHealth(health.CurrentHealth, health.MaxHealth, true);
    }

    public void Bind(IMana mana)
    {
        mana.OnManaChanged += UpdateMana;
        UpdateMana(mana.CurrentMana, mana.MaxMana, true);
    }

    private void UpdateHealth(float current, float max, bool isInstant)
    {
        _targetHealth = current;
        _maxHealth = max;
        if (isInstant) { _displayedHealth = current; _healthTimer = DisplayDuration; }
        else { _healthTimer = 0f; }
        UpdateHealthText();
    }

    private void UpdateMana(float current, float max, bool isInstant)
    {
        _targetMana = current;
        _maxMana = max;
        if (isInstant) { _displayedMana = current; _manaTimer = DisplayDuration; }
        else { _manaTimer = 0f; }
        UpdateManaText();
    }

    public override void _Process(double delta)
    {
        float fDelta = (float)delta;
        if (_healthTimer < DisplayDuration)
        {
            _healthTimer += fDelta;
            _displayedHealth = Mathf.Lerp(_displayedHealth, _targetHealth, fDelta / (DisplayDuration - _healthTimer + fDelta));
            UpdateHealthText();
        }

        if (_manaTimer < DisplayDuration)
        {
            _manaTimer += fDelta;
            _displayedMana = Mathf.Lerp(_displayedMana, _targetMana, fDelta / (DisplayDuration - _manaTimer + fDelta));
            UpdateManaText();
        }
    }


    private void UpdateZoneText(int zoneLevel) => zoneText.Text = $"Monster level: {zoneLevel}";
    private void UpdateHealthText() => healthText.Text = $"{Mathf.RoundToInt(_displayedHealth)} / {Mathf.RoundToInt(_maxHealth)}";
    private void UpdateManaText() => manaText.Text = $"{Mathf.RoundToInt(_displayedMana)} / {Mathf.RoundToInt(_maxMana)}";

    public async void ShowSpeechBubble(string text, float duration)
    {
        var speechText = speechBubble.GetNode<Label>("Label");
        speechText.Text = text;
        speechBubble.Visible = true;

        if (duration < 0f) duration = Mathf.Clamp(text.Length * 0.06f, 1.5f, 4f);
        await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);
        speechBubble.Visible = false;
    }

    public override void _ExitTree()
    {
        if (DungeonManager.Instance != null)
            DungeonManager.Instance.OnZoneLevelChanged -= UpdateZoneText;
    }
}