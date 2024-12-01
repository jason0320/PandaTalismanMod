using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Mod_PandaTalismanMod
{

    [BepInPlugin("panda.talisman.mod", "Panda's Talisman Mod", "1.0.0.0")]

    public class Mod_PandaTalismanMod : BaseUnityPlugin
    {
        private void Start()
        {
            var harmony = new Harmony("Panda's Talisman Mod");
            harmony.PatchAll();
        }

        public void OnStartCore()
        {
            ProcessRecipe.OnStartCore();
        }

    }
}