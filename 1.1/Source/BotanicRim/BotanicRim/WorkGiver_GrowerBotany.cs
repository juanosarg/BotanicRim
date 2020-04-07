using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace BotanicRim
{
    public abstract class WorkGiver_GrowerBotany : WorkGiver_Scanner
    {
        protected static ThingDef wantedPlantDef;

        public override bool AllowUnreachable
        {
            get
            {
                return true;
            }
        }

        protected virtual bool ExtraRequirements(IPlantToGrowSettable settable, Pawn pawn)
        {
            return true;
        }

        [DebuggerHidden]
        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            Danger maxDanger = pawn.NormalMaxDanger();
            List<Building> bList = pawn.Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < bList.Count; i++)
            {
                Building_PlantGrower b = bList[i] as Building_PlantGrower;
                if ((b != null)&&b.TryGetComp<CompBotanyPlanter>().GetIsBotanyPlanter)
                {
                    if (this.ExtraRequirements(b, pawn))
                    {
                        if (!b.IsForbidden(pawn))
                        {
                            if (pawn.CanReach(b, PathEndMode.OnCell, maxDanger, false, TraverseMode.ByPawn))
                            {
                                if (!b.IsBurning())
                                {

                                    foreach (IntVec3 intVec in b.OccupiedRect())
                                    {
                                        yield return intVec;
                                    }
                                    WorkGiver_GrowerBotany.wantedPlantDef = null;

                                    
                                }
                            }
                        }
                    }
                }
            }
            WorkGiver_GrowerBotany.wantedPlantDef = null;
            List<Zone> zonesList = pawn.Map.zoneManager.AllZones;
            for (int j = 0; j < zonesList.Count; j++)
            {
                Zone_GrowingBotanics growZone = zonesList[j] as Zone_GrowingBotanics;
                if (growZone != null)
                {
                    if (growZone.cells.Count == 0)
                    {
                        Log.ErrorOnce("Grow zone has 0 cells: " + growZone, -563487, false);
                    }
                    else if (this.ExtraRequirements(growZone, pawn))
                    {
                        if (!growZone.ContainsStaticFire)
                        {
                            if (pawn.CanReach(growZone.Cells[0], PathEndMode.OnCell, maxDanger, false, TraverseMode.ByPawn))
                            {
                                for (int k = 0; k < growZone.cells.Count; k++)
                                {
                                    yield return growZone.cells[k];
                                }
                                WorkGiver_GrowerBotany.wantedPlantDef = null;
                            }
                        }
                    }
                }
            }
            WorkGiver_GrowerBotany.wantedPlantDef = null;
        }

        public static ThingDef CalculateWantedPlantDef(IntVec3 c, Map map)
        {
            IPlantToGrowSettable plantToGrowSettable = c.GetPlantToGrowSettable(map);
            if (plantToGrowSettable == null)
            {
                return null;
            }
            return plantToGrowSettable.GetPlantDefToGrow();
        }
    }
}
