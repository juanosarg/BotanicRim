using Harmony;
using RimWorld;
using System.Reflection;
using Verse;

using System;

// So, let's comment this code, since it uses Harmony and has moderate complexity

namespace BotanicRim
{



    //Setting the Harmony instance
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            var harmony = HarmonyInstance.Create("com.botanicrim");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }


    }





    /*This Harmony Postfix allows us to regrow plants with a 25% percentage if an active beehouse is nearby
*/
    [HarmonyPatch(typeof(PlantUtility))]
    [HarmonyPatch("CanSowOnGrower")]
    public static class PlantUtility_CanSowOnGrower_Patch
    {
        [HarmonyPostfix]
        public static void SowTagsOnBotanicPlants( ThingDef plantDef, object obj, ref bool __result)
        {
            if (obj is Zone_GrowingBotanics)
            {
                __result = plantDef.plant.sowTags.Contains("BR_Zone_Planting");
            }



        }


    }


}











