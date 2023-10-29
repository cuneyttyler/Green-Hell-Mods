using AIs;
using Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HarvestingMod
{
    internal class DeadBodyExtended : DeadBody, IHarvestingTarget
    {
        public new void Harvest()
        {
            base.gameObject.ReplRequestOwnership(false);
            FarmAnimalParams farmAnimalParams = this.m_FarmAnimalParams;
            AIParams aiparams = AIManager.Get().m_AIParamsMap[(int)this.m_AIID];
            List<GameObject> list = aiparams.m_HarvestingResult;
            if (farmAnimalParams != null)
            {
                if (this.m_Maturity < 0.5f)
                {
                    list = farmAnimalParams.m_HarvestingResult0_50;
                }
                else if (this.m_Maturity < 1f)
                {
                    list = farmAnimalParams.m_HarvestingResult50_100;
                }
            }
            foreach (GameObject gameObject in list)
            {
                Item component = gameObject.GetComponent<Item>();
                DebugUtils.Assert(component != null, "[DeadBody:OnExecute] Harvesting result list contains object without Item component - " + gameObject.name, true, DebugUtils.AssertType.Info);
                ItemID itemID = (ItemID)Enum.Parse(typeof(ItemID), component.m_InfoName);
                if (MainLevel.s_GameTime - this.m_StartTime >= aiparams.m_DeadBodyFoodSpoilTime)
                {
                    ItemInfo info = ItemsManager.Get().GetInfo(itemID);
                    if (info.IsFood())
                    {
                        FoodInfo foodInfo = (FoodInfo)info;
                        if (foodInfo.m_SpoilEffectID != ItemID.None)
                        {
                            itemID = foodInfo.m_SpoilEffectID;
                        }
                    }
                }
                for (int i = 0; i < Skill.Get<HarvestingAnimalsSkill>().GetItemsCountMul(); i++)
                {
                    Item item = ItemsManager.Get().CreateItem(itemID, false, base.transform);
                    if (!IsItemBoneOrSkull(itemID))
                    {
                        item.Take(true);
                    }
                        
                    if (itemID == ItemID.Tapir_skull || itemID == ItemID.tapir_skin || itemID == ItemID.Turtle_shell || itemID == ItemID.Armadillo_Shell)
                    {
                        break;
                    }
                }
            }
            if (this.m_AddHarvestingItem != ItemID.None)
            {
                ItemsManager.Get().CreateItem(this.m_AddHarvestingItem, false, base.transform).Take(true);
            }
            Item[] componentsInChildren = base.transform.GetComponentsInChildren<Item>();
            for (int j = 0; j < componentsInChildren.Length; j++)
            {
                componentsInChildren[j].transform.parent = null;
                componentsInChildren[j].StaticPhxRequestRemove();
                componentsInChildren[j].Take(true);
            }
            this.OnHarvest();
            UnityEngine.Object.Destroy(base.gameObject);
            if (!AI.IsHuman(this.m_AIID))
            {
                Skill.Get<HarvestingAnimalsSkill>().OnSkillAction(true);
            }
        }

        bool IsItemBoneOrSkull(ItemID itemID)
        {
            return itemID == ItemID.Tapir_skull || itemID == ItemID.Bone;
        }
    }
}
