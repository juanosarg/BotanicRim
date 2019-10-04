﻿using System;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;

namespace BotanicRim
{
    public class CompPipeTrader : CompPipe
    {
        public Action powerStartedAction;

        public Action powerStoppedAction;

        private bool powerOnInt;

        public float powerOutputInt;

        private bool powerLastOutputted;

        private Sustainer sustainerPowered;

        protected CompFlickable flickableComp;

        public const string PowerTurnedOnSignal = "PowerTurnedOn";

        public const string PowerTurnedOffSignal = "PowerTurnedOff";

        public float PowerOutput
        {
            get
            {
                return this.powerOutputInt;
            }
            set
            {
                this.powerOutputInt = value;
                if (this.powerOutputInt > 0f)
                {
                    this.powerLastOutputted = true;
                }
                if (this.powerOutputInt < 0f)
                {
                    this.powerLastOutputted = false;
                }
            }
        }

        public float EnergyOutputPerTick
        {
            get
            {
                return this.PowerOutput * CompPipe.WattsToWattDaysPerTick;
            }
        }

        public bool PowerOn
        {
            get
            {
                return this.powerOnInt;
            }
            set
            {
                if (this.powerOnInt == value)
                {
                    return;
                }
                this.powerOnInt = value;
                if (this.powerOnInt)
                {
                    if (!FlickUtility.WantsToBeOn(this.parent))
                    {
                        Log.Warning("Tried to power on " + this.parent + " which did not desire it.", false);
                        return;
                    }
                    if (this.parent.IsBrokenDown())
                    {
                        Log.Warning("Tried to power on " + this.parent + " which is broken down.", false);
                        return;
                    }
                    if (this.powerStartedAction != null)
                    {
                        this.powerStartedAction();
                    }
                    this.parent.BroadcastCompSignal("PowerTurnedOn");
                    SoundDef soundDef = ((CompProperties_Pipe)this.parent.def.CompDefForAssignableFrom<CompPipeTrader>()).soundPowerOn;
                    if (soundDef.NullOrUndefined())
                    {
                        soundDef = SoundDefOf.Power_OnSmall;
                    }
                    soundDef.PlayOneShot(new TargetInfo(this.parent.Position, this.parent.Map, false));
                    this.StartSustainerPoweredIfInactive();
                }
                else
                {
                    if (this.powerStoppedAction != null)
                    {
                        this.powerStoppedAction();
                    }
                    this.parent.BroadcastCompSignal("PowerTurnedOff");
                    SoundDef soundDef2 = ((CompProperties_Pipe)this.parent.def.CompDefForAssignableFrom<CompPipeTrader>()).soundPowerOff;
                    if (soundDef2.NullOrUndefined())
                    {
                        soundDef2 = SoundDefOf.Power_OffSmall;
                    }
                    if (this.parent.Spawned)
                    {
                        soundDef2.PlayOneShot(new TargetInfo(this.parent.Position, this.parent.Map, false));
                    }
                    this.EndSustainerPoweredIfActive();
                }
            }
        }

        public string DebugString
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(this.parent.LabelCap + " CompPower:");
                stringBuilder.AppendLine("   PowerOn: " + this.PowerOn);
                stringBuilder.AppendLine("   energyProduction: " + this.PowerOutput);
                return stringBuilder.ToString();
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "FlickedOff" || signal == "ScheduledOff" || signal == "Breakdown")
            {
                this.PowerOn = false;
            }
            if (signal == "RanOutOfFuel" && this.powerLastOutputted)
            {
                this.PowerOn = false;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.flickableComp = this.parent.GetComp<CompFlickable>();
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            this.EndSustainerPoweredIfActive();
            this.powerOutputInt = 0f;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.powerOnInt, "powerOn", true, false);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (!this.parent.IsBrokenDown())
            {
                if (this.flickableComp != null && !this.flickableComp.SwitchIsOn)
                {
                    this.parent.Map.overlayDrawer.DrawOverlay(this.parent, OverlayTypes.PowerOff);
                }
                else if (FlickUtility.WantsToBeOn(this.parent))
                {
                    if (!this.PowerOn)
                    {
                        this.parent.Map.overlayDrawer.DrawOverlay(this.parent, OverlayTypes.NeedsPower);
                    }
                }
            }
        }

        public override void SetUpPowerVars()
        {
            base.SetUpPowerVars();
            CompProperties_Pipe props = base.Props;
            this.PowerOutput = -1f * props.basePowerConsumption;
            this.powerLastOutputted = (props.basePowerConsumption <= 0f);
        }

        public override void ResetPowerVars()
        {
            base.ResetPowerVars();
            this.powerOnInt = false;
            this.powerOutputInt = 0f;
            this.powerLastOutputted = false;
            this.sustainerPowered = null;
            if (this.flickableComp != null)
            {
                this.flickableComp.ResetToOn();
            }
        }

        public override void LostConnectParent()
        {
            base.LostConnectParent();
            this.PowerOn = false;
        }

        public override string CompInspectStringExtra()
        {
            string str;
            if (this.powerLastOutputted)
            {
                str = "GR_LiquidOutput".Translate() + ": " + this.PowerOutput.ToString("#####0") + " ml";
            }
            else
            {
                str = "GR_NutrientNeeded".Translate() + ": " + (-this.PowerOutput).ToString("#####0") + " ml";
            }
            return str + "\n" + base.CompInspectStringExtra();
        }

        private void StartSustainerPoweredIfInactive()
        {
            CompProperties_Pipe props = base.Props;
            if (!props.soundAmbientPowered.NullOrUndefined() && this.sustainerPowered == null)
            {
                SoundInfo info = SoundInfo.InMap(this.parent, MaintenanceType.None);
                this.sustainerPowered = props.soundAmbientPowered.TrySpawnSustainer(info);
            }
        }

        private void EndSustainerPoweredIfActive()
        {
            if (this.sustainerPowered != null)
            {
                this.sustainerPowered.End();
                this.sustainerPowered = null;
            }
        }
    }
}
