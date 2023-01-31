using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ConditionDefinitions;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

public class SkinOfRetributionLogic
{
    public const string NAME = "SkinOfRetribution";
    private const string ConditionName = $"Condition{NAME}";
    public const int TEMP_HP_PER_LEVEL = 5;

    private static ConditionDefinition _condition;

    private SkinOfRetributionLogic() { }

    public static ConditionDefinition Condition => _condition ??= BuildCondition();

    private static SkinOfRetributionLogic Marker { get; } = new();

    private static ConditionDefinition BuildCondition()
    {
        var spriteReference = Sprites.GetSprite("ConditionMirrorImage", Resources.ConditionMirrorImage, 32);

        return ConditionDefinitionBuilder
            .Create(ConditionName)
            .SetGuiPresentation(Category.Spell, spriteReference)
            .SetCustomSubFeatures(Marker)
            .SetSilent(Silent.WhenAdded)
            .CopyParticleReferences(ConditionBlurred)
            .SetPossessive()
            .SetFeatures(BuildFeatures())
            .AddToDB();
    }

    private static FeatureDefinition[] BuildFeatures()
    {
        var powerSkinOfRetribution = FeatureDefinitionPowerBuilder
            .Create("PowerSkinOfRetribution")
            .SetGuiPresentationNoContent(true)
            .SetUsesFixed(ActivationTime.NoCost)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetDamageForm(DamageTypeCold, bonusDamage: TEMP_HP_PER_LEVEL)
                            .Build())
                    .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, TEMP_HP_PER_LEVEL)
                    .Build())
            //.SetCustomSubFeatures(new ModifyMagicEffectSkinOfRetribution())
            .SetUniqueInstance()
            .AddToDB();

        var damageSkinOfRetribution = FeatureDefinitionDamageAffinityBuilder
            .Create($"DamageAffinity{NAME}")
            .SetGuiPresentationNoContent(true)
            .SetGuiPresentation(NAME, Category.Spell)
            .SetDamageAffinityType(DamageAffinityType.None)
            //.SetCustomSubFeatures(SkinProvider.Mark)
            .SetCustomSubFeatures(SkinProvider.Mark, new ModifyMagicEffectSkinOfRetribution())
            .SetRetaliate(powerSkinOfRetribution, 1, true)
            .AddToDB();

        return new[] { damageSkinOfRetribution };
    }

    private static List<RulesetCondition> GetConditions(RulesetActor character)
    {
        var conditions = new List<RulesetCondition>();

        if (character == null)
        {
            return conditions;
        }

        character.GetAllConditions(conditions);

        return conditions.FindAll(c =>
                c.ConditionDefinition.HasSubFeatureOfType<SkinOfRetributionLogic>())
            .ToList();
    }

    internal static void AttackRollPostfix(
        RulesetActor target)
    {
        var conditions = GetConditions(target as RulesetCharacter);

        foreach (var condition in conditions
                     .Where(_ => ((RulesetCharacter)target).temporaryHitPoints == 0))
        {
            target.RemoveCondition(condition);
        }
    }


    internal class SkinProvider : ICustomConditionFeature
    {
        private SkinProvider()
        {
        }

        public static ICustomConditionFeature Mark { get; } = new SkinProvider();

        public void ApplyFeature(RulesetCharacter target, RulesetCondition rulesetCondition)
        {
            var condition = RulesetCondition.CreateActiveCondition(
                target.Guid, Condition, DurationType.Hour, 1,
                TurnOccurenceType.EndOfTurn, target.Guid, target.CurrentFaction.Name);

            target.AddConditionOfCategory(AttributeDefinitions.TagEffect, condition);
        }

        public void RemoveFeature(RulesetCharacter target, RulesetCondition rulesetCondition)
        {
            var conditions = GetConditions(target);

            foreach (var condition in conditions)
            {
                target.RemoveCondition(condition);
            }
        }
    }

    private sealed class ModifyMagicEffectSkinOfRetribution : IModifyMagicEffect
    {
        public EffectDescription ModifyEffect(BaseDefinition definition, EffectDescription effect, RulesetCharacter character)
        {
            var rulesetCondition =
                character.AllConditions.FirstOrDefault(x => x.EffectDefinitionName == "SkinOfRetribution");

            if (rulesetCondition == null || !effect.HasDamageForm())
            {
                return effect;
            }

            var effectLevel = rulesetCondition.EffectLevel;
            var damageForm = effect.FindFirstDamageForm();

            damageForm.bonusDamage *= effectLevel;

            return effect;
        }
    }
}
