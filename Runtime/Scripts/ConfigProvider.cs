using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Serbull.GameAssets
{
    public class ConfigProvider : MonoBehaviour
    {
        public enum ConfigType
        {
            Localization,
            Rarity,
            Audio,
            PlaytimeGift,
            Roulette,
        }

        private const string LocalDevPath = "Assets/Serbull/Game Assets/Editor/Scriptables/";
        private const string PackagePath = "Packages/com.serbull.gameassets/Editor/Scriptables/";
        private const string CopyTargetPath = "Assets/Resources/";

        public static T LoadConfig<T>(ConfigType configType) where T : ScriptableObject
        {
            var assetName = configType + "Config";
            var assetFullName = assetName + ".asset";
            var localDevPath = LocalDevPath + assetFullName;
            var packagePath = PackagePath + assetFullName;
            var copyTargetPath = CopyTargetPath + assetFullName;

#if UNITY_EDITOR
            if (File.Exists(localDevPath))
            {
                return AssetDatabase.LoadAssetAtPath<T>(localDevPath);
            }

            if (!File.Exists(copyTargetPath) && File.Exists(packagePath))
            {
                File.Copy(packagePath, copyTargetPath);
                AssetDatabase.Refresh();
            }
#endif
            return Resources.Load<T>(assetName);
        }
    }
}
