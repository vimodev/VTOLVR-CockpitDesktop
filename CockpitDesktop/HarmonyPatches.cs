using Harmony;
using UnityEngine;

namespace CockpitDesktop
{

    [HarmonyPatch(typeof(ExternalCamManager))]
    [HarmonyPatch("Start")]
    public class Patch0
    {
        public static bool Prefix(ExternalCamManager __instance)
        {
            return true;
        }
    }

    [HarmonyPatch(typeof(ExternalCamManager))]
    [HarmonyPatch("NextCamera")]
    public class Patch1
    {
        public static bool Prefix(ExternalCamManager __instance)
        {
            return true;
        }
    }

    [HarmonyPatch(typeof(ExternalCamManager))]
    [HarmonyPatch("PrevCamera")]
    public class Patch2
    {
        public static bool Prefix(ExternalCamManager __instance)
        {
            return true;
        }
    }

}