﻿using System.Collections.Generic;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Validators;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionFightingStyleChoices;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.FightingStyles;

internal sealed class Torchbearer : AbstractFightingStyle
{
    private static readonly FeatureDefinitionPower PowerFightingStyleTorchbearer = FeatureDefinitionPowerBuilder
        .Create("PowerFightingStyleTorchbearer")
        .SetGuiPresentation(Category.Feature,
            Sprites.GetSprite("PowerTorchBearer", Resources.PowerTorchBearer, 256, 128))
        .SetUsesFixed(ActivationTime.BonusAction)
        .SetEffectDescription(
            EffectDescriptionBuilder
                .Create(SpellDefinitions.Fireball.EffectDescription)
                .SetCanBePlacedOnCharacter(false)
                .SetDurationData(DurationType.Round, 3)
                .SetSpeed(SpeedType.Instant, 11f)
                .SetTargetingData(Side.Enemy, RangeType.Touch, 0, TargetType.IndividualsUnique)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(ConditionDefinitions.ConditionOnFire1D4, ConditionForm.ConditionOperation.Add)
                        .HasSavingThrow(EffectSavingThrowType.Negates, TurnOccurenceType.StartOfTurn)
                        .Build())
                .SetSavingThrowData(
                    false,
                    AttributeDefinitions.Dexterity,
                    false,
                    EffectDifficultyClassComputation.FixedValue,
                    AttributeDefinitions.Dexterity,
                    8)
                .Build())
        .SetShowCasting(false)
        .AddCustomSubFeatures(new ValidatorsValidatePowerUse(ValidatorsCharacter.HasLightSourceOffHand))
        .AddToDB();

    internal override FightingStyleDefinition FightingStyle { get; } = FightingStyleBuilder
        .Create("Torchbearer")
        .SetGuiPresentation(Category.FightingStyle, Sprites.GetSprite("Torchbearer", Resources.Torchbearer, 256))
        .SetFeatures(
            FeatureDefinitionBuilder
                .Create("AddExtraAttackTorchbearer")
                .SetGuiPresentationNoContent(true)
                .AddCustomSubFeatures(new AddBonusTorchAttack(PowerFightingStyleTorchbearer))
                .AddToDB())
        .AddToDB();

    internal override List<FeatureDefinitionFightingStyleChoice> FightingStyleChoice =>
    [
        CharacterContext.FightingStyleChoiceBarbarian,
        CharacterContext.FightingStyleChoiceMonk,
        CharacterContext.FightingStyleChoiceRogue,
        FightingStyleChampionAdditional,
        FightingStyleFighter,
        FightingStyleRanger
    ];
}
