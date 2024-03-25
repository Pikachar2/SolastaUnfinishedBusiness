﻿using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Behaviors.Specific;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Validators;
using static RuleDefinitions;
using static FeatureDefinitionAttributeModifier;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

// ReSharper disable once IdentifierTypo
[UsedImplicitly]
public sealed class PathOfTheReaver : AbstractSubclass
{
    private const string Name = "PathOfTheReaver";

    public PathOfTheReaver()
    {
        // LEVEL 03

        var featureVoraciousFury = FeatureDefinitionBuilder
            .Create($"Feature{Name}VoraciousFury")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        var attributeModifierDraconicResilience = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{Name}ProfaneVitality")
            .SetGuiPresentation(Category.Feature)
            .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.HitPointBonusPerLevel, 1)
            .AddCustomSubFeatures(new CustomLevelUpLogicDraconicResilience())
            .AddToDB();

        // LEVEL 06

        var featureSetProfaneVitality = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}ProfaneVitality")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionDamageAffinitys.DamageAffinityNecroticResistance,
                FeatureDefinitionDamageAffinitys.DamageAffinityPoisonResistance)
            .AddToDB();

        // LEVEL 10

        var powerBloodbath = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}Bloodbath")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.ShortRest)
            .AddToDB();

        // LEVEL 14

        var featureCorruptedBlood = FeatureDefinitionBuilder
            .Create($"Feature{Name}CorruptedBlood")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        // CONNECT ALL THEM TOGETHER NOW

        featureVoraciousFury.AddCustomSubFeatures(
            new PhysicalAttackFinishedByMeVoraciousFury(featureVoraciousFury, powerBloodbath));
        powerBloodbath.AddCustomSubFeatures(
            ModifyPowerVisibility.Hidden,
            new OnReducedToZeroHpByMeBloodbath(powerBloodbath));
        featureCorruptedBlood.AddCustomSubFeatures(
            new PhysicalAttackFinishedOnMeCorruptedBlood(featureCorruptedBlood, powerBloodbath));

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.PathOfTheReaver, 256))
            .AddFeaturesAtLevel(3, featureVoraciousFury, attributeModifierDraconicResilience)
            .AddFeaturesAtLevel(6, featureSetProfaneVitality)
            .AddFeaturesAtLevel(10, powerBloodbath)
            .AddFeaturesAtLevel(14, featureCorruptedBlood)
            .AddToDB();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Barbarian;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceBarbarianPrimalPath;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    //
    // Common Helpers
    //

    private static void InflictDamage(
        // ReSharper disable once SuggestBaseTypeForParameter
        GameLocationCharacter attacker,
        GameLocationCharacter defender,
        int totalDamage,
        List<string> attackTags)
    {
        var damageForm = new DamageForm
        {
            DamageType = DamageTypeNecrotic, DieType = DieType.D1, DiceNumber = 0, BonusDamage = totalDamage
        };
        var rulesetAttacker = attacker.RulesetCharacter;
        var rulesetDefender = defender.RulesetActor;
        var applyFormsParams = new RulesetImplementationDefinitions.ApplyFormsParams
        {
            sourceCharacter = rulesetAttacker,
            targetCharacter = rulesetDefender,
            position = defender.LocationPosition
        };

        RulesetActor.InflictDamage(
            totalDamage,
            damageForm,
            DamageTypeNecrotic,
            applyFormsParams,
            rulesetDefender,
            false,
            rulesetAttacker.Guid,
            false,
            attackTags,
            new RollInfo(DieType.D1, [], totalDamage),
            false,
            out _);
    }

    private static void ReceiveHealing(GameLocationCharacter gameLocationCharacter, int totalHealing)
    {
        EffectHelpers.StartVisualEffect(
            gameLocationCharacter, gameLocationCharacter, Heal, EffectHelpers.EffectType.Effect);
        gameLocationCharacter.RulesetCharacter.ReceiveHealing(totalHealing, true, gameLocationCharacter.Guid);
    }

    private static IEnumerator HandleEnemyDeath(
        GameLocationCharacter attacker,
        RulesetAttackMode attackMode,
        FeatureDefinitionPower featureDefinitionPower)
    {
        var gameLocationActionService =
            ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;
        var gameLocationBattleService =
            ServiceRepository.GetService<IGameLocationBattleService>() as GameLocationBattleManager;

        if (gameLocationActionService == null || gameLocationBattleService is not { IsBattleInProgress: true })
        {
            yield break;
        }

        var rulesetAttacker = attacker.RulesetCharacter;

        if (rulesetAttacker.MissingHitPoints == 0 ||
            !rulesetAttacker.HasConditionOfTypeOrSubType(ConditionRaging) ||
            rulesetAttacker.GetRemainingPowerUses(featureDefinitionPower) == 0)
        {
            yield break;
        }

        if (!ValidatorsWeapon.IsMelee(attackMode) && !ValidatorsWeapon.IsUnarmed(attackMode))
        {
            yield break;
        }

        var classLevel = rulesetAttacker.GetClassLevel(CharacterClassDefinitions.Barbarian);
        var totalHealing = 2 * classLevel;

        var implementationManagerService =
            ServiceRepository.GetService<IRulesetImplementationService>() as RulesetImplementationManager;

        var usablePower = PowerProvider.Get(featureDefinitionPower, rulesetAttacker);
        var reactionParams = new CharacterActionParams(attacker, ActionDefinitions.Id.PowerNoCost)
        {
            StringParameter = "Bloodbath",
            StringParameter2 = "UseBloodbathDescription".Formatted(
                Category.Reaction, totalHealing.ToString()),
            RulesetEffect = implementationManagerService
                .MyInstantiateEffectPower(rulesetAttacker, usablePower, false),
            UsablePower = usablePower
        };

        var count = gameLocationActionService.PendingReactionRequestGroups.Count;

        gameLocationActionService.ReactToUsePower(reactionParams, "UsePower", attacker);

        yield return gameLocationBattleService.WaitForReactions(attacker, gameLocationActionService, count);

        if (!reactionParams.ReactionValidated)
        {
            yield break;
        }

        ReceiveHealing(attacker, totalHealing);
    }

    //
    // Draconic Resilience
    //

    private sealed class CustomLevelUpLogicDraconicResilience : ICustomLevelUpLogic
    {
        public void ApplyFeature(RulesetCharacterHero hero, string tag)
        {
            if (hero.TryGetAttribute(AttributeDefinitions.HitPoints, out var attribute))
            {
                attribute.maxValue += 3;
            }
        }

        public void RemoveFeature(RulesetCharacterHero hero, string tag)
        {
            // empty
        }
    }

    //
    // Voracious Fury
    //

    private sealed class PhysicalAttackFinishedByMeVoraciousFury(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        FeatureDefinition featureVoraciousFury,
        FeatureDefinitionPower powerBloodBath)
        : IPhysicalAttackFinishedByMe
    {
        public IEnumerator OnPhysicalAttackFinishedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RollOutcome rollOutcome,
            int damageAmount)
        {
            if (rollOutcome != RollOutcome.Success && rollOutcome != RollOutcome.CriticalSuccess)
            {
                yield break;
            }

            var rulesetAttacker = attacker.RulesetCharacter;

            if (rulesetAttacker is not { IsDeadOrDyingOrUnconscious: false })
            {
                yield break;
            }

            if (!IsVoraciousFuryValidContext(rulesetAttacker, attackMode))
            {
                yield break;
            }

            if (!attacker.OnceInMyTurnIsValid(featureVoraciousFury.Name))
            {
                yield break;
            }

            attacker.UsedSpecialFeatures.TryAdd(featureVoraciousFury.Name, 1);

            var multiplier = 1;

            if (rollOutcome is RollOutcome.CriticalSuccess)
            {
                multiplier += 1;
            }

            if (rulesetAttacker.MissingHitPoints > rulesetAttacker.CurrentHitPoints)
            {
                multiplier += 1;
            }

            var proficiencyBonus = rulesetAttacker.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);
            var totalDamageOrHealing = proficiencyBonus * multiplier;

            ReceiveHealing(attacker, totalDamageOrHealing);

            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false })
            {
                yield break;
            }

            rulesetAttacker.LogCharacterUsedFeature(featureVoraciousFury);
            EffectHelpers.StartVisualEffect(attacker, defender, VampiricTouch, EffectHelpers.EffectType.Effect);
            InflictDamage(attacker, defender, totalDamageOrHealing, attackMode.AttackTags);

            if (rulesetDefender.IsDeadOrDying)
            {
                yield return HandleEnemyDeath(attacker, attackMode, powerBloodBath);
            }
        }

        private static bool IsVoraciousFuryValidContext(RulesetCharacter rulesetCharacter, RulesetAttackMode attackMode)
        {
            var isValid =
                attackMode?.thrown == false &&
                (ValidatorsWeapon.IsMelee(attackMode) || ValidatorsWeapon.IsUnarmed(attackMode)) &&
                ValidatorsCharacter.DoesNotHaveHeavyArmor(rulesetCharacter) &&
                ValidatorsCharacter.HasAnyOfConditions(ConditionRaging)(rulesetCharacter);

            return isValid;
        }
    }

    //
    // Bloodbath
    //

    private class OnReducedToZeroHpByMeBloodbath(FeatureDefinitionPower powerBloodBath) : IOnReducedToZeroHpByMe
    {
        public IEnumerator HandleReducedToZeroHpByMe(
            GameLocationCharacter attacker,
            GameLocationCharacter downedCreature,
            RulesetAttackMode attackMode,
            RulesetEffect activeEffect)
        {
            yield return HandleEnemyDeath(attacker, attackMode, powerBloodBath);
        }
    }

    //
    // Corrupted Blood
    //

    private class PhysicalAttackFinishedOnMeCorruptedBlood(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        FeatureDefinition featureCorruptedBlood,
        FeatureDefinitionPower powerBloodBath)
        : IPhysicalAttackFinishedOnMe
    {
        public IEnumerator OnPhysicalAttackFinishedOnMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RollOutcome rollOutcome,
            int damageAmount)
        {
            if (rollOutcome != RollOutcome.Success && rollOutcome != RollOutcome.CriticalSuccess)
            {
                yield break;
            }

            var rulesetAttacker = attacker.RulesetCharacter;
            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false } ||
                rulesetAttacker is not { IsDeadOrDyingOrUnconscious: false })
            {
                yield break;
            }

            var constitution = rulesetDefender.TryGetAttributeValue(AttributeDefinitions.Constitution);
            var totalDamage = AttributeDefinitions.ComputeAbilityScoreModifier(constitution);
            var defenderAttackTags =
                defender.FindActionAttackMode(ActionDefinitions.Id.AttackMain)?.AttackTags ?? [];

            rulesetDefender.LogCharacterUsedFeature(featureCorruptedBlood);
            EffectHelpers.StartVisualEffect(attacker, defender, PowerSorcererChildRiftOffering);
            InflictDamage(attacker, defender, totalDamage, defenderAttackTags);

            if (rulesetAttacker.IsDeadOrDying)
            {
                yield return HandleEnemyDeath(defender, attackMode, powerBloodBath);
            }
        }
    }
}
