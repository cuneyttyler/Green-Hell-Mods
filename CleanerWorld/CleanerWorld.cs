using AIs;
using CJTools;
using Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CleanerWorld
{
    public class BalanceSystem20Extended : BalanceSystem20
    {

    }

    public class FarmExtended : Farm
    {
        protected override void OnTriggerExit(Collider other)
        {
            if (!other.gameObject)
            {
                return;
            }
            ItemReplacer component = other.gameObject.GetComponent<ItemReplacer>();
            Trough component2 = other.gameObject.GetComponent<Trough>();
            if (component2)
            {
                this.RemoveTrough(component2);
                return;
            }
            AI component3 = other.gameObject.GetComponent<AI>();
            if (component3)
            {
                if (component3.ReplicationIsOwner())
                {
                    this.RemoveAI(component3, false, false);
                    this.m_AIsInTrigger.Remove(component3);
                }
                return;
            }
            AIHeavyObject component4 = other.gameObject.GetComponent<AIHeavyObject>();
            if (component4)
            {
                component4.SetFarm(null);
                return;
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject)
            {
                return;
            }
            Trough component = other.gameObject.GetComponent<Trough>();
            if (component)
            {
                this.AddTrough(component);
                return;
            }
            AI component2 = other.gameObject.GetComponent<AI>();
            if (component2 && !this.m_AIsInTrigger.Contains(component2))
            {
                this.m_AIsInTrigger.Add(component2);
                return;
            }
            AIHeavyObject component3 = other.gameObject.GetComponent<AIHeavyObject>();
            if (component3)
            {
                component3.SetFarm(this);
                return;
            }
        }

        protected override void Awake()
        {
            base.m_StaticPhx = true;
            this.m_FarmDoor.m_Farm = this;
            this.m_Troughs.Clear();
            this.m_AIsInTrigger.Clear();
            this.m_AIs.Clear();
        }

        public override void ConstantUpdate()
        {
            this.UpdateAIsInTrigger();
            foreach (AI ai in this.m_AIs)
            {
                if (ai.ReplicationIsOwner() && !(this.m_BoxCollider.ClosestPoint(ai.transform.position) == ai.transform.position) && !ai.IsDead())
                {
                    this.MoveToLastPosInFarm(ai);
                }
            }
            if (!base.ReplIsOwner())
            {
                return;
            }
            this.UpdateAIActivity();
            this.UpdateDebug();
        }
    }

    public class FarmAnimalStatsExtended : FarmAnimalStats
    {
        protected override void UpdatePoison(float delta)
        {
        }
    }


    public class ItemExtended : Item
    {
        public override void OnTake(bool play_sound = true)
        {
            EventsManager.OnEvent(Enums.Event.TakeItem, 1, (int)this.m_Info.m_ID, false);
            this.AddItemsCountMessage(this);
            if (play_sound)
            {
                Player.Get().GetComponent<PlayerAudioModule>().PlayItemSound(this.m_Info);
            }
            if (this.m_Info != null && this.m_Info.IsHeavyObject())
            {
                PlayerAudioModule.Get().PlayHOPickupSound();
            }
            if (this.m_Rigidbody && this.m_Rigidbody.IsSleeping())
            {
                this.m_Rigidbody.WakeUp();
            }
            if (!Inventory3DManager.Get().gameObject.activeSelf && this.m_CurrentSlot == InventoryBackpack.Get().m_EquippedItemSlot && !SwimController.Get().IsActive())
            {
                Player.Get().SetWantedItem(this.m_Info.IsBow() ? Hand.Left : Hand.Right, this, true);
            }
            ItemsManager.Get().OnTaken(this.m_Info.m_ID);
            base.TryRemoveFromFallenObjectsMan();
            this.m_ForceZeroLocalPos = false;
            if (this.m_Acre)
            {
                this.m_Acre.GetComponent<Acre>().OnTake(this);
            }
        }
    }

    public class PlayerConditionModuleExtended : PlayerConditionModule
    {
        protected override void ParseFile(string script_name)
        {
            ScriptParser scriptParser = new ScriptParser();
            scriptParser.Parse("Player/" + script_name, true);
            for (int i = 0; i < scriptParser.GetKeysCount(); i++)
            {
                Key key = scriptParser.GetKey(i);
                if (key.GetName() == "MaxStamina")
                {
                    this.m_MaxStamina = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "Stamina")
                {
                    this.m_Stamina = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "MaxEnergy")
                {
                    this.m_MaxEnergy = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "Energy")
                {
                    this.m_Energy = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "MaxHP")
                {
                    this.m_MaxHP = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "HP")
                {
                    this.m_HP = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "MaxNutritionFat")
                {
                    this.m_MaxNutritionFat = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionFat")
                {
                    this.m_NutritionFat = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "MaxNutritionCarbo")
                {
                    this.m_MaxNutritionCarbo = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionCarbo")
                {
                    this.m_NutritionCarbo = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "MaxNutritionProtein")
                {
                    this.m_MaxNutritionProteins = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionProtein")
                {
                    this.m_NutritionProteins = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "MaxHydration")
                {
                    this.m_MaxHydration = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "Hydration")
                {
                    this.m_Hydration = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "StaminaConsumptionWalkPerSecond")
                {
                    this.m_StaminaConsumptionWalkPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "StaminaConsumptionRunPerSecond")
                {
                    this.m_StaminaConsumptionRunPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "StaminaConsumptionDepletedPerSecond")
                {
                    this.m_StaminaConsumptionDepletedPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "StaminaRegenerationPerSecond")
                {
                    this.m_StaminaRegenerationPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "StaminaDepletedLevel")
                {
                    this.m_StaminaDepletedLevel = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "LowStaminaLevel")
                {
                    this.m_LowStaminaLevel = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "LowStaminaRecoveryLevel")
                {
                    this.m_LowStaminaRecoveryLevel = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "StaminaDecrease")
                {
                    this.m_StaminaDecreaseMap.Add((int)Enum.Parse(typeof(StaminaDecreaseReason), key.GetVariable(0).SValue), key.GetVariable(1).FValue);
                }
                else if (key.GetName() == "EnergyDecrease")
                {
                    this.m_EnergyDecreaseMap.Add((int)Enum.Parse(typeof(EnergyDecreaseReason), key.GetVariable(0).SValue), key.GetVariable(1).FValue);
                }
                else if (key.GetName() == "OxygenConsumptionPerSecond")
                {
                    this.m_OxygenConsumptionPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "EnergyConsumptionPerSecond")
                {
                    this.m_EnergyConsumptionPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "EnergyConsumptionPerSecondNoNutrition")
                {
                    this.m_EnergyConsumptionPerSecondNoNutrition = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "EnergyConsumptionPerSecondFever")
                {
                    this.m_EnergyConsumptionPerSecondFever = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "EnergyConsumptionPerSecondFoodPoison")
                {
                    this.m_EnergyConsumptionPerSecondFoodPoison = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "HealtLossPerSecondNoNutrition")
                {
                    this.m_HealthLossPerSecondNoNutrition = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "HealthLossPerSecondNoHydration")
                {
                    this.m_HealthLossPerSecondNoHydration = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "HealthRecoveryPerDayEasyMode")
                {
                    this.m_HealthRecoveryPerDayEasyMode = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "HealthRecoveryPerDayNormalMode")
                {
                    this.m_HealthRecoveryPerDayNormalMode = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "HealthRecoveryPerDayHardMode")
                {
                    this.m_HealthRecoveryPerDayHardMode = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionCarbohydratesConsumptionPerSecond")
                {
                    this.m_NutritionCarbohydratesConsumptionPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionFatConsumptionPerSecond")
                {
                    this.m_NutritionFatConsumptionPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionProteinsConsumptionPerSecond")
                {
                    this.m_NutritionProteinsConsumptionPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionFatConsumptionMulNoCarbs")
                {
                    this.m_NutritionFatConsumptionMulNoCarbs = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionProteinsConsumptionMulNoCarbs")
                {
                    this.m_NutritionProteinsConsumptionMulNoCarbs = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionCarbohydratesConsumptionRunMul")
                {
                    this.m_NutritionCarbohydratesConsumptionRunMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionFatConsumptionRunMul")
                {
                    this.m_NutritionFatConsumptionRunMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionProteinsConsumptionRunMul")
                {
                    this.m_NutritionProteinsConsumptionRunMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionCarbohydratesConsumptionActionMul")
                {
                    this.m_NutritionCarbohydratesConsumptionActionMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionFatConsumptionActionMul")
                {
                    this.m_NutritionFatConsumptionActionMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionProteinsConsumptionActionMul")
                {
                    this.m_NutritionProteinsConsumptionActionMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionCarbohydratesConsumptionWeightNormalMul")
                {
                    this.m_NutritionCarbohydratesConsumptionWeightNormalMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionFatConsumptionWeightNormalMul")
                {
                    this.m_NutritionFatConsumptionWeightNormalMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionFatConsumptionWeightNormalMul")
                {
                    this.m_NutritionFatConsumptionWeightNormalMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionProteinsConsumptionWeightNormalMul")
                {
                    this.m_NutritionProteinsConsumptionWeightNormalMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionCarbohydratesConsumptionWeightOverloadMul")
                {
                    this.m_NutritionCarbohydratesConsumptionWeightOverloadMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionFatConsumptionWeightOverloadMul")
                {
                    this.m_NutritionFatConsumptionWeightOverloadMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionProteinsConsumptionWeightOverloadMul")
                {
                    this.m_NutritionProteinsConsumptionWeightOverloadMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionCarbohydratesConsumptionWeightCriticalMul")
                {
                    this.m_NutritionCarbohydratesConsumptionWeightCriticalMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionFatConsumptionWeightCriticalMul")
                {
                    this.m_NutritionFatConsumptionWeightCriticalMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "NutritionProteinsConsumptionWeightCriticalMul")
                {
                    this.m_NutritionProteinsConsumptionWeightCriticalMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "HydrationConsumptionPerSecond")
                {
                    this.m_HydrationConsumptionPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "HydrationConsumptionDuringFeverPerSecond")
                {
                    this.m_HydrationConsumptionDuringFeverPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "HydrationConsumptionRunMul")
                {
                    this.m_HydrationConsumptionRunMul = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "HydrationDecreaseJump")
                {
                    this.m_HydrationDecreaseJump = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "EnergyLossDueLackOfNutritionPerSecond")
                {
                    this.m_EnergyLossDueLackOfNutritionPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "EnergyRecoveryDueNutritionPerSecond")
                {
                    this.m_EnergyRecoveryDueNutritionPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "EnergyRecoveryDueHydrationPerSecond")
                {
                    this.m_EnergyRecoveryDueHydrationPerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "HealthLossPerSecondNoOxygen")
                {
                    this.m_HealthLossPerSecondNoOxygen = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "DirtinessIncreasePerSecond")
                {
                    this.m_DirtinessIncreasePerSecond = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "DirtAddChoppingPlants")
                {
                    this.m_DirtAddChoppingPlants = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "DirtAddPlow")
                {
                    this.m_DirtAddPlow = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "DirtAddPickickgUpHeavyObject")
                {
                    this.m_DirtAddPickickgUpHeavyObject = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "DirtAddSleepingOnGround")
                {
                    this.m_DirtAddSleepingOnGround = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "DirtAddUsingMud")
                {
                    this.m_DirtAddUsingMud = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "DirtAddCombat")
                {
                    this.m_DirtAddCombat = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "DirtAddLossConsciousness")
                {
                    this.m_DirtAddLossConsciousness = key.GetVariable(0).FValue;
                }
                else if (key.GetName() == "StaminaConsumptionWalkPerSecond")
                {
                    this.m_StaminaConsumptionWalkPerSecond = key.GetVariable(0).FValue;
                }
            }
        }

        public override void GetDirtinessAdd(GetDirtyReason reason, HeavyObjectInfo item_info = null)
        {
            if (this.m_BlockGettingDirty)
            {
                return;
            }
            switch (reason)
            {
                case GetDirtyReason.ChopPlants:
                    this.m_Dirtiness += this.m_DirtAddChoppingPlants;
                    break;
                case GetDirtyReason.HeavyObject:
                    this.m_Dirtiness += item_info.m_DirtinessOnTake;
                    break;
                case GetDirtyReason.SleepingOnGround:
                    this.m_Dirtiness += this.m_DirtAddSleepingOnGround;
                    break;
                case GetDirtyReason.UsingMud:
                    this.m_Dirtiness += this.m_DirtAddUsingMud;
                    break;
                case GetDirtyReason.Combat:
                    this.m_Dirtiness += this.m_DirtAddCombat;
                    break;
                case GetDirtyReason.LossConsciousness:
                    this.m_Dirtiness += this.m_DirtAddLossConsciousness;
                    break;
                case GetDirtyReason.Plow:
                    this.m_Dirtiness += this.m_DirtAddPlow;
                    break;
            }
            this.m_Dirtiness = Mathf.Clamp(this.m_Dirtiness, 0f, this.m_MaxDirtiness);
        }
    }

    public class TroughExtended : Trough
    {
        protected override void UpdateDirt()
        {
            if (this.m_DirtLevel >= 3)
            {
                return;
            }
            if (this.m_DirtCounter >= (float)this.m_TroughInfo.m_DirtLevelTime)
            {
                this.m_DirtLevel++;
                this.m_DirtLevel = Mathf.Min(3, this.m_DirtLevel);
                this.m_DirtCounter = 0f;
                this.SetupVis();
            }
        }
    }

    public class TroughInfoExtended : TroughInfo
    {
        protected override void LoadParams(Key key)
        {
            if (key.GetName() == "Capacity")
            {
                this.m_Capacity = key.GetVariable(0).FValue;
                return;
            }
            if (key.GetName() == "DirtLevelTime")
            {
                this.m_DirtLevelTime = key.GetVariable(0).IValue;
                return;
            }
        }
    }

    public class AIExtended : AI
    {
        public void TryShit()
        {
            Debug.Log("Don't Try.");
        }
    }

    public class FarmAnimalParamsExtended : FarmAnimalParams
    {
        public override void Load(Key key)
        {
            for (int i = 0; i < key.GetKeysCount(); i++)
            {
                Key key2 = key.GetKey(i);
                if (key2.GetName() == "WaterLevelToDrink")
                {
                    this.m_WaterLevelToDrink = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "FoodLevelToEat")
                {
                    this.m_FoodLevelToEat = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "TrustDecreaseOnHitMe")
                {
                    this.m_TrustDecreaseOnHitMe = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "TrustDecreaseOnHitOther")
                {
                    this.m_TrustDecreaseOnHitOther = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "TrustLevelToRunAway")
                {
                    this.m_TrustLevelToRunAway = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "TimeToCollapse")
                {
                    this.m_TimeToCollapse = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "SleepTime")
                {
                    this.m_SleepTime = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "PregnantCooldown")
                {
                    this.m_PregnantCooldown = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "PregnantDuration")
                {
                    this.m_PregnantDuration = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "MaturationPerSec")
                {
                    this.m_MaturationPerSec = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "DecreaseFoodLevelPerSec")
                {
                    this.m_DecreaseFoodLevelPerSec = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "DecreaseWaterLevelPerSec")
                {
                    this.m_DecreaseWaterLevelPerSec = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "DecreasePoisonLevelPerSec")
                {
                    this.m_DecreasePoisonLevelPerSec = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "DecreaseHealthPerSec")
                {
                    this.m_DecreaseHealthPerSec = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "IncreaseHealthPerSec")
                {
                    this.m_IncreaseHealthPerSec = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "MinFoodToGainTrust")
                {
                    this.m_MinFoodToGainTrust = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "MinWaterToGainTrust")
                {
                    this.m_MinWaterToGainTrust = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "DecreaseTrustPerSec")
                {
                    this.m_DecreaseTrustPerSec = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "IncreaseTrustPerSec")
                {
                    this.m_IncreaseTrustPerSec = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "FoodCapacity")
                {
                    this.m_FoodCapacity = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "WaterCapacity")
                {
                    this.m_WaterCapacity = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "NoTrustDistanceToPlayer")
                {
                    this.m_NoTrustDistanceToPlayer = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "FollowWhistlerDuration")
                {
                    this.m_FollowWhistlerDuration = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "FarmTriggerIconName")
                {
                    this.m_FarmTriggerIconName = key2.GetVariable(0).SValue;
                }
                else if (key2.GetName() == "SleepingTriggerIconName")
                {
                    this.m_SleepingTriggerIconName = key2.GetVariable(0).SValue;
                }
                else if (key2.GetName() == "DurationOfBeingTied")
                {
                    this.m_DurationOfBeingTied = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "MaturityMinScale")
                {
                    this.m_MaturityMinScale = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "MinTrustToWhistle")
                {
                    this.m_MinTrustToWhistle = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "MinTrustToPet")
                {
                    this.m_MinTrustToPet = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "OutsideFarmDecreaseTrustPerSec")
                {
                    this.m_OutsideFarmDecreaseTrustPerSec = key2.GetVariable(0).FValue;
                }
                else if (key2.GetName() == "HarvestingResult0_50")
                {
                    string[] array = key2.GetVariable(0).SValue.Split(';', StringSplitOptions.None);
                    for (int j = 0; j < array.Length; j++)
                    {
                        string[] array2 = array[j].Split('*', StringSplitOptions.None);
                        GameObject prefab = GreenHellGame.Instance.GetPrefab(array2[0]);
                        int num = (array2.Length > 1) ? int.Parse(array2[1]) : 1;
                        for (int k = 0; k < num; k++)
                        {
                            this.m_HarvestingResult0_50.Add(prefab);
                        }
                    }
                }
                else if (key2.GetName() == "HarvestingResult50_100")
                {
                    string[] array3 = key2.GetVariable(0).SValue.Split(';', StringSplitOptions.None);
                    for (int l = 0; l < array3.Length; l++)
                    {
                        string[] array4 = array3[l].Split('*', StringSplitOptions.None);
                        GameObject prefab2 = GreenHellGame.Instance.GetPrefab(array4[0]);
                        if (prefab2)
                        {
                            int num2 = (array4.Length > 1) ? int.Parse(array4[1]) : 1;
                            for (int m = 0; m < num2; m++)
                            {
                                this.m_HarvestingResult50_100.Add(prefab2);
                            }
                        }
                    }
                }
                else if (key2.GetName() == "MinTrustToSetName")
                {
                    this.m_MinTrustToSetName = key2.GetVariable(0).FValue;
                }
            }
        }
    }

    public class GoalShitExtended : GoalShit
    {
        public override bool ShouldPerform()
        {
            return false;
        }

        public override void OnStopAction(AIAction action)
        {
        }

        protected override void OnDeactivate()
        {
        }
    }

    public class GoalsModuleExtended : GoalsModule
    {
        public override void Initialize(Being being)
        {
            if (this.m_ActiveGoal != null)
            {
                this.m_ActiveGoal.Deactivate();
            }
            this.m_Goals.Clear();
            if (!AIManager.Get().m_GoalParsers.ContainsKey((int)this.m_AI.m_ID))
            {
                DebugUtils.Assert("[GoalsModule:Initialize] ERROR, missing goals parser of ai " + this.m_AI.m_ID.ToString(), true, DebugUtils.AssertType.Info);
                return;
            }
            TextAssetParser textAssetParser = null;
            if (this.m_AI.m_PresetName != string.Empty)
            {
                textAssetParser = AIManager.Get().GetGoalParser(this.m_AI.m_PresetName);
            }
            if (textAssetParser == null)
            {
                textAssetParser = AIManager.Get().GetRandomGoalsParser(this.m_AI.m_ID);
            }
            int num = 0;
            for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
            {
                Key key = textAssetParser.GetKey(i);
                if (key.GetName() == "Goal")
                {
                    AIGoal aigoal = this.CreateGoal(key.GetVariable(0).SValue);
                    aigoal.m_Priority = num;
                    if (key.GetVariablesCount() > 1)
                    {
                        aigoal.SetProbability(key.GetVariable(1).FValue);
                    }
                    else
                    {
                        aigoal.SetProbability(1f);
                        num++;
                    }
                    if (aigoal.m_Type == AIGoalType.HumanJumpBack || aigoal.m_Type == AIGoalType.JumpBack)
                    {
                        this.m_JumpBackGoal = aigoal;
                    }
                    else if (aigoal.m_Type == AIGoalType.HumanPunchBack || aigoal.m_Type == AIGoalType.PunchBack)
                    {
                        this.m_PunchBackGoal = aigoal;
                    }
                    else if (aigoal.m_Type == AIGoalType.HumanTaunt)
                    {
                        this.m_TauntGoal = aigoal;
                    }
                    else if (aigoal.m_Type == AIGoalType.FollowWhistler)
                    {
                        this.m_FollowWhistlerGoal = (GoalFollowWhistler)aigoal;
                    }
                    this.m_Goals.Add(aigoal);
                }
                else
                {
                    DebugUtils.Assert("[GoalsModule::Initialize] Unknown keyword - " + key.GetName(), true, DebugUtils.AssertType.Info);
                }
            }
            if (this.m_GoalToActivate != AIGoalType.None)
            {
                this.ActivateGoal(this.m_GoalToActivate);
                this.m_GoalToActivate = AIGoalType.None;
            }
        }

        public override void OnUpdate()
        {
            if (this.m_ActiveGoal != null && !this.m_ActiveGoal.ShouldPerform())
            {
                this.m_ActiveGoal.Deactivate();
            }
            this.SetupActiveGoal();
            if (this.m_ActiveGoal != null)
            {
                this.m_ActiveGoal.OnUpdate();
            }
        }
    }

    public class Logger
    {
        public static void Log(String log)
        {
            ModAPI.Log.Write(log);
            CJDebug.Log(log);
        }
    }
}
