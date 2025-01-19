using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;

namespace Mod_PandaTalismanMod
{
    [HarmonyPatch]
    internal class ActMeleePerformPatch
    {
        internal static MethodInfo TargetMethod()
        {
            return typeof(ActMelee).GetNestedTypes(AccessTools.all)
                .Append(typeof(ActMelee))
                .SelectMany(t => t.GetMethods(AccessTools.all))
                .First(mi => mi.GetParameters().Length == 4 && mi.Name.Contains("<Attack>g__Attack|"));
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> OnAttackIl(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(o => o.opcode == OpCodes.Stfld &&
                                       o.operand.ToString().Contains("usedTalisman")))
                .SetInstruction(
                    new CodeInstruction(OpCodes.Ldc_I4_0))
                .InstructionEnumeration();
        }
    }
}
