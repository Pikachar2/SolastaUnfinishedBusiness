﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Behaviors.Specific;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Validators;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionActionAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public sealed class SorcerousPsion : AbstractSubclass
{
    private const string Name = "SorcerousPsion";

    public SorcerousPsion()
    {
        // LEVEL 01

        var autoPreparedSpellsPsion = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create($"AutoPreparedSpells{Name}")
            .SetGuiPresentation("ExpandedSpells", Category.Feature)
            .SetAutoTag("Origin")
            .SetSpellcastingClass(CharacterClassDefinitions.Sorcerer)
            .AddPreparedSpellGroup(1, Shield)
            .AddPreparedSpellGroup(3, SpellsContext.PsychicWhip)
            .AddPreparedSpellGroup(5, SpellsContext.PulseWave)
            .AddPreparedSpellGroup(7, Confusion)
            .AddPreparedSpellGroup(9, MindTwist)
            .AddToDB();

        // Psionic Mind

        var bonusCantripsPsionicMind = FeatureDefinitionDamageAffinityBuilder
            .Create($"DamageAffinity{Name}PsionicMind")
            .SetGuiPresentation(Category.Feature)
            .SetDamageAffinityType(DamageAffinityType.Resistance)
            .SetDamageType(DamageTypePsychic)
            .AddToDB();

        // Psychokinesis

        var powerPsychokinesisFixed = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}PsychokinesisFixed")
            .SetGuiPresentation($"FeatureSet{Name}Psychokinesis", Category.Feature, PowerMonkStepOfTheWindDash)
            .SetUsesProficiencyBonus(ActivationTime.BonusAction)
            .AddToDB();

        powerPsychokinesisFixed.AddCustomSubFeatures(
            new ValidatorsValidatePowerUse(c =>
                c.GetRemainingPowerUses(powerPsychokinesisFixed) > 0 ||
                c.GetClassLevel(CharacterClassDefinitions.Sorcerer) < 2));

        var powerPsychokinesisFixedDrag = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Name}PsychokinesisFixedDrag")
            .SetGuiPresentation($"Power{Name}PsychokinesisDrag", Category.Feature, hidden: true)
            .SetSharedPool(ActivationTime.BonusAction, powerPsychokinesisFixed)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.All, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetSavingThrowData(true, AttributeDefinitions.Strength, true,
                        EffectDifficultyClassComputation.SpellCastingFeature)
                    .ExcludeCaster()
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetMotionForm(MotionForm.MotionType.DragToOrigin, 3)
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .Build())
                    .SetParticleEffectParameters(PowerSpellBladeSpellTyrant)
                    .Build())
            .AddToDB();

        var powerPsychokinesisFixedPush = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Name}PsychokinesisFixedPush")
            .SetGuiPresentation($"Power{Name}PsychokinesisPush", Category.Feature, hidden: true)
            .SetSharedPool(ActivationTime.BonusAction, powerPsychokinesisFixed)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.All, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetSavingThrowData(true, AttributeDefinitions.Strength, true,
                        EffectDifficultyClassComputation.SpellCastingFeature)
                    .ExcludeCaster()
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetMotionForm(MotionForm.MotionType.PushFromOrigin, 3)
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .Build())
                    .SetParticleEffectParameters(PowerSpellBladeSpellTyrant)
                    .Build())
            .AddToDB();

        var powerPsychokinesisPoints = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}PsychokinesisPoints")
            .SetGuiPresentation($"FeatureSet{Name}Psychokinesis", Category.Feature, PowerMonkStepOfTheWindDash)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.SorceryPoints, 1, 0)
            .AddToDB();

        var powerPsychokinesisPointsDrag = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Name}PsychokinesisPointsDrag")
            .SetGuiPresentation($"Power{Name}PsychokinesisDrag", Category.Feature, hidden: true)
            .SetSharedPool(ActivationTime.BonusAction, powerPsychokinesisPoints)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.All, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetSavingThrowData(true, AttributeDefinitions.Strength, true,
                        EffectDifficultyClassComputation.SpellCastingFeature)
                    .ExcludeCaster()
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetMotionForm(MotionForm.MotionType.DragToOrigin, 3)
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .Build())
                    .SetParticleEffectParameters(PowerSpellBladeSpellTyrant)
                    .Build())
            .AddToDB();

        var powerPsychokinesisPointsPush = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Name}PsychokinesisPointsPush")
            .SetGuiPresentation($"Power{Name}PsychokinesisPush", Category.Feature, hidden: true)
            .SetSharedPool(ActivationTime.BonusAction, powerPsychokinesisPoints)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.All, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetSavingThrowData(true, AttributeDefinitions.Strength, true,
                        EffectDifficultyClassComputation.SpellCastingFeature)
                    .ExcludeCaster()
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetMotionForm(MotionForm.MotionType.PushFromOrigin, 3)
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .Build())
                    .SetParticleEffectParameters(PowerSpellBladeSpellTyrant)
                    .Build())
            .AddToDB();

        powerPsychokinesisPoints.AddCustomSubFeatures(
            new ValidatorsValidatePowerUse(c =>
                c.GetRemainingPowerUses(powerPsychokinesisFixed) == 0 &&
                c.GetClassLevel(CharacterClassDefinitions.Sorcerer) >= 2));

        PowerBundle.RegisterPowerBundle(powerPsychokinesisFixed, true,
            powerPsychokinesisFixedPush, powerPsychokinesisFixedDrag);

        PowerBundle.RegisterPowerBundle(powerPsychokinesisPoints, true,
            powerPsychokinesisPointsPush, powerPsychokinesisPointsDrag);

        var featureSetPsychokinesis = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}Psychokinesis")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                powerPsychokinesisFixed,
                powerPsychokinesisFixedDrag,
                powerPsychokinesisFixedPush,
                powerPsychokinesisPoints,
                powerPsychokinesisPointsDrag,
                powerPsychokinesisPointsPush)
            .AddToDB();

        // LEVEL 06

        // Mind Sculpt

        var actionAffinityMindSculpt = FeatureDefinitionActionAffinityBuilder
            .Create(ActionAffinitySorcererMetamagicToggle, $"ActionAffinity{Name}MindSculpt")
            .SetGuiPresentation(Category.Feature)
            .SetAuthorizedActions((ActionDefinitions.Id)ExtraActionId.MindSculptToggle)
            .AddCustomSubFeatures(new CustomBehaviorMindSculpt())
            .AddToDB();

        // LEVEL 14

        // Mind over Matter

        var powerMindOverMatter = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}MindOverMatter")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.LongRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.Distance, 0, TargetType.IndividualsUnique)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetMotionForm(MotionForm.MotionType.FallProne)
                            .Build())
                    .SetParticleEffectParameters(PowerDomainSunHeraldOfTheSun)
                    .SetCasterEffectParameters(PowerPatronFiendDarkOnesBlessing.EffectDescription
                        .EffectParticleParameters.effectParticleReference)
                    .Build())
            .AddToDB();

        powerMindOverMatter.AddCustomSubFeatures(
            ModifyPowerVisibility.Hidden,
            new OnReducedToZeroHpByEnemyMindOverMatter(powerMindOverMatter));

        // LEVEL 18

        // Supreme Will

        var powerSupremeWill = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}SupremeWill")
            .SetGuiPresentation($"FeatureSet{Name}SupremeWill", Category.Feature, hidden: true)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.ShortRest)
            .AddToDB();

        var actionAffinitySupremeWill = FeatureDefinitionActionAffinityBuilder
            .Create(ActionAffinitySorcererMetamagicToggle, $"ActionAffinity{Name}SupremeWill")
            .SetGuiPresentationNoContent(true)
            .SetAuthorizedActions((ActionDefinitions.Id)ExtraActionId.SupremeWillToggle)
            .AddCustomSubFeatures(
                new CustomBehaviorSupremeWill(powerSupremeWill),
                new ValidateDefinitionApplication(ValidatorsCharacter.HasAvailablePowerUsage(powerSupremeWill)))
            .AddToDB();

        var featureSetSupremeWill = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}SupremeWill")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(actionAffinitySupremeWill, powerSupremeWill)
            .AddToDB();

        // 
        // MAIN
        //

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.PathOfTheElements, 256))
            .AddFeaturesAtLevel(1, autoPreparedSpellsPsion, bonusCantripsPsionicMind, featureSetPsychokinesis)
            .AddFeaturesAtLevel(6, actionAffinityMindSculpt)
            .AddFeaturesAtLevel(14, powerMindOverMatter)
            .AddFeaturesAtLevel(18, featureSetSupremeWill)
            .AddToDB();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Sorcerer;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceSorcerousOrigin;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    //
    // Mind Sculpt
    //

    private sealed class CustomBehaviorMindSculpt : IMagicEffectBeforeHitConfirmedOnEnemy, IMagicEffectFinishedByMeAny
    {
        private bool _hasDamageChanged;

        public IEnumerator OnMagicEffectBeforeHitConfirmedOnEnemy(
            GameLocationBattleManager battleManager,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier actionModifier,
            RulesetEffect rulesetEffect,
            List<EffectForm> actualEffectForms,
            bool firstTarget,
            bool criticalHit)
        {
            _hasDamageChanged = false;

            var rulesetCharacter = attacker.RulesetCharacter;

            if (rulesetEffect is RulesetEffectSpell rulesetEffectSpell &&
                rulesetEffectSpell.EffectDescription.HasDamageForm() &&
                rulesetCharacter.IsToggleEnabled((ActionDefinitions.Id)ExtraActionId.MindSculptToggle) &&
                rulesetCharacter.RemainingSorceryPoints > 0)
            {
                foreach (var effectForm in actualEffectForms
                             .Where(x => x.FormType == EffectForm.EffectFormType.Damage))
                {
                    _hasDamageChanged = _hasDamageChanged || effectForm.DamageForm.DamageType != DamageTypePsychic;
                    effectForm.DamageForm.DamageType = DamageTypePsychic;
                }

                _hasDamageChanged = true;
            }

            if (!firstTarget)
            {
                yield break;
            }

            var charismaModifier = AttributeDefinitions.ComputeAbilityScoreModifier(
                rulesetCharacter.TryGetAttributeValue(AttributeDefinitions.Charisma));

            foreach (var effectForm in actualEffectForms
                         .Where(x =>
                             x.FormType == EffectForm.EffectFormType.Damage
                             && x.DamageForm.DamageType == DamageTypePsychic))
            {
                effectForm.DamageForm.BonusDamage = charismaModifier;
            }
        }

        public IEnumerator OnMagicEffectFinishedByMeAny(
            CharacterActionMagicEffect action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender)
        {
            if (action is not CharacterActionCastSpell)
            {
                yield break;
            }

            if (!_hasDamageChanged)
            {
                yield break;
            }

            _hasDamageChanged = false;

            var rulesetAttacker = attacker.RulesetCharacter;

            rulesetAttacker.SpendSorceryPoints(1);
            rulesetAttacker.SorceryPointsAltered?.Invoke(rulesetAttacker, rulesetAttacker.RemainingSorceryPoints);
        }
    }

    //
    // Mind over Matter
    //

    private sealed class OnReducedToZeroHpByEnemyMindOverMatter(FeatureDefinitionPower powerMindOverMatter)
        : IOnReducedToZeroHpByEnemy
    {
        public IEnumerator HandleReducedToZeroHpByEnemy(
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RulesetEffect activeEffect)
        {
            var gameLocationActionService =
                ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;
            var gameLocationBattleService =
                ServiceRepository.GetService<IGameLocationBattleService>() as GameLocationBattleManager;

            if (gameLocationActionService == null || gameLocationBattleService is not { IsBattleInProgress: true })
            {
                yield break;
            }

            var rulesetCharacter = defender.RulesetCharacter;

            if (rulesetCharacter.GetRemainingPowerUses(powerMindOverMatter) == 0)
            {
                yield break;
            }

            var implementationManagerService =
                ServiceRepository.GetService<IRulesetImplementationService>() as RulesetImplementationManager;

            var usablePower = PowerProvider.Get(powerMindOverMatter, rulesetCharacter);
            var targets = gameLocationBattleService.Battle
                .GetContenders(defender, withinRange: 2);
            var reactionParams = new CharacterActionParams(defender, ActionDefinitions.Id.PowerNoCost)
            {
                StringParameter = "MindOverMatter",
                ActionModifiers = Enumerable.Repeat(new ActionModifier(), targets.Count).ToList(),
                RulesetEffect = implementationManagerService
                    .MyInstantiateEffectPower(rulesetCharacter, usablePower, false),
                UsablePower = usablePower,
                targetCharacters = targets
            };

            var count = gameLocationActionService.PendingReactionRequestGroups.Count;

            gameLocationActionService.ReactToUsePower(reactionParams, "UsePower", defender);

            yield return gameLocationBattleService.WaitForReactions(attacker, gameLocationActionService, count);

            if (!reactionParams.ReactionValidated)
            {
                yield break;
            }

            var tempHitPoints = rulesetCharacter.GetClassLevel(CharacterClassDefinitions.Sorcerer) * 3;

            rulesetCharacter.StabilizeAndGainHitPoints(1);
            rulesetCharacter.ReceiveTemporaryHitPoints(
                tempHitPoints, DurationType.UntilLongRest, 0, TurnOccurenceType.StartOfTurn, rulesetCharacter.Guid);

            ServiceRepository.GetService<ICommandService>()?
                .ExecuteAction(new CharacterActionParams(defender, ActionDefinitions.Id.StandUp), null, true);
        }
    }

    //
    // Supreme Will
    //

    private sealed class CustomBehaviorSupremeWill(FeatureDefinitionPower powerSupremeWill)
        : IModifyConcentrationRequirement, IMagicEffectFinishedByMeAny
    {
        private bool _hasConcentrationChanged;

        public IEnumerator OnMagicEffectFinishedByMeAny(
            CharacterActionMagicEffect action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender)
        {
            if (action is not CharacterActionCastSpell actionCastSpell)
            {
                yield break;
            }

            if (!_hasConcentrationChanged)
            {
                yield break;
            }

            _hasConcentrationChanged = false;

            var rulesetCharacter = attacker.RulesetCharacter;
            var usablePower = PowerProvider.Get(powerSupremeWill, rulesetCharacter);

            rulesetCharacter.UsePower(usablePower);
            rulesetCharacter.SpendSorceryPoints(2 * actionCastSpell.ActiveSpell.EffectLevel);
            rulesetCharacter.SorceryPointsAltered?.Invoke(rulesetCharacter, rulesetCharacter.RemainingSorceryPoints);
        }

        public bool RequiresConcentration(RulesetCharacter rulesetCharacter, RulesetEffectSpell rulesetEffectSpell)
        {
            if (!rulesetCharacter.IsToggleEnabled((ActionDefinitions.Id)ExtraActionId.SupremeWillToggle))
            {
                return rulesetEffectSpell.SpellDefinition.RequiresConcentration;
            }

            if (rulesetCharacter.GetRemainingPowerUses(powerSupremeWill) == 0)
            {
                return rulesetEffectSpell.SpellDefinition.RequiresConcentration;
            }

            if (!rulesetEffectSpell.SpellDefinition.RequiresConcentration)
            {
                return rulesetEffectSpell.SpellDefinition.RequiresConcentration;
            }

            var requiredPoints = rulesetEffectSpell.EffectLevel * 2;

            _hasConcentrationChanged = rulesetCharacter.RemainingSorceryPoints >= requiredPoints;

            return !_hasConcentrationChanged;
        }
    }
}
