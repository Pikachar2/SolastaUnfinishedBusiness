﻿using SolastaCommunityExpansion.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolastaCommunityExpansion.Models
{
    internal static class ClassesContext
    {
        public static Dictionary<string, AbstractClass> Classes { get; private set; } = new Dictionary<string, AbstractClass>();

        internal static void Load()
        {
            LoadClass(new Witch());
        }

        private static void LoadClass(AbstractClass classBuilder)
        {
            CharacterClassDefinition customClass = classBuilder.GetClass();

            if (!Classes.ContainsKey(customClass.Name))
            {
                Classes.Add(customClass.Name, classBuilder);
            }

            Classes = Classes.OrderBy(x => Gui.Format(x.Value.GetClass().GuiPresentation.Title)).ToDictionary(x => x.Key, x => x.Value);
        }

        public static string GenerateClassDescription()
        {
            StringBuilder outString = new StringBuilder("[heading]Classes[/heading]");

            outString.Append("\n[list]");

            foreach (AbstractClass customClass in Classes.Values)
            {
                outString.Append("\n[*][b]");
                outString.Append(Gui.Format(customClass.GetClass().GuiPresentation.Title));
                outString.Append("[/b]: ");
                outString.Append(Gui.Format(customClass.GetClass().GuiPresentation.Description));
            }

            outString.Append("\n[/list]");

            return outString.ToString();
        }
    }

}
