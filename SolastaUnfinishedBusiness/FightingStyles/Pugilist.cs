﻿using System.Collections.Generic;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Validators;
using static ActionDefinitions;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionFightingStyleChoices;

namespace SolastaUnfinishedBusiness.FightingStyles;

internal sealed class Pugilist : AbstractFightingStyle
{
    private const string PugilistName = "Pugilist";

    internal override FightingStyleDefinition FightingStyle { get; } = FightingStyleBuilder
        .Create(PugilistName)
        .SetGuiPresentation(Category.FightingStyle, Sprites.GetSprite("Pugilist", Resources.Pugilist, 256))
        .SetFeatures(
            FeatureDefinitionActionAffinityBuilder
                .Create("ActionAffinityFightingStylePugilist")
                .SetGuiPresentation(PugilistName, Category.FightingStyle)
                .SetAuthorizedActions(Id.ShoveBonus)
                .AddCustomSubFeatures(
                    new AddExtraUnarmedAttack(ActionType.Bonus),
                    new AdditionalUnarmedDice(),
                    new ValidateDefinitionApplication(ValidatorsCharacter.HasFreeHand))
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

    private sealed class AdditionalUnarmedDice : IModifyWeaponAttackMode
    {
        public void ModifyAttackMode(RulesetCharacter character, RulesetAttackMode attackMode)
        {
            if (!ValidatorsWeapon.IsUnarmed(attackMode))
            {
                return;
            }

            var effectDescription = attackMode.EffectDescription;
            var damage = effectDescription.FindFirstDamageForm();
            var k = effectDescription.EffectForms.FindIndex(form => form.damageForm == damage);

            if (k < 0 || damage == null)
            {
                return;
            }

            var additionalDice = EffectFormBuilder
                .Create()
                .SetDamageForm(damage.damageType, 1, DieType.D4)
                .Build();

            effectDescription.EffectForms.Insert(k + 1, additionalDice);
        }
    }
}
