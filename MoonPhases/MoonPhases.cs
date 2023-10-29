using CJTools;
using DG.Tweening;
using DG.Tweening.Plugins.Core.PathCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace MoonPhases
{
    public class TOD_SkyExtended : TOD_Sky
    {
        private static AssetBundle ASSET_BUNDLE;
        private static bool IsBundleLoaded = false;
        private List<Texture2D> MoonTextures;

        public static int MoonCycleDays;

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

            int Year = Cycle.Year;
            int Month = Cycle.Month;
            int Day = Cycle.Day;

            int MoonHalfCycleDays = (int) MoonCycleDays / 2;
            int MoonDay = TimeHelper.CalculateMoonDay(Year, Month, Day);

            int waning = (int)(MoonDay / MoonHalfCycleDays);

            float waningPart = (float)((MoonHalfCycleDays - Mathf.Abs(MoonDay % MoonHalfCycleDays) - 2) * (waning));
            float waxingPart = (float)(Mathf.Abs((MoonDay) % MoonHalfCycleDays) * (1 - waning));
            float MoonLightIntensity = (waxingPart + waningPart) / ((float) (MoonHalfCycleDays - 1));
            MoonLightIntensity = Mathf.Clamp(MoonLightIntensity, 0.01f, 1f);

            if (IsNight)
            {
                Components.LightSource.intensity = Mathf.Lerp(0f, Night.LightIntensity * Night.m_SanityLightIntensityMul, MoonVisibility * MoonLightIntensity);
            }

            int MoonPhase = MoonDay;
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

    public class TimeHelper
    {
        public static int CalculateMoonDay(int Year, int Month, int Day)
        {
            DateTime now = new DateTime(Year, Month, Day);
            DateTime reference = new DateTime(2019, 12, 29);

            TimeSpan diff;

            if(now.CompareTo(reference) < 0)
            {
                diff = reference - now;
                return TOD_SkyExtended.MoonCycleDays - diff.Days % TOD_SkyExtended.MoonCycleDays - 1;
            } else
            {
                diff = now - reference;
                return diff.Days % TOD_SkyExtended.MoonCycleDays - 1;
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