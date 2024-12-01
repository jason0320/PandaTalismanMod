using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace Mod_PandaTalismanMod
{

    [HarmonyPatch(typeof(ActThrow), nameof(ActThrow.Throw), new Type[] { typeof(Card), typeof(Point), typeof(Card), typeof(Thing), typeof(ThrowMethod) })]

    class ActThrowThrowPatch {
        static bool Prefix(Card c, Point p, Card target, Thing t, ThrowMethod method = ThrowMethod.Default)
        {
            if (t.parent != EClass._zone && !t.HasElement(410))
            {
                EClass._zone.AddCard(t, c.pos).KillAnime();
            }
            Act.TP.Set(p);
            Act.TC = target;
            if (t.trait.ThrowType == ThrowType.Snow)
            {
                t.dir = EClass.rnd(2);
                c.Talk("snow_throw");
                if (EClass.rnd(2) == 0)
                {
                    Act.TC = null;
                }
            }
            c.Say("throw", c, t.GetName(NameStyle.Full, 1));
            c.LookAt(p);
            c.renderer.NextFrame();
            c.PlaySound("throw");
            EffectIRenderer result = null;
            if (c.isSynced || p.IsSync)
            {
                result = Effect.Get<EffectIRenderer>((t.trait is TraitBall) ? "throw_ball" : "throw").Play((c.isChara && c.Chara.host != null) ? c.Chara.host : c, t, c.pos, p, 0.2f);
                t.renderer.SetFirst(first: false, c.renderer.position);
            }
            if (!t.HasElement(410))
            {
                t._Move(p);
            }
            if (!t.trait.CanBeDestroyed)
            {
                c.PlaySound("miss");
                return result;
            }
            ThrowType throwType = t.trait.ThrowType;
            if ((uint)(throwType - 1) <= 1u)
            {
                Msg.Say("shatter");
            }
            bool flag = method == ThrowMethod.Reward;
            bool flag2 = method == ThrowMethod.Default;
            switch (t.trait.ThrowType)
            {
                case ThrowType.Explosive:
                    flag = true;
                    t.c_uidRefCard = c.uid;
                    t.Die(null, c, AttackSource.Throw);
                    break;
                case ThrowType.Vase:
                    t.Die(null, null, AttackSource.Throw);
                    break;
                case ThrowType.Potion:
                    flag = true;
                    if (Act.TC != null)
                    {
                        Act.TC.Say("throw_hit", t, Act.TC);
                    }
                    Act.TP.ModFire(-50);
                    if (Act.TC != null && Act.TC.isChara)
                    {
                        if (t.trait.CanDrink(Act.TC.Chara))
                        {
                            t.trait.OnDrink(Act.TC.Chara);
                        }
                        flag2 = t.IsNegativeGift;
                        Act.TC.Chara.AddCondition<ConWet>();
                    }
                    else
                    {
                        t.trait.OnThrowGround(c.Chara, Act.TP);
                    }
                    t.Die(null, null, AttackSource.Throw);
                    c.ModExp(108, 50);
                    break;
                case ThrowType.Snow:
                    flag = true;
                    flag2 = false;
                    if (Act.TC != null && Act.TC.isChara)
                    {
                        Act.TC.Say("throw_hit", t, Act.TC);
                        if (EClass.rnd(2) == 0)
                        {
                            c.Talk("snow_hit");
                        }
                        Act.TC.Chara.AddCondition<ConWet>(50);
                        t.Die(null, null, AttackSource.Throw);
                        c.ModExp(108, 50);
                    }
                    break;
                case ThrowType.Ball:
                    flag = true;
                    flag2 = false;
                    if (Act.TC != null && Act.TC.isChara)
                    {
                        Act.TC.Say("throw_hit", t, Act.TC);
                        if (EClass.rnd(2) == 0)
                        {
                            c.Talk("snow_hit");
                        }
                        Act.TC.Say("ball_hit");
                        Act.TC.Chara?.Pick(t);
                        c.ModExp(108, 50);
                    }
                    break;
                case ThrowType.Flyer:
                    flag = true;
                    flag2 = false;
                    if (Act.TC != null && Act.TC.isChara && c.isChara)
                    {
                        Act.TC.Say("throw_hit", t, Act.TC);
                        c.Chara.GiveGift(Act.TC.Chara, t);
                        c.ModExp(108, 50);
                    }
                    break;
                case ThrowType.MonsterBall:
                    {
                        flag = true;
                        flag2 = false;
                        TraitMonsterBall traitMonsterBall = t.trait as TraitMonsterBall;
                        if (traitMonsterBall.chara != null)
                        {
                            if (traitMonsterBall.IsLittleBall && !(EClass._zone is Zone_LittleGarden))
                            {
                                break;
                            }
                            Chara _c = EClass._zone.AddCard(traitMonsterBall.chara, p).Chara;
                            _c.PlayEffect("identify");
                            t.Die();
                            if (traitMonsterBall.IsLittleBall && _c.id == "littleOne")
                            {
                                _c.orgPos = c.pos.Copy();
                                Chara chara = _c;
                                Hostility c_originalHostility = (_c.hostility = Hostility.Neutral);
                                chara.c_originalHostility = c_originalHostility;
                                EClass._zone.ModInfluence(5);
                                _c.PlaySound("chime_angel");
                                EClass.core.actionsNextFrame.Add(delegate
                                {
                                    _c.Talk("little_saved");
                                });
                                EClass.player.flags.little_saved = true;
                                EClass.player.little_saved++;
                            }
                            else
                            {
                                _c.MakeAlly();
                            }
                        }
                        else
                        {
                            if (Act.TC == null || !Act.TC.isChara)
                            {
                                break;
                            }
                            Act.TC.Say("throw_hit", t, Act.TC);
                            Chara chara2 = Act.TC.Chara;
                            if (traitMonsterBall.IsLittleBall)
                            {
                                if (chara2.id != "littleOne" || EClass._zone is Zone_LittleGarden || EClass._zone.IsUserZone)
                                {
                                    Msg.Say("monsterball_invalid");
                                    break;
                                }
                            }
                            else
                            {
                                if (!chara2.trait.CanBeTamed || EClass._zone.IsUserZone)
                                {
                                    Msg.Say("monsterball_invalid");
                                    break;
                                }
                                if (chara2.LV > traitMonsterBall.owner.LV)
                                {
                                    Msg.Say("monsterball_lv");
                                    break;
                                }
                                if (!EClass.debug.enable && chara2.hp > chara2.MaxHP / 10)
                                {
                                    Msg.Say("monsterball_hp");
                                    break;
                                }
                            }
                            Msg.Say("monsterball_capture", c, chara2);
                            chara2.PlaySound("identify");
                            chara2.PlayEffect("identify");
                            t.ChangeMaterial("copper");
                            if (chara2.IsLocalChara)
                            {
                                Debug.Log("Creating Replacement NPC for:" + chara2);
                                EClass._map.deadCharas.Add(chara2.CreateReplacement());
                            }
                            traitMonsterBall.chara = chara2;
                            EClass._zone.RemoveCard(chara2);
                            chara2.homeZone = null;
                            c.ModExp(108, 100);
                        }
                        break;
                    }
            }
            if (t.trait is TraitDye)
            {
                t.trait.OnThrowGround(c.Chara, Act.TP);
            }
            if (!flag && Act.TC != null)
            {
                AttackProcess.Current.Prepare(c.Chara, t, Act.TC, Act.TP, 0, _isThrow: true);
                bool hasHit = AttackProcess.Current.Perform(0, hasHit: false);
                if (hasHit)
                {
                    if (t.c_ammo > 0 && !Act.CC.HasCondition<ConReload>())
                    {
                        bool flag3 = true;
                        TraitAmmo traitAmmo = ((t.ammoData == null) ? null : (t.ammoData.trait as TraitAmmo));
                        if (traitAmmo != null && traitAmmo is TraitAmmoTalisman traitAmmoTalisman)
                        {
                            flag3 = false;
                            if (hasHit && Act.TC != null && Act.TC.IsAliveInCurrentZone)
                            {
                                Act act = Act.CC.elements.GetElement(traitAmmoTalisman.owner.refVal)?.act ?? ACT.Create(traitAmmoTalisman.owner.refVal);
                                Act.powerMod = traitAmmo.owner.encLV;
                                Card tC = Act.TC;
                                if (act.Perform(Act.CC, Act.TC, Act.TP))
                                {
                                    flag3 = true;
                                    int spellExp = Act.CC.elements.GetSpellExp(Act.CC, act, 200);
                                    Act.CC.ModExp(act.id, spellExp);
                                }
                                Act.TC = tC;
                                Act.powerMod = 100;
                            }
                        }
                        if (flag3)
                        {
                            t.c_ammo--;
                            if (t.ammoData != null)
                            {
                                t.ammoData.Num = t.c_ammo;
                            }
                            if (t.c_ammo <= 0)
                            {
                                t.c_ammo = 0;
                                t.ammoData = null;
                            }
                            LayerInventory.SetDirty(t);
                        }
                    }
                    if (Act.TC.IsAliveInCurrentZone && t.trait is TraitErohon && Act.TC.id == t.c_idRefName)
                    {
                        Act.TC.Chara.OnGiveErohon(t);
                    }
                    if (!t.isDestroyed && t.trait.CanBeDestroyed && !t.IsFurniture && !t.category.IsChildOf("instrument") && !t.IsUnique && !t.HasElement(410))
                    {
                        t.Destroy();
                    }
                }
                else
                {
                    c.PlaySound("miss");
                }
            }
            if (EClass.rnd(2) == 0)
            {
                c.Chara.RemoveCondition<ConInvisibility>();
            }
            if (Act.TC != null)
            {
                if (flag2)
                {
                    c.Chara.DoHostileAction(Act.TC);
                }
                if ((Act.TC.trait.CanBeAttacked || Act.TC.IsRestrainedResident) && EClass.rnd(2) == 0)
                {
                    c.Chara.stamina.Mod(-1);
                }
            }
            if (t.HasElement(410) && Act.TC != null && Act.CC == EClass.pc && !(Act.CC.ai is AI_PracticeDummy) && (Act.TC.trait is TraitTrainingDummy || Act.TC.IsRestrainedResident) && Act.CC.stamina.value > 0)
            {
                Act.CC.SetAI(new AI_PracticeDummy
                {
                    target = Act.TC,
                    throwItem = t
                });
            }
            return true;
        }
    }
}