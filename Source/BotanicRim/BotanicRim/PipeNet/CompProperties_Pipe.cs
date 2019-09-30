using System;
using Verse;

namespace BotanicRim
{
    public class CompProperties_Pipe : CompProperties
    {
        public CompProperties_Pipe()
        {
            this.compClass = typeof(CompPipe);
        }

        public Type PipeNetClass = typeof(NutrientPipeNet);

        public PipeType mode;

        public bool stuffed;
    }
}
