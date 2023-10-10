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
        private float m_CarboDecrease;
        private float m_FatDecrease;
        private float m_ProteinsDecrease;
        private float m_HydrationDecrease;
        private float m_CarboFactor;
        private float m_FatFactor;
        private float m_ProteinsFactor;
        private float m_HydrationFactor;
        private float m_fatCarboIncreaseFactor;
        private float m_proteinFatIncreaseFactor;
        private float m_proteinCarboIncreaseFactor;
        private float m_MaxHydrationDecreaseFactor;

        public override void Initialize(Being being)
        {
            base.Initialize(being);

            m_MaxHydrationDecreaseFactor = 4f;

            m_MaxHydration /= m_MaxHydrationDecreaseFactor;
            m_HydrationConsumptionPerSecond /= (m_MaxHydrationDecreaseFactor * 1.5f);
            m_HydrationConsumptionRunMul /= (m_MaxHydrationDecreaseFactor * 1.5f);
            m_HydrationConsumptionDuringFeverPerSecond /= (m_MaxHydrationDecreaseFactor * 1.5f);

            m_fatCarboIncreaseFactor = 0.5f;
            m_proteinFatIncreaseFactor = 0.25f;
            m_proteinCarboIncreaseFactor = 0.25f;

            m_CarboFactor = 0.1f;
            m_FatFactor = 0.08f;
            m_ProteinsFactor = 0.08f;
            m_HydrationFactor = 0.08f;

            Log("m_MaxHydration: " + m_MaxHydration);
            Log("m_HydrationConsumptionPerSecond: " + m_HydrationConsumptionPerSecond);
            Log("m_HydrationConsumptionRunMul: " + m_HydrationConsumptionRunMul);
            Log("m_HydrationConsumptionDuringFeverPerSecond: " + m_HydrationConsumptionDuringFeverPerSecond);
            Log("Initialized.");
    }

        protected override void UpdateNutrition()
        {
            base.UpdateNutrition();

            try
            {
                if (m_NutritionCarbo > CalculatePercentageValue(m_MaxNutritionCarbo, 60))
                {
                    m_CarboDecrease = m_NutritionCarbohydratesConsumptionPerSecond * m_CarboFactor;
                    m_FatDecrease = m_NutritionFatConsumptionPerSecond * m_FatFactor;
                    m_ProteinsDecrease = m_NutritionProteinsConsumptionPerSecond * m_ProteinsFactor;

                }
                else if (m_NutritionCarbo > CalculatePercentageValue(m_MaxNutritionCarbo, 30))
                {
                    m_CarboDecrease = m_NutritionCarbohydratesConsumptionPerSecond * m_CarboFactor * 0.66f;

                    if (m_NutritionFat > CalculatePercentageValue(m_MaxNutritionFat, 30))
                    {
                        m_FatDecrease = m_NutritionFatConsumptionPerSecond * m_FatFactor;
                    }
                    else
                    {
                        m_FatDecrease = m_NutritionFatConsumptionPerSecond * m_FatFactor * 0.5f;
                    }

                    m_ProteinsDecrease = m_NutritionProteinsConsumptionPerSecond * m_ProteinsFactor;
                }
                else if (m_NutritionCarbo >= 0)
                {
                    m_CarboDecrease = m_NutritionCarbohydratesConsumptionPerSecond * m_CarboFactor * 0.33f;

                    if (m_NutritionFat > CalculatePercentageValue(m_MaxNutritionFat, 30))
                    {
                        m_FatDecrease = m_NutritionFatConsumptionPerSecond * m_FatFactor;
                        m_ProteinsDecrease = m_NutritionProteinsConsumptionPerSecond * m_ProteinsFactor * 2;
                    }
                    else
                    {
                        m_FatDecrease = m_NutritionFatConsumptionPerSecond * m_FatFactor * 0.5f;
                        m_ProteinsDecrease = m_NutritionProteinsConsumptionPerSecond * m_ProteinsFactor;
                    }
                }
                else
                {
                    m_CarboDecrease = 0f;
                    m_FatDecrease = 0f;
                    m_ProteinsDecrease = 0f;
                }

                m_NutritionCarbo -= m_CarboDecrease;
                m_NutritionFat -= m_FatDecrease;
                m_NutritionProteins -= m_ProteinsDecrease;

                m_NutritionCarbo = Mathf.Clamp(this.m_NutritionCarbo, 0f, GetMaxNutritionCarbo());
                m_NutritionFat = Mathf.Clamp(this.m_NutritionFat, 0f, GetMaxNutritionFat());
                m_NutritionProteins = Mathf.Clamp(this.m_NutritionProteins, 0f, GetMaxNutritionProtein());


                CalculateNutritionNeeds();
            } catch(Exception e)
            {
                Log(e.ToString());
            }
        }

        private void CalculateNutritionNeeds()
        {
            float carboIncrease = CalculateCarboNeed();
            float fatIncrease = CalculateFatNeed(carboIncrease);
            CalculateProteinsNeed(carboIncrease, fatIncrease);    
        }

        float CalculateCarboNeed()
        {
            Log("Calculating carbohydrate need...");

            float carboPercentage = GetNutritionPercentage(0);
            float carboIncrease = 0.15f * (1f - carboPercentage) * 100;

            Log("CarboIncrease = " + carboIncrease);

            m_NeedCarbo = (1f - carboPercentage) * 100 + carboIncrease;
            m_NeedCarbo = Mathf.Clamp(this.m_NeedCarbo, 0f, GetMaxCarboNeed());

            Log("m_NutritionCarbo = " + m_NutritionCarbo);
            Log("m_NeedCarbo = " + m_NeedCarbo);

            return carboIncrease;
        }

        float CalculateFatNeed(float carboIncrease)
        {
            Log("Calculating fat need...");

            float fatPercentage = GetNutritionPercentage(1);
            float fatIncrease = 0.15f * (1f - fatPercentage) * 100;

            Log("fatIncrease = " + fatIncrease);

            m_NeedFat = (1f - fatPercentage) * 100 + fatIncrease + carboIncrease * m_fatCarboIncreaseFactor;
            m_NeedFat = Mathf.Clamp(this.m_NeedFat, 0f, GetMaxFatNeed());

            Log("m_NutritionFat = " + m_NutritionFat);
            Log("m_NeedFat = " + m_NeedFat);

            return fatIncrease;
        }

        void CalculateProteinsNeed(float carboIncrease, float fatIncrease)
        {
            Log("Calculating proteins need...");
            float proteinPercentage = GetNutritionPercentage(1);
            float proteinIncrease = 0.15f * (1f - proteinPercentage) * 100;

            Log("proteinIncrease = " + proteinIncrease);

            m_NeedProteins = (1f - proteinPercentage) * 100 + proteinIncrease + fatIncrease * m_proteinFatIncreaseFactor + carboIncrease * m_proteinCarboIncreaseFactor;
            m_NeedProteins = Mathf.Clamp(this.m_NeedProteins, 0f, GetMaxProteinNeed());

            Log("m_NutritionProteins = " + m_NutritionProteins);
            Log("m_NeedProteins = " + m_NeedProteins);
        }

        protected override void UpdateHydration()
        {
            base.UpdateHydration();

            try { 
                float nutritionPercentage = GetNutritionPercentage();

                m_HydrationDecrease = m_HydrationConsumptionPerSecond * nutritionPercentage * m_HydrationFactor;
                m_Hydration -= m_HydrationDecrease;
                m_Hydration = Mathf.Clamp(this.m_Hydration, 0f, GetMaxHydration());

                CalculateHydrationNeed();
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        private void CalculateHydrationNeed()
        {

            Log("Calculating hydration need...");

            float hydrationPercentage = GetHydrationPercentage();
            float nutritionPercentage = GetNutritionPercentage();
            float nutritionIncrease = 0.15f * (nutritionPercentage) * 100;

            Log("NutritionIncrease = " + nutritionIncrease);
            Log("hydrationPercentage = " + hydrationPercentage);

            m_NeedHydration = hydrationPercentage > 0.9f ? (1f - hydrationPercentage) * 100 : (1f - hydrationPercentage) * 100 + nutritionIncrease;
            m_NeedHydration = Mathf.Clamp(this.m_NeedHydration, 0f, GetMaxHydrationNeed());

            Log("m_Hydration = " + m_Hydration);
            Log("m_NeedHydration = " + m_NeedHydration);
        }

        protected override void UpdateMaxHP()
        {
            this.m_MaxHP = this.m_Hydration * 0.25f * this.m_MaxHydrationDecreaseFactor + this.m_NutritionFat * 0.25f + this.m_NutritionCarbo * 0.25f + this.m_NutritionProteins * 0.25f;
            this.m_MaxHP = Mathf.Clamp(this.m_MaxHP, 0f, 100f);
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
            //CJDebug.Log("ModifyNutritionRate: " + log);
            ModAPI.Log.Write("ModifyNutritionRate: " + log);
        }
    }

    public class SleepControllerExtended : SleepController
    {
        protected override void UpdateSleeping()
        {
            CJDebug.Log("UpdateSleeping ");

            base.UpdateSleeping();

            ModifyNutritionRate modifyNutritionRate = (ModifyNutritionRate)ModifyNutritionRate.Get();

            float sleepCarboSpeedFactor = 1f, sleepFatSpeedFactor = 30f, sleepProteinsSpeedFactor = 30f, sleepHydrationSpeedFactor = 0.3f;

            CJDebug.Log("" + modifyNutritionRate.GetProteinsDecrease() * sleepProteinsSpeedFactor);

            modifyNutritionRate.m_NutritionCarbo -= modifyNutritionRate.GetCarboDecrease() * sleepCarboSpeedFactor;
            modifyNutritionRate.m_NutritionCarbo = Mathf.Clamp(modifyNutritionRate.m_NutritionCarbo, 0f, modifyNutritionRate.GetMaxNutritionCarbo());
            modifyNutritionRate.m_NutritionFat -= modifyNutritionRate.GetFatDecrease() * sleepFatSpeedFactor;
            modifyNutritionRate.m_NutritionFat = Mathf.Clamp(modifyNutritionRate.m_NutritionFat, 0f, modifyNutritionRate.GetMaxNutritionFat());
            modifyNutritionRate.m_NutritionProteins -= modifyNutritionRate.GetProteinsDecrease() * sleepProteinsSpeedFactor;
            modifyNutritionRate.m_NutritionProteins = Mathf.Clamp(modifyNutritionRate.m_NutritionProteins, 0f, modifyNutritionRate.GetMaxNutritionProtein());
            //modifyNutritionRate.m_Hydration -= modifyNutritionRate.GetHydrationDecrease() * sleepHydrationSpeedFactor;
            //modifyNutritionRate.m_Hydration = Mathf.Clamp(modifyNutritionRate.m_Hydration, 0f, modifyNutritionRate.GetMaxHydration());
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

    public class RemindersManagerExtended : RemindersManager
    {
        protected override void CheckWater()
        {
            if (Time.time < this.m_NextCheckWaterTime)
            {
                return;
            }
            if (PlayerConditionModule.Get().GetHydration() >= (PlayerConditionModule.Get().GetMaxHydration() / 4f))
            {
                this.m_NextCheckWaterTime = Time.time + 5f;
                return;
            }
            if (DialogsManager.Get().IsAnyDialogPlaying())
            {
                this.m_NextCheckWaterTime = Time.time + 60f;
                return;
            }
            DialogsManager.Get().StartDialog(this.m_LowWaterDialogs[this.m_CheckWaterIndex]);
            if (this.m_CheckWaterIndex == 0)
            {
                HintsManager.Get().ShowHint(this.m_LowWaterHint, 10f);
            }
            this.m_CheckWaterIndex++;
            if (this.m_CheckWaterIndex >= this.m_LowWaterDialogs.Length)
            {
                this.m_CheckWaterIndex = 0;
            }
            this.m_NextCheckWaterTime = Time.time + 120f;
        }
    }
}
