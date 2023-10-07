using Enums;
using Mono.Math.Prime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using static P2PStats.ReplicationStat;

namespace ModifyNutritionRate
{
    public class ModifyNutritionRate : PlayerConditionModule
    {
        private float m_NeedCarbo;
        private float m_NeedFat;
        private float m_NeedProteins;
        private float m_NeedHydration;
        private float m_CarboDecrease = 0f;
        private float m_FatDecrease = 0f;
        private float m_ProteinsDecrease = 0f;
        private float m_HydrationDecrease = 0f;

        protected override void UpdateNutrition()
        {
            base.UpdateNutrition();

            float m_CarboFactor = 1f;
            float m_FatFactor = 0.66f;
            float m_ProteinsFactor = 0.33f;
            float m_CarboSpeedFactor = 0.25f;
            float m_FatSpeedFactor = 0.25f;
            float m_ProteinSpeedsFactor = 0.25f;

            float carboFactor = m_CarboFactor * m_CarboSpeedFactor;
            float fatFactor = m_FatFactor * m_FatSpeedFactor;
            float proteinsFactor = m_ProteinsFactor * m_ProteinSpeedsFactor;

            if(m_NutritionCarbo > CalculatePercentageValue(m_MaxNutritionCarbo, 60)) {
                m_CarboDecrease = m_NutritionCarbohydratesConsumptionPerSecond * carboFactor;
                m_FatDecrease = m_NutritionFatConsumptionPerSecond * fatFactor;
                m_ProteinsDecrease = m_NutritionProteinsConsumptionPerSecond * proteinsFactor;

            } else if (m_NutritionCarbo > CalculatePercentageValue(m_MaxNutritionCarbo, 30))
            {
                m_CarboDecrease = m_NutritionCarbohydratesConsumptionPerSecond * carboFactor * 0.66f;
                
                if(m_NutritionFat > CalculatePercentageValue(m_MaxNutritionFat,30))
                {
                    m_FatDecrease = m_NutritionFatConsumptionPerSecond * fatFactor;
                } else
                {
                    m_FatDecrease = m_NutritionFatConsumptionPerSecond * fatFactor * 0.5f;
                }

                m_ProteinsDecrease = m_NutritionProteinsConsumptionPerSecond * proteinsFactor;
            } else if (m_NutritionCarbo >= 0)
            {
                m_CarboDecrease = m_NutritionCarbohydratesConsumptionPerSecond * carboFactor * 0.33f;

                if (m_NutritionFat > CalculatePercentageValue(m_MaxNutritionFat,30))
                {
                    m_FatDecrease = m_NutritionFatConsumptionPerSecond * fatFactor;
                    m_ProteinsDecrease = m_NutritionProteinsConsumptionPerSecond * proteinsFactor * 2;
                }
                else
                {
                    m_FatDecrease = m_NutritionFatConsumptionPerSecond * fatFactor * 0.5f;
                    m_ProteinsDecrease = m_NutritionProteinsConsumptionPerSecond * proteinsFactor;
                }
            }

            m_NutritionCarbo -= m_CarboDecrease;
            m_NutritionFat -= m_FatDecrease;
            m_NutritionProteins -= m_ProteinsDecrease;

            m_NutritionCarbo = Mathf.Clamp(this.m_NutritionCarbo, 0f, GetMaxNutritionCarbo());
            m_NutritionFat = Mathf.Clamp(this.m_NutritionFat, 0f, GetMaxNutritionFat());
            m_NutritionProteins = Mathf.Clamp(this.m_NutritionProteins, 0f, GetMaxNutritionProtein());


            CalculateNutritionNeeds();
        }

        private void CalculateNutritionNeeds()
        {
            float fatCarboIncreaseFactor = 0.5f, proteinFatIncreaseFactor = 0.25f, proteinCarboIncreaseFactor = 0.25f;

            Log("Calculating carbohydrate need...");

            float carboPercentage = GetNutritionPercentage(0);
            float carboIncrease = 0.15f * (1f - carboPercentage) * 100;

            Log("CarboIncrease = " + carboIncrease);

            m_NeedCarbo = (1f - carboPercentage) * 100 + carboIncrease;
            m_NeedCarbo = Mathf.Clamp(this.m_NeedCarbo, 0f, GetMaxNutritionCarbo());

            Log("m_NutritionCarbo = " + m_NutritionCarbo);
            Log("m_NeedCarbo = " + m_NeedCarbo);

            Log("Calculating fat need...");

            float fatPercentage = GetNutritionPercentage(1);
            float fatIncrease = 0.15f * (1f - fatPercentage) * 100;

            Log("fatIncrease = " + fatIncrease);

            m_NeedFat = (1f - fatPercentage) * 100 + fatIncrease + carboIncrease * fatCarboIncreaseFactor;
            m_NeedFat = Mathf.Clamp(this.m_NeedFat, 0f, GetMaxNutritionFat());

            Log("m_NutritionFat = " + m_NutritionFat);
            Log("m_NeedFat = " + m_NeedFat);

            Log("Calculating proteins need...");
            float proteinPercentage = GetNutritionPercentage(1);
            float proteinIncrease = 0.15f * (1f - proteinPercentage) * 100;

            Log("proteinIncrease = " + proteinIncrease);

            m_NeedProteins = (1f - proteinPercentage) * 100 + proteinIncrease + fatIncrease * proteinFatIncreaseFactor + carboIncrease * proteinCarboIncreaseFactor;
            m_NeedProteins = Mathf.Clamp(this.m_NeedProteins, 0f, GetMaxNutritionProtein());

            Log("m_NutritionProteins = " + m_NutritionProteins);
            Log("m_NeedProteins = " + m_NeedProteins);
        }

        protected override void UpdateHydration()
        {
            base.UpdateHydration();

            float m_HydrationSpeedFactor = 0.15f;
            float m_HydrationFactor = GetNutritionPercentage();

            float hydrationFactor = m_HydrationFactor * m_HydrationSpeedFactor;

            m_HydrationDecrease = m_HydrationConsumptionPerSecond * hydrationFactor;
            m_Hydration -= m_HydrationDecrease;
            m_Hydration = Mathf.Clamp(this.m_Hydration, 0f, GetMaxHydration());

            CalculateHydrationNeed();
        }

        private void CalculateHydrationNeed()
        {

            Log("Calculating hydration need...");

            float hydrationPercentage = GetHydrationPercentage();
            float nutritionPercentage = GetNutritionPercentage();
            float nutritionIncrease = 0.15f * (nutritionPercentage) * 100;

            Log("NutritionIncrease = " + nutritionIncrease);

            m_NeedHydration = (1f - hydrationPercentage) * 100 + nutritionIncrease;
            m_NeedHydration = Mathf.Clamp(this.m_NeedHydration, 0f, GetMaxHydration());

            Log("m_Hydration = " + m_Hydration);
            Log("m_NeedHydration = " + m_NeedHydration);
        }

        private float GetNutritionPercentage(int type)
        {
            switch(type)
            {
                case 0: return m_NutritionCarbo / m_MaxNutritionCarbo;
                case 1: return m_NutritionFat / m_MaxNutritionFat;
                case 2: return m_NutritionProteins / m_MaxNutritionProteins;
                default: return 100f;
            }
        }
        private float GetNutritionPercentage()
        {
            float total = m_NutritionCarbo + m_NutritionFat + m_NutritionProteins;

            return total / (m_MaxNutritionCarbo + m_MaxNutritionFat + m_MaxNutritionProteins);
        }

        private float GetHydrationPercentage()
        {
            return m_Hydration / m_MaxHydration;
        }

        private float CalculatePercentageValue(float value, int percentage)
        {
            return (value * percentage) / 100;
        }

        public float GetCarboNeed()
        {
            return m_NeedCarbo;
        }

        public float GetFatNeed()
        {
            return m_NeedFat;
        }

        public float GetProteinNeed()
        {
            return m_NeedProteins;
        }

        public float GetHydrationNeed()
        {
            return m_NeedHydration;
        }

        public float GetMaxCarboNeed()
        {
            return 100f;
        }

        public float GetMaxFatNeed()
        {
            return 100f;
        }

        public float GetMaxProteinNeed()
        {
            return 100f;
        }

        public float GetMaxHydrationNeed()
        {
            return 100f;
        }

        public float GetCarboDecrease()
        {
            return m_CarboDecrease;
        }

        public float GetFatDecrease()
        {
            return m_FatDecrease;
        }

        public float GetProteinsDecrease()
        {
            return m_ProteinsDecrease;
        }

        public float GetHydrationDecrease()
        {
            return m_HydrationDecrease;
        }

        private void Log(string log)
        {
            //CJDebug.Log(log);
        }
    }

    public class SleepControllerExtended : SleepController
    {
        protected override void UpdateSleeping()
        {
            CJDebug.Log("UpdateSleeping ");

            base.UpdateSleeping();

            ModifyNutritionRate modifyNutritionRate = (ModifyNutritionRate)ModifyNutritionRate.Get();

            float sleepCarboSpeedFactor = 1f, sleepFatSpeedFactor = 50f, sleepProteinsSpeedFactor = 50f, sleepHydrationSpeedFactor = 50f;

            CJDebug.Log("" + modifyNutritionRate.GetProteinsDecrease() * sleepProteinsSpeedFactor);

            modifyNutritionRate.m_NutritionCarbo -= modifyNutritionRate.GetCarboDecrease() * sleepCarboSpeedFactor;
            modifyNutritionRate.m_NutritionCarbo = Mathf.Clamp(modifyNutritionRate.m_NutritionCarbo, 0f, modifyNutritionRate.GetMaxNutritionCarbo());
            modifyNutritionRate.m_NutritionFat -= modifyNutritionRate.GetFatDecrease() * sleepFatSpeedFactor;
            modifyNutritionRate.m_NutritionFat = Mathf.Clamp(modifyNutritionRate.m_NutritionFat, 0f, modifyNutritionRate.GetMaxNutritionFat());
            modifyNutritionRate.m_NutritionProteins -= modifyNutritionRate.GetProteinsDecrease() * sleepProteinsSpeedFactor;
            modifyNutritionRate.m_NutritionProteins = Mathf.Clamp(modifyNutritionRate.m_NutritionProteins, 0f, modifyNutritionRate.GetMaxNutritionProtein());
            modifyNutritionRate.m_Hydration -= modifyNutritionRate.GetHydrationDecrease() * sleepHydrationSpeedFactor;
            modifyNutritionRate.m_Hydration = Mathf.Clamp(modifyNutritionRate.m_Hydration, 0f, modifyNutritionRate.GetMaxHydration());
        }
    }

    public class WatchExtended : Watch
    {
        protected override void UpdateState()
        {
            base.UpdateState(); 

            if(this.m_State == Watch.State.Macronutrients)
            {
                ModifyNutritionRate modifyNutritionRate = (ModifyNutritionRate) ModifyNutritionRate.Get();

                WatchMacronutrientsData watchMacronutrientsData = (WatchMacronutrientsData)this.m_Datas[2];
                float fillAmount = modifyNutritionRate.GetFatNeed() / modifyNutritionRate.GetMaxFatNeed();
                watchMacronutrientsData.m_Fat.fillAmount = fillAmount;
                float fillAmount2 = modifyNutritionRate.GetCarboNeed() / modifyNutritionRate.GetMaxCarboNeed();
                watchMacronutrientsData.m_Carbo.fillAmount = fillAmount2;
                float fillAmount3 = modifyNutritionRate.GetProteinNeed() / modifyNutritionRate.GetMaxProteinNeed();
                watchMacronutrientsData.m_Proteins.fillAmount = fillAmount3;
                float fillAmount4 = modifyNutritionRate.GetHydrationNeed() / modifyNutritionRate.GetMaxHydrationNeed();
                watchMacronutrientsData.m_Hydration.fillAmount = fillAmount4;
            }
        }
    }
}
