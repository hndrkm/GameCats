using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class RuntimeSettings
    {

        public const string KEY_MUSIC_VOLUME = "MusicVolume";
        public const string KEY_EFFECTS_VOLUME = "EffectsVolume";
        public const string KEY_LIMIT_FPS = "LimitFPS";
        public const string KEY_TARGET_FPS = "TargetFPS";
        public const string KEY_REGION = "Region";
        public const string KEY_SENSITIVITY = "Sensitivity";
        public const string KEY_AIM_SENSITIVITY = "AimSensitivity";
        public const string KEY_VSYNC = "VSync";

        public Options Options => _options;

        public float MusicVolume { get { return _options.GetFloat(KEY_MUSIC_VOLUME); } set { _options.Set(KEY_MUSIC_VOLUME, value, false); } }
        public float EffectsVolume { get { return _options.GetFloat(KEY_EFFECTS_VOLUME); } set { _options.Set(KEY_EFFECTS_VOLUME, value, false); } }
        public bool VSync { get { return _options.GetBool(KEY_VSYNC); } set { _options.Set(KEY_VSYNC, value, false); } }
        public bool LimitFPS { get { return _options.GetBool(KEY_LIMIT_FPS); } set { _options.Set(KEY_LIMIT_FPS, value, false); } }
        public int TargetFPS { get { return _options.GetInt(KEY_TARGET_FPS); } set { _options.Set(KEY_TARGET_FPS, value, false); } }
        public float Sensitivity { get { return _options.GetFloat(KEY_SENSITIVITY); } set { _options.Set(KEY_SENSITIVITY, value, false); } }
        public float AimSensitivity { get { return _options.GetFloat(KEY_AIM_SENSITIVITY); } set { _options.Set(KEY_AIM_SENSITIVITY, value, false); } }

        public string Region { get { return _options.GetString(KEY_REGION); } set { _options.Set(KEY_REGION, value, true); } }

        private Options _options = new Options();


        public void Initialize(GlobalSettings settings)
        {
            _options.Initialize(settings.DefaultOptions, true, "Options.V3.");

            QualitySettings.vSyncCount = VSync == true ? 1 : 0;
            Application.targetFrameRate = LimitFPS == true ? TargetFPS : -1;

            _options.SaveChanges();
        }
    }
}
