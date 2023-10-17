using CJTools;
using DG.Tweening;
using DG.Tweening.Plugins.Core.PathCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoonPhases
{
    public class TOD_ComponentsExtended : TOD_Components
    {

        private static AssetBundle ASSET_BUNDLE;

        private Material OriginalMoonMaterial;
        private Material NewMoonMaterial;

        public override void Initialize()
        {
            base.Initialize();

            //LoadAssetBundle();
            //LoadMoon();
            //UpdateMoonMaterial();
        }

        void LoadAssetBundle()
        {
            try
            {
                String ASSET_PATH = Application.streamingAssetsPath + "/moonphases.default";

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

        void LoadMoonMaterial()
        {
            NewMoonMaterial = ASSET_BUNDLE.LoadAsset<Material>("Phase 24");

            if (NewMoonMaterial == null)
            {
                Logger.LogError("Moon material couldn't be loaded.");
                return;
            }
            else
            {
                Logger.Log("Moon material loaded.");
            }
        }

        void UpdateMoonMaterial()
        {
            if (NewMoonMaterial == null)
            {
                return;
            }

            Logger.Log("Updating moon material.");
            OriginalMoonMaterial = MoonMaterial;
            try
            {
                MoonRenderer.material = MoonRenderer.sharedMaterial = NewMoonMaterial;
                MoonMaterial = NewMoonMaterial;
                Logger.Log("Moon material updated.");
            }
            catch (Exception e)
            {
                Logger.LogError("Exception at loading Moon material:");
                Logger.LogError(e.ToString());
                MoonMaterial = OriginalMoonMaterial;
            }
        }
    }

    public class Logger
    {
        public static void Log(String log)
        {
            CJDebug.Log("MoonPhases:" + log);
            ModAPI.Log.Write("MoonPhases:" + log);
        }

        public static void LogError(String log)
        {
            CJDebug.Log("MoonPhases:Error:" + log);
            ModAPI.Log.Write("MoonPhases:Error:" + log);
        }
    }
}
