﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolastaModApi;
using SolastaModApi.Extensions;
using SolastaModApi.BuilderHelpers;
using SolastaCommunityExpansion.CustomFeatureDefinitions;

namespace SolastaCommunityExpansion.Feats
{
    class DualFlurryFeatBuilder : BaseDefinitionBuilder<FeatDefinition>
    {
        public static Guid DualFlurryGuid = new Guid("03C523EB-91B9-4F1B-A697-804D1BC2D6DD");
        const string DualFlurryFeatName = "DualFlurryFeat";
        private static readonly string DualFlurryFeatNameGuid = GuidHelper.Create(DualFlurryGuid, DualFlurryFeatName).ToString();

        protected DualFlurryFeatBuilder(string name, string guid) : base(DatabaseHelper.FeatDefinitions.Ambidextrous, name, guid)
        {
            Definition.GuiPresentation.Title = "Feat/&DualFlurryTitle";
            Definition.GuiPresentation.Description = "Feat/&DualFlurryDescription";

            Definition.Features.Clear();
            Definition.Features.Add(buildFeatureDualFlurry());

            Definition.SetMinimalAbilityScorePrerequisite(false);
        }

        public static FeatDefinition CreateAndAddToDB(string name, string guid)
            => new DualFlurryFeatBuilder(name, guid).AddToDB();

        public static FeatDefinition DualFlurryFeat = CreateAndAddToDB(DualFlurryFeatName, DualFlurryFeatNameGuid);

        public static void AddToFeatList()
        {
            var DualFlurryFeat = DualFlurryFeatBuilder.DualFlurryFeat;
        }

        private static FeatureDefinition buildFeatureDualFlurry()
        {
            FeatureDefinitionOnAttackHitEffectBuilder builder = new FeatureDefinitionOnAttackHitEffectBuilder(
                "FeatureDualFlurry", GuidHelper.Create(DualFlurryGuid, "FeatureDualFlurry").ToString(),
                OnAttackHit, new GuiPresentationBuilder("Feature/&DualFlurryDescription", "Feature/&DualFlurryTitle").Build());

            return builder.AddToDB();
        }

        private static void OnAttackHit(GameLocationCharacter attacker,
                GameLocationCharacter defender, ActionModifier attackModifier, RulesetAttackMode attackMode,
                bool rangedAttack, RuleDefinitions.AdvantageType advantageType, List<EffectForm> actualEffectForms,
                RulesetEffect rulesetEffect, bool criticalHit, bool firstTarget)
        {
            // Note the game code currently always passes attackMode = null for magic attacks,
            // if that changes this will need to be updated.
            if (rangedAttack || attackMode == null)
            {
                return;
            }

            var condition = attacker.RulesetCharacter.HasConditionOfType(ConditionDualFlurryApplyBuilder.GetOrAdd().Name) ?
                ConditionDualFlurryGrantBuilder.GetOrAdd() : ConditionDualFlurryApplyBuilder.GetOrAdd();

            RulesetCondition active_condition = RulesetCondition.CreateActiveCondition(attacker.RulesetCharacter.Guid,
                                                                                       condition, RuleDefinitions.DurationType.Turn, 1,
                                                                                       RuleDefinitions.TurnOccurenceType.EndOfTurn,
                                                                                       attacker.RulesetCharacter.Guid,
                                                                                       attacker.RulesetCharacter.CurrentFaction.Name);
            attacker.RulesetCharacter.AddConditionOfCategory("10Combat", active_condition, true);
        }
    }

    internal class ConditionDualFlurryApplyBuilder : BaseDefinitionBuilder<ConditionDefinition>
    {
        protected ConditionDualFlurryApplyBuilder(string name, string guid) : base(DatabaseHelper.ConditionDefinitions.ConditionSurged, name, guid)
        {
            Definition.GuiPresentation.Title = "Condition/&ConditionDualFlurryApplyTitle";
            Definition.GuiPresentation.Description = "Condition/&ConditionDualFlurryApplyDescription";

            Definition.SetAllowMultipleInstances(false);
            Definition.SetDurationParameter(1);
            Definition.SetDurationType(RuleDefinitions.DurationType.Turn);
            Definition.SetTurnOccurence(RuleDefinitions.TurnOccurenceType.EndOfTurn);
            Definition.SetPossessive(true);
            Definition.SetSilentWhenAdded(true);
            Definition.SetSilentWhenRemoved(true);
            Definition.SetConditionType(RuleDefinitions.ConditionType.Beneficial);
            Definition.Features.Clear();
        }

        public static ConditionDefinition CreateAndAddToDB()
            => new ConditionDualFlurryApplyBuilder("ConditionDualFlurryApply", GuidHelper.Create(DualFlurryFeatBuilder.DualFlurryGuid, "ConditionDualFlurryApply").ToString()).AddToDB();

        public static ConditionDefinition GetOrAdd()
        {
            var db = DatabaseRepository.GetDatabase<ConditionDefinition>();
            return db.TryGetElement("ConditionDualFlurryApply", GuidHelper.Create(DualFlurryFeatBuilder.DualFlurryGuid, "ConditionDualFlurryApply").ToString()) ?? CreateAndAddToDB();
        }
    }

    internal class ConditionDualFlurryGrantBuilder : BaseDefinitionBuilder<ConditionDefinition>
    {
        protected ConditionDualFlurryGrantBuilder(string name, string guid) : base(DatabaseHelper.ConditionDefinitions.ConditionSurged, name, guid)
        {
            Definition.GuiPresentation.Title = "Condition/&ConditionDualFlurryGrantTitle";
            Definition.GuiPresentation.Description = "Condition/&ConditionDualFlurryGrantDescription";
            Definition.GuiPresentation.SetHidden(true);

            Definition.SetAllowMultipleInstances(false);
            Definition.SetDurationParameter(1);
            Definition.SetDurationType(RuleDefinitions.DurationType.Turn);
            Definition.SetTurnOccurence(RuleDefinitions.TurnOccurenceType.EndOfTurn);
            Definition.SetPossessive(true);
            Definition.SetSilentWhenAdded(false);
            Definition.SetSilentWhenRemoved(false);
            Definition.SetConditionType(RuleDefinitions.ConditionType.Beneficial);
            Definition.Features.Clear();
            Definition.Features.Add(buildAdditionalActionDualFlurry());
        }

        public static ConditionDefinition CreateAndAddToDB()
            => new ConditionDualFlurryGrantBuilder("ConditionDualFlurryGrant", GuidHelper.Create(DualFlurryFeatBuilder.DualFlurryGuid, "ConditionDualFlurryGrant").ToString()).AddToDB();

        public static ConditionDefinition GetOrAdd()
        {
            var db = DatabaseRepository.GetDatabase<ConditionDefinition>();
            return db.TryGetElement("ConditionDualFlurryGrant", GuidHelper.Create(DualFlurryFeatBuilder.DualFlurryGuid, "ConditionDualFlurryGrant").ToString()) ?? CreateAndAddToDB();
        }

        private static FeatureDefinition buildAdditionalActionDualFlurry()
        {
            GuiPresentationBuilder guiBuilder = new GuiPresentationBuilder("Feature/&AdditionalActionDualFlurryDescription",
                "Feature/&AdditionalActionDualFlurryTitle")
                .SetSpriteReference(DatabaseHelper.FeatureDefinitionAdditionalActions.AdditionalActionSurgedMain.GuiPresentation.SpriteReference);
            FeatureDefinitionAdditionalActionBuilder flurryBuilder = new FeatureDefinitionAdditionalActionBuilder(
                DatabaseHelper.FeatureDefinitionAdditionalActions.AdditionalActionSurgedMain, "AdditionalActionDualFlurry",
                GuidHelper.Create(DualFlurryFeatBuilder.DualFlurryGuid, "AdditionalActionDualFlurry").ToString())
                .SetGuiPresentation(guiBuilder.Build())
                .SetActionType(ActionDefinitions.ActionType.Bonus)
                .SetAuthorizedActions(new List<ActionDefinitions.Id>())
                .SetForbiddenActions(new List<ActionDefinitions.Id>())
                .SetRestrictedActions(new List<ActionDefinitions.Id>() { ActionDefinitions.Id.AttackOff });
            return flurryBuilder.AddToDB();
        }
    }
}