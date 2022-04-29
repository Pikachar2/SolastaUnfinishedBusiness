﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaCommunityExpansion.Models;

namespace SolastaCommunityExpansion.Patches.CustomFeatures.RecursiveGrantCustomFeatures
{
    [HarmonyPatch(typeof(CharacterBuildingManager), "GrantFeatures")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class CharacterBuildingManager_GrantFeatures
    {
        /**
         * When a character is being granted features, this patch will apply the effect of custom features.
         */

        internal static void Postfix(RulesetCharacterHero hero, List<FeatureDefinition> grantedFeatures, bool clearPrevious = true)
        {
            if (!clearPrevious)
            {
                CustomFeaturesContext.RecursiveGrantCustomFeatures(hero, grantedFeatures);
            }
            else
            {
                CustomFeaturesContext.RecursiveRemoveCustomFeatures(hero, grantedFeatures);              
            }
        }
    }
}
