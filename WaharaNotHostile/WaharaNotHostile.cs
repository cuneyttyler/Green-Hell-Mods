using AIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaharaNotHostile
{
    public class WaharaNotHostile : Player
    {
        // Create an instance of SaveGameFixer
        protected override void Start()
        {
            base.Start();

            AddReputationForEachVillage();
        }

        void AddReputationForEachVillage()
        {
            Logger.Log("Reputations are being added.");

            BadTribeVillageManager btvm = BadTribeVillageManager.Get();

            Dictionary<string, float> reputationMap = new Dictionary<string, float>();
            foreach (BadTribeVillageManager.BadVillageConfiguration villageConfig in btvm.m_BadVillageConfiguration)
            {
                Logger.Log("Running for " + villageConfig.m_Name + ", " + villageConfig.m_VillageToAddReputationName);
                
                VillageReputationManager villageReputationManager = ReputationSystem.Get().GetVillageByName(villageConfig.m_VillageToAddReputationName);
                if (villageReputationManager)
                {
                    Logger.Log("Construction count: " + btvm.m_AllConstructions[villageConfig.m_Name].Count);
                    if (btvm.m_AllConstructions[villageConfig.m_Name].Count > 0) {
                        Logger.Log("Adding reputation.");

                        villageReputationManager.AddReputation(villageConfig.m_ReputationEvent, new UnityEngine.Vector3(0, 0, 0), false);

                        if (!reputationMap.ContainsKey(villageConfig.m_VillageToAddReputationName))
                        {
                            reputationMap.Add(villageConfig.m_VillageToAddReputationName, ReputationSystem.Get().GetReputationValueForEvent(villageConfig.m_ReputationEvent));
                        }
                        else
                        {
                            float preValue = reputationMap[villageConfig.m_VillageToAddReputationName];
                            reputationMap.Add(villageConfig.m_VillageToAddReputationName, preValue + ReputationSystem.Get().GetReputationValueForEvent(villageConfig.m_ReputationEvent));
                        }
                    }
                }
            }

            Logger.Log("Reputations are added successfully.");

            foreach (KeyValuePair<string, float> pair in reputationMap)
            {
                Logger.Log(pair.Key + " -> " + pair.Value);
            }
        }
    }

    public class HumanAIPatrolExtended : HumanAIPatrol
    {
        protected override void InitializeAI(HumanAI ai, AIPathPoint point, UnityEngine.Vector3 curr_to_next)
        {
            if (ai && ai.m_HostileStateModule) { 
                ai.m_HostileStateModule.m_State = HostileStateModule.State.Calm;
                Logger.Log("AI state set to calm for " + ai.GetName());
            }
        }
    }

    class Logger
    {
        public static void Log(String log)
        {
            CJDebug.Log("WNH: " + log);
            ModAPI.Log.Write( log);
        }
    }
}
