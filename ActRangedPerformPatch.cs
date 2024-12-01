﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;

namespace Mod_PandaTalismanMod
{
    [HarmonyPatch]
    internal class ActRangedPerformPatch
    {
        internal static MethodInfo TargetMethod()
        {
            var closures = AccessTools.FirstInner(typeof(ActRanged), t => t.Name.Contains("DisplayClass10_0"));
            return AccessTools.Method(closures, "<Perform>g__Shoot|1");
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> OnShootIl(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchEndForward(
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(
                        typeof(AttackProcess),
                        nameof(AttackProcess.Perform))),
                    new CodeMatch(OpCodes.Brfalse))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Dup),
                    Transpilers.EmitDelegate<Action<bool>>(ProcTalisman))
                .InstructionEnumeration();
        }

        internal static void ProcTalisman(bool flagHit)
        {
            var weapon = Act.CC.ranged;
            if (weapon.c_ammo <= 0 || Act.CC.HasCondition<ConReload>())
            {
                return;
            }
            if (!(weapon.ammoData?.trait is TraitAmmo traitAmmo && weapon.ammoData?.trait is TraitAmmoTalisman traitAmmoTalisman ))
            {
                return;
            }
            if (!flagHit || Act.TC == null || !Act.TC.IsAliveInCurrentZone)
            {
                return;
            }
            var act = Act.CC.elements.GetElement(traitAmmoTalisman.owner.refVal)?.act ?? ACT.Create(traitAmmoTalisman.owner.refVal);
            Act.powerMod = traitAmmo.owner.encLV;
            var tC = Act.TC;
            if (act.Perform(Act.CC, Act.TC, Act.TP))
            {
                var spellExp = Act.CC.elements.GetSpellExp(Act.CC, act, 200);
                Act.CC.ModExp(act.id, spellExp);
            }
            Act.TC = tC;
            Act.powerMod = 100;
        }
    }
}
