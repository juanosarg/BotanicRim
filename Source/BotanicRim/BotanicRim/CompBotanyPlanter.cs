using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotanicRim
{
    class CompBotanyPlanter : ThingComp
    {

        public CompProperties_BotanyPlanter Props
        {
            get
            {
                return (CompProperties_BotanyPlanter)this.props;
            }
        }

        public bool GetIsBotanyPlanter
        {
            get
            {
                return this.Props.isBotanyPlanter;
            }
        }
    }
}
