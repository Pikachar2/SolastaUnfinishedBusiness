﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Feats;
using UnityEngine;
using UnityEngine.UI;

namespace SolastaUnfinishedBusiness.Models;

internal static class FeatsContext
{
    private const int Columns = 3;
    internal const int Width = 300;
    internal const int Height = 44;
    internal const int Spacing = 5;

    internal static HashSet<FeatDefinition> Feats { get; private set; } = [];
    internal static HashSet<FeatDefinition> FeatGroups { get; private set; } = [];

    internal static void LateLoad()
    {
        var feats = new List<FeatDefinition>();

        // generate feats here and fill the list
        ArmorFeats.CreateFeats(feats);
        CasterFeats.CreateFeats(feats);
        OtherFeats.CreateFeats(feats); // must come before Class Feats
        ClassFeats.CreateFeats(feats);
        CraftyFeats.CreateFeats(feats);
        CriticalVirtuosoFeats.CreateFeats(feats);
        DefenseExpertFeats.CreateFeats(feats);
        MeleeCombatFeats.CreateFeats(feats);
        PrecisionFocusedFeats.CreateFeats(feats);
        RaceFeats.CreateFeats(feats);
        RangedCombatFeats.CreateFeats(feats);
        TwoWeaponCombatFeats.CreateFeats(feats);

        // load them in mod UI
        feats.ForEach(LoadFeat);
        GroupFeats.Load(LoadFeatGroup);

        // tweak the groups to make display simpler on mod UI
        Feats.RemoveWhere(x => x.FormatTitle().Contains("["));

        foreach (var featGroup in FeatGroups
                     .Where(featGroup => !string.IsNullOrEmpty(featGroup.FamilyTag))
                     .ToList())
        {
            FeatGroups.Remove(featGroup);
            LoadFeat(featGroup);
        }

        // sorting
        Feats = Feats.OrderBy(x => x.FormatTitle()).ToHashSet();
        FeatGroups = FeatGroups.OrderBy(x => x.FormatTitle()).ToHashSet();

        // settings paring feats
        foreach (var name in Main.Settings.FeatEnabled
                     .Where(name => Feats.All(x => x.Name != name))
                     .ToList())
        {
            Main.Settings.FeatEnabled.Remove(name);
        }

        // settings paring groups
        foreach (var name in Main.Settings.FeatGroupEnabled
                     .Where(name => FeatGroups.All(x => x.Name != name))
                     .ToList())
        {
            Main.Settings.FeatGroupEnabled.Remove(name);
        }

        // avoids restart on level up UI
        GuiWrapperContext.RecacheFeats();
    }

    private static void LoadFeat([NotNull] FeatDefinition featDefinition)
    {
        Feats.Add(featDefinition);
        UpdateFeatsVisibility(featDefinition);
    }

    private static void LoadFeatGroup([NotNull] FeatDefinition featDefinition)
    {
        FeatGroups.Add(featDefinition);
        UpdateFeatGroupsVisibility(featDefinition);
    }

    private static void UpdateFeatsVisibility([NotNull] BaseDefinition featDefinition)
    {
        var hidden = !Main.Settings.FeatEnabled.Contains(featDefinition.Name);

        featDefinition.GuiPresentation.hidden = hidden;

        var groupedFeat = featDefinition.GetFirstSubFeatureOfType<GroupedFeat>();

        groupedFeat?.GetSubFeats(true, true).ForEach(x => x.GuiPresentation.hidden = hidden);
    }

    private static void UpdateFeatGroupsVisibility([NotNull] BaseDefinition featDefinition)
    {
        featDefinition.GuiPresentation.hidden = !Main.Settings.FeatGroupEnabled.Contains(featDefinition.Name);
    }

    internal static void SwitchFeat(FeatDefinition featDefinition, bool active)
    {
        if (!Feats.Contains(featDefinition))
        {
            return;
        }

        var name = featDefinition.Name;

        if (active)
        {
            Main.Settings.FeatEnabled.TryAdd(name);
        }
        else
        {
            Main.Settings.FeatEnabled.Remove(name);
        }

        UpdateFeatsVisibility(featDefinition);
        GuiWrapperContext.RecacheFeats();
    }

    internal static void SwitchFeatGroup(FeatDefinition featDefinition, bool active)
    {
        if (!FeatGroups.Contains(featDefinition))
        {
            return;
        }

        var name = featDefinition.Name;

        if (active)
        {
            Main.Settings.FeatGroupEnabled.TryAdd(name);
        }
        else
        {
            Main.Settings.FeatGroupEnabled.Remove(name);
        }

        UpdateFeatGroupsVisibility(featDefinition);
        GuiWrapperContext.RecacheFeats();
    }

    internal static void UpdatePanelChildren(FeatSubPanel panel)
    {
        // get missing children from pool
        while (panel.table.childCount < panel.relevantFeats.Count)
        {
            Gui.GetPrefabFromPool(panel.itemPrefab, panel.table);
        }

        // release extra children to pool
        while (panel.table.childCount > panel.relevantFeats.Count)
        {
            Gui.ReleaseInstanceToPool(panel.table.GetChild(panel.table.childCount - 1).gameObject);
        }
    }

    // called before sorting feats to hide sub-feats during level up
    private static void ProcessFeatGroups(FeatSubPanel panel, bool active, Transform table)
    {
        //this is not feat learning - skip manipulations
        if (!active)
        {
            return;
        }

        var toRemove = new List<FeatDefinition>();

        foreach (var group in panel.relevantFeats
                     .Select(feat => feat.GetFirstSubFeatureOfType<IGroupedFeat>())
                     .Where(group => group is { HideSubFeats: true }))
        {
            toRemove.AddRange(group.GetSubFeats());
        }

        for (var i = 0; i < table.childCount; i++)
        {
            var child = table.GetChild(i);
            var featItem = child.GetComponent<FeatItem>();

            if (toRemove.Contains(featItem.GuiFeatDefinition.FeatDefinition))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    internal static void SortFeats(FeatSubPanel panel)
    {
        panel.relevantFeats.Sort(CompareFeats);
    }

    internal static int CompareFeats(FeatDefinition a, FeatDefinition b)
    {
        return string.Compare(a.FormatTitle(), b.FormatTitle(),
            StringComparison.CurrentCultureIgnoreCase);
    }

    internal static void UpdateRelevantFeatList(FeatSubPanel panel)
    {
        var dbFeatDefinition = DatabaseRepository.GetDatabase<FeatDefinition>();
        var visibleFeats = dbFeatDefinition
            .Where(x => !x.GuiPresentation.Hidden)
            .ToList();

        panel.relevantFeats.SetRange(visibleFeats
            .Where(f => f.GetFirstSubFeatureOfType<IGroupedFeat>() is not { } group
                        || group.GetSubFeats().Count(s => visibleFeats.Contains(s)) > 1)
        );
    }

    internal static void ForceSameWidth(RectTransform table, bool active, FeatSubPanel panel)
    {
        ProcessFeatGroups(panel, active, table);

        if (active && Main.Settings.EnableSameWidthFeatSelection)
        {
            var hero = Global.LevelUpHero;
            var buildingData = hero?.GetHeroBuildingData();

            if (buildingData == null)
            {
                return;
            }

            var trainedFeats = buildingData.LevelupTrainedFeats.SelectMany(x => x.Value).ToList();

            trainedFeats.AddRange(hero.TrainedFeats);

            var j = 0;
            RectTransform rect;

            for (var i = 0; i < table.childCount; i++)
            {
                var child = table.GetChild(i);
                var featItem = child.GetComponent<FeatItem>();

                if (!child.gameObject.activeSelf || trainedFeats.Contains(featItem.GuiFeatDefinition.FeatDefinition))
                {
                    continue;
                }

                var x = j % Columns;
                var y = j / Columns;
                var posX = x * (Width + (Spacing * 2));
                var posY = -y * (Height + Spacing);

                rect = child.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(posX, posY);
                rect.sizeDelta = new Vector2(Width, Height);

                j++;
            }

            rect = table.GetComponent<RectTransform>();
            // ReSharper disable once PossibleLossOfFraction
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ((j / Columns) + 1) * (Height + Spacing));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(table);
    }
}
