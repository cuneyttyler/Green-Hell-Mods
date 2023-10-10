using AIs;
using CJTools;
using Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SmallTrapDestroyAnimal
{
    public class AIInTrapDestroyTrigger : AIInTrapTrigger
    {
        public override void OnExecute(TriggerAction.TYPE action)
        {
            AI.AIID id = this.m_AI.m_ID;
            Item item = ItemsManager.Get().CreateItem(id.ToString() + "_Body", false);
            if (id == AI.AIID.PoisonDartFrog)
            {
                Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
                Material material = null;
                for (int i = 0; i < componentsDeepChild.Length; i++)
                {
                    material = componentsDeepChild[i].material;
                }
                item.ApplyMaterial(material);
            }
            if(id != AI.AIID.GoliathBirdEater && id != AI.AIID.Scorpion && id != AI.AIID.CaneToad
                && id != AI.AIID.BrasilianWanderingSpider && id != AI.AIID.Mouse)
            {
                if (!item.Take(true))
                {
                    Inventory3DManager.Get().DropItem(item);
                }
                AchievementsManager.OnAchivementEvent(AchivementsEvent.TrapHarvested, false);
            }

            if (!this.m_AI.ReplIsOwner())
            {
                this.m_AI.ReplRequestOwnership(false);
            }
            UnityEngine.Object.Destroy(this.m_AI.gameObject);
            this.m_AI = null;
        }
    }

    public class HUDTriggerExtended : HUDTrigger
    {
        protected override void UpdateActions()
        {
            try
            {
                UpdateActionsInternal();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }
        private void UpdateActionsInternal()
        {
            Trigger trigger = this.GetTrigger();
            if (!trigger || !trigger.CanExecuteActions() || this.IsExpanded())
            {
                for (int i = 0; i < this.m_Actions.Length; i++)
                {
                    this.m_KeyFrames[i].gameObject.SetActive(false);
                    this.m_Keys[i].gameObject.SetActive(false);
                    this.m_Actions[i].gameObject.SetActive(false);
                    this.m_PadIcons[i].gameObject.SetActive(false);
                    if (this.m_MouseRMBIcon[i])
                    {
                        this.m_MouseRMBIcon[i].gameObject.SetActive(false);
                    }
                }
                return;
            }
            this.m_TriggerActions.Clear();
            this.m_ItemSlotToInsert = null;
            if (!this.IsExpanded())
            {
                if (Inventory3DManager.Get().gameObject.activeSelf)
                {
                    Item item = trigger.IsItem() ? ((Item)trigger) : null;
                    if (item)
                    {
                        if (item.m_OnCraftingTable)
                        {
                            this.m_TriggerActions.Add(TriggerAction.TYPE.Remove);
                            if (GreenHellGame.IsPadControllerActive())
                            {
                                this.m_TriggerActions.Add(TriggerAction.TYPE.Pick);
                            }
                        }
                        else
                        {
                            if (GreenHellGame.IsPadControllerActive())
                            {
                                this.m_TriggerActions.Add(TriggerAction.TYPE.Pick);
                                this.TryAddAddToCrafting(item);
                                this.TryAddInsertToSlot(item);
                            }
                            if (item.CanShowExpandMenu())
                            {
                                this.m_TriggerActions.Add(TriggerAction.TYPE.InventoryExpand);
                            }
                        }
                    }
                    else if (trigger.IsShelfSet())
                    {
                        this.m_TriggerActions.Add(TriggerAction.TYPE.InventoryExpand);
                    }
                }
                else
                {
                    trigger.GetActions(this.m_TriggerActions);
                }
            }

            this.m_Pos = Vector3.zero;
            bool flag = GreenHellGame.IsPadControllerActive();
            int num = 0;
            while (num < this.m_TriggerActions.Count && num < 3)
            {
                this.m_KeyFrames[num].gameObject.SetActive(!flag);
                this.m_Keys[num].gameObject.SetActive(!flag);
                this.m_Actions[num].gameObject.SetActive(true);
                this.m_PadIcons[num].gameObject.SetActive(flag);
                if (this.m_MouseRMBIcon[num])
                {
                    this.m_MouseRMBIcon[num].gameObject.SetActive(false);
                }
                this.m_Keys[num].text = string.Empty;
                this.m_Actions[num].text = string.Empty;
                TriggerAction.TYPE type = this.m_TriggerActions[num];
                InputActionData inputActionData = InputsManager.Get().GetInputActionData(type);
                if (inputActionData != null)
                {
                    if (flag)
                    {
                        this.m_PadIcons[num].sprite = inputActionData.m_PadIcon;
                    }
                    else if (inputActionData.m_KeyCode == KeyCode.Mouse1)
                    {
                        if (this.m_MouseRMBIcon[num])
                        {
                            this.m_MouseRMBIcon[num].gameObject.SetActive(true);
                            this.m_KeyFrames[num].gameObject.SetActive(false);
                        }
                        this.m_Keys[num].gameObject.SetActive(false);
                    }
                    else
                    {
                        Text text = this.m_Keys[num];
                        text.text += KeyCodeToString.GetString(inputActionData.m_KeyCode);
                    }
                    if (inputActionData.m_Hold > 0f)
                    {
                        Text text2 = this.m_Actions[num];
                        text2.text = text2.text + GreenHellGame.Instance.GetLocalization().Get("HUD_Trigger_Hold", true) + " ";
                    }
                    SmallStorage smallStorage = trigger.IsUniversalStorage() ? ((SmallStorage)trigger) : null;
                    if (type == TriggerAction.TYPE.Expand && smallStorage != null && (!smallStorage.m_storedItemTypeDefined || smallStorage.m_NumItems == 0))
                    {
                        Text text3 = this.m_Actions[num];
                        text3.text += GreenHellGame.Instance.GetLocalization().Get("HUD_Trigger_Choose_item", true);
                    }
                    else
                    {
                        Text text4 = this.m_Actions[num];
                        text4.text += GreenHellGame.Instance.GetLocalization().Get(TriggerAction.GetText(type), true);
                    }
                }
                else
                {
                    this.m_Actions[num].text = GreenHellGame.Instance.GetLocalization().Get(TriggerAction.GetText(type), true);
                }
                if (num == 0)
                {
                    this.UpdatePos(num);
                }
                else if (this.m_KeyFrameParents[num] != null)
                {
                    this.m_KeyFrameParents[num].position = this.m_Pos;
                    this.UpdatePos(num);
                }
                num++;
            }
            for (int j = this.m_TriggerActions.Count; j < this.m_Actions.Length; j++)
            {
                this.m_KeyFrames[j].gameObject.SetActive(false);
                this.m_Keys[j].gameObject.SetActive(false);
                this.m_Actions[j].gameObject.SetActive(false);
                this.m_PadIcons[j].gameObject.SetActive(false);
                if (this.m_MouseRMBIcon[j] != null)
                {
                    this.m_MouseRMBIcon[j].gameObject.SetActive(false);
                }
            }

            AIInTrapTrigger aiTrigger = trigger is AIInTrapTrigger ? ((AIInTrapTrigger)trigger) : null;

            if ((aiTrigger != null && aiTrigger.m_AI != null) && (aiTrigger.m_AI.m_ID == AI.AIID.GoliathBirdEater || aiTrigger.m_AI.m_ID == AI.AIID.Scorpion || aiTrigger.m_AI.m_ID == AI.AIID.CaneToad
                || aiTrigger.m_AI.m_ID == AI.AIID.BrasilianWanderingSpider || aiTrigger.m_AI.m_ID == AI.AIID.Mouse))
            {
                int num2 = 0;
                while (num2 < this.m_TriggerActions.Count && num2 < 3)
                {
                    if (this.m_TriggerActions[num2] == TriggerAction.TYPE.Take)
                    {
                        this.m_Actions[num2].text = "Destroy";
                    }
                    num2++;
                }
            }
        }
    }

    public class TrapExtended : Trap
    {
        protected void Catch(AI.AIID id, int index)
        {
            if (id != AI.AIID.None && id != AI.AIID.CaimanLizard)
            {
                GameObject prefab = GreenHellGame.Instance.GetPrefab(id.ToString());
                GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
                SetupAI(gameObject, prefab.name, index);
                AIReplicator component = gameObject.GetComponent<AIReplicator>();
                if ((bool)component)
                {
                    IReplicatedBehaviourExtension.ReplSetDirty(component);
                }
            }

            if ((bool)m_Bait && (bool)m_Bait.m_Item)
            {
                m_Bait.DeleteItem();
            }

            SetArmed(set: false);
            m_ChanceToCatchOutsideSpawner = 0f;
            ReplSetDirty();
        }
    }

    public class Logger
    {
        public static void Log(string log)
        {
            CJDebug.Log("SmallTrapDestroyAnimal: " + log);
            ModAPI.Log.Write("SmallTrapDestroyAnimal: " + log);
        }

        public static void LogError(string log)
        {
            CJDebug.Log("SmallTrapDestroyAnimal:Error: " + log);
            ModAPI.Log.Write("SmallTrapDestroyAnimal:Error: " + log);
        }
    }

}