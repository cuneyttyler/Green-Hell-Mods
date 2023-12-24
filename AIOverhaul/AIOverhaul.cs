using AIs;
using Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
    public class AIOverhaul
    {
        public static bool IsPlayerAttacking = false;
        public static int CustomGoalTriggerCount = 0;
        public static float LastCustomGoalTime = float.MaxValue;
        public static float LastCustomReplaceGoalTime = float.MaxValue;
    }

    public class WeaponControllerExtended : WeaponController {
        protected override void SetState(WeaponControllerState state)
        {
            base.SetState(state);

            if(state == WeaponControllerState.Swing)
            {
                AIOverhaul.IsPlayerAttacking = true;
            } else
            {
                AIOverhaul.IsPlayerAttacking = false;
            }
        }
    }

    public class WeaponSpearControllerExtended : WeaponSpearController
    {

        protected override void OnEnterState()
        {
            base.OnEnterState();


            if (IsAttack() || IsThrowAim())
            {
                AIOverhaul.IsPlayerAttacking = true;
            }
            else
            {
                AIOverhaul.IsPlayerAttacking = false;
            }
        }
    }

    public class BowControllerExtended : BowController
    {
        protected override void OnEnterState()
        {
            base.OnEnterState();

            if(m_State == State.AimLoop || m_State == State.Shot)
            {
                AIOverhaul.IsPlayerAttacking = true;
            } else
            {
                AIOverhaul.IsPlayerAttacking = false;
            }
        }
    }

    public class GoalsModuleExtended : GoalsModule
    {
        public override void ActivateGoal(AIGoalType type)
        {
            if (this.m_Goals.Count == 0)
            {
                this.m_GoalToActivate = type;
                return;
            }

            if (type == AIGoalType.MoveAwayFromEnemy)
            {
                Logger.Log("GoalsModule: Activating MoveAway Goal.");
                this.ActivateGoal(this.CreateGoal("MoveAwayFromEnemy"));
                return;
            }

            if (type == AIGoalType.HumanMoveToEnemy && m_AI is HumanAI)
            {
                Logger.Log("GoalsModule: Activating HumanMoveToEnemy Goal.");
                this.ActivateGoal(this.CreateGoal("HumanMoveToEnemy"));
                return;
            }

            for (int i = 0; i < this.m_Goals.Count; i++)
            {
                if (this.m_Goals[i].m_Type == type)
                {
                    this.ActivateGoal(this.m_Goals[i]);
                    return;
                }
            }
        }
    }

    public class GoalFlankExtended : GoalFlank
    {
        protected override void Prepare()
        {
            this.m_AI.m_MoveStyle = AIMoveStyle.Run;
            this.m_Strafe.SetupParams(UnityEngine.Random.Range(4f, 6f), (UnityEngine.Random.Range(0, 2) == 0) ? Direction.Left : Direction.Right);
            base.AddToPlan(this.m_Strafe);
            this.m_AI.m_PathModule.CalcPath(PathModule.PathType.Flank);
        }
    }

    public class GoalHumanThrowerMoveToEnemyExtended : GoalHumanThrowerMoveToEnemy
    {
        protected override void Prepare()
        {
            this.m_LastEnemyPos = this.m_AI.m_EnemyModule.m_Enemy.transform.position;
            this.m_AI.m_MoveStyle = ((this.m_HumanAI.GetState() == HumanAI.State.Attack || this.m_HumanAI.GetState() == HumanAI.State.StartWave) ? AIMoveStyle.Run : AIMoveStyle.Walk);
            this.Setup();
            base.StartAction(this.m_HumanMoveTo);
        }
    }
    public class GoalHumanMoveToEnemyExtended : GoalHumanMoveToEnemy
    {
        protected override void Prepare()
        {
            if (!this.m_AI.m_EnemyModule.m_Enemy)
            {
                base.Deactivate();
                return;
            }
            this.m_LastEnemyPos = this.m_AI.m_EnemyModule.m_Enemy.transform.position;
            this.m_AI.m_MoveStyle = (((this.m_HumanAI.GetState() == HumanAI.State.Attack) || this.m_HumanAI.GetState() == HumanAI.State.StartWave) ? AIMoveStyle.Run : AIMoveStyle.Walk);
            this.Setup();
            base.StartAction(this.m_HumanMoveTo);
        }
    }

    public class GoalMoveAwayFromEnemyExtended : GoalMoveAwayFromEnemy
    {
        public override void OnStopAction(AIAction action)
        {
            
        }

        public override AIMoveStyle GetWantedMoveStyle()
        {
            return AIMoveStyle.Run;
        }
    }

    public class PathModuleExtended : PathModule
    {
        private int m_MaxIteration;
        private int m_MaxDistance;
        private int m_ChangeTimeInterval;
        private int iteration;

        private void Init()
        {
            this.m_MaxIteration = 5;
            this.m_MaxDistance = 2;
            this.m_ChangeTimeInterval = 2;
            this.iteration = 0;
        }

        public override bool CalcPath(PathType path_type)
        {
            Init();

            if (path_type == PathType.MoveAwayFromEnemy)
            {
                //Logger.Log("PathModule: CalcPath");
                SetupPath();
            } else
            {
                base.CalcPath(path_type);
            }

            return m_PathValid;
        }

        void SetupPath()
        {
            //Logger.Log("PathModule: Running iteration " + iteration);

            Vector3 distanceVector = (this.m_AI.transform.position - this.m_AI.m_EnemyModule.m_Enemy.transform.position);
            Vector3 temp;

            Vector3 vector;
            if (UnityEngine.Random.Range(1f, 10f) > 5)
            {
                temp = vector = Vector3.Cross(distanceVector, Vector3.up);
                vector -= distanceVector / 2;
            }
            else
            {
                temp = vector = Vector3.Cross(Vector3.up, distanceVector);
                vector -= distanceVector / 2;
            }

            vector += this.m_AI.m_EnemyModule.m_Enemy.transform.position;

            //Logger.Log("PathModule: AI Position: " + this.m_AI.transform.position);
            //Logger.Log("PathModule: Player Position: " + this.m_AI.m_EnemyModule.m_Enemy.transform.position);
            //Logger.Log("PathModule: Distance Vector: " + distanceVector);
            //Logger.Log("PathModule: Cross Product: " + temp);
            //Logger.Log("PathModule: Result: " + vector);

            for (int i = 0; i < 10; i++)
            {
                vector.y = MainLevel.GetTerrainY(vector);
                UnityEngine.AI.NavMeshHit navMeshHit;
                if (UnityEngine.AI.NavMesh.SamplePosition(vector, out navMeshHit, 1f, AIManager.s_HumanWalkableAreaMask) && UnityEngine.AI.NavMesh.CalculatePath(this.m_AI.m_PathModule.m_Agent.nextPosition, navMeshHit.position, AIManager.s_HumanWalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                {
                    //Logger.Log("PathModule: Path found.");

                    m_Agent.ResetPath();
                    m_Agent.SetPath(m_TempPath);
                    m_PathValid = true;
                    m_CurrPathType = PathType.MoveAwayFromEnemy;

                    if (distanceVector.magnitude > this.m_MaxDistance && ++this.iteration < this.m_MaxIteration)
                    {
                        Thread.Sleep(this.m_ChangeTimeInterval);
                        this.SetupPath();
                    }
                    break;
                }
            }
        }
    }

    public class AIGoalExtended : AIGoal
    {
        protected override void OnActivate()
        {

            if(AIOverhaul.IsPlayerAttacking && (IsHumanGoal() || IsAttackGoal()) && Mathf.Abs(Time.time - AIOverhaul.LastCustomGoalTime) > 2)
            {   
                if (m_AI.transform.position.Distance(m_AI.m_EnemyModule.m_Enemy.transform.position) > 1)
                {
                    Logger.Log("AIGoal:OnActivate: Activating MoveAway Goal.");
                    AIOverhaul.CustomGoalTriggerCount++;
                    AIOverhaul.LastCustomGoalTime = Time.time;
                    m_AI.m_GoalsModule.ActivateGoal(AIGoalType.MoveAwayFromEnemy);
                    return;
                }
            }
            else if(!AIOverhaul.IsPlayerAttacking && AIOverhaul.CustomGoalTriggerCount > 0 && Math.Abs(Time.time - AIOverhaul.LastCustomReplaceGoalTime) > 1)
            {
                Logger.Log("AIGoal:OnActivate: Activating HumanMoveToEnemy Goal.");
                AIOverhaul.CustomGoalTriggerCount--;
                AIOverhaul.LastCustomReplaceGoalTime = Time.time;
                Deactivate();
                m_AI.m_GoalsModule.ActivateGoal(AIGoalType.HumanMoveToEnemy);

                return;
            } 
            else if(IsTauntGoal() && UnityEngine.Random.Range(1f,10f) != 1)
            {
                m_AI.m_GoalsModule.ActivateGoal(AIGoalType.Attack);

                return;
            }

            base.OnActivate();
        }

        private bool IsHumanGoal()
        {
            return GetType().IsSubclassOf(typeof(GoalHuman));
        }

        private bool IsAttackGoal()
        {
            return GetType().IsSubclassOf(typeof(GoalAttack)) || GetType().IsSubclassOf(typeof(GoalHumanAttack));
        }

        private bool IsTauntGoal()
        {
            return GetType() == typeof(GoalHumanTaunt) || GetType() == typeof(GoalSpearmanTaunt);
        }

        private bool IsEligibleForReplacing()
        {
            return GetType() == typeof(GoalMoveAwayFromEnemy);
        }

        private bool IsEligibleForReplacingOld()
        {
            return GetType() != typeof(GoalSpearManMoveAroundEnemy) && GetType() != typeof(GoalStrafe)
                && GetType() != typeof(GoalThugCharge) && GetType() != typeof(GoalRotateToEnemy)
                && GetType() != typeof(GoalPrepareToAttack) && GetType() != typeof(GoalIdle)
                && GetType() != typeof(GoalLoiter) && GetType() != typeof(GoalLoiterInFarm)
                && !GetType().IsSubclassOf(typeof(GoalHunter)) && GetType() != typeof(GoalHumanEmergency)
                && GetType() != typeof(GoalHumanFollowPatrolPath) && GetType() != typeof(GoalHumanHitReaction)
                && GetType() != typeof(GoalHumanMoveToConstruction) && GetType() != typeof(GoalHumanJumpAttack)
                && GetType() != typeof(GoalHumanJumpBack) && GetType() != typeof(GoalHumanRest)
                && GetType() != typeof(GoalHumanUpset);
        }
    }

    public class PlayerExtended : Player
    {
        public override bool TakeDamage(DamageInfo info)
        {
            if (!info.m_FromInjury)
            {
                Item rightItem = GetCurrentItem(Hand.Right);
                Item leftItem = GetCurrentItem(Hand.Left);

                if (rightItem && shouldDrop(rightItem))
                {
                    DropItem(GetCurrentItem(Hand.Right));
                }

                if (leftItem && shouldDrop(leftItem))
                {
                    DropItem(GetCurrentItem(Hand.Left));
                }
            }

            return base.TakeDamage(info);
        }

        private bool shouldDrop(Item item)
        {
            return item && ((item.m_Info.IsKnife() && UnityEngine.Random.Range(0, 0.99f) >= 1 - calculateChance("Blade"))
                || (item.m_Info.IsSpear() && UnityEngine.Random.Range(0, 0.99f) >= 1 - calculateChance("Spear"))
                || (item.m_Info.IsAxe() && UnityEngine.Random.Range(0, 0.99f) >= 1 - calculateChance("Axe"))
                || (item.m_Info.IsMachete() && UnityEngine.Random.Range(0, 0.99f) >= 1 - calculateChance("Machete"))
                || (item.m_Info.IsBow() && UnityEngine.Random.Range(0, 0.99f) >= 1 - calculateChance("Archery"))
                || (item.m_Info.IsTwoHanded() && UnityEngine.Random.Range(0, 0.99f) >= 1 - calculateChance("TwoHanded"))
                || (item.m_Info.IsBlowpipe() && UnityEngine.Random.Range(0, 0.99f) >= 1 - calculateChance("Blowgun"))
                || (!isWeapon(item) && UnityEngine.Random.Range(0,1f) > 0.5f));
        }

        private float calculateChance(String skill)
        {
            if (SkillsManager.Get().SkillGreaterOrEqual(skill, 100))
                return 0;
            else if (SkillsManager.Get().SkillGreaterOrEqual(skill, 90))
                return 0.1f;
            else if (SkillsManager.Get().SkillGreaterOrEqual(skill, 80))
                return 0.2f;
            else if (SkillsManager.Get().SkillGreaterOrEqual(skill, 70))
                return 0.3f;
            else if (SkillsManager.Get().SkillGreaterOrEqual(skill, 60))
                return 0.4f;
            else if (SkillsManager.Get().SkillGreaterOrEqual(skill, 50))
                return 0.5f;
            else if (SkillsManager.Get().SkillGreaterOrEqual(skill, 40))
                return 0.6f;
            else if (SkillsManager.Get().SkillGreaterOrEqual(skill, 30))
                return 0.7f;
            else if (SkillsManager.Get().SkillGreaterOrEqual(skill, 20))
                return 0.75f;
            else if (SkillsManager.Get().SkillGreaterOrEqual(skill, 10))
                return 0.8f;
            else
                return 0.85f;
        }

        private bool isWeapon(Item item)
        {
            return item.m_Info.IsKnife() || item.m_Info.IsSpear() || item.m_Info.IsAxe()
                || item.m_Info.IsMachete() || item.m_Info.IsBow() || item.m_Info.IsTwoHanded()
                || item.m_Info.IsBlowpipe();
        }
    }

    public class Logger
    {
        public static void Log(String log)
        {
            CJDebug.Log("AIOverhaul: " +  log);
            ModAPI.Log.Write(log);
        }
    }
}
