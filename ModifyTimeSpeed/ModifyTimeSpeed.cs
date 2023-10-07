using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifyTimeSpeed
{
    internal class ModifyTimeSpeed : TOD_Time
    {
        public override void RefreshTimeCurve()
        {
            MainLevel.Instance.m_TODTime.m_DayLengthInMinutes = 40f;
            MainLevel.Instance.m_TODTime.m_NightLengthInMinutes = 20f;

            base.RefreshTimeCurve();
        }
    }
}
