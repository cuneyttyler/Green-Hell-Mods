using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifyEnergyDecay
{
    public class ModifyEnergyDecay : PlayerConditionModule
    {
        public override void Initialize(Being being)
        {
            CJDebug.Log("Updating Energy Consumption Per Second.");
            base.m_EnergyConsumptionPerSecond = 0.05f;

            base.Initialize(being);
        }
    }
}
