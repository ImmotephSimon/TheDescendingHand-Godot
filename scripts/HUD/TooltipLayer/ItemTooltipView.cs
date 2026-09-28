[GlobalClass]
public partial class ItemTooltipView : Control
{
    [Export] private RichTextLabel nameText;
    [Export] private RichTextLabel baseTypeText;
    [Export] private RichTextLabel implicitsText;
    [Export] private RichTextLabel explicitsText;
    [Export] private RichTextLabel loreText;

    [Export] private Control bottomSection;

    public void SetItem(ItemTooltipDto item)
    {
        GameData.Instance.Items.TryGetDefinition(item.BaseTypeId, out ItemDefinition definition);
        GameData.Instance.Items.TryGetRarity(item.RarityId, out Rarity rarity);

        SetTextSection(nameText, definition.DisplayName, rarity.DisplayColor);

        var equipComp = GetEquipComponent(definition);
        if (equipComp != null)
        {
            SetTextSection(baseTypeText, equipComp.EquipmentType.ResourceName);
        }

        SetTextSection(implicitsText, BuildImplicits(item));
        SetTextSection(explicitsText, AffixesToString(item.Explicits));

        bool hasLore = !string.IsNullOrEmpty(definition.Lore);
        if (bottomSection != null)
        {
            bottomSection.Visible = hasLore;
        }

        SetTextSection(loreText, definition.Lore);
    }

    public EquipComponentDefinition GetEquipComponent(ItemDefinition definition)
    {
        return definition.Components
            .OfType<EquipComponentDefinition>()
            .FirstOrDefault();
    }

    private void SetTextSection(RichTextLabel field, string content)
    {
        if (field == null) return;

        bool hasContent = !string.IsNullOrEmpty(content);
        field.Visible = hasContent;

        if (hasContent)
            field.Text = ConvertHtmlColorToBbCode(content);
    }

    private void SetTextSection(RichTextLabel field, string content, Color color)
    {
        if (field == null) return;

        bool hasContent = !string.IsNullOrEmpty(content);
        field.Visible = hasContent;

        if (hasContent)
        {
            field.Text = ConvertHtmlColorToBbCode(content);
            field.AddThemeColorOverride("default_color", color);
        }
    }

    private string ConvertHtmlColorToBbCode(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Replace("<color=", "[color=").Replace("</color>", "[/color]").Replace(">", "]");
    }

    private string BuildImplicits(ItemTooltipDto item)
    {
        var lines = new List<string>();

        string damage = GetDamageText(item);
        if (!string.IsNullOrEmpty(damage))
            lines.Add(damage);

        string rawImplicits = GetRawImplicitText(item);
        if (!string.IsNullOrEmpty(rawImplicits))
            lines.Add(rawImplicits);

        string crit = GetCritText(item);
        if (!string.IsNullOrEmpty(crit))
            lines.Add(crit);

        string speed = GetSpeedText(item);
        if (!string.IsNullOrEmpty(speed))
            lines.Add(speed);

        return string.Join("\n", lines);
    }

    private string GetRawImplicitText(ItemTooltipDto item)
    {
        if (item.Implicits == null)
            return string.Empty;

        var lines = new List<string>();

        foreach (var affix in item.Implicits)
        {
            if (affix.ResolvedMods == null) continue;

            foreach (var mod in affix.ResolvedMods)
            {
                if (!IsDisplayedAsWeaponStat(mod.Stat))
                    lines.Add(mod.ToString());
            }
        }

        return string.Join("\n", lines);
    }

    private bool IsDamageStat(GameTag stat)
    {
        if (stat == null) return false;

        if (stat == GameTags.ModOffenseDamage ||
            stat == GameTags.ModOffenseDamageMin ||
            stat == GameTags.ModOffenseDamageMax)
            return true;

        foreach (var damageType in GameTags.DamageTypes)
        {
            if (stat == damageType) return true;
        }

        return false;
    }

    private bool IsDisplayedAsWeaponStat(GameTag stat)
    {
        return IsDamageStat(stat) ||
               stat == GameTags.ModOffenseCritical ||
               stat == GameTags.ModOffenseCastSpeed;
    }

    private List<GameTag> GetElementTags(TagRequirement requirement, GameTag stat)
    {
        var elements = new List<GameTag>();

        foreach (var element in GameTags.DamageTypes)
        {
            if (requirement.Tags != null && requirement.Tags.HasTag(element))
                elements.Add(element);
            else if (stat == element)
                elements.Add(element);
        }

        return elements;
    }

    private string GetDamageText(ItemTooltipDto item)
    {
        var ranges = new Dictionary<GameTag, (float Min, float Max)>();

        void AddModifier(GameTag stat, MathOp op, TagRequirement tags, float value)
        {
            if (!IsDamageStat(stat) || op != MathOp.Added)
                return;

            var elements = GetElementTags(tags, stat);
            if (elements.Count == 0)
                elements.Add(GameTags.RestrictionPhysical);

            foreach (var element in elements)
            {
                var range = ranges.GetValueOrDefault(element);

                if (stat == GameTags.ModOffenseDamageMin)
                    range.Min += value;
                else if (stat == GameTags.ModOffenseDamageMax)
                    range.Max += value;
                else
                {
                    range.Min += value;
                    range.Max += value;
                }

                ranges[element] = range;
            }
        }

        if (item.Implicits != null)
        {
            foreach (var affix in item.Implicits)
            {
                if (affix.ResolvedMods == null) continue;
                foreach (var mod in affix.ResolvedMods)
                    AddModifier(mod.Stat, mod.Op, mod.RequiredTags, mod.Value);
            }
        }

        if (item.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix.ResolvedMods == null) continue;
                foreach (var mod in affix.ResolvedMods)
                    AddModifier(mod.Stat, mod.Op, mod.RequiredTags, mod.Value);
            }
        }

        void ApplyMultiplier(GameTag stat, TagRequirement tags, float value)
        {
            if (!IsDamageStat(stat)) return;

            var elements = GetElementTags(tags, stat);
            if (elements.Count == 0)
                elements.Add(GameTags.RestrictionPhysical);

            float multiplier = 1f + NormalizePercent(value);

            foreach (var element in elements)
            {
                if (!ranges.TryGetValue(element, out var range))
                    continue;

                range.Min *= multiplier;
                range.Max *= multiplier;
                ranges[element] = range;
            }
        }

        if (item.Implicits != null)
        {
            foreach (var affix in item.Implicits)
            {
                if (affix.ResolvedMods == null) continue;
                foreach (var mod in affix.ResolvedMods)
                {
                    if (mod.Op == MathOp.Multiplicative)
                        ApplyMultiplier(mod.Stat, mod.RequiredTags, mod.Value);
                }
            }
        }

        if (item.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix.ResolvedMods == null) continue;
                foreach (var mod in affix.ResolvedMods)
                {
                    if (mod.Op == MathOp.Multiplicative)
                        ApplyMultiplier(mod.Stat, mod.RequiredTags, mod.Value);
                }
            }
        }

        var lines = new List<string>();

        foreach (var (element, range) in ranges)
        {
            int min = Mathf.RoundToInt(range.Min);
            int max = Mathf.RoundToInt(range.Max);

            if (element == GameTags.RestrictionPhysical)
            {
                lines.Add($"Damage: {min}-{max}");
            }
            else
            {
                lines.Add($"{GetDamageLabel(element)}: <color={GetDamageColorHex(element)}>{min}-{max}</color>");
            }
        }

        return string.Join("\n", lines);
    }

    private string GetCritText(ItemTooltipDto item)
    {
        float baseCrit = 0f;
        float critMultiplier = 1f;

        void ProcessAffix(GameTag stat, MathOp op, float value)
        {
            if (stat != GameTags.ModOffenseCritical) return;

            if (op == MathOp.Added)
                baseCrit += value;
            else if (op == MathOp.Multiplicative)
                critMultiplier *= 1f + NormalizePercent(value);
        }

        if (item.Implicits != null)
        {
            foreach (var affix in item.Implicits)
            {
                if (affix.ResolvedMods == null) continue;
                foreach (var mod in affix.ResolvedMods)
                    ProcessAffix(mod.Stat, mod.Op, mod.Value);
            }
        }

        if (item.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix.ResolvedMods == null) continue;
                foreach (var mod in affix.ResolvedMods)
                    ProcessAffix(mod.Stat, mod.Op, mod.Value);
            }
        }

        if (baseCrit <= 0f) return string.Empty;

        float finalCrit = baseCrit * critMultiplier;
        return $"Critical: {finalCrit:F2}%";
    }

    private string GetSpeedText(ItemTooltipDto item)
    {
        float castSpeed = 0f;
        bool found = false;

        if (item.Implicits != null)
        {
            foreach (var affix in item.Implicits)
            {
                if (affix.ResolvedMods == null) continue;

                foreach (var mod in affix.ResolvedMods)
                {
                    if (mod.Stat != GameTags.ModOffenseCastSpeed) continue;

                    castSpeed = mod.Value;
                    found = true;
                    break;
                }

                if (found) break;
            }
        }

        if (!found) return string.Empty;

        if (item.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix.ResolvedMods == null) continue;

                foreach (var mod in affix.ResolvedMods)
                {
                    if (mod.Stat != GameTags.ModOffenseCastSpeed) continue;

                    castSpeed *= 1f + NormalizePercent(mod.Value);
                }
            }
        }

        return $"Cast Speed: {castSpeed:F2}";
    }

    private float NormalizePercent(float value)
    {
        return value > 1f ? value / 100f : value;
    }

    private string GetDamageLabel(GameTag element)
    {
        if (element == GameTags.RestrictionElementFire) return "Fire Damage";
        if (element == GameTags.RestrictionElementCold) return "Cold Damage";
        if (element == GameTags.RestrictionElementLightning) return "Lightning Damage";
        if (element == GameTags.RestrictionChaos) return "Chaos Damage";
        return "Damage";
    }

    private string GetDamageColorHex(GameTag element)
    {
        if (element == GameTags.RestrictionElementFire) return "#FF5555";
        if (element == GameTags.RestrictionElementCold) return "#55FFFF";
        if (element == GameTags.RestrictionElementLightning) return "#FFFF55";
        if (element == GameTags.RestrictionChaos) return "#D355FF";
        return "#FFFFFF";
    }

    private string AffixesToString(List<AffixState> states)
    {
        if (states == null || states.Count == 0) return string.Empty;
        return string.Join("\n", states
            .Where(s => s.ResolvedMods != null)
            .SelectMany(s => s.ResolvedMods)
            .Select(m => m.ToString()));
    }
}