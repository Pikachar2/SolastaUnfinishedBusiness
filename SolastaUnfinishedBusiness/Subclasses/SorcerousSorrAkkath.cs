﻿using System.Collections;
using System.Linq;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Properties;
using static AttributeDefinitions;
using static FeatureDefinitionAttributeModifier;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class SorcerousSorrAkkath : AbstractSubclass
{
    private const string Name = "SorcerousSorrAkkath";
    private const string DeceptiveHeritage = "DeceptiveHeritage";
    private const string SpellSneakAttack = "SpellSneakAttack";
    private const string BloodOfSorrAkkath = "BloodOfSorrAkkath";
    private const string DarknessAffinity = "DarknessAffinity";
    private const string TouchOfDarkness = "TouchOfDarkness";

    internal SorcerousSorrAkkath()
    {
        // LEVEL 01

        var autoPreparedSpells = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create($"AutoPreparedSpells{Name}")
            .SetGuiPresentation("ExpandedSpells", Category.Feature)
            .SetAutoTag("Origin")
            .SetSpellcastingClass(CharacterClassDefinitions.Sorcerer)
            .AddPreparedSpellGroup(1, HideousLaughter)
            .AddPreparedSpellGroup(3, Invisibility)
            .AddPreparedSpellGroup(5, Fear)
            .AddPreparedSpellGroup(7, GreaterInvisibility)
            .AddPreparedSpellGroup(9, DominatePerson)
            .AddPreparedSpellGroup(11, GlobeOfInvulnerability)
            .AddToDB();

        // Deceptive Heritage

        var bonusCantripsDeceptiveHeritage = FeatureDefinitionBonusCantripsBuilder
            .Create($"BonusCantrips{Name}{DeceptiveHeritage}")
            .SetGuiPresentationNoContent(true)
            .SetBonusCantrips(VenomousSpike)
            .AddToDB();

        var proficiencyDeceptiveHeritage = FeatureDefinitionProficiencyBuilder
            .Create($"Proficiency{Name}{DeceptiveHeritage}")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.SkillOrExpertise, SkillDefinitions.Deception, SkillDefinitions.Stealth)
            .AddToDB();

        var featureSetDeceptiveHeritage = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}{DeceptiveHeritage}")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                bonusCantripsDeceptiveHeritage,
                proficiencyDeceptiveHeritage,
                FeatureDefinitionSenses.SenseDarkvision12)
            .AddToDB();

        // Spell Sneak Attack

        var additionalDamageSpellSneakAttack = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{Name}{SpellSneakAttack}")
            .SetGuiPresentation(Category.Feature)
            .SetNotificationTag(SpellSneakAttack)
            .SetDamageDice(DieType.D6, 1)
            .SetAdvancement(AdditionalDamageAdvancement.ClassLevel, 2, 1, 6, 5)
            .SetRequiredProperty(RestrictedContextRequiredProperty.SpellWithAttackRoll)
            .SetTriggerCondition(AdditionalDamageTriggerCondition.AdvantageOrNearbyAlly)
            .SetFrequencyLimit(FeatureLimitedUsage.OncePerTurn)
            .SetSavingThrowData(EffectDifficultyClassComputation.SpellCastingFeature, EffectSavingThrowType.None)
            .AddToDB();

        // another odd dice damage progression
        for (var i = 0; i < 4; i++)
        {
            additionalDamageSpellSneakAttack.DiceByRankTable[i].diceNumber = 1;
        }

        // LEVEL 06

        // Blood of Sorr-Akkath

        var conditionBloodOfSorrAkkath = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionPoisoned, $"Condition{Name}{BloodOfSorrAkkath}")
            .SetSpecialDuration(DurationType.Round, 1, TurnOccurenceType.EndOfSourceTurn)
            .AddToDB();

        var additionalDamageBloodOfSorrAkkath = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{Name}{BloodOfSorrAkkath}")
            .SetGuiPresentation($"AdditionalDamage{Name}{SpellSneakAttack}", Category.Feature)
            .SetNotificationTag(SpellSneakAttack)
            .SetDamageDice(DieType.D6, 1)
            .SetAdvancement(AdditionalDamageAdvancement.ClassLevel, 2, 1, 6, 5)
            .SetRequiredProperty(RestrictedContextRequiredProperty.SpellWithAttackRoll)
            .SetTriggerCondition(AdditionalDamageTriggerCondition.AdvantageOrNearbyAlly)
            .SetFrequencyLimit(FeatureLimitedUsage.OncePerTurn)
            .SetSavingThrowData(EffectDifficultyClassComputation.SpellCastingFeature, EffectSavingThrowType.None)
            .SetConditionOperations(
                new ConditionOperationDescription
                {
                    hasSavingThrow = true,
                    saveOccurence = TurnOccurenceType.EndOfTurn,
                    saveAffinity = EffectSavingThrowType.Negates,
                    operation = ConditionOperationDescription.ConditionOperation.Add,
                    conditionDefinition = conditionBloodOfSorrAkkath
                })
            .SetCustomSubFeatures(new FeatureDefinitionCustomCodeBloodOfSorrAkkath(additionalDamageSpellSneakAttack))
            .AddToDB();

        // another odd dice damage progression
        for (var i = 0; i < 4; i++)
        {
            additionalDamageBloodOfSorrAkkath.DiceByRankTable[i].diceNumber = 1;
        }

        var featureSetBloodOfSorrAkkath = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}{BloodOfSorrAkkath}")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionConditionAffinitys.ConditionAffinityPoisonImmunity,
                FeatureDefinitionDamageAffinitys.DamageAffinityPoisonResistance,
                additionalDamageBloodOfSorrAkkath)
            .AddToDB();

        // LEVEL 14

        // Darkness Affinity

        const string DARKNESS_AFFINITY_NAME = $"FeatureSet{Name}{DarknessAffinity}";

        var attackModifierDarknessAffinity = FeatureDefinitionAttackModifierBuilder
            .Create($"AttackModifier{Name}{DarknessAffinity}")
            .SetGuiPresentation(DARKNESS_AFFINITY_NAME, Category.Feature)
            .SetAttackRollModifier(2)
            .SetCustomSubFeatures(ValidatorsCharacter.IsNotInBrightLight)
            .AddToDB();

        var attributeModifierDarknessAffinity = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{Name}{DarknessAffinity}")
            .SetGuiPresentation(DARKNESS_AFFINITY_NAME, Category.Feature)
            .SetSituationalContext(ExtraSituationalContext.IsNotInBrightLight)
            .SetModifier(AttributeModifierOperation.Additive, ArmorClass, 2)
            .AddToDB();

        var magicAffinityDarknessAffinity = FeatureDefinitionMagicAffinityBuilder
            .Create($"MagicAffinity{Name}{DarknessAffinity}")
            .SetGuiPresentation(DARKNESS_AFFINITY_NAME, Category.Feature)
            .SetCastingModifiers(2)
            .SetCustomSubFeatures(ValidatorsCharacter.IsNotInBrightLight)
            .AddToDB();

        var regenerationDarknessAffinity = FeatureDefinitionRegenerationBuilder
            .Create(FeatureDefinitionRegenerations.RegenerationRing, $"Regeneration{Name}{DarknessAffinity}")
            .SetGuiPresentation(DARKNESS_AFFINITY_NAME, Category.Feature)
            .SetDuration(DurationType.Round, 1)
            .SetRegenerationDice(DieType.D1, 0, 2)
            .SetCustomSubFeatures(ValidatorsCharacter.IsNotInBrightLight)
            .AddToDB();

        var savingThrowAffinityDarknessAffinity = FeatureDefinitionSavingThrowAffinityBuilder
            .Create($"SavingThrowAffinity{Name}{DarknessAffinity}")
            .SetGuiPresentation(DARKNESS_AFFINITY_NAME, Category.Feature)
            .SetModifiers(FeatureDefinitionSavingThrowAffinity.ModifierType.AddDice, DieType.D1, 2, false,
                Charisma, Constitution, Dexterity, Intelligence, Strength, Wisdom)
            .SetCustomSubFeatures(ValidatorsCharacter.IsNotInBrightLight)
            .AddToDB();

        var featureSetDarknessAffinity = FeatureDefinitionFeatureSetBuilder
            .Create(DARKNESS_AFFINITY_NAME)
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                attackModifierDarknessAffinity,
                attributeModifierDarknessAffinity,
                magicAffinityDarknessAffinity,
                regenerationDarknessAffinity,
                savingThrowAffinityDarknessAffinity)
            .AddToDB();

        // LEVEL 18

        // Touch of Darkness

        const string TOUCH_OF_DARKNESS_NAME = $"FeatureSet{Name}{TouchOfDarkness}";

        var powerTouchOfDarkness = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}{TouchOfDarkness}")
            .SetGuiPresentation(TOUCH_OF_DARKNESS_NAME, Category.Feature, hidden: true)
            .SetUsesFixed(ActivationTime.Permanent, RechargeRate.LongRest, 1, 3)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create(VampiricTouch)
                    .SetDurationData(DurationType.Instantaneous)
                    .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.IndividualsUnique)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetDamageForm(DamageTypeNecrotic, 6, DieType.D8, 0, HealFromInflictedDamage.Full)
                            .Build())
                    .Build())
            .AddToDB();

        powerTouchOfDarkness.SetCustomSubFeatures(new CustomMagicEffectActionTouchOfDarkness(powerTouchOfDarkness));

        var powerTouchOfDarknessRefund = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}{TouchOfDarkness}Refund")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.Action)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Instantaneous)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .Build())
            .AddToDB();

        powerTouchOfDarknessRefund.SetCustomSubFeatures(
            new CustomBehaviorRefundTouchOfDarkness(powerTouchOfDarkness, powerTouchOfDarknessRefund));

        var featureSetTouchOfDarkness = FeatureDefinitionFeatureSetBuilder
            .Create(TOUCH_OF_DARKNESS_NAME)
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(powerTouchOfDarkness, powerTouchOfDarknessRefund)
            .AddToDB();

        // BUGFIX

        ChillTouch.EffectDescription.EffectForms[0].savingThrowAffinity = EffectSavingThrowType.None;
        RayOfFrost.EffectDescription.EffectForms[0].savingThrowAffinity = EffectSavingThrowType.None;

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.SorcererSorrAkkath, 256))
            .AddFeaturesAtLevel(1,
                autoPreparedSpells,
                featureSetDeceptiveHeritage,
                additionalDamageSpellSneakAttack)
            .AddFeaturesAtLevel(6,
                featureSetBloodOfSorrAkkath)
            .AddFeaturesAtLevel(14,
                featureSetDarknessAffinity)
            .AddFeaturesAtLevel(18,
                featureSetTouchOfDarkness)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceSorcerousOrigin;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    //
    // Blood of Sorr-Akkath
    //

    private sealed class FeatureDefinitionCustomCodeBloodOfSorrAkkath : IFeatureDefinitionCustomCode
    {
        private readonly FeatureDefinitionAdditionalDamage _featureDefinitionAdditionalDamage;

        public FeatureDefinitionCustomCodeBloodOfSorrAkkath(
            FeatureDefinitionAdditionalDamage featureDefinitionAdditionalDamage)
        {
            _featureDefinitionAdditionalDamage = featureDefinitionAdditionalDamage;
        }

        // remove original sneak attack as we've added a conditional one otherwise ours will never trigger
        public void ApplyFeature(RulesetCharacterHero hero, string tag)
        {
            foreach (var featureDefinitions in hero.ActiveFeatures.Values)
            {
                featureDefinitions.RemoveAll(x => x == _featureDefinitionAdditionalDamage);
            }
        }
    }

    //
    // Touch of Darkness
    //

    private class CustomMagicEffectActionTouchOfDarkness : ICustomMagicEffectAction
    {
        private readonly FeatureDefinitionPower _featureDefinitionPower;

        public CustomMagicEffectActionTouchOfDarkness(FeatureDefinitionPower featureDefinitionPower)
        {
            _featureDefinitionPower = featureDefinitionPower;
        }

        public IEnumerator ProcessCustomEffect(CharacterActionMagicEffect action)
        {
            var attacker = action.ActingCharacter;
            var rulesetAttacker = attacker.RulesetCharacter;

            if (rulesetAttacker.GetRemainingPowerCharges(_featureDefinitionPower) <= 0)
            {
                yield break;
            }

            var gameLocationActionService =
                ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;
            var gameLocationBattleService =
                ServiceRepository.GetService<IGameLocationBattleService>() as GameLocationBattleManager;
            var rulesetImplementationService = ServiceRepository.GetService<IRulesetImplementationService>();

            if (gameLocationActionService == null ||
                gameLocationBattleService == null ||
                rulesetImplementationService == null)
            {
                yield break;
            }

            var reactionParams =
                new CharacterActionParams(attacker, (ActionDefinitions.Id)ExtraActionId.DoNothingFree)
                {
                    StringParameter = "Reaction/&CustomReactionTouchOfDarknessDescription"
                };
            var previousReactionCount = gameLocationActionService.PendingReactionRequestGroups.Count;
            var reactionRequest = new ReactionRequestCustom("TouchOfDarkness", reactionParams);

            gameLocationActionService.AddInterruptRequest(reactionRequest);

            yield return gameLocationBattleService.WaitForReactions(
                attacker, gameLocationActionService, previousReactionCount);

            if (!reactionParams.ReactionValidated)
            {
                yield break;
            }

            GameConsoleHelper.LogCharacterUsedPower(rulesetAttacker, _featureDefinitionPower);

            var usablePower =
                rulesetAttacker.UsablePowers.FirstOrDefault(x => x.PowerDefinition == _featureDefinitionPower);
            var effectPower = rulesetImplementationService.InstantiateEffectPower(rulesetAttacker, usablePower, false);
            var defender = action.actionParams.TargetCharacters[0];

            effectPower.ApplyEffectOnCharacter(defender.RulesetCharacter, true, defender.LocationPosition);
            rulesetAttacker.UpdateUsageForPower(_featureDefinitionPower, _featureDefinitionPower.CostPerUse);
            GameConsoleHelper.LogCharacterUsedPower(rulesetAttacker, _featureDefinitionPower);
        }
    }

    //
    // Refund Touch of Darkness
    //

    private class CustomBehaviorRefundTouchOfDarkness : IPowerUseValidity, IOnAfterActionFeature
    {
        private readonly FeatureDefinitionPower _powerTouch;
        private readonly FeatureDefinitionPower _powerTouchRefund;

        public CustomBehaviorRefundTouchOfDarkness(
            FeatureDefinitionPower powerTouch,
            FeatureDefinitionPower powerTouchRefund)
        {
            _powerTouch = powerTouch;
            _powerTouchRefund = powerTouchRefund;
        }

        public void OnAfterAction(CharacterAction action)
        {
            if (action is not CharacterActionUsePower characterActionUsePower ||
                characterActionUsePower.activePower.PowerDefinition != _powerTouchRefund)
            {
                return;
            }

            var rulesetCharacter = action.ActingCharacter.RulesetCharacter;
            var rulesetPower = rulesetCharacter.UsablePowers.FirstOrDefault(x => x.PowerDefinition == _powerTouch);

            if (rulesetPower == null)
            {
                return;
            }

            rulesetCharacter.SpendSorceryPoints(4);
        }

        public bool CanUsePower(RulesetCharacter character, FeatureDefinitionPower featureDefinitionPower)
        {
            var hasSorceryPointsAvailable = character.RemainingSorceryPoints >= 4;
            var hasUsedPowerActivate = character.UsablePowers
                .Any(x => x.RemainingUses == 0 && x.PowerDefinition == _powerTouch);

            return hasUsedPowerActivate && hasSorceryPointsAvailable;
        }
    }
}
