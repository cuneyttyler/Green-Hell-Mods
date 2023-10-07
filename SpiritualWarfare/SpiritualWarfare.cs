using DG.Tweening.Plugins.Core.PathCore;
using System;
using System.Collections.Generic;
using System.IO;
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

        private static AssetBundle ASSET_BUNDLE;
        public static List<TransformData> transformData = new List<TransformData>();
        private List<GameObject> totemObjects = new List<GameObject> ();

        public static SpiritualWarfare Get()
        {
            return SpiritualWarfare.Instance;
        }

        private void Start()
        {
            try
            {
                LoadAssetBundle();
                transformData = FileUtil.ReadTransformData();
                InitializeObjects();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }

        void LoadAssetBundle()
        {
            try
            {
                Logger.Log("Loading asset bundle from path " + ASSET_PATH + ".");
                AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(ASSET_PATH));

                if(request.assetBundle == null)
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
            Logger.Log("Building previously built totems." + transformData.Count + " in total.");
            foreach(TransformData transformData in transformData)
            {
                try { 
                    BuildTotem(CROSS_RESOURCE_ID, transformData, false);
                } catch(Exception e)
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
                if (Input.GetKeyDown(KeyCode.T))
                {
                    BuildTotem(SpiritualWarfare.CROSS_RESOURCE_ID);
                    PlayMusic(CROSS_CHOIR_RESOURCE_ID);
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    RevertLastBuild();
                }
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    PlayMusic(SALVATION_RESOURCE_ID);
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
            if(transform != null)
            {
                Logger.Log("Transform: " + transform.ToString());
            }

            Vector3 position = transform != null ? transform.position : CalculatePosition();
            Quaternion rotation = transform != null ? transform.rotation : Quaternion.Euler(-90, -180, -90);

            ConstructionGhost ghostComponent = prefab.AddComponent<ConstructionGhost>();
            prefab = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);

            Logger.Log("Totem built.");
            transformData.Add(new TransformData(position, rotation, new Vector3()));
            totemObjects.Add(prefab);
        }

        Vector3 CalculatePosition()
        {
            float distanceInFront = 2.0f;
            float heightOffset = 1.0f;

            Vector3 playerPosition = Player.Get().transform.position;
            Vector3 playerForward = Player.Get().transform.forward;

            // Calculate the position in front of the player
            Vector3 spawnPosition = playerPosition + playerForward * distanceInFront + Vector3.up * heightOffset;

            return spawnPosition;
        }

        void RevertLastBuild()
        {
            Logger.Log("Reverting last built totem.");
            if(totemObjects.Count > 0)
            {
                UnityEngine.Object.Destroy(totemObjects[totemObjects.Count - 1]);
                transformData.RemoveAt(transformData.Count - 1);
            }
        }

        void PlayMusic(String path)
        {
            Logger.Log("Loading music - " + path);
            AudioClip audioClip = Load<AudioClip>(path);
            if (!audioClip)
            {
                Logger.LogError("Sound prefab could not be found in the assets - " + path);
                return;
            }
            Logger.Log("Music loaded. Playing...");
            PlayerAudioModule.Get().PlaySound(audioClip);
        }

        static T Load<T>(String path) where T : UnityEngine.Object
        {
            Logger.Log("Load asset " + path);
            return ASSET_BUNDLE.LoadAsset<T>(path);
        }
    }

    public static class FileUtil
    {
        private static string FILE_NAME = "transform_data";

        public static List<TransformData> ReadTransformData()
        {
            List<TransformData> transformData = new List<TransformData>();

            Logger.Log("Loading save " + SaveGame.s_MainSaveName);

            int start = SaveGame.SP_SLOT_NAME.Length;
            int length = SaveGame.s_MainSaveName.LastIndexOf(".") - start;
            string slotId = SaveGame.s_MainSaveName.Substring(start, length);
            for (int k = 0; k < 400; k++)
            {
                string save_file_name = FILE_NAME + "_" + slotId + "_" + (k++) + ".sav";
                if (GreenHellGame.Instance.m_RemoteStorage.FileExistsInRemoteStorage(save_file_name))
                {
                    GreenHellGame.Instance.m_RemoteStorage.FileDelete(save_file_name);
                    string json = System.Text.Encoding.ASCII.GetString(System.Text.Encoding.UTF8.GetBytes(save_file_name));
                    transformData.Add(JsonUtility.FromJson<TransformData>(json));
                }
            }

            return transformData;
        }
        
        public static void writeTransformData(int slotId)
        {
            Logger.Log("Cleaning up old data for slot " + slotId);
            for (int j = 0; j < 4; j++)
            {
                for(int k = 0; k < 400; k++)
                {
                    string save_file_name = FILE_NAME + "_" + j + "_" + (k++) + ".sav";
                    if (GreenHellGame.Instance.m_RemoteStorage.FileExistsInRemoteStorage(save_file_name))
                    {
                        GreenHellGame.Instance.m_RemoteStorage.FileDelete(save_file_name);
                    }
                }
            }

            Logger.Log("Writing transform data for slot " + slotId);
            int i = 0;
            foreach (TransformData data in SpiritualWarfare.transformData)
            {
                string json = JsonUtility.ToJson(data);
                string save_file_name = FILE_NAME + "_" + slotId + "_" + (i++) + ".sav";
                Logger.Log("Saving file " + save_file_name + ".");
                GreenHellGame.Instance.m_RemoteStorage.FileWrite(save_file_name, System.Text.Encoding.ASCII.GetBytes(json));
            }
        }
    }

    public class SaveGameMenuExtended : SaveGameMenu
    {
        protected override void DoSaveGame()
        {
            base.DoSaveGame();
            Logger.Log("Saving totems.");
            try { 
                FileUtil.writeTransformData(this.m_SlotIdx);
            } catch(Exception e)
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
