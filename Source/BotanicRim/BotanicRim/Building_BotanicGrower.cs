using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;
using RimWorld;

namespace BotanicRim
{
    public class Building_BotanicGrower : Building_PlantGrower
    {
        

        public override string GetInspectString()
        {
            string text = "Oh my god \n";
            if (base.Spawned)
            {
                
                if (PlantUtility.GrowthSeasonNow(base.Position, base.Map, true))
                {
                    text = text + "\n" + "GrowSeasonHereNow".Translate();
                }
                else
                {
                    text = text + "\n" + "CannotGrowBadSeasonTemperature".Translate();
                }
            }
            return text;
        }

     
    }
}

