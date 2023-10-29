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

            Logger.Log("v3");
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
        public static List<TransformData> TransformData = new List<TransformData>();
        private List<GameObject> TotemObjects = new List<GameObject>();
        public static bool IsLastActionDestroy = false;

        public static bool PrayerMode;
        private Timer PrayerTimer;

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
            TransformData = new List<TransformData>();
            TotemObjects = new List<GameObject>();
            PrayerMode = false;
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
            Logger.Log("Building previously built totems." + TransformData.Count + " in total.");
            foreach (TransformData transformData in TransformData)
            {
                try
                {
                    BuildTotem(CROSS_RESOURCE_ID, transformData, false, false);
                }
                catch (Exception e)
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
                KeyCode Button_X = KeyHelper.KeyFromPad(KeyHelper.PadButton.Button_X);
                KeyCode Button_Y = KeyHelper.KeyFromPad(KeyHelper.PadButton.Button_Y);
                KeyCode Button_A = KeyHelper.KeyFromPad(KeyHelper.PadButton.Button_A);
                KeyCode Button_B = KeyHelper.KeyFromPad(KeyHelper.PadButton.Button_B);
                KeyCode LB = KeyHelper.KeyFromPad(KeyHelper.PadButton.LB);
                KeyCode RB = KeyHelper.KeyFromPad(KeyHelper.PadButton.RB);

                if (Input.GetKeyDown(Button_Y) || Input.GetKeyDown(Button_A) || Input.GetKeyDown(Button_B))
                {
                    EnterPrayerMode();
                    DisableInventory();
                }

                if (Input.GetKeyUp(Button_Y) || Input.GetKeyUp(Button_A) || Input.GetKeyUp(Button_B))
                {
                    QuitPrayerMode();
                    EnableInventory();
                }

                if (Input.GetKeyDown(Button_X))
                {

                    SetButtonXDownTimer();
                }

                if (Input.GetKeyUp(Button_X))
                {
                    QuitPrayerMode();
                    EnableInventory();
                }

                if (Input.GetKeyUp(RB))
                {
                    DisableInventory();
                }

                if (Input.GetKeyUp(RB))
                {
                    EnableInventory();
                }

                if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(RB))
                {
                    Item currentItem = Player.Get().GetCurrentItem(Enums.Hand.Right);
                    if (currentItem != null && currentItem.GetInfoID() == Enums.ItemID.Log)
                    {
                        DropAndDestroyLog(currentItem);
                        BuildTotem(SpiritualWarfare.CROSS_RESOURCE_ID);
                        PlayStopSound(CROSS_CHOIR_RESOURCE_ID);
                    }
                }
                if (Input.GetKeyDown(KeyCode.L) ||
                    (Input.GetKeyDown(Button_X) && Input.GetKeyDown(Button_Y)))
                {
                    RevertLastBuild();
                }
                if (Input.GetKeyDown(KeyCode.Y) ||
                    (Input.GetKeyDown(RB) && Input.GetKeyDown(LB)))
                {
                    PlayStopSound(SALVATION_RESOURCE_ID);
                }
                if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R)) ||
                    (Input.GetKey(Button_Y) && Input.GetKeyDown(RB)))
                {
                    PlayPrayer(PATER_NOSTER_RESOURCE_ID);
                }
                if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.T)) ||
                    (Input.GetKey(Button_X) && Input.GetKeyDown(RB)))
                {
                    PlayPrayer(AVE_MARIA_RESOURCE_ID);
                }
                if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.V)) ||
                    (Input.GetKey(Button_Y) && Input.GetKeyDown(LB)))
                {
                    PlayPrayer(SIGNUM_CRUCIS_RESOURCE_ID);
                }
                if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.B)) ||
                    (Input.GetKey(Button_X) && Input.GetKeyDown(LB)))
                {
                    PlayPrayer(GLORIA_PATRI_RESOURCE_ID);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }

        void SetButtonXDownTimer()
        {
            TimerCallback callback = ButtonXDownTimerFun;
            new Timer(callback, null, 100, Timeout.Infinite);
        }

        void ButtonXDownTimerFun(object state)
        {
            EnterPrayerMode();
            DisableInventory();
            SetButtonXUpTimer();
        }

        void SetButtonXUpTimer()
        {
            TimerCallback callback = ButtonXUpTimerFun;
            new Timer(callback, null, 100, Timeout.Infinite);
        }

        void ButtonXUpTimerFun(object state)
        {
            QuitPrayerMode();
            EnableInventory();
        }

        void BuildTotem(String item, TransformData transform = null, bool write = true, bool debug = false)
        {
            if (debug)
            {
                Logger.Log("Loading prefab " + item + ".");
            }
            GameObject prefab = SpiritualWarfare.Load<GameObject>(item);

            if (prefab == null)
            {
                Logger.LogError("Prefab couldn't be loaded - " + item);
                return;
            }

            if (debug)
            {
                Logger.Log("Prefab successfully loaded. Building totem.");
            }

            Vector3 position = transform != null ? transform.position : CalculatePosition();

            Quaternion rotation = Player.Get().transform.rotation;
            rotation.eulerAngles = new Vector3(-90, rotation.eulerAngles.y - 90, rotation.eulerAngles.z - 90);
            rotation = transform != null ? transform.rotation : rotation;

            ConstructionGhost ghostComponent = prefab.AddComponent<ConstructionGhost>();
            prefab = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);

            if (write)
            {
                TransformData.Add(new TransformData(position, rotation, new Vector3()));
            }

            TotemObjects.Add(prefab);

            IsLastActionDestroy = false;
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
            if (IsLastActionDestroy == true)
            {
                return;
            }
            IsLastActionDestroy = true;

            if (TotemObjects.Count > 0)
            {
                TransformData data = TransformData[TransformData.Count - 1];

                UnityEngine.Object.Destroy(TotemObjects[TotemObjects.Count - 1]);
                TransformData.RemoveAt(TransformData.Count - 1);

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
            if (IsPrayerPlaying())
            {
                StopPrayer();
                StopLookController();
                QuitPrayerMode();
                DisableTimer();
                return;
            }

            SetPrayerTimer(path);
            StartLookController();
            PlayStopSound(path);
        }

        void SetPrayerTimer(String path)
        {
            TimerCallback callback = PrayerTimerFun;
            PrayerTimer = new Timer(callback, null, GetPrayerTime(path), Timeout.Infinite);
        }

        void PrayerTimerFun(object state)
        {
            StopLookController();
            QuitPrayerMode();
        }

        int GetPrayerTime(string prayer)
        {
            int time = 0;

            if (prayer == PATER_NOSTER_RESOURCE_ID)
            {
                time = 32000;
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

        void DisableTimer()
        {
            if (PrayerTimer != null)
            {
                PrayerTimer.Dispose();
            }
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

        void EnterPrayerMode()
        {
            PrayerMode = true;
        }

        void QuitPrayerMode()
        {
            if (!IsPrayerPlaying())
            {
                PrayerMode = false;
            }
        }

        void EnableInventory()
        {
            Inventory3DManager.Get().enabled = true;
        }

        void DisableInventory()
        {
            Inventory3DManager.Get().enabled = false;
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

    public class PlayerExtended : Player
    {
        public override void StartControllerInternal()
        {
            if (SpiritualWarfare.PrayerMode && m_ControllerToStart == PlayerControllerType.Watch)
            {
                Logger.Log("PrayerMode on. Not starting watch controller");
                m_ControllerToStart = PlayerControllerType.Unknown;
                return;
            }
            else if (m_ControllerToStart == PlayerControllerType.Watch)
            {
                Logger.Log("Starting watch controller.");
            }

            base.StartControllerInternal();
        }
    }

    public class Inventory3DManagerExtended : Inventory3DManager
    {
        public override void Activate()
        {
            if (enabled)
            {
                base.Activate();
            }
        }
    }

    public static class KeyHelper
    {
        public enum PadButton
        {
            None = -1,
            Button_X,
            Button_Y,
            Button_A,
            Button_B,
            LB,
            RB,
            Back,
            Start,
            R3,
            L3,
            LeftStickRot,
            RightStickRot
        }

        public static KeyCode KeyFromPad(PadButton pad_button)
        {
            return KeyFromPad(pad_button, GreenHellGame.IsPadControllerActive() ? InputsManager.Get().m_PadControllerType : InputsManager.PadControllerType.None);
        }

        public static KeyCode KeyFromPad(PadButton pad_button, InputsManager.PadControllerType controller_type)
        {
            if (controller_type == InputsManager.PadControllerType.Ps4)
            {
                switch (pad_button)
                {
                    case PadButton.Button_X: return KeyCode.JoystickButton0;
                    case PadButton.Button_Y: return KeyCode.JoystickButton3;
                    case PadButton.Button_A: return KeyCode.JoystickButton1;
                    case PadButton.Button_B: return KeyCode.JoystickButton2;
                    case PadButton.LB: return KeyCode.JoystickButton4;
                    case PadButton.RB: return KeyCode.JoystickButton5;
                    case PadButton.Back: return KeyCode.JoystickButton8;
                    case PadButton.Start: return KeyCode.JoystickButton9;
                    case PadButton.R3: return KeyCode.JoystickButton11;
                    case PadButton.RightStickRot: return KeyCode.JoystickButton11;
                    case PadButton.L3: return KeyCode.JoystickButton10;
                    case PadButton.LeftStickRot: return KeyCode.JoystickButton10;
                    default: return KeyCode.None;
                }
            }
            else
            {
                switch (pad_button)
                {
                    case PadButton.Button_X: return KeyCode.JoystickButton2;
                    case PadButton.Button_Y: return KeyCode.JoystickButton3;
                    case PadButton.Button_A: return KeyCode.JoystickButton0;
                    case PadButton.Button_B: return KeyCode.JoystickButton1;
                    case PadButton.LB: return KeyCode.JoystickButton4;
                    case PadButton.RB: return KeyCode.JoystickButton5;
                    case PadButton.Back: return KeyCode.JoystickButton6;
                    case PadButton.Start: return KeyCode.JoystickButton7;
                    case PadButton.R3: return KeyCode.JoystickButton9;
                    case PadButton.RightStickRot: return KeyCode.JoystickButton11;
                    case PadButton.L3: return KeyCode.JoystickButton8;
                    case PadButton.LeftStickRot: return KeyCode.JoystickButton10;
                    default: return KeyCode.None;
                }
            }
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
                SpiritualWarfare.TransformData.Add(JsonUtility.FromJson<TransformData>(json));
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
            foreach (TransformData data in SpiritualWarfare.TransformData)
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