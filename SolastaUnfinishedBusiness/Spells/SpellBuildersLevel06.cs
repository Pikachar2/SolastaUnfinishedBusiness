﻿using System.Collections;
using System.Linq;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Subclasses;
using SolastaUnfinishedBusiness.Validators;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ConditionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static FeatureDefinitionAttributeModifier;

namespace SolastaUnfinishedBusiness.Spells;

internal static partial class SpellBuilders
{
    #region Mystical Cloak

    internal static SpellDefinition BuildMysticalCloak()
    {
        const string NAME = "MysticalCloak";

        var attributeModifierAC = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{NAME}")
            .SetGuiPresentation(NAME, Category.Spell)
            .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.ArmorClass, 2)
            .AddToDB();

        var conditionLowerPlane = ConditionDefinitionBuilder
            .Create($"Condition{NAME}LowerPlane")
            .SetGuiPresentation($"{NAME}LowerPlane", Category.Spell,
                ConditionDefinitions.ConditionMagicallyArmored)
            .SetPossessive()
            .SetParentCondition(ConditionDefinitions.ConditionFlying)
            .SetFeatures(
                attributeModifierAC,
                CommonBuilders.AttributeModifierCasterFightingExtraAttack,
                FeatureDefinitionMoveModes.MoveModeFly8,
                FeatureDefinitionDamageAffinitys.DamageAffinityFireImmunity,
                FeatureDefinitionDamageAffinitys.DamageAffinityPoisonImmunity,
                FeatureDefinitionConditionAffinitys.ConditionAffinityPoisonImmunity)
            .AddCustomSubFeatures(
                CanUseAttribute.SpellCastingAbility,
                new AddTagToWeaponWeaponAttack(
                    TagsDefinitions.MagicalWeapon, ValidatorsWeapon.AlwaysValid))
            .AddToDB();

        // there is indeed a typo on tag
        // ReSharper disable once StringLiteralTypo
        conditionLowerPlane.ConditionTags.Add("Verticality");

        var lowerPlane = SpellDefinitionBuilder
            .Create($"{NAME}LowerPlane")
            .SetGuiPresentation(Category.Spell)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolTransmutation)
            .SetSpellLevel(6)
            .SetCastingTime(ActivationTime.BonusAction)
            .SetMaterialComponent(MaterialComponentType.Specific)
            .SetSpecificMaterialComponent(TagsDefinitions.ItemTagDiamond, 500, true)
            .SetVerboseComponent(true)
            .SetSomaticComponent(true)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetRequiresConcentration(true)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 1)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(EffectFormBuilder.ConditionForm(conditionLowerPlane))
                    .SetParticleEffectParameters(MageArmor)
                    .Build())
            .AddToDB();

        lowerPlane.EffectDescription.EffectParticleParameters.conditionStartParticleReference =
            ConditionDefinitions.ConditionFlyingAdaptive.conditionStartParticleReference;
        lowerPlane.EffectDescription.EffectParticleParameters.conditionParticleReference =
            ConditionDefinitions.ConditionFlyingAdaptive.conditionParticleReference;
        lowerPlane.EffectDescription.EffectParticleParameters.conditionEndParticleReference =
            ConditionDefinitions.ConditionFlyingAdaptive.conditionEndParticleReference;

        var conditionHigherPlane = ConditionDefinitionBuilder
            .Create($"Condition{NAME}HigherPlane")
            .SetGuiPresentation($"{NAME}HigherPlane", Category.Spell,
                ConditionDefinitions.ConditionMagicallyArmored)
            .SetPossessive()
            .SetParentCondition(ConditionDefinitions.ConditionFlying)
            .SetFeatures(
                attributeModifierAC,
                CommonBuilders.AttributeModifierCasterFightingExtraAttack,
                FeatureDefinitionMoveModes.MoveModeFly8,
                FeatureDefinitionDamageAffinitys.DamageAffinityRadiantImmunity,
                FeatureDefinitionDamageAffinitys.DamageAffinityNecroticImmunity,
                FeatureDefinitionConditionAffinitys.ConditionAffinityCalmEmotionCharmedImmunity)
            .AddCustomSubFeatures(
                CanUseAttribute.SpellCastingAbility,
                new AddTagToWeaponWeaponAttack(TagsDefinitions.MagicalWeapon, ValidatorsWeapon.AlwaysValid))
            .AddToDB();

        // there is indeed a typo on tag
        // ReSharper disable once StringLiteralTypo
        conditionHigherPlane.ConditionTags.Add("Verticality");

        var higherPlane = SpellDefinitionBuilder
            .Create($"{NAME}HigherPlane")
            .SetGuiPresentation(Category.Spell)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolTransmutation)
            .SetSpellLevel(6)
            .SetCastingTime(ActivationTime.BonusAction)
            .SetMaterialComponent(MaterialComponentType.Specific)
            .SetSpecificMaterialComponent(TagsDefinitions.ItemTagDiamond, 500, true)
            .SetVerboseComponent(true)
            .SetSomaticComponent(true)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetRequiresConcentration(true)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 1)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(EffectFormBuilder.ConditionForm(conditionHigherPlane))
                    .SetParticleEffectParameters(MageArmor)
                    .Build())
            .AddToDB();

        higherPlane.EffectDescription.EffectParticleParameters.conditionStartParticleReference =
            ConditionDefinitions.ConditionFlyingAdaptive.conditionStartParticleReference;
        higherPlane.EffectDescription.EffectParticleParameters.conditionParticleReference =
            ConditionDefinitions.ConditionFlyingAdaptive.conditionParticleReference;
        higherPlane.EffectDescription.EffectParticleParameters.conditionEndParticleReference =
            ConditionDefinitions.ConditionFlyingAdaptive.conditionEndParticleReference;

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.MysticalCloak, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolTransmutation)
            .SetSpellLevel(6)
            .SetCastingTime(ActivationTime.BonusAction)
            .SetMaterialComponent(MaterialComponentType.Specific)
            .SetSpecificMaterialComponent(TagsDefinitions.ItemTagDiamond, 500, true)
            .SetVerboseComponent(true)
            .SetSomaticComponent(true)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetRequiresConcentration(true)
            .SetSubSpells(lowerPlane, higherPlane)
            .AddToDB();

        return spell;
    }

    #endregion

    #region Poison Wave

    internal static SpellDefinition BuildPoisonWave()
    {
        const string NAME = "PoisonWave";

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.PoisonWave, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetSpellLevel(6)
            .SetCastingTime(ActivationTime.Action)
            .SetMaterialComponent(MaterialComponentType.Specific)
            .SetSpecificMaterialComponent(TagsDefinitions.ItemTagGlass, 50, false)
            .SetVerboseComponent(true)
            .SetSomaticComponent(true)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 1)
                    .SetTargetingData(Side.All, RangeType.Self, 0, TargetType.Sphere, 4)
                    .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, additionalDicePerIncrement: 1)
                    .ExcludeCaster()
                    .SetSavingThrowData(false, AttributeDefinitions.Constitution, false,
                        EffectDifficultyClassComputation.SpellCastingFeature)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .HasSavingThrow(EffectSavingThrowType.HalfDamage)
                            .SetDamageForm(DamageTypePoison, 6, DieType.D10)
                            .Build(),
                        EffectFormBuilder
                            .Create()
                            .HasSavingThrow(EffectSavingThrowType.Negates, TurnOccurenceType.EndOfTurn, true)
                            .SetConditionForm(ConditionDefinitions.ConditionPoisoned,
                                ConditionForm.ConditionOperation.Add)
                            .Build())
                    .SetParticleEffectParameters(PoisonSpray)
                    .SetImpactEffectParameters(PowerDragonBreath_Poison)
                    .SetEffectEffectParameters(PowerVrockSpores)
                    .Build())
            .AddToDB();

        return spell;
    }

    #endregion

    #region Heroic Infusion

    internal static SpellDefinition BuildHeroicInfusion()
    {
        const string NAME = "HeroicInfusion";

        var attackModifierHeroicInfusion = FeatureDefinitionCombatAffinityBuilder
            .Create($"AttackModifier{NAME}")
            .SetGuiPresentation($"Condition{NAME}", Category.Condition, Gui.NoLocalization)
            .SetMyAttackAdvantage(AdvantageType.Advantage)
            .SetSituationalContext(ExtraSituationalContext.HasSimpleOrMartialWeaponInHands)
            .AddToDB();

        var additionalDamageHeroicInfusion = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{NAME}")
            .SetGuiPresentation($"Condition{NAME}", Category.Condition, Gui.NoLocalization)
            .SetNotificationTag(NAME)
            .SetDamageDice(DieType.D12, 2)
            .SetSpecificDamageType(DamageTypeForce)
            .AddToDB();

        var actionAffinityHeroicInfusion = FeatureDefinitionActionAffinityBuilder
            .Create($"ActionAffinity{NAME}")
            .SetGuiPresentation($"Condition{NAME}", Category.Condition, Gui.NoLocalization)
            .SetAuthorizedActions()
            .SetForbiddenActions(
                ActionDefinitions.Id.CastBonus, ActionDefinitions.Id.CastInvocation,
                ActionDefinitions.Id.CastMain, ActionDefinitions.Id.CastReaction,
                ActionDefinitions.Id.CastReadied, ActionDefinitions.Id.CastRitual, ActionDefinitions.Id.CastNoCost)
            .AddToDB();

        var conditionExhausted = ConditionDefinitionBuilder
            .Create(ConditionExhausted, $"Condition{NAME}Exhausted")
            .SetOrUpdateGuiPresentation("ConditionExhausted", Category.Rules, ConditionLethargic)
            .AddToDB();

        var conditionHeroicInfusion = ConditionDefinitionBuilder
            .Create($"Condition{NAME}")
            .SetGuiPresentation(Category.Condition, ConditionHeroism)
            .SetPossessive()
            .SetSpecialDuration(DurationType.Minute, 10)
            .SetFeatures(
                attackModifierHeroicInfusion,
                additionalDamageHeroicInfusion,
                actionAffinityHeroicInfusion,
                CommonBuilders.AttributeModifierCasterFightingExtraAttack,
                FeatureDefinitionProficiencys.ProficiencyFighterArmor,
                FeatureDefinitionProficiencys.ProficiencyFighterSavingThrow,
                FeatureDefinitionProficiencys.ProficiencyFighterWeapon)
            .AddCustomSubFeatures(new OnConditionAddedOrRemovedHeroicInfusion(conditionExhausted))
            .AddToDB();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.HeroicInfusion, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolTransmutation)
            .SetSpellLevel(6)
            .SetCastingTime(ActivationTime.Action)
            .SetMaterialComponent(MaterialComponentType.Mundane)
            .SetVerboseComponent(true)
            .SetSomaticComponent(true)
            .SetVocalSpellSameType(VocalSpellSemeType.Buff)
            .SetRequiresConcentration(true)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.UntilLongRest)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(
                        EffectFormBuilder.ConditionForm(conditionHeroicInfusion),
                        EffectFormBuilder
                            .Create()
                            .SetTempHpForm(50)
                            .Build())
                    .SetParticleEffectParameters(DivineFavor)
                    .Build())
            .AddToDB();

        return spell;
    }

    private sealed class OnConditionAddedOrRemovedHeroicInfusion(ConditionDefinition conditionExhausted)
        : IOnConditionAddedOrRemoved
    {
        public void OnConditionAdded(RulesetCharacter target, RulesetCondition rulesetCondition)
        {
            // empty
        }

        public void OnConditionRemoved(RulesetCharacter target, RulesetCondition rulesetCondition)
        {
            target.TemporaryHitPoints = 0;


            var modifierTrend = target.actionModifier.savingThrowModifierTrends;
            var advantageTrends = target.actionModifier.savingThrowAdvantageTrends;
            var conModifier = AttributeDefinitions.ComputeAbilityScoreModifier(
                target.TryGetAttributeValue(AttributeDefinitions.Constitution));

            target.RollSavingThrow(0, AttributeDefinitions.Constitution, null, modifierTrend,
                advantageTrends, conModifier, 15, false, out var savingOutcome, out _);

            if (savingOutcome is RollOutcome.Success)
            {
                return;
            }

            target.InflictCondition(
                conditionExhausted.Name,
                conditionExhausted.DurationType,
                conditionExhausted.DurationParameter,
                conditionExhausted.TurnOccurence,
                AttributeDefinitions.TagEffect,
                target.guid,
                target.CurrentFaction.Name,
                1,
                conditionExhausted.Name,
                0,
                0,
                0);
        }
    }

    #endregion

    #region Ring of Blades

    internal static SpellDefinition BuildRingOfBlades()
    {
        const string NAME = "RingOfBlades";

        var powerRingOfBlades = FeatureDefinitionPowerBuilder
            .Create($"Power{NAME}")
            .SetGuiPresentation(Category.Feature, Sprites.GetSprite($"Power{NAME}", Resources.PowerRingOfBlades, 128))
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.None, 1, 6)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.RangeHit, 12, TargetType.IndividualsUnique)
                    .SetEffectForms(EffectFormBuilder.DamageForm(DamageTypeForce, 4, DieType.D8))
                    .SetParticleEffectParameters(ShadowDagger)
                    .SetCasterEffectParameters(PowerDomainLawWordOfLaw)
                    .Build())
            .AddToDB();

        var powerRingOfBladesFree = FeatureDefinitionPowerBuilder
            .Create($"Power{NAME}Free")
            .SetGuiPresentation($"Power{NAME}", Category.Feature,
                Sprites.GetSprite($"Power{NAME}", Resources.PowerRingOfBlades, 128))
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.None, 1, 6)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.RangeHit, 12, TargetType.IndividualsUnique)
                    .SetEffectForms(EffectFormBuilder.DamageForm(DamageTypeForce, 4, DieType.D8))
                    .SetParticleEffectParameters(ShadowDagger)
                    .SetCasterEffectParameters(PowerDomainLawWordOfLaw)
                    .Build())
            .AddToDB();

        var conditionRingOfBlades = ConditionDefinitionBuilder
            .Create($"Condition{NAME}")
            .SetGuiPresentation($"Power{NAME}", Category.Feature, ConditionGuided)
            .SetPossessive()
            .SetConditionType(ConditionType.Beneficial)
            .SetFeatures(powerRingOfBlades)
            .AddCustomSubFeatures(AddUsablePowersFromCondition.Marker)
            .CopyParticleReferences(PowerSorcererChildRiftDeflection)
            .AddToDB();

        conditionRingOfBlades.GuiPresentation.description = Gui.NoLocalization;

        var conditionRingOfBladesFree = ConditionDefinitionBuilder
            .Create($"Condition{NAME}Free")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(powerRingOfBladesFree)
            .AddCustomSubFeatures(AddUsablePowersFromCondition.Marker)
            .SetSpecialInterruptions(ConditionInterruption.AnyBattleTurnEnd)
            .AddToDB();

        powerRingOfBlades.AddCustomSubFeatures(
            new CustomBehaviorPowerRingOfBlades(powerRingOfBlades, conditionRingOfBlades));
        powerRingOfBladesFree.AddCustomSubFeatures(
            ValidatorsValidatePowerUse.InCombat,
            // it's indeed powerRingOfBlades here
            new MagicEffectFinishedByMeRingOfBladesFree(powerRingOfBlades, conditionRingOfBladesFree),
            new CustomBehaviorPowerRingOfBlades(powerRingOfBladesFree, conditionRingOfBlades));

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.RingOfBlades, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolConjuration)
            .SetSpellLevel(6)
            .SetCastingTime(ActivationTime.BonusAction)
            .SetMaterialComponent(MaterialComponentType.Specific)
            .SetSpecificMaterialComponent(TagsDefinitions.WeaponTagMelee, 500, false)
            .SetVerboseComponent(true)
            .SetSomaticComponent(true)
            .SetVocalSpellSameType(VocalSpellSemeType.Buff)
            .SetRequiresConcentration(true)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 10)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, additionalDicePerIncrement: 1)
                    .SetEffectForms(
                        EffectFormBuilder.ConditionForm(conditionRingOfBlades),
                        EffectFormBuilder.ConditionForm(conditionRingOfBladesFree))
                    .SetParticleEffectParameters(HypnoticPattern)
                    .SetEffectEffectParameters(PowerMagebaneSpellCrusher)
                    .Build())
            .AddCustomSubFeatures(new MagicEffectFinishedByMeSpellRingOfBlades(conditionRingOfBlades))
            .AddToDB();

        return spell;
    }

    private sealed class MagicEffectFinishedByMeRingOfBladesFree(
        FeatureDefinitionPower powerRingOfBlades,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ConditionDefinition conditionRingOfBladesFree) : IMagicEffectFinishedByMe
    {
        public IEnumerator OnMagicEffectFinishedByMe(CharacterActionMagicEffect action, BaseDefinition baseDefinition)
        {
            var rulesetCharacter = action.ActingCharacter.RulesetCharacter;
            var usablePower = PowerProvider.Get(powerRingOfBlades, rulesetCharacter);

            rulesetCharacter.UsePower(usablePower);

            if (rulesetCharacter.TryGetConditionOfCategoryAndType(
                    AttributeDefinitions.TagEffect, conditionRingOfBladesFree.Name, out var activeCondition))
            {
                rulesetCharacter.RemoveCondition(activeCondition);
            }

            yield break;
        }
    }

    private sealed class CustomBehaviorPowerRingOfBlades(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        FeatureDefinitionPower powerRingOfBlades,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ConditionDefinition conditionRingOfBlades)
        : IMagicEffectInitiatedByMe, IModifyEffectDescription
    {
        // STEP 1: change attackRollModifier to use spell casting feature
        public IEnumerator OnMagicEffectInitiatedByMe(CharacterActionMagicEffect action, BaseDefinition baseDefinition)
        {
            var rulesetAttacker = action.ActingCharacter.RulesetCharacter;

            if (action.ActionParams.actionModifiers.Count == 0)
            {
                yield break;
            }

            if (!rulesetAttacker.TryGetConditionOfCategoryAndType(
                    AttributeDefinitions.TagEffect,
                    conditionRingOfBlades.Name,
                    out var activeCondition))
            {
                yield break;
            }

            var rulesetCaster = EffectHelpers.GetCharacterByGuid(activeCondition.sourceGuid);

            if (rulesetCaster == null)
            {
                yield break;
            }

            var spellRepertoireIndex = activeCondition.Amount;

            if (activeCondition.Amount < 0 || rulesetCaster.SpellRepertoires.Count <= spellRepertoireIndex)
            {
                yield break;
            }

            var actionModifier = action.ActionParams.actionModifiers[0];

            rulesetCaster.EnumerateFeaturesToBrowse<ISpellCastingAffinityProvider>(
                rulesetCaster.FeaturesToBrowse, rulesetCaster.FeaturesOrigin);
            rulesetCaster.ComputeSpellAttackBonus(rulesetCaster.SpellRepertoires[spellRepertoireIndex]);
            actionModifier.AttacktoHitTrends.SetRange(rulesetCaster.magicAttackTrends);
            actionModifier.AttackRollModifier = rulesetCaster.magicAttackTrends.Sum(x => x.value);
        }

        // STEP 2: add additional dice if required
        public bool IsValid(BaseDefinition definition, RulesetCharacter character, EffectDescription effectDescription)
        {
            return definition == powerRingOfBlades;
        }

        public EffectDescription GetEffectDescription(
            BaseDefinition definition,
            EffectDescription effectDescription,
            RulesetCharacter character,
            RulesetEffect rulesetEffect)
        {
            var damageForm = effectDescription.FindFirstDamageForm();

            if (damageForm == null)
            {
                return effectDescription;
            }

            if (!character.TryGetConditionOfCategoryAndType(
                    AttributeDefinitions.TagEffect,
                    conditionRingOfBlades.Name,
                    out var activeCondition))
            {
                return effectDescription;
            }

            damageForm.diceNumber = 4 + activeCondition.EffectLevel - 6;

            return effectDescription;
        }
    }

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    private sealed class MagicEffectFinishedByMeSpellRingOfBlades(ConditionDefinition conditionRingOfBlades)
        : IMagicEffectFinishedByMe
    {
        public IEnumerator OnMagicEffectFinishedByMe(CharacterActionMagicEffect action, BaseDefinition baseDefinition)
        {
            if (action is not CharacterActionCastSpell actionCastSpell)
            {
                yield break;
            }

            var rulesetCaster = action.ActingCharacter.RulesetCharacter;

            foreach (var rulesetTarget in action.ActionParams.TargetCharacters
                         .Select(targetCharacter => targetCharacter.RulesetCharacter))
            {
                if (rulesetTarget.TryGetConditionOfCategoryAndType(
                        AttributeDefinitions.TagEffect,
                        conditionRingOfBlades.Name,
                        out var activeCondition))
                {
                    activeCondition.Amount =
                        rulesetCaster.SpellRepertoires.IndexOf(actionCastSpell.activeSpell.SpellRepertoire);
                }
            }
        }
    }

    #endregion

    #region Flash Freeze

    internal static SpellDefinition BuildFlashFreeze()
    {
        const string NAME = "FlashFreeze";

        var conditionFlashFreeze = ConditionDefinitionBuilder
            .Create(ConditionGrappledRestrainedRemorhaz, $"Condition{NAME}")
            .SetGuiPresentation(
                RuleDefinitions.ConditionRestrained, Category.Rules, ConditionDefinitions.ConditionChilled)
            .SetPossessive()
            .SetParentCondition(ConditionRestrainedByWeb)
            .AddToDB();

        conditionFlashFreeze.specialDuration = false;
        conditionFlashFreeze.specialInterruptions.Clear();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.FLashFreeze, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetSpellLevel(6)
            .SetCastingTime(ActivationTime.Action)
            .SetMaterialComponent(MaterialComponentType.None)
            .SetVerboseComponent(true)
            .SetSomaticComponent(true)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetDurationData(DurationType.Minute, 1)
                .SetTargetingData(Side.Enemy, RangeType.Distance, 12, TargetType.IndividualsUnique)
                .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, additionalDicePerIncrement: 2)
                .SetSavingThrowData(false, AttributeDefinitions.Dexterity, false,
                    EffectDifficultyClassComputation.SpellCastingFeature)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .HasSavingThrow(EffectSavingThrowType.HalfDamage)
                        .SetDamageForm(DamageTypeCold, 10, DieType.D6)
                        .Build(),
                    EffectFormBuilder
                        .Create()
                        .HasSavingThrow(EffectSavingThrowType.Negates)
                        .SetConditionForm(conditionFlashFreeze, ConditionForm.ConditionOperation.Add)
                        .Build())
                .SetParticleEffectParameters(PowerDomainElementalHeraldOfTheElementsCold)
                .SetCasterEffectParameters(SleetStorm)
                .Build())
            .AddToDB();

        spell.AddCustomSubFeatures(new FilterTargetingCharacterFlashFreeze(spell));

        spell.EffectDescription.EffectParticleParameters.conditionStartParticleReference =
            ConditionDefinitions.ConditionRestrained.conditionStartParticleReference;
        spell.EffectDescription.EffectParticleParameters.conditionParticleReference =
            ConditionDefinitions.ConditionRestrained.conditionParticleReference;
        spell.EffectDescription.EffectParticleParameters.conditionEndParticleReference =
            ConditionDefinitions.ConditionRestrained.conditionEndParticleReference;

        return spell;
    }

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    private sealed class FilterTargetingCharacterFlashFreeze(SpellDefinition spellFlashFreeze)
        : IFilterTargetingCharacter
    {
        public bool EnforceFullSelection => false;

        public bool IsValid(CursorLocationSelectTarget __instance, GameLocationCharacter target)
        {
            if (__instance.actionParams.RulesetEffect is not RulesetEffectSpell rulesetEffectSpell
                || rulesetEffectSpell.SpellDefinition != spellFlashFreeze)
            {
                return true;
            }

            var rulesetTarget = target.RulesetCharacter;

            var isValid = rulesetTarget.SizeDefinition != CharacterSizeDefinitions.DragonSize
                          && rulesetTarget.SizeDefinition != CharacterSizeDefinitions.Gargantuan
                          && rulesetTarget.SizeDefinition != CharacterSizeDefinitions.Huge
                          && rulesetTarget.SizeDefinition != CharacterSizeDefinitions.SpiderQueenSize;

            if (!isValid)
            {
                __instance.actionModifier.FailureFlags.Add("Tooltip/&MustBeLargeOrSmaller");
            }

            return isValid;
        }
    }

    #endregion
}
