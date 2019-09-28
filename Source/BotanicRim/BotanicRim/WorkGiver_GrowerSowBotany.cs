﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace BotanicRim
{
    public class WorkGiver_GrowerSowBotany : WorkGiver_GrowerBotany
    {
        protected static string CantSowCavePlantBecauseOfLightTrans;

        protected static string CantSowCavePlantBecauseUnroofedTrans;

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.ClosestTouch;
            }
        }

        public static void ResetStaticData()
        {
            WorkGiver_GrowerSowBotany.CantSowCavePlantBecauseOfLightTrans = "CantSowCavePlantBecauseOfLight".Translate();
            WorkGiver_GrowerSowBotany.CantSowCavePlantBecauseUnroofedTrans = "CantSowCavePlantBecauseUnroofed".Translate();
        }

        protected override bool ExtraRequirements(IPlantToGrowSettable settable, Pawn pawn)
        {
            if (!settable.CanAcceptSowNow())
            {
                return false;
            }
            Zone_GrowingBotanics zone_Growing = settable as Zone_GrowingBotanics;
            IntVec3 c;
            if (zone_Growing != null)
            {
                if (!zone_Growing.allowSow)
                {
                    return false;
                }
                c = zone_Growing.Cells[0];
            }
            else
            {
                c = ((Thing)settable).Position;
            }
            WorkGiver_GrowerBotany.wantedPlantDef = WorkGiver_GrowerBotany.CalculateWantedPlantDef(c, pawn.Map);
            return WorkGiver_GrowerBotany.wantedPlantDef != null;
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        {
            Map map = pawn.Map;
            if (c.IsForbidden(pawn))
            {
                return null;
            }
            if (!PlantUtility.GrowthSeasonNow(c, map, true))
            {
                return null;
            }
            if (WorkGiver_GrowerBotany.wantedPlantDef == null)
            {
                WorkGiver_GrowerBotany.wantedPlantDef = WorkGiver_GrowerBotany.CalculateWantedPlantDef(c, map);
                if (WorkGiver_GrowerBotany.wantedPlantDef == null)
                {
                    return null;
                }
            }
            List<Thing> thingList = c.GetThingList(map);
            bool flag = false;
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (thing.def == WorkGiver_GrowerBotany.wantedPlantDef)
                {
                    return null;
                }
                if ((thing is Blueprint || thing is Frame) && thing.Faction == pawn.Faction)
                {
                    flag = true;
                }
            }
            if (flag)
            {
                Thing edifice = c.GetEdifice(map);
                if (edifice == null || edifice.def.fertility < 0f)
                {
                    return null;
                }
            }
            if (WorkGiver_GrowerBotany.wantedPlantDef.plant.cavePlant)
            {
                if (!c.Roofed(map))
                {
                    JobFailReason.Is(WorkGiver_GrowerSowBotany.CantSowCavePlantBecauseUnroofedTrans, null);
                    return null;
                }
                if (map.glowGrid.GameGlowAt(c, true) > 0f)
                {
                    JobFailReason.Is(WorkGiver_GrowerSowBotany.CantSowCavePlantBecauseOfLightTrans, null);
                    return null;
                }
            }
            if (WorkGiver_GrowerBotany.wantedPlantDef.plant.interferesWithRoof && c.Roofed(pawn.Map))
            {
                return null;
            }
            Plant plant = c.GetPlant(map);
            if (plant != null && plant.def.plant.blockAdjacentSow)
            {
                LocalTargetInfo target = plant;
                if (!pawn.CanReserve(target, 1, -1, null, forced) || plant.IsForbidden(pawn))
                {
                    return null;
                }
                return new Job(JobDefOf.CutPlant, plant);
            }
            else
            {
                Thing thing2 = PlantUtility.AdjacentSowBlocker(WorkGiver_GrowerBotany.wantedPlantDef, c, map);
                if (thing2 != null)
                {
                    Plant plant2 = thing2 as Plant;
                    if (plant2 != null)
                    {
                        LocalTargetInfo target = plant2;
                        if (pawn.CanReserve(target, 1, -1, null, forced) && !plant2.IsForbidden(pawn))
                        {
                            IPlantToGrowSettable plantToGrowSettable = plant2.Position.GetPlantToGrowSettable(plant2.Map);
                            if (plantToGrowSettable == null || plantToGrowSettable.GetPlantDefToGrow() != plant2.def)
                            {
                                return new Job(JobDefOf.CutPlant, plant2);
                            }
                        }
                    }
                    return null;
                }
                if (WorkGiver_GrowerBotany.wantedPlantDef.plant.sowMinSkill > 0 && pawn.skills != null && pawn.skills.GetSkill(SkillDefOf.Plants).Level < WorkGiver_GrowerBotany.wantedPlantDef.plant.sowMinSkill)
                {
                    return null;
                }
                int j = 0;
                while (j < thingList.Count)
                {
                    Thing thing3 = thingList[j];
                    if (thing3.def.BlockPlanting)
                    {
                        LocalTargetInfo target = thing3;
                        if (!pawn.CanReserve(target, 1, -1, null, forced))
                        {
                            return null;
                        }
                        if (thing3.def.category == ThingCategory.Plant)
                        {
                            if (!thing3.IsForbidden(pawn))
                            {
                                return new Job(JobDefOf.CutPlant, thing3);
                            }
                            return null;
                        }
                        else
                        {
                            if (thing3.def.EverHaulable)
                            {
                                return HaulAIUtility.HaulAsideJobFor(pawn, thing3);
                            }
                            return null;
                        }
                    }
                    else
                    {
                        j++;
                    }
                }
                if (WorkGiver_GrowerBotany.wantedPlantDef.CanEverPlantAt(c, map) && PlantUtility.GrowthSeasonNow(c, map, true))
                {
                    LocalTargetInfo target = c;
                    if (pawn.CanReserve(target, 1, -1, null, forced))
                    {
                        return new Job(JobDefOf.Sow, c)
                        {
                            plantDefToSow = WorkGiver_GrowerBotany.wantedPlantDef
                        };
                    }
                }
                return null;
            }
        }
    }
}
