﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Validators;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionFightingStyleChoices;

namespace SolastaUnfinishedBusiness.FightingStyles;

internal sealed class Interception : AbstractFightingStyle
{
    private const string InterceptionName = "Interception";

    internal override FightingStyleDefinition FightingStyle { get; } = FightingStyleBuilder
        .Create(InterceptionName)
        .SetGuiPresentation(Category.FightingStyle, DatabaseHelper.FightingStyleDefinitions.Defense)
        .SetFeatures(
            FeatureDefinitionPowerBuilder
                .Create("PowerInterception")
                .SetGuiPresentation(InterceptionName, Category.FightingStyle)
                .SetUsesFixed(ActivationTime.Reaction)
                .SetReactionContext(ExtraReactionContext.Custom)
                .AddCustomSubFeatures(new AttackBeforeHitPossibleOnMeOrAllyInterception(
                    ConditionDefinitionBuilder
                        .Create("ConditionInterception")
                        .SetGuiPresentationNoContent(true)
                        .SetSilent(Silent.WhenAddedOrRemoved)
                        .SetSpecialDuration(DurationType.Round, 0, TurnOccurenceType.StartOfTurn)
                        .SetSpecialInterruptions(ExtraConditionInterruption.AfterWasAttacked)
                        .SetAmountOrigin(ConditionDefinition.OriginOfAmount.Fixed)
                        .AddFeatures(
                            FeatureDefinitionReduceDamageBuilder
                                .Create("ReduceDamageInterception")
                                .SetGuiPresentation(InterceptionName, Category.FightingStyle)
                                .SetAlwaysActiveReducedDamage(
                                    (_, defender) => defender.RulesetCharacter.AllConditions.FirstOrDefault(
                                        x => x.ConditionDefinition.Name == "ConditionInterception")!.Amount)
                                .AddToDB())
                        .AddToDB()))
                .AddToDB())
        .AddToDB();

    internal override List<FeatureDefinitionFightingStyleChoice> FightingStyleChoice =>
    [
        CharacterContext.FightingStyleChoiceBarbarian,
        FightingStyleChampionAdditional,
        FightingStyleFighter,
        FightingStylePaladin,
        FightingStyleRanger
    ];

    private sealed class AttackBeforeHitPossibleOnMeOrAllyInterception(ConditionDefinition conditionDefinition)
        : IAttackBeforeHitConfirmedOnMeOrAlly
    {
        public IEnumerator OnAttackBeforeHitConfirmedOnMeOrAlly(
            GameLocationBattleManager battleManager,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper,
            ActionModifier actionModifier,
            RulesetAttackMode attackMode,
            bool rangedAttack,
            AdvantageType advantageType,
            List<EffectForm> actualEffectForms,
            RulesetEffect rulesetEffect,
            bool firstTarget,
            bool criticalHit)
        {
            if (helper == defender)
            {
                yield break;
            }

            if (!helper.IsWithinRange(defender, 1))
            {
                yield break;
            }

            if (!helper.CanReact())
            {
                yield break;
            }

            var unitCharacter = helper.RulesetCharacter;

            if (ValidatorsWeapon.IsUnarmed(unitCharacter.GetMainWeapon()?.ItemDefinition, null)
                && ValidatorsWeapon.IsUnarmed(unitCharacter.GetOffhandWeapon()?.ItemDefinition, null))
            {
                yield break;
            }

            var manager = ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;

            if (manager == null)
            {
                yield break;
            }

            var reactionParams =
                new CharacterActionParams(helper, (ActionDefinitions.Id)ExtraActionId.DoNothingReaction)
                {
                    StringParameter = "CustomReactionInterceptionDescription"
                        .Formatted(Category.Reaction, defender.Name, attacker.Name)
                };
            var previousReactionCount = manager.PendingReactionRequestGroups.Count;
            var reactionRequest = new ReactionRequestCustom($"{InterceptionName}", reactionParams);

            manager.AddInterruptRequest(reactionRequest);

            yield return battleManager.WaitForReactions(helper, manager, previousReactionCount);

            if (!reactionParams.ReactionValidated)
            {
                yield break;
            }

            var roll = unitCharacter.RollDie(DieType.D10, RollContext.None, true, AdvantageType.None, out _, out _);
            var reducedDamage = roll + unitCharacter.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);

            defender.RulesetCharacter.InflictCondition(
                conditionDefinition.Name,
                conditionDefinition.DurationType,
                conditionDefinition.DurationParameter,
                conditionDefinition.TurnOccurence,
                AttributeDefinitions.TagEffect,
                unitCharacter.guid,
                unitCharacter.CurrentFaction.Name,
                1,
                conditionDefinition.Name,
                reducedDamage,
                0,
                0);
        }
    }
}
