using AIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using UnityEngine;

namespace EnemyPatrolMod
{
    public class AddMyGameObject : Player
    {
        // Create an instance of SaveGameFixer
        protected override void Start()
        {
            base.Start();
            
            if(LogWindow.Instance == null)
            {
                LogWindow.Instance = new GameObject("__LogWindow__").AddComponent<LogWindow>();
            }
        }
    }

    public class HumanAIPatrolExtended : HumanAIPatrol
    {
        bool init;

        public override void Initialize()
        {
            base.Initialize();

            init = true;
        }

        public override bool CanBeActivated()
        {
            // Log("Checking if enemy patrol can be activated.");

            bool flag = false;
            if (CanSpawn(false))
            {
                if (base.m_Members.Count != 0)
                {
                    // Log("Enemy patrol already has members. Being activated.");
                    flag = true;
                }
                else if (base.CanRespawn() && (float)MainLevel.Instance.m_TODSky.Cycle.GameTime - base.m_LastSpawnGameTime > base.GetCalculatedCooldown(base.m_SpawnCooldownHours))
                {
                    // Log("Enemy patrol doesn't have any members. Activating.");
                    flag = true;
                }
                else
                {
                    // Log("Enemy patrol doesn't have any members. Other conditions aren't met. Not activating.");
                    flag = false;
                }
            }
            else
            {
                // Log("Enemy patrol can't be spawned. Not activating.");
                flag = false;
            }

            return flag;
        }

        protected override void OnActivate()
        {
            // Log("ChangeEnemyPatrols activated. Checking if should patrol AI be spawned. (Member count = " + m_Members.Count + ")");
            //LogWindow.Instance.ClearLog();

            AIPathPoint aIPathPoint = GetClosestPathPointInRange(EnemyAISpawnManager.Get().m_MinActivationDistance, EnemyAISpawnManager.Get().m_MaxActivationDistance);
            if (!aIPathPoint)
            {
                float distance = 0f;
                aIPathPoint = base.GetClosestPathPoint(out distance);
            }

            bool canSpawn = this.CanSpawn(true);
            if (base.m_Members.Count == 0 && canSpawn && base.CanRespawn())
            {
                Log("There is no member in the patrol, spawning...");
                base.SpawnAisForGroup();
            }
            else if (base.m_Members.Count > 0 && !canSpawn)
            {
                Log("Already have members, but it shouldn't have been spawned. Removing AI...");
                int i = 0;
                while (i < base.m_Members.Count)
                {
                    HumanAI humanAI = base.m_Members[i];
                    base.RemovedAI(humanAI, false);
                }

                UnityEngine.Object.Destroy(base.gameObject);
            }
            else if (base.m_Members.Count > 0)
            {
                Log("Already have members, not spawning...");
            }

            Vector3 normalized = (aIPathPoint.m_Next.transform.position - aIPathPoint.transform.position).normalized;
            int num = 0;
            while (num < base.m_Members.Count)
            {
                HumanAI humanAI = base.m_Members[num];
                if (humanAI != null)
                {
                    base.InitializeAI(humanAI, aIPathPoint, normalized);
                    num++;
                    continue;
                }

                if (humanAI != null && ReplTools.ReplIsOwner(humanAI))
                {
                    UnityEngine.Object.Destroy(humanAI.gameObject);
                }

                base.m_Members.RemoveAt(num);
            }

            if (base.m_Members.Count > 0)
            {
                base.m_Leader = m_Members[UnityEngine.Random.Range(0, m_Members.Count)];
            }
            else
            {
                base.m_Leader = null;
            }
        }

        public bool IsAllTotemsDestroyed()
        {
            int count = 0;

            if (!base.IsOwner())
            {
                return false;
            }

            foreach (TotemData totemsDatum in base.m_TotemsData)
            {
                if (totemsDatum.m_IsDestroyed)
                {
                    count++;
                }
            }

            return count >= m_TotemsData.Count;
        }

        public int GetDestroyedTotemCount()
        {
            int count = 0;

            if (!base.IsOwner())
            {
                return 0;
            }

            foreach (TotemData totemsDatum in base.m_TotemsData)
            {
                if (totemsDatum.m_IsDestroyed)
                {
                    count++;
                }
            }

            return count;
        }

        private bool CanSpawn(bool debug)
        {
            bool isAllVillagesDestroyed = base.IsAllConnectedVillagesDestroyed();
            bool isAllTotemsDestroyed = this.IsAllTotemsDestroyed();
            bool flag = (base.m_ShouldRespawn || base.m_LastSpawnGameTime == 0f) && (!isAllVillagesDestroyed || !isAllTotemsDestroyed);


            //foreach (TotemData totemsDatum in base.m_TotemsData)
            //{
            //    if (init && debug)
            //    {
                    //Log("Respawning totem. " + totemsDatum.m_Position.position.ToString());
                    //base.RespawnTotem(totemsDatum);
                    //Log("Totem respawned.");
                    //init = false;
            //    }
            //}
            if (debug) { 
                Log("Village count: " + m_ConnectedVillages.Count + ", Destroyed village count: " + GetDestroyedVillageCount());
                Log("Totem count: " + m_TotemsData.Count + ", Destroyed totem count: " + GetDestroyedTotemCount());
                foreach (TotemData totemsDatum in base.m_TotemsData)
                {
                    if(!totemsDatum.m_IsDestroyed)
                        Log("Non-Destroyed totem position: " + totemsDatum.m_Position.position.ToString());
                }
            }

            if (flag && debug)
            {
                if (!isAllVillagesDestroyed)
                {
                    Log("You need to destroy all the villages in the area to make patrols disappear.");
                }
                if (!isAllTotemsDestroyed)
                {
                    Log("You need to destroy all the totems in the area to make patrols disappear.");
                }
            }
            else if (debug)
            {
                Log("Not spawning enemy patrols...");
                if (isAllVillagesDestroyed)
                {
                    Log("All villages in the area are destroyed.");
                }
                if (isAllTotemsDestroyed)
                {
                    Log("All totems in the area are destroyed.");
                }
            }

            return flag;
        }

        public int GetDestroyedVillageCount()
        {
            int num = 0;
            foreach (string connectedVillage in m_ConnectedVillages)
            {
                if ((bool)BadTribeVillageManager.Get() && BadTribeVillageManager.Get().IsVillageDestroyed(connectedVillage))
                {
                    num++;
                }
            }

            return num;
        }

        public override void TryRespawnTotems()
        {
            if (this.CanSpawn(false))
            {
                base.TryRespawnTotems();
            }

            base.TryRespawnTotems();
        }

        private void Log(String log)
        {
            CJDebug.Log("EnemyPatrolMod: " + log);
            ModAPI.Log.Write("EnemyPatrolMod: " + log);
            LogWindow.Instance.WriteLog("EnemyPatrolMod: " + log);
        }
    }

    public class LogWindow : MonoBehaviour
    {
        public static LogWindow Instance;
        private bool visible = false;
        protected GUIStyle labelStyle;
        private String logText;
        private String label;

        private void OnGUI()
        {
            if (visible)
            {
                // toggleCursor(true);

                GUI.skin = ModAPI.Interface.Skin;

                // apply label style if not existing
                if (this.labelStyle == null)
                {
                    this.labelStyle = new GUIStyle(GUI.skin.label);
                    this.labelStyle.fontSize = 12;
                }

                // create box (background)
                GUI.Box(new Rect(10f, 10f, 480f, 550f), "", GUI.skin.window);

                // Label
                GUI.Label(new Rect(20f, 30f, 100f, 20f), "EnemyPatrolMod:Logs", this.labelStyle);
                
                label = GUI.TextArea(new Rect(20f, 60f, 450f, 20f), label, this.labelStyle);

                // Text-input
                logText = GUI.TextArea(new Rect(20f, 90f, 450f, 450f), logText, GUI.skin.textField);

                // Button
                if (GUI.Button(new Rect(20f, 550f, 100f, 20f), "Clear"))
                {
                    logText = "";
                }
            }
            else
            {
                // toggleCursor(false);
            }
        }

        public void UpdateLabel(String position)
        {
            label = position;
        }

        private void toggleCursor(bool enabled)
        {
            CursorManager.Get().ShowCursor(enabled, false);
            Player player = Player.Get();

            //optionally block some other actions while your GUI is open... remove these or comment out
            if (enabled)
            {
                player.BlockMoves();
                player.BlockRotation();
                player.BlockInspection();
            }
            else
            {
                player.UnblockMoves();
                player.UnblockRotation();
                player.UnblockInspection();
            }
        }

        private void Update()
        {
            LogWindow.Instance.UpdateLabel(Player.Get().GetWorldPosition().ToString());

            if (Input.GetKeyDown(KeyCode.F11))
            {
                visible = !visible;
            } 
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                logText = "";
            }
        }

        public void WriteLog(String log)
        {
            logText += "\n" + log;
        }

        public void ClearLog()
        {
            logText = "";
        }
    }
}
