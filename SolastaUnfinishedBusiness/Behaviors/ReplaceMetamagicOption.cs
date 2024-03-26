using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;

namespace SolastaUnfinishedBusiness.Behaviors;

public class ReplaceMetamagicOption
{
    private readonly List<MetamagicOptionDefinition> _options = [];

    public ReplaceMetamagicOption(params MetamagicOptionDefinition[] options)
    {
        _options.AddRange(options);
    }

    private static List<MetamagicOptionDefinition> GetOptions(RulesetCharacterHero hero)
    {
        List<MetamagicOptionDefinition> list = null;
        var options = hero.TrainedMetamagicOptions;
        Console.WriteLine("MetamagicGetOptions - Hero features");
        Console.WriteLine("MetamagicGetOptions - Hero features cache");
        foreach (var item in hero.featuresCache)
        {
            Console.WriteLine(item);
        }
        Console.WriteLine("MetamagicGetOptions - Hero Active features");
        foreach (var item in hero.activeFeatures)
        {
            Console.WriteLine(item);
        }
        //        Console.WriteLine(hero.featuresCache.ForEach(string);
//        characterBuildingManager.GrantFeatures(hero, grantedFeatures, $"02Race{characterLevel}", false);

        foreach (var option in options)
        {
            var replacer = option.GetFirstSubFeatureOfType<ReplaceMetamagicOption>();

            if (replacer == null)
            {
                continue;
            }

            list ??= [..options];
            list.Remove(option);
            list.AddRange(replacer._options);
        }

        return list ?? options;
    }

    public static IEnumerable<CodeInstruction> PatchMetamagicGetter(IEnumerable<CodeInstruction> instructions,
        string context)
    {
        var getter = typeof(RulesetCharacterHero)
            .GetProperty(nameof(RulesetCharacterHero.TrainedMetamagicOptions))
            ?.GetGetMethod();

        var hidden = typeof(GuiPresentation)
            .GetProperty(nameof(GuiPresentation.Hidden))
            ?.GetGetMethod();

        var customHidden = new Func<GuiPresentation, bool>(Hidden).Method;
        var customGetter = new Func<RulesetCharacterHero, List<MetamagicOptionDefinition>>(GetOptions).Method;

        return instructions
            //Replace getter with custom one that changes metamagic options
            .ReplaceCalls(getter, context + ".Getter", new CodeInstruction(OpCodes.Call, customGetter))
            //Ensure hidden metamagic are processed
            .ReplaceCalls(hidden, context + ".Hidden", new CodeInstruction(OpCodes.Call, customHidden));
    }

    private static bool Hidden(GuiPresentation gui)
    {
        return false;
    }
}
