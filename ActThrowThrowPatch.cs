using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace Mod_PandaTalismanMod
{

    [HarmonyPatch]
    internal class ActThrowThrowPatch
    {

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ActThrow), nameof(ActThrow.Throw), new Type[] { typeof(Card), typeof(Point), typeof(Card), typeof(Thing), typeof(ThrowMethod) })]
        internal static IEnumerable<CodeInstruction> OnThrowIl(IEnumerable<CodeInstruction> instructions)
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
            if (Act.CC.IsThrownWeapon == false && Act.CC.Tool == null && Act.CC.TryGetThrowable() == null)
            {
                return;
            }
            var weapon = new Thing { };
            if (Act.CC.ai.IsRunning)
            {
                weapon = Act.CC.TryGetThrowable();
            }
            else if (!Act.CC.ai.IsRunning)
            {
                weapon = Act.CC.Tool;
            }
            else 
            {
                return;
            }
            bool flag3 = false;
            if (weapon.c_ammo <= 0 || Act.CC.HasCondition<ConReload>())
            {
                return;
            }
            if (!(weapon.ammoData?.trait is TraitAmmo traitAmmo && weapon.ammoData?.trait is TraitAmmoTalisman traitAmmoTalisman))
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
                flag3 = true;
                var spellExp = Act.CC.elements.GetSpellExp(Act.CC, act, 200);
                Act.CC.ModExp(act.id, spellExp);
            }
            Act.TC = tC;
            Act.powerMod = 100;
            if (!flag3)
            {
                return;
            }
            weapon.c_ammo--;
            if (weapon.ammoData != null)
            {
                weapon.ammoData.Num = weapon.c_ammo;
            }
            if (weapon.c_ammo <= 0)
            {
                weapon.c_ammo = 0;
                weapon.ammoData = null;
            }
            LayerInventory.SetDirty(weapon);
        }
    }
}
