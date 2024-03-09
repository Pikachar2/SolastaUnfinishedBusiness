using System.Collections.Generic;
using System.Linq;
using Mono.CSharp;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Validators;
using UnityEngine;
using static MetricsDefinitions;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses.Builders;

internal static class MetamagicBuilders
{
    private const string MetamagicAltruistic = "MetamagicAltruisticSpell";
    private const string MetamagicFocused = "MetamagicFocusedSpell";
    private const string MetamagicPowerful = "MetamagicPowerfulSpell";
    private const string MetamagicTransmuted = "MetamagicTransmutedSpell";
    private const string MetamagicWidened = "MetamagicWidenedSpell";

    private static readonly (string, string)[] ELEMENTAL_DAMAGE_TYPES =
    [
        ("DamageAcid", "Acid"), ("DamageCold", "Cold"), ("DamageFire", "Fire"), 
        ("DamageLightning", "Lightning"), ("DamagePoison", "Poison"), ("DamageThunder", "Thunder")
    ];


    #region Metamagic Altruistic

    internal static MetamagicOptionDefinition BuildMetamagicAltruisticSpell()
    {
        var validator = new ValidateMetamagicApplication(IsMetamagicAltruisticSpellValid);

        var altruisticAlly = MetamagicOptionDefinitionBuilder
            .Create($"{MetamagicAltruistic}Ally")
            .SetGuiPresentation(Category.Feature, hidden: true)
            .SetCost()
            .AddCustomSubFeatures(new MetamagicAltruisticAlly(), validator)
            .AddToDB();

        var altruisticSelf = MetamagicOptionDefinitionBuilder
            .Create($"{MetamagicAltruistic}Self")
            .SetGuiPresentation(Category.Feature, hidden: true)
            .SetCost(sorceryPointsCost: 3)
            .AddCustomSubFeatures(new MetamagicAltruisticSelf(), validator)
            .AddToDB();

        return MetamagicOptionDefinitionBuilder
            .Create(MetamagicAltruistic)
            .SetGuiPresentation(Category.Feature)
            .SetCost(MetamagicCostMethod.SpellLevel)
            .AddCustomSubFeatures(new ReplaceMetamagicOption(altruisticAlly, altruisticSelf))
            .AddToDB();
    }

    private static void IsMetamagicAltruisticSpellValid(
        RulesetCharacter caster,
        RulesetEffectSpell rulesetEffectSpell,
        MetamagicOptionDefinition metamagicOption,
        ref bool result,
        ref string failure)
    {
        var effect = rulesetEffectSpell.EffectDescription;

        if (effect.rangeType == RangeType.Self && effect.targetType == TargetType.Self)
        {
            return;
        }

        failure = "Failure/&FailureFlagSpellRangeMustBeSelf";

        result = false;
    }

    private sealed class MetamagicAltruisticAlly : IModifyEffectDescription
    {
        public bool IsValid(
            BaseDefinition definition,
            RulesetCharacter character,
            EffectDescription effectDescription)
        {
            return true;
        }

        public EffectDescription GetEffectDescription(
            BaseDefinition definition,
            EffectDescription effectDescription,
            RulesetCharacter character,
            RulesetEffect rulesetEffect)
        {
            effectDescription.rangeType = RangeType.Distance;
            effectDescription.rangeParameter = 6;
            effectDescription.targetType = TargetType.IndividualsUnique;
            effectDescription.targetParameter = 1;
            effectDescription.targetExcludeCaster = true;

            return effectDescription;
        }
    }

    private sealed class MetamagicAltruisticSelf : IModifyEffectDescription
    {
        public bool IsValid(
            BaseDefinition definition,
            RulesetCharacter character,
            EffectDescription effectDescription)
        {
            return true;
        }

        public EffectDescription GetEffectDescription(
            BaseDefinition definition,
            EffectDescription effectDescription,
            RulesetCharacter character,
            RulesetEffect rulesetEffect)
        {
            effectDescription.rangeType = RangeType.Distance;
            effectDescription.rangeParameter = 6;
            effectDescription.targetType = TargetType.IndividualsUnique;
            effectDescription.targetParameter = 1;
            effectDescription.inviteOptionalAlly = true;

            return effectDescription;
        }
    }

    #endregion

    #region Metamagic Focused

    internal static MetamagicOptionDefinition BuildMetamagicFocusedSpell()
    {
        var validator = new ValidateMetamagicApplication(IsMetamagicFocusedSpellValid);

        var magicAffinity = FeatureDefinitionMagicAffinityBuilder
            .Create($"MagiAffinity{MetamagicFocused}")
            .SetGuiPresentation(MetamagicFocused, Category.Feature)
            .SetConcentrationModifiers(ConcentrationAffinity.Advantage)
            .AddToDB();

        var condition = ConditionDefinitionBuilder
            .Create($"Condition{MetamagicFocused}")
            .SetGuiPresentation(MetamagicFocused, Category.Feature,
                DatabaseHelper.ConditionDefinitions.ConditionBearsEndurance)
            .SetPossessive()
            .AddFeatures(magicAffinity)
            .AddToDB();

        return MetamagicOptionDefinitionBuilder
            .Create(MetamagicFocused)
            .SetGuiPresentation(Category.Feature)
            .SetCost()
            .AddCustomSubFeatures(new ModifyEffectDescriptionMetamagicFocused(condition), validator)
            .AddToDB();
    }

    private static void IsMetamagicFocusedSpellValid(
        RulesetCharacter caster,
        RulesetEffectSpell rulesetEffectSpell,
        MetamagicOptionDefinition metamagicOption,
        ref bool result,
        ref string failure)
    {
        var spell = rulesetEffectSpell.SpellDefinition;

        if (spell.RequiresConcentration)
        {
            return;
        }

        failure = "Failure/&FailureFlagSpellMustRequireConcentration";

        result = false;
    }

    private sealed class ModifyEffectDescriptionMetamagicFocused(ConditionDefinition conditionFocused)
        : IModifyEffectDescription
    {
        public bool IsValid(
            BaseDefinition definition,
            RulesetCharacter character,
            EffectDescription effectDescription)
        {
            return true;
        }

        public EffectDescription GetEffectDescription(
            BaseDefinition definition,
            EffectDescription effectDescription,
            RulesetCharacter character,
            RulesetEffect rulesetEffect)
        {
            effectDescription.EffectForms.Add(
                EffectFormBuilder
                    .Create()
                    .SetConditionForm(conditionFocused, ConditionForm.ConditionOperation.Add, true)
                    .Build());

            return effectDescription;
        }
    }

    #endregion

    #region Metamagic Powerful

    internal static MetamagicOptionDefinition BuildMetamagicPowerfulSpell()
    {
        var validator = new ValidateMetamagicApplication(IsMetamagicPowerfulSpellValid);

        return MetamagicOptionDefinitionBuilder
            .Create(MetamagicPowerful)
            .SetGuiPresentation(Category.Feature)
            .SetCost()
            .AddCustomSubFeatures(new ModifyEffectDescriptionMetamagicPowerful(), validator)
            .AddToDB();
    }

    private static void IsMetamagicPowerfulSpellValid(
        RulesetCharacter caster,
        RulesetEffectSpell rulesetEffectSpell,
        MetamagicOptionDefinition metamagicOption,
        ref bool result,
        ref string failure)
    {
        var effect = rulesetEffectSpell.EffectDescription;

        if (effect.EffectForms.Any(x => x.FormType == EffectForm.EffectFormType.Damage))
        {
            return;
        }

        failure = "Failure/&FailureFlagSpellMustHaveDamageForm";

        result = false;
    }

    private sealed class ModifyEffectDescriptionMetamagicPowerful : IModifyEffectDescription
    {
        public bool IsValid(
            BaseDefinition definition,
            RulesetCharacter character,
            EffectDescription effectDescription)
        {
            return true;
        }

        public EffectDescription GetEffectDescription(BaseDefinition definition, EffectDescription effectDescription,
            RulesetCharacter character, RulesetEffect rulesetEffect)
        {
            foreach (var effectForm in effectDescription.EffectForms
                         .Where(x => x.FormType == EffectForm.EffectFormType.Damage))
            {
                effectForm.DamageForm.diceNumber += 1;
            }

            return effectDescription;
        }
    }

    #endregion

    #region Metamagic Transmuted

    internal static MetamagicOptionDefinition BuildMetamagicTransmutedSpell()
    {
        var validator = new ValidateMetamagicApplication(IsMetamagicTransmutedSpellValid);

        List<MetamagicOptionDefinition> transmutedItems = buildMetamagicTransmutedSpellItem(validator);

        return MetamagicOptionDefinitionBuilder
            .Create(MetamagicTransmuted)
            .SetGuiPresentation(Category.Feature)
            .SetCost(MetamagicCostMethod.SpellLevel)
            .AddCustomSubFeatures(new ReplaceMetamagicOption(transmutedItems.ToArray()))
//            .AddCustomSubFeatures(transmutedItems.ToArray()[0], transmutedItems.ToArray()[1])
            .AddToDB();
    }

    private static List<MetamagicOptionDefinition> buildMetamagicTransmutedSpellItem(ValidateMetamagicApplication validator)
    {
        List<MetamagicOptionDefinition> transmutedSpellList = [];
        foreach ((string damage, string type) in ELEMENTAL_DAMAGE_TYPES) 
        {
            var transmutedItem = MetamagicOptionDefinitionBuilder
                .Create($"{MetamagicTransmuted}" + type)
                .SetGuiPresentation(Category.Feature, hidden: true)
                .SetCost()
                .AddCustomSubFeatures(new ModifyEffectDescriptionMetamagicTransmuted(damage), validator)
                .AddToDB();
            transmutedSpellList.Add(transmutedItem);
        }
        return transmutedSpellList;
    }

    private static void IsMetamagicTransmutedSpellValid(
        RulesetCharacter caster,
        RulesetEffectSpell rulesetEffectSpell,
        MetamagicOptionDefinition metamagicOption,
        ref bool result,
        ref string failure)
    {
        var effect = rulesetEffectSpell.EffectDescription;
        System.Console.WriteLine("SOMETHIGN!!~!!");

        List<string> dummy = ELEMENTAL_DAMAGE_TYPES.Select(x => x.Item1).ToList();
        System.Console.WriteLine("damage Type List: " + dummy.ToString());

        if (null != effect.FindFirstDamageFormOfType(dummy))
//        if (null != effect.FindFirstDamageFormOfType([.. ELEMENTAL_DAMAGE_TYPES]))
            {
                return;
        }

        failure = "Failure/&FailureFlagSpellMustHaveDamageFormElemental";

        result = false;
    }

    private sealed class ModifyEffectDescriptionMetamagicTransmuted(string damageType) : IModifyEffectDescription
    {
        private readonly string damageType = damageType;

        public bool IsValid(
            BaseDefinition definition,
            RulesetCharacter character,
            EffectDescription effectDescription)
        {
            return true;
        }

        public EffectDescription GetEffectDescription(BaseDefinition definition, EffectDescription effectDescription,
            RulesetCharacter character, RulesetEffect rulesetEffect)
        {
            foreach (var effectForm in effectDescription.EffectForms
                         .Where(x => x.FormType == EffectForm.EffectFormType.Damage))
            {
                System.Console.WriteLine("DamageType before: " + effectForm.DamageForm.damageType);
                effectForm.DamageForm.damageType = damageType;
                System.Console.WriteLine("DamageType after: " + effectForm.DamageForm.damageType);
            }

            return effectDescription;
        }
    }

    #endregion

    #region Metamagic Widened

    internal static MetamagicOptionDefinition BuildMetamagicWidenedSpell()
    {
        var validator = new ValidateMetamagicApplication(IsMetamagicWidenedSpellValid);

        return MetamagicOptionDefinitionBuilder
            .Create(MetamagicWidened)
            .SetGuiPresentation(Category.Feature)
            .SetCost(MetamagicCostMethod.FixedValue, 2)
            .AddCustomSubFeatures(new ModifyEffectDescriptionMetamagicWidened(), validator)
            .AddToDB();
    }

    private static void IsMetamagicWidenedSpellValid(
        RulesetCharacter caster,
        RulesetEffectSpell rulesetEffectSpell,
        MetamagicOptionDefinition metamagicOption,
        ref bool result,
        ref string failure)
    {
        var effect = rulesetEffectSpell.EffectDescription;
        var shapeType = effect.TargetType switch
        {
            TargetType.Line => GeometricShapeType.Line,
            TargetType.Cone => GeometricShapeType.Cone,
            TargetType.Cube or TargetType.CubeWithOffset => GeometricShapeType.Cube,
            TargetType.Cylinder or TargetType.CylinderWithDiameter => GeometricShapeType.Cylinder,
            TargetType.Sphere or TargetType.PerceivingWithinDistance or TargetType.InLineOfSightWithinDistance
                or TargetType.ClosestWithinDistance => GeometricShapeType.Sphere,
            TargetType.WallLine => GeometricShapeType.WallLine,
            TargetType.WallRing => GeometricShapeType.WallRing,
            _ => GeometricShapeType.None
        };

        if (shapeType
            is GeometricShapeType.Cone
            or GeometricShapeType.Cube
            or GeometricShapeType.Cylinder
            or GeometricShapeType.Sphere)
        {
            return;
        }

        failure = "Failure/&FailureFlagSpellMustBeOfTargetArea";

        result = false;
    }

    private sealed class ModifyEffectDescriptionMetamagicWidened : IModifyEffectDescription
    {
        public bool IsValid(
            BaseDefinition definition,
            RulesetCharacter character,
            EffectDescription effectDescription)
        {
            return true;
        }

        public EffectDescription GetEffectDescription(
            BaseDefinition definition,
            EffectDescription effectDescription,
            RulesetCharacter character,
            RulesetEffect rulesetEffect)
        {
            effectDescription.targetParameter += effectDescription.TargetType == TargetType.Cube ? 2 : 1;

            return effectDescription;
        }
    }

    #endregion
}
