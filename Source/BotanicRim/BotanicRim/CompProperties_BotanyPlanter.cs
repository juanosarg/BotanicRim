using RimWorld;
using Verse;
using System.Linq;
using System.Text;

namespace BotanicRim
{
    class CompProperties_BotanyPlanter:CompProperties
    {
        public bool isBotanyPlanter = false;

        public CompProperties_BotanyPlanter()
        {
            this.compClass = typeof(CompBotanyPlanter);
        }
    }
}
