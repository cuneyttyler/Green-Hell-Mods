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

            Item item;
            bool flag = id != AI.AIID.GoliathBirdEater && id != AI.AIID.Scorpion && id != AI.AIID.CaneToad
                && id != AI.AIID.BrasilianWanderingSpider && id != AI.AIID.Mouse;
            if (flag)
            {
                item = ItemsManager.Get().CreateItem(id.ToString(), false);
            } else
            {
                item = ItemsManager.Get().CreateItem(id.ToString() + "_Body", false);
            }
                
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
            if (flag)
            {
                if (!item.Take(true))
                {
                    Inventory3DManager.Get().DropItem(item);
                }
                AchievementsManager.OnAchivementEvent(AchivementsEvent.TrapHarvested, false);

                if (!this.m_AI.ReplIsOwner())
                {
                    this.m_AI.ReplRequestOwnership(false);
                }
            }

            UnityEngine.Object.Destroy(this.m_AI.gameObject);
            UnityEngine.Object.Destroy(this.GetComponent<AudioClip>());
            this.m_AI = null;
            this.m_PlayGrabAnimOnExecute = false;
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
        public override void Catch()
        {
            int index = -1;
            for (int i = 0; i < m_AIIDs.Count; i++)
            {
                if (m_AIIDs[i] == AI.AIID.CaimanLizard) 
                {
                    index = i;
                }
            }

            if(index != -1)
            {
                Logger.Log("Removing Caiman Lizard from catch list.");
                m_AIIDs.RemoveAt(index);
            }

            if (this.m_AIs.Count > 0)
            {
                return;
            }
            AI.AIID aiid = AI.AIID.None;
            if (!this.m_FishTrap)
            {
                List<AI.AIID> list = new List<AI.AIID>();
                for (int i = 0; i < AIManager.Get().m_Spawners.Count; i++)
                {
                    AISpawner aispawner = AIManager.Get().m_Spawners[i] as AISpawner;
                    if (aispawner && aispawner.enabled)
                    {
                        if (!aispawner.m_Bounds.Contains(base.transform.position))
                        {
                            Vector3 to = aispawner.m_Bounds.ClosestPoint(base.transform.position);
                            if (base.transform.position.Distance(to) > this.m_AdditionalDist)
                            {
                                goto IL_C7;
                            }
                        }
                        if (this.m_AIIDs.Contains(aispawner.m_ID) && DifficultySettings.IsAIIDEnabled(aispawner.m_ID) && aispawner.m_ID != AI.AIID.CaimanLizard)
                        {
                            list.Add(aispawner.m_ID);
                        }
                    }
                IL_C7:;
                }
                if (list.Count > 0)
                {
                    aiid = list[UnityEngine.Random.Range(0, list.Count)];
                }
                else
                {
                    if (UnityEngine.Random.Range(0f, 1f) < this.m_ChanceToCatchOutsideSpawner)
                    {
                        aiid = this.m_AIIDs[UnityEngine.Random.Range(0, this.m_AIIDs.Count)];
                    }
                    if (!DifficultySettings.IsAIIDEnabled(aiid))
                    {
                        aiid = AI.AIID.None;
                    }
                }
            }
            else
            {
                OccurringFishes occurringFishes = this.m_WaterColl ? this.m_WaterColl.GetComponent<OccurringFishes>() : null;
                if (occurringFishes)
                {
                    List<AI.AIID> list2 = new List<AI.AIID>();
                    foreach (AI.AIID aiid2 in this.m_AIIDs)
                    {
                        if (occurringFishes.m_IDs.Contains(aiid2) && DifficultySettings.IsAIIDEnabled(aiid2))
                        {
                            list2.Add(aiid2);
                        }
                    }
                    if (list2.Count > 0)
                    {
                        aiid = list2[UnityEngine.Random.Range(0, list2.Count)];
                    }
                    else
                    {
                        LiquidSource liquidSource = this.m_WaterColl ? this.m_WaterColl.GetComponent<LiquidSource>() : null;
                        if (liquidSource == null || liquidSource.m_LiquidType != LiquidType.PoisonedWater)
                        {
                            if (UnityEngine.Random.Range(0f, 1f) < this.m_ChanceToCatchOutsideSpawner)
                            {
                                aiid = this.m_AIIDs[UnityEngine.Random.Range(0, this.m_AIIDs.Count)];
                            }
                            if (!DifficultySettings.IsAIIDEnabled(aiid))
                            {
                                aiid = AI.AIID.None;
                            }
                        }
                    }
                }
                else
                {
                    aiid = this.m_AIIDs[UnityEngine.Random.Range(0, this.m_AIIDs.Count)];
                    if (!DifficultySettings.IsAIIDEnabled(aiid))
                    {
                        aiid = AI.AIID.None;
                    }
                }
            }
            if (aiid != AI.AIID.None)
            {
                if (this.m_SpecificAIDummies.ContainsKey((int)aiid))
                {
                    for (int j = 0; j < this.m_SpecificAIDummies[(int)aiid].Count; j++)
                    {
                        this.Catch(aiid, j);
                    }
                }
                else
                {
                    this.Catch(aiid, 0);
                }
            }
            else
            {
                if (this.m_Bait && this.m_Bait.m_Item)
                {
                    this.m_Bait.DeleteItem();
                }
                this.SetArmed(false);
                this.m_ChanceToCatchOutsideSpawner += this.m_ChanceToCatchOutsideSpawnerChange;
            }
            base.ReplSetDirty();
        }

        protected void Catch(AI.AIID id, int index)
        {
            if (id != AI.AIID.None && id != AI.AIID.CaimanLizard)
            {
                GameObject prefab;
                bool flag = id != AI.AIID.GoliathBirdEater && id != AI.AIID.Scorpion && id != AI.AIID.CaneToad
                && id != AI.AIID.BrasilianWanderingSpider && id != AI.AIID.Mouse;
                if (flag)
                {
                    Logger.Log("Changing prefab to body.");
                    prefab = GreenHellGame.Instance.GetPrefab(id.ToString() + "_Body");
                } else
                {
                    prefab = GreenHellGame.Instance.GetPrefab(id.ToString());
                    UnityEngine.Object.Destroy(prefab.GetComponent<AudioClip>());
                }

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

        public override void SetupAI(GameObject ai_obj, string ai_name, int index)
        {
            ai_obj.name = ai_name;
            AI component = ai_obj.GetComponent<AI>();
            component.SetTrap(this);
            Behaviour[] components = ai_obj.GetComponents<Behaviour>();
            for (int i = 0; i < components.Length; i++)
            {
                Type type = components[i].GetType();
                if (type != typeof(Transform) && type != typeof(BoxCollider) && type != typeof(Animator) && type != typeof(AI) && !type.IsSubclassOf(typeof(AI)) && type != typeof(SkinnedMeshRenderer) && type != typeof(AnimationEventsReceiver) && type != typeof(GuidComponent) && type != typeof(ReplicationComponent) && type != typeof(Relevance) && type != typeof(AIReplicator) && !type.IsSubclassOf(typeof(AISoundModule)) && type != typeof(AudioSource))
                {
                    if (components[i] is IReplicatedBehaviour)
                    {
                        components[i].enabled = false;
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(components[i]);
                    }
                }

                if(type.IsSubclassOf(typeof(AISoundModule)) || type == typeof(AudioSource))
                {
                    UnityEngine.Object.Destroy(components[i]);
                }
            }

            component.m_BoxCollider.isTrigger = true;
            component.SetTrap(this);
            component.m_TrapDummyIndex = index;
            if ((m_Effect == Effect.Block || m_Info.m_ID == ItemID.Snare_Trap) && (bool)component.m_SoundModule)
            {
                component.m_SoundModule.SetPanicForSnareTrap();
            }

            if ((bool)GreenHellGame.Instance.GetPrefab(component.m_ID.ToString() + "_Body"))
            {
                ai_obj.AddComponent<AIInTrapTrigger>().m_AI = component;
            }
            else
            {
                component.AddDeadBodyComponent();
            }

            Transform transform = null;
            if (!m_SpecificAIDummies.ContainsKey((int)component.m_ID))
            {
                transform = ((!(m_AIDummy != null)) ? base.transform : m_AIDummy);
            }
            else
            {
                if (component.m_TrapDummyIndex >= m_SpecificAIDummies[(int)component.m_ID].Count)
                {
                    component.m_TrapDummyIndex = 0;
                }

                transform = m_SpecificAIDummies[(int)component.m_ID][component.m_TrapDummyIndex];
            }

            ai_obj.transform.position = transform.position;
            ai_obj.transform.rotation = transform.rotation;
            if ((bool)m_WaterColl)
            {
                Vector3 position = ai_obj.transform.position;
                position.y = m_WaterColl.bounds.max.y - component.m_BoxCollider.size.y * 0.5f;
                ai_obj.transform.position = position;
            }

            if (!m_AIs.Contains(component))
            {
                m_AIs.Add(component);
            }

            UpdateEffect();
        }
    }

    public class Logger
    {
        public static void Log(string log)
        {
            CJDebug.Log("SmallTrapDestroyAnimal:" + log);
            ModAPI.Log.Write("SmallTrapDestroyAnimal:" + log);
        }

        public static void LogError(string log)
        {
            CJDebug.Log("SmallTrapDestroyAnimal:Error:" + log);
            ModAPI.Log.Write("SmallTrapDestroyAnimal:Error:" + log);
        }
    }

}