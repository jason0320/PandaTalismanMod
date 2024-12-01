using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Mod_PandaTalismanMod
{
    [HarmonyPatch]
    public class ProcessRecipe
    {
        public static void OnStartCore()
        {
            SourceManager sources = Core.Instance.sources;
            List<SourceRecipe.Row> rows = sources.recipes.rows;
            SourceRecipe.Row baseRecipe = rows.Find((SourceRecipe.Row x) => x.factory == "Talisman" && x.id < 100);
            List<SourceRecipe.Row> list = new List<SourceRecipe.Row>
        {
            AddRecipe(baseRecipe, 123456001, "#ranged", "#spellbook")
        };
            foreach (SourceRecipe.Row item in list)
            {
                sources.recipes.rows.Add(item);
            }
        }

        public static T CreateCopy<T>(T baseItem) where T : new()
        {
            FieldInfo[] fields = baseItem.GetType().GetFields();
            T val = new T();
            FieldInfo[] array = fields;
            foreach (FieldInfo fieldInfo in array)
            {
                val.SetField(fieldInfo.Name, baseItem.GetField<object>(fieldInfo.Name));
            }
            return val;
        }

        public static SourceRecipe.Row AddRecipe(SourceRecipe.Row baseRecipe, int newId, string ing1, string ing2)
        {
            SourceRecipe.Row row = CreateCopy(baseRecipe);
            row.id = newId;
            row.thing = "";
            row.ing1 = new string[1] { ing1 };
            row.ing2 = new string[1] { ing2 };
            row.ing3 = new string[0];
            row.tag = new string[1] { "known" };
            return row;
        }
    }

}
