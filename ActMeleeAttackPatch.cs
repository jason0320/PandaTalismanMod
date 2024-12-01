using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;

namespace Mod_PandaTalismanMod
{
    [HarmonyPatch]
    internal class ActMeleePerformPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ActMelee), "<Attack>g__Attack|13_1")]
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
