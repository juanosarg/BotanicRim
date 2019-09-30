using System;
using System.Linq;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace BotanicRim
{
    public class CompPipe : ThingComp
    {
        public bool closed
        {
            get
            {
                return this.flicker != null && !this.flicker.SwitchIsOn;
            }
        }

        public int GridID { get; set; } = -1;

        public PipeType mode
        {
            get
            {
                return this.Props.mode;
            }
        }

        public CompProperties_Pipe Props
        {
            get
            {
                return (CompProperties_Pipe)this.props;
            }
        }

        public override string CompInspectStringExtra()
        {
            if (DebugSettings.godMode)
            {
                return this.mode.ToString() + "_ID:" + this.GridID;
            }
            return null;
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal == "FlickedOn" || signal == "FlickedOff")
            {
                this.MapComp.DirtyPipeGrid(this.mode);
                this.MapComp.RegenGrids();
            }
        }

        public override void PostDeSpawn(Map map)
        {
            map.GetComponent<PipeMapComponent>().DeregisterPipe(this);
            this.pipeNet.DeregisterPipe(this.parent);
            base.PostDeSpawn(map);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.IsHashIntervalTick(3))
            {
                if (this.fuel != null)
                {
                    if (this.fuel.configuredTargetFuelLevel == -1f)
                    {
                        if (this.fuel.Props.fuelCapacity - this.fuel.Fuel >= 1f)
                        {
                            float num = Mathf.Min(1f, this.fuel.Props.fuelCapacity - this.fuel.Fuel);
                            if (this.pipeNet != null && this.pipeNet.PullFuel((double)num))
                            {
                                this.fuel.Refuel(num);
                            }
                        }
                    }
                    else if (this.fuel.TargetFuelLevel - this.fuel.Fuel >= 1f)
                    {
                        float num2 = Mathf.Min(1f, this.fuel.TargetFuelLevel - this.fuel.Fuel);
                        if (this.pipeNet != null && this.pipeNet.PullFuel((double)num2))
                        {
                            this.fuel.Refuel(num2);
                        }
                    }
                }
                if (this.GetFuelCountToFullyRefuel != null && this.GetFuelCountToFullyRefuel.GetValue<int>() > 1 && this.pipeNet != null && this.pipeNet.PullFuel(1.0))
                {
                    this.Chemfuel.Value += 1f;
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            /*if (this.parent is Building_Valve)
            {
                this.flicker = this.parent.GetComp<CompFlickable>();
            }*/
            this.fuel = this.parent.GetComp<CompRefuelable>();
            if (this.fuel != null && !this.fuel.Props.fuelFilter.Allows(ThingDefOf.Chemfuel))
            {
                this.fuel = null;
            }
            this.MapComp = this.parent.Map.GetComponent<PipeMapComponent>();
            foreach (Thing thing in this.parent.Position.GetThingList(this.parent.Map).ToList<Thing>())
            {
                if (thing != this.parent && thing.TryGetComp<CompPipe>() != null && thing.TryGetComp<CompPipe>().mode == this.mode)
                {
                    thing.Destroy(DestroyMode.Vanish);
                }
            }
            this.parent.Map.GetComponent<PipeMapComponent>().RegisterPipe(this, respawningAfterLoad);
            base.PostSpawnSetup(respawningAfterLoad);
            
        }

        public void PrintForGrid(SectionLayer layer)
        {
            if (!this.closed)
            {
                GraphicsCache.pipeOverlayDick[(int)this.mode].Print(layer, this.parent);
            }
        }

        public CompFlickable flicker;

        public NutrientPipeNet pipeNet;

        private CompRefuelable fuel;

        public PipeMapComponent MapComp;

        public Building puProc;

        private Traverse GetFuelCountToFullyRefuel;

        private Traverse<float> Chemfuel;
    }
}

