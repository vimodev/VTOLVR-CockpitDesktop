using Harmony;
using UnityEngine;

namespace CockpitDesktop
{

    [HarmonyPatch(typeof(MFDPortalManager))]
    [HarmonyPatch("Start")]
    public class Patch0
    {
        public static bool Prefix(MFDPortalManager __instance)
        {
            return true;
        }
    }

}