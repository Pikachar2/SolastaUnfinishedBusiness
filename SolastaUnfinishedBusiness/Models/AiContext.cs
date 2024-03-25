﻿using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Builders;
using TA.AI;
using TA.AI.Considerations;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Models;

internal static class AiContext
{
    internal const string DoNothing = "1";
    internal const string DoStrengthCheckCasterDC = "2";

    internal static readonly List<string> DoNothingConditions =
        ["ConditionNoxiousSpray", "ConditionVileBrew", "ConditionGrappledRestrainedIceBound"];

    internal static readonly List<string> DoStrengthCheckCasterDCConditions =
    [
        "ConditionFlashFreeze", "ConditionGrappledRestrainedEnsnared",
        "ConditionGrappledRestrainedSpellWeb", "ConditionRestrainedByEntangle"
    ];

    internal static void Load()
    {
        // order matters as same weight
        // this code needs a refactoring. meanwhile check:
        // - CharacterActionPanelPatcher SelectBreakFreeMode and add condition there if spell also aims allies
        foreach (var condition in DoNothingConditions)
        {
            BuildDecisionBreakFreeFromCondition(condition, DoNothing);
        }

        foreach (var condition in DoStrengthCheckCasterDCConditions)
        {
            BuildDecisionBreakFreeFromCondition(condition, DoStrengthCheckCasterDC);
        }
    }

    // boolParameter false won't do any ability check
    private static void BuildDecisionBreakFreeFromCondition(string conditionName, string action)
    {
        //TODO: create proper builders

        // create considerations copies

        var baseDecision = DatabaseHelper.GetDefinition<DecisionDefinition>("BreakConcentration_FlyingInMelee");
        var considerationHasCondition = baseDecision.Decision.Scorer.considerations.FirstOrDefault(x =>
            x.consideration.name == "HasConditionFlying");
        var considerationMainActionNotFullyConsumed = baseDecision.Decision.Scorer.considerations.FirstOrDefault(x =>
            x.consideration.name == "MainActionNotFullyConsumed");

        if (considerationHasCondition == null || considerationMainActionNotFullyConsumed == null)
        {
            Main.Error("fetching considerations at BuildDecisionBreakFreeFromCondition");

            return;
        }

        var considerationHasConditionBreakFree = new WeightedConsiderationDescription
        {
            consideration = Object.Instantiate(considerationHasCondition.consideration),
            weight = considerationHasCondition.weight
        };

        considerationHasConditionBreakFree.consideration.name = $"Has{conditionName}";
        considerationHasConditionBreakFree.consideration.consideration = new ConsiderationDescription
        {
            considerationType = nameof(HasCondition),
            curve = considerationHasCondition.consideration.consideration.curve,
            boolParameter = considerationHasCondition.consideration.consideration.boolParameter,
            intParameter = considerationHasCondition.consideration.consideration.intParameter,
            floatParameter = considerationHasCondition.consideration.consideration.floatParameter,
            stringParameter = conditionName
        };

        // create scorer copy

        var scorer = Object.Instantiate(baseDecision.Decision.scorer);

        scorer.name = "BreakFree";
        scorer.scorer.considerations = [considerationHasConditionBreakFree, considerationMainActionNotFullyConsumed];

        // create and assign decision definition to all decision packages

        var decisionBreakFree = DecisionDefinitionBuilder
            .Create($"DecisionBreakFree{conditionName}")
            .SetGuiPresentationNoContent(true)
            .SetDecisionDescription(
                "if restrained and can use main action, try to break free",
                "BreakFree",
                scorer,
                action,
                enumParameter: 1,
                floatParameter: 3f)
            .AddToDB();

        foreach (var decisionPackageDefinition in DatabaseRepository.GetDatabase<DecisionPackageDefinition>())
        {
            decisionPackageDefinition.package.weightedDecisions.Add(
                new WeightedDecisionDescription(decisionBreakFree, 1, 0, false));
        }
    }
}
