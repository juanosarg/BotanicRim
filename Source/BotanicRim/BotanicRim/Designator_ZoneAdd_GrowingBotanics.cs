using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace BotanicRim
{
    public class Designator_ZoneAdd_GrowingBotanics : Designator_ZoneAdd
    {
        protected override string NewZoneLabel
        {
            get
            {
                return "RB_BotanicsGrowingZone".Translate();
            }
        }

        public Designator_ZoneAdd_GrowingBotanics()
        {
           
            this.zoneTypeToPlace = typeof(Zone_GrowingBotanics);
            this.defaultLabel = "RB_BotanicsGrowingZone".Translate();
            this.defaultDesc = "RB_DesignatorGrowingZoneDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/RB_ZoneCreate_GrowingBotanic", true);
            this.hotKey = KeyBindingDefOf.Misc2;
           
            //  this.tutorTag = "ZoneAdd_Growing";
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!base.CanDesignateCell(c).Accepted)
            {
                return false;
            }
            if (base.Map.fertilityGrid.FertilityAt(c) < 0.1)
            {
                return false;
            }
            return true;
        }

        protected override Zone MakeNewZone()
        {
           // PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.GrowingFood, KnowledgeAmount.Total);
            return new Zone_GrowingBotanics(Find.CurrentMap.zoneManager);
        }
    }
}
