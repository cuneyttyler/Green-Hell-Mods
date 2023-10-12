using AIs;
using Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace NoCentipede
{
    internal class HeavyObjectControllerExtended : HeavyObjectController
    {
        protected override void OnDisable()
        {
            this.m_Animator.SetInteger(this.m_IHeavyObjectState, 0);
            this.SetState(HeavyObjectControllerState.None);
            this.m_DropScheduled = false;
            if (this.m_Player.GetCurrentItem(Enums.Hand.Right))
            {
                this.DropItem(true);
            }
        }

        public override void Drop()
        {
            this.m_DropScheduled = true;
        }

        public override void OnInsertToTrigger(Trigger trigger)
        {
            this.SetState(HeavyObjectControllerState.Leaving);
            this.m_Animator.SetInteger(this.m_IHeavyObjectState, 2);
            this.m_TriggerToInsert = trigger;
        }

        public override void InsertToGhostSlot(GhostSlot slot)
        {
            this.SetState(HeavyObjectControllerState.Leaving);
            this.m_Animator.SetInteger(this.m_IHeavyObjectState, 2);
            this.m_GhostSlot = slot;
        }

        public override void InsertToGhostPart(GhostPart part)
        {
            this.SetState(HeavyObjectControllerState.Leaving);
            this.m_Animator.SetInteger(this.m_IHeavyObjectState, 2);
            this.m_GhostPart = part;
        }

        public override void ControllerUpdate()
        {
            base.ControllerUpdate();
            AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(1);
            if (currentAnimatorStateInfo.shortNameHash == this.m_IHeavyObjectState && !this.m_Animator.IsInTransition(1))
            {
                if (this.m_DropScheduled)
                {
                    this.SetState(HeavyObjectControllerState.Leaving);
                    this.m_Animator.SetInteger(this.m_IHeavyObjectState, 0);
                    this.m_DropScheduled = false;
                }
                else if (this.m_State != HeavyObjectControllerState.Leaving)
                {
                    this.m_Animator.SetInteger(this.m_IHeavyObjectState, 1);
                }
            }
            if (this.m_State == HeavyObjectControllerState.Leaving && ((currentAnimatorStateInfo.shortNameHash == this.m_IPostHeavyObjectState && currentAnimatorStateInfo.normalizedTime >= 0.9f) || (currentAnimatorStateInfo.shortNameHash == this.m_IInsertHeavyObjectToSlotState && currentAnimatorStateInfo.normalizedTime >= 0.9f) || currentAnimatorStateInfo.shortNameHash == this.m_UnarmedIdle))
            {
                Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
                this.DropItem(this.m_GhostSlot == null && this.m_GhostPart == null && this.m_TriggerToInsert == null);
                if (this.m_GhostSlot)
                {
                    if (this.m_GhostSlot.transform.parent)
                    {
                        this.m_GhostSlot.transform.parent.gameObject.ReplRequestOwnership(false);
                        ConstructionGhostChain component = this.m_GhostSlot.transform.parent.gameObject.GetComponent<ConstructionGhostChain>();
                        if (component && component.m_ChainOwner)
                        {
                            component.m_ChainOwner.ReplRequestOwnership();
                        }
                    }
                    this.m_GhostSlot.Fulfill(false);
                    this.m_GhostSlot = null;
                    if (currentItem.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(currentItem.gameObject);
                    }
                }
                if (this.m_GhostPart)
                {
                    this.m_GhostPart.Fulfill(false);
                    this.m_GhostPart = null;
                    if (currentItem.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(currentItem.gameObject);
                    }
                }
                if (this.m_TriggerToInsert)
                {
                    this.m_TriggerToInsert = null;
                    if (currentItem.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(currentItem.gameObject);
                    }
                }
            }
        }

        protected override void DropItem(bool callEvent)
        {
            Item currentItem = this.m_Player.GetCurrentItem(Enums.Hand.Right);
            List<Item> list = new List<Item>();
            HeavyObject heavyObject = currentItem as HeavyObject;
            list.Add(heavyObject);
            if (heavyObject.m_Attached != null && heavyObject.m_Attached.Values.Count > 0)
            {
                list.AddRange((IEnumerable<Item>) heavyObject.m_Attached.Values.ToList<HeavyObject>());
            }
            this.m_Player.DropItem(currentItem, true);
            if (currentItem)
            {
                currentItem.transform.rotation = Player.Get().transform.rotation;
                currentItem.transform.position = this.GetPositionToDropItem();
                if (currentItem.IsAIHeavyObject())
                {
                    this.AllignToTerrain(currentItem);
                }
            }
            Player.Get().OnDropHeavyItem();
            if (!callEvent)
            {
                return;
            }
            if (heavyObject.m_Attached != null && heavyObject.m_Attached.Values.Count > 0)
            {
                list.AddRange((IEnumerable<Item>) heavyObject.m_Attached.Values.ToList<HeavyObject>());
            }
            foreach (Item item in list)
            {
                if (item != null)
                {
                    EventBroker.OnItemDropped.CallEvent(item);
                }
            }
        }

        protected override void SetActivityState(HeavyObjectController.CentipedeOnPlayerActivityState state)
        {
        }

        protected override void OnEnterCentipedeState(HeavyObjectController.CentipedeOnPlayerActivityState state)
        {
        }
        public override void UpdateCentipede()
        {
        }
        public override void CentipedeOnItemAttached(HeavyObject ho)
        {
        }

        public override void CentipedeOnDrop()
        {
        }

        public override void CentipedeOnItemDeattached()
        {
        }

        protected bool ShouldShowCentipede()
        {
            return false;
        }

        // Token: 0x06000743 RID: 1859 RVA: 0x000286F8 File Offset: 0x000268F8
        protected override float GetCentipedeProbability()
        {
            return 0f;
        }

        // Token: 0x06000744 RID: 1860 RVA: 0x00028774 File Offset: 0x00026974
        protected override void ShowCentipede()
        {
        }

        // Token: 0x06000745 RID: 1861 RVA: 0x000287C8 File Offset: 0x000269C8
        protected override void UpdateCentipedeOffset()
        {
        }

        // Token: 0x06000746 RID: 1862 RVA: 0x0002882A File Offset: 0x00026A2A
        protected override void HideCentipede()
        {
        }

        // Token: 0x06000747 RID: 1863 RVA: 0x00028850 File Offset: 0x00026A50
        protected override void SpawnCentipedeAI()
        {
        }

        // Token: 0x06000748 RID: 1864 RVA: 0x000288B8 File Offset: 0x00026AB8
        protected override Vector3 GetPositionToSpawnCentipede()
        {
            return new Vector3(0,0,0);
        }

        // Token: 0x06000749 RID: 1865 RVA: 0x0002891D File Offset: 0x00026B1D
        public override Vector3 GetRandomCentipedeSpawnPosition()
        {
            return new Vector3(0, 0, 0);
        }

        protected override Vector3 GetCentipedeAttachOffset(ItemID ho_id)
        {
            return new Vector3(0, 0, 0);
        }

        // Token: 0x0600074C RID: 1868 RVA: 0x00028A30 File Offset: 0x00026C30
        public override void OnCentipedeDebugInput()
        {
        }
    }

    public class HeavyObjectExtended : HeavyObject
    {
        public override void OnItemAttachedToHand()
        {
            base.OnItemAttachedToHand();
            if (!this.wasPickedUp)
            {
                this.wasPickedUp = true;
            }
            this.m_InPlayersHand = true;
            this.ReplSendAsap();
        }

        public override void OnItemDetachedFromHand()
        {
            this.IsMudbrickInMixer = false;
            base.OnItemDetachedFromHand();
            this.m_InPlayersHand = false;
            this.SetLayer(base.transform, this.m_DefaultLayer);
            this.ReplSendAsap();
        }

        protected override bool CanSpawnCentipede()
        {
            return false;
        }

        public override float GetCentipedeSpawnProbability()
        {
            return 0f;    
        }
    }
}
