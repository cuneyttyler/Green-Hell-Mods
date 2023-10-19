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
    public class TOD_SkyExtended : TOD_Sky
    {
        private static AssetBundle ASSET_BUNDLE;
        private static bool IsBundleLoaded = false;

        private int MoonCycleDays;
        private List<Texture2D> MoonTextures;
        
        protected override void Initialize()
        {
            base.Initialize();

            Logger.Log("Setting up update of Moon and Lights.");

            if (!IsBundleLoaded)
            {
                LoadAssetBundle();
            }

            InitializeMoonCycleDays();
            LoadMoonTextures();
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
                IsBundleLoaded = true;
                Logger.Log("Bundle loaded.");
            }
            catch (Exception e)
            {
                Logger.LogError("Exception while loading bundle.");
                Logger.LogError(e.ToString());
            }
        }

        private void LoadMoonTextures()
        {
            MoonTextures = new List<Texture2D>();

            Logger.Log("Loading Moon Textures.");
            for(int i = 0; i < MoonCycleDays; i++)
            {
                MoonTextures.Add(LoadMoonTexture(i));
            }
            Logger.Log("Moon Textures Loaded.");
        }

        private void InitializeMoonCycleDays()
        {
            MoonCycleDays = 29;
        }

        protected override void UpdateCelestials()
        {
            base.UpdateCelestials();

            int year = Cycle.Year;
            int month = Cycle.Month;
            int day = Cycle.Day;

            int MoonHalfCycleDays = MoonCycleDays / 2;

            int MoonDayOfMonth = day % MoonCycleDays;

            int waning = (int)(MoonDayOfMonth / MoonHalfCycleDays);

            float waxingPart = (float)(Mathf.Abs(MoonDayOfMonth % MoonHalfCycleDays) * (1 - waning));
            float waningPart = (float)((MoonHalfCycleDays - Mathf.Abs(MoonDayOfMonth % MoonHalfCycleDays)) * (waning));
            float MoonLightIntensity = (waxingPart + waningPart) / ((float)MoonHalfCycleDays);
            MoonLightIntensity = Mathf.Clamp(MoonLightIntensity, 0.1f, 1f);

            if (IsNight)
            {
                Components.LightSource.intensity = Mathf.Lerp(0f, Night.LightIntensity * Night.m_SanityLightIntensityMul, MoonVisibility * MoonLightIntensity);
            }

            int MoonPhase = MoonDayOfMonth;
            Texture2D MoonTexture = MoonTextures[MoonPhase];
            Components.MoonRenderer.material.SetTexture("_MainTex", MoonTexture);
        }

        Texture2D LoadMoonTexture(int phase)
        {
            Texture2D MoonTexture = ASSET_BUNDLE.LoadAsset<Texture2D>("Phase " + phase);

            if (MoonTexture == null)
            {
                throw new Exception("Moon texture couldn't be loaded.");
            }

            return MoonTexture;
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