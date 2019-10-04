using System;
using Verse;

namespace BotanicRim
{
    public class CompProperties_Pipe : CompProperties
    {
       
        public bool transmitsPower = true;
        public float basePowerConsumption;
        public bool shortCircuitInRain;
        public SoundDef soundPowerOn;
        public SoundDef soundPowerOff;
        public SoundDef soundAmbientPowered;
    }
}
