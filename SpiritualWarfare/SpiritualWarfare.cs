using DG.Tweening.Plugins.Core.PathCore;
using Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace SpiritualWarfare
{
    public class AddMyGameObject : Player
    {
        protected override void Start()
        {
            base.Start();

            Logger.Log("Initializing...");

            if (SpiritualWarfare.Instance == null)
            {
                SpiritualWarfare.Instance = new GameObject("__SpiritualWarfare__").AddComponent<SpiritualWarfare>();
            }
        }
    }

    public class SpiritualWarfare : MonoBehaviour
    {
        public static SpiritualWarfare Instance;
        public static String ASSET_PATH = Application.streamingAssetsPath + "/spiritualwarfare.default";
        public static String CROSS_RESOURCE_ID = "Cross";
        public static String SALVATION_RESOURCE_ID = "Salvation";
        public static String CROSS_CHOIR_RESOURCE_ID = "CrossChoir";
        public static String PATER_NOSTER_RESOURCE_ID = "Jake_Pater_Noster";
        public static String AVE_MARIA_RESOURCE_ID = "Jake_Ave_Maria";
        public static String SIGNUM_CRUCIS_RESOURCE_ID = "Jake_Signum_Crucis";
        public static String GLORIA_PATRI_RESOURCE_ID = "Jake_Gloria_Patri";

        private static AssetBundle ASSET_BUNDLE;
        public static List<TransformData> transformData = new List<TransformData>();
        private List<GameObject> totemObjects = new List<GameObject>();
        public static bool isLastActionDestroy = false;

        public static SpiritualWarfare Get()
        {
            return SpiritualWarfare.Instance;
        }

        private void Start()
        {
            try
            {
                InitData();
                LoadAssetBundle();
                FileUtil.ReadTransformData();
                InitializeObjects();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }

        void InitData()
        {
            transformData = new List<TransformData>();
            totemObjects = new List<GameObject>();
        }

        void LoadAssetBundle()
        {
            try
            {
                Logger.Log("Loading asset bundle from path " + ASSET_PATH + ".");
                AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(ASSET_PATH));

                if (request.assetBundle == null)
                {
                    Logger.LogError("Bundle loaded as null!");
                    return;
                }

                ASSET_BUNDLE = request.assetBundle;
                Logger.Log("Bundle loaded.");
            }
            catch (Exception e)
            {
                Logger.LogError("Exception while loading bundle.");
                Logger.LogError(e.ToString());
            }
        }

        private void InitializeObjects()
        {
            Logger.Log("Initializing objects.");
            Logger.Log("Building previously built totems." + transformData.Count + " in total.");
            foreach (TransformData transformData in transformData)
            {
                try {
                    BuildTotem(CROSS_RESOURCE_ID, transformData, false);
                } catch (Exception e)
                {
                    Logger.LogError("Exception while building totem. " + e.ToString());
                }
            }
            Logger.Log("Build initialisation completed.");
        }

        private void Update()
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.JoystickButton5))
                {
                    Item currentItem = Player.Get().GetCurrentItem(Enums.Hand.Right);
                    if (currentItem.GetInfoID() == Enums.ItemID.Log)
                    {
                        DropAndDestroyLog(currentItem);
                        BuildTotem(SpiritualWarfare.CROSS_RESOURCE_ID);
                        PlayStopSound(CROSS_CHOIR_RESOURCE_ID);

                    }
                }
                if (Input.GetKeyDown(KeyCode.L) || (Input.GetKeyDown(KeyCode.JoystickButton0) && Input.GetKeyDown(KeyCode.JoystickButton3)))
                {
                    RevertLastBuild();
                }
                if (Input.GetKeyDown(KeyCode.Y) || (Input.GetKeyDown(KeyCode.JoystickButton7)) && Input.GetKeyDown(KeyCode.JoystickButton3))
                {
                    PlayStopSound(SALVATION_RESOURCE_ID);
                }
                if ((Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R)) || (Input.GetKeyDown(KeyCode.JoystickButton7) && Input.GetKeyDown(KeyCode.JoystickButton6)))
                {
                    PlayPrayer(PATER_NOSTER_RESOURCE_ID);
                }
                if ((Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.T)) || (Input.GetKeyDown(KeyCode.JoystickButton6) && Input.GetKeyDown(KeyCode.JoystickButton3)))
                {
                    PlayPrayer(AVE_MARIA_RESOURCE_ID);
                }
                if ((Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.V)) || (Input.GetKeyDown(KeyCode.JoystickButton6) && Input.GetKeyDown(KeyCode.JoystickButton0)))
                {
                    PlayPrayer(SIGNUM_CRUCIS_RESOURCE_ID);
                }
                if ((Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.B)) || (Input.GetKeyDown(KeyCode.JoystickButton7) && Input.GetKeyDown(KeyCode.JoystickButton0)))
                {
                    PlayPrayer(GLORIA_PATRI_RESOURCE_ID);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }

        void BuildTotem(String item, TransformData transform = null, bool write = true)
        {
            Logger.Log("Loading prefab " + item + ".");
            GameObject prefab = SpiritualWarfare.Load<GameObject>(item);

            if (prefab == null)
            {
                Logger.Log("Prefab couldn't be loaded.");
                return;
            }

            Logger.Log("Prefab successfully loaded. Building totem.");

            Vector3 position = transform != null ? transform.position : CalculatePosition();

            Quaternion rotation = Player.Get().transform.rotation;
            rotation.eulerAngles = new Vector3(-90, rotation.eulerAngles.y - 90, rotation.eulerAngles.z - 90);
            rotation = transform != null ? transform.rotation : rotation;

            ConstructionGhost ghostComponent = prefab.AddComponent<ConstructionGhost>();
            prefab = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);

            if (write)
            {
                transformData.Add(new TransformData(position, rotation, new Vector3()));
            }

            totemObjects.Add(prefab);

            isLastActionDestroy = false;
        }

        void DropAndDestroyLog(Item log)
        {
            Player.Get().DropItem(log);
            UnityEngine.Object.Destroy(log.gameObject);
        }

        Vector3 CalculatePosition(float heightOffset = 0.63f)
        {
            float distanceInFront = 2.0f;

            Vector3 playerPosition = Player.Get().transform.position;
            Vector3 playerForward = Player.Get().transform.forward;

            // Calculate the position in front of the player
            Vector3 spawnPosition = new Vector3(playerPosition.x, 0, playerPosition.z) + playerForward * distanceInFront + Vector3.up * heightOffset;

            spawnPosition += Vector3.up * MainLevel.GetTerrainY(spawnPosition);

            return spawnPosition;
        }

        void RevertLastBuild()
        {
            if (isLastActionDestroy == true)
            {
                return;
            }
            isLastActionDestroy = true;

            if (totemObjects.Count > 0)
            {
                TransformData data = transformData[transformData.Count - 1];

                UnityEngine.Object.Destroy(totemObjects[totemObjects.Count - 1]);
                transformData.RemoveAt(transformData.Count - 1);

                ItemsManager.Get().CreateItem(ItemID.Log, false, data.position, Quaternion.identity, false);
            }
        }

        void PlayStopSound(String path)
        {
            AudioClip audioClip = LoadAudio(path);
            if (!audioClip)
            {
                return;
            }

            if (PlayerAudioModule.Get().IsSoundPlaying(audioClip))
            {
                PlayerAudioModule.Get().StopSound(audioClip);
                return;
            }

            PlayerAudioModule.Get().PlaySound(audioClip);
        }

        void PlayPrayer(String path)
        {
            if(IsPrayerPlaying())
            {
                StopPrayer();
                StopLookController();
                return;
            }


            SetPrayerTimer(path);
            StartLookController();
            PlayStopSound(path);
        }

        void SetPrayerTimer(String path)
        {
            TimerCallback callback = PrayerTimer;
            Timer timer = new Timer(callback, null, GetPrayerTime(path), Timeout.Infinite);
        }

        void PrayerTimer(object state)
        {
            StopLookController();
        }

        int GetPrayerTime(string prayer)
        {
            int time = 0;

            if (prayer == PATER_NOSTER_RESOURCE_ID)
            {
                time = 33000;
            }
            else if (prayer == AVE_MARIA_RESOURCE_ID)
            {
                time = 24000;
            }
            else if (prayer == SIGNUM_CRUCIS_RESOURCE_ID)
            {
                time = 5000;
            }
            else if (prayer == GLORIA_PATRI_RESOURCE_ID)
            {
                time = 25000;
            }

            return time;
        }

        bool IsPrayerPlaying()
        {
            bool ret = PlayerAudioModule.Get().IsSoundPlaying(LoadAudio(PATER_NOSTER_RESOURCE_ID)) || PlayerAudioModule.Get().IsSoundPlaying(LoadAudio(AVE_MARIA_RESOURCE_ID)) ||
                PlayerAudioModule.Get().IsSoundPlaying(LoadAudio(SIGNUM_CRUCIS_RESOURCE_ID)) || PlayerAudioModule.Get().IsSoundPlaying(LoadAudio(GLORIA_PATRI_RESOURCE_ID));
            return ret;
        }

        void StopPrayer()
        {
            PlayerAudioModule.Get().StopSound(LoadAudio(PATER_NOSTER_RESOURCE_ID));
            PlayerAudioModule.Get().StopSound(LoadAudio(AVE_MARIA_RESOURCE_ID));
            PlayerAudioModule.Get().StopSound(LoadAudio(SIGNUM_CRUCIS_RESOURCE_ID));
            PlayerAudioModule.Get().StopSound(LoadAudio(GLORIA_PATRI_RESOURCE_ID));
        }

        void StartLookController()
        {
            Player.Get().StartController(PlayerControllerType.Look);
        }

        void StopLookController()
        {
            Player.Get().StopController(PlayerControllerType.Look);
        }

        AudioClip LoadAudio(String path)
        {
            AudioClip audioClip = Load<AudioClip>(path);
            if (!audioClip)
            {
                Logger.LogError("Sound prefab could not be found in the assets - " + path);
                return null;
            }

            return audioClip;
        }

        static T Load<T>(String path) where T : UnityEngine.Object
        {
            return ASSET_BUNDLE.LoadAsset<T>(path);
        }
    }

    public static class FileUtil
    {
        private static string FILE_NAME = "transform_data";

        public static void ReadTransformData()
        {
            Logger.Log("Loading save " + SaveGame.s_MainSaveName);

            string folderPath = System.IO.Path.Combine(Application.persistentDataPath, "SpiritualWarfareData");

            int start = SaveGame.SP_SLOT_NAME.Length;
            int length = SaveGame.s_MainSaveName.LastIndexOf(".") - start;
            string slotId = SaveGame.s_MainSaveName.Substring(start, length);
            folderPath = System.IO.Path.Combine(folderPath, slotId);

            string[] files = Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories);
            foreach (string filePath in files)
            {
                string json = File.ReadAllText(filePath);
                SpiritualWarfare.transformData.Add(JsonUtility.FromJson<TransformData>(json));
            }
        }

        public static void writeTransformData(int slotId)
        {
            Logger.Log("Saving data.");

            string folderPath = System.IO.Path.Combine(Application.persistentDataPath, "SpiritualWarfareData");
            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                }
                catch (Exception e)
                {
                    Logger.LogError("Exception when creating directory - " + folderPath);
                }
            }

            string slotFolderPath = System.IO.Path.Combine(folderPath, slotId + "");
            Logger.Log("Removing old slot backup directory and recreating - " + slotFolderPath);
            if (Directory.Exists(slotFolderPath))
            {
                Directory.Delete(slotFolderPath, true);
            }
            Directory.CreateDirectory(slotFolderPath);

            Logger.Log("Writing data to files.");
            int i = 0;
            foreach (TransformData data in SpiritualWarfare.transformData)
            {
                string json = JsonUtility.ToJson(data);
                string filePath = System.IO.Path.Combine(slotFolderPath, "transform_data_" + slotId + "_" + (i++) + ".json");
                File.WriteAllText(filePath, json);
            }
        }
    }

    public class SaveGameMenuExtended : SaveGameMenu
    {
        protected override void DoSaveGame()
        {
            base.DoSaveGame();
            Logger.Log("Saving totems.");
            try
            {
                FileUtil.writeTransformData(this.m_SlotIdx);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }
    }

    [Serializable]
    public class TransformData
    {
        [SerializeField] public Vector3 position;
        [SerializeField] public Quaternion rotation;
        [SerializeField] public Vector3 scale;

        public TransformData(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
            scale = transform.localScale;
        }

        public TransformData(Vector3 _position, Quaternion _rotation, Vector3 _scale)
        {
            position = _position;
            rotation = _rotation;
            scale = _scale;
        }
    }

    public static class Logger
    {
        public static void Log(string log)
        {
            CJDebug.Log("SpiritualWarfare:" + log);
            ModAPI.Log.Write("SpiritualWarfare:" + log);
        }

        public static void LogError(string log)
        {
            CJDebug.Log("SpiritualWarfare:Error:" + log);
            ModAPI.Log.Write("SpiritualWarfare:Error:" + log);
        }
    }
}
