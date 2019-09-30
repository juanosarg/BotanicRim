using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BotanicRim
{
    public abstract class PipeMapComponent : MapComponent
    {

        public List<NutrientPipeNet> PipeNets = new List<NutrientPipeNet>();

        public List<CompPipe> cachedPipes = new List<CompPipe>();

        public int masterID;

        public int[,] PipeGrid;

        public bool[] DirtyPipeFlag;

        public PipeMapComponent(Map map) : base(map)
        {
            int length = Enum.GetValues(typeof(PipeType)).Length;
            this.PipeGrid = new int[length, map.cellIndices.NumGridCells];
            this.DirtyPipeFlag = new bool[length];
            for (int i = 0; i < this.DirtyPipeFlag.Length; i++)
            {
                this.DirtyPipeFlag[i] = true;
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            foreach (NutrientPipeNet pipelineNet in this.PipeNets)
            {
                pipelineNet.Tick();
            }
        }

        public override void MapGenerated()
        {
            base.MapGenerated();
            this.RegenGrids();
        }

        public void RegenGrids()
        {
            for (int i = 0; i < this.DirtyPipeFlag.Length; i++)
            {
                if (this.DirtyPipeFlag[i])
                {
                    this.RebuildPipeGrid(i);
                }
            }
        }

        public bool PerfectMatch(IntVec3 pos, PipeType P, int ID)
        {
            return this.PipeGrid[(int)P, this.map.cellIndices.CellToIndex(pos)] == ID;
        }

        public int IDAt(IntVec3 pos, PipeType P)
        {
            return this.PipeGrid[(int)P, this.map.cellIndices.CellToIndex(pos)];
        }

        public bool ZoneAt(IntVec3 pos, PipeType P)
        {
            return this.PipeGrid[(int)P, this.map.cellIndices.CellToIndex(pos)] >= 0;
        }

        public void RegisterPipe(CompPipe pipe, bool respawningAfterLoad)
        {
            if (!this.cachedPipes.Contains(pipe))
            {
                this.cachedPipes.Add(pipe);
                this.cachedPipes.Shuffle<CompPipe>();
            }
            this.DirtyPipeGrid(pipe.mode);
            if (!respawningAfterLoad)
            {
                this.RegenGrids();
            }
        }

        public void DeregisterPipe(CompPipe pipe)
        {
            if (this.cachedPipes.Contains(pipe))
            {
                this.cachedPipes.Remove(pipe);
                this.cachedPipes.Shuffle<CompPipe>();
            }
            this.DirtyPipeGrid(pipe.mode);
            this.RegenGrids();
        }

        public void DirtyPipeGrid(PipeType p)
        {
            this.DirtyPipeFlag[(int)p] = true;
        }

        public void DirtyAllPipeGrids()
        {
            for (int i = 0; i < this.DirtyPipeFlag.Length; i++)
            {
                this.DirtyPipeFlag[i] = true;
            }
            this.RegenGrids();
        }

        public void RebuildPipeGrid(int P)
        {
            this.DirtyPipeFlag[P] = false;
            for (int i = 0; i < this.PipeGrid.GetLength(1); i++)
            {
                this.PipeGrid[P, i] = -1;
            }
            this.PipeNets.RemoveAll((NutrientPipeNet x) => x.NetType == P);
            (from x in this.cachedPipes
             where x.mode == (PipeType)P
             select x).ToList<CompPipe>().ForEach(delegate (CompPipe j)
             {
                 j.GridID = -1;
             });
            Func<CompPipe, bool> <> 9__7;
            Func<CompPipe, bool> <> 9__6;
            IEnumerable<CompPipe> source;
            Func<CompPipe, bool> predicate;
            for (CompPipe compPipe = this.cachedPipes.FirstOrDefault((CompPipe k) => k.mode == (PipeType)P && !k.closed && k.GridID == -1); compPipe != null; compPipe = source.FirstOrDefault(predicate))
            {
                NutrientPipeNet newNet = Activator.CreateInstance(compPipe.Props.PipeNetClass) as NutrientPipeNet;
                newNet.MapComp = (this as MapComponent_Rimefeller);
                newNet.NetID = this.masterID;
                newNet.NetType = P;
                this.PipeNets.Add(newNet);
                Predicate<IntVec3> passCheck = delegate (IntVec3 c)
                {
                    foreach (ThingWithComps thingWithComps in c.GetThingList(this.map).OfType<ThingWithComps>())
                    {
                        IEnumerable<CompPipe> comps = thingWithComps.GetComps<CompPipe>();
                        Func<CompPipe, bool> predicate2;
                        if ((predicate2 = <> 9__7) == null)
                        {
                            predicate2 = (<> 9__7 = ((CompPipe x) => x.mode == (PipeType)P));
                        }
                        CompPipe compPipe2 = comps.FirstOrDefault(predicate2);
                        if (compPipe2 != null && compPipe2.mode == (PipeType)P && !compPipe2.closed)
                        {
                            compPipe2.GridID = this.masterID;
                            compPipe2.pipeNet = newNet;
                            newNet.PipedThings.Add(compPipe2.parent);
                            this.map.mapDrawer.MapMeshDirty(compPipe2.parent.Position, MapMeshFlag.Buildings);
                            this.map.mapDrawer.MapMeshDirty(compPipe2.parent.Position, MapMeshFlag.Things);
                            this.PipeGrid[P, this.map.cellIndices.CellToIndex(c)] = this.masterID;
                            return true;
                        }
                    }
                    return false;
                };
                Action<IntVec3> processor = delegate (IntVec3 c)
                {
                };
                this.map.floodFiller.FloodFill(compPipe.parent.Position, passCheck, processor, int.MaxValue, false, null);
                this.masterID++;
                source = this.cachedPipes;
                if ((predicate = <> 9__6) == null)
                {
                    predicate = (<> 9__6 = ((CompPipe k) => k.mode == (PipeType)P && !k.closed && k.GridID == -1));
                }
            }
            foreach (NutrientPipeNet pipelineNet in this.PipeNets)
            {
                pipelineNet.InitNet();
            }
        }

       
    }
}

