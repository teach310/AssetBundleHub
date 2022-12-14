using System;
using UnityEngine;

namespace AssetBundleHub
{
    [System.Serializable]
    public class AssetBundleHubSettings
    {
        static AssetBundleHubSettings instance = null;
        public static AssetBundleHubSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new Exception("AssetBundleHubSettings is not loaded");
                }
                return instance;
            }
        }

        // Initiallize後に入れる想定
        public int timeoutSec = 30;
        public string baseUrl;
        public int parallelCount = 4; // ダウンロード時の並列数
        public bool shuffle = true; // ダウンロード順番をシャッフルさせるかどうか

        public string tempSavePath;
        public string TempSavePath => tempSavePath;
        public string saveDataPath;
        public string SaveDataPath => saveDataPath;
        // TODO: BuiltinAssetのパス

        // AssetBundleListの方針が見えていないのでとりあえず外だししておいている。
        public string assetBundleListUrl;
        public string assetBundleListName;

        public string localAssetBundleTablePath;

        public TimeSpan Timeout => TimeSpan.FromSeconds(timeoutSec);

        static AssetBundleHubSettings EditorSettings()
        {
            var settings = BuildDefault();
            settings.tempSavePath = "Temp/AB";
            settings.saveDataPath = "SaveData/AB";
            settings.localAssetBundleTablePath = "SaveData/AB/LocalAssetBundleTable.json";
            return settings;
        }

        // Default。ABHubのInitialize前なら差し替え可
        // TODO: iosでBackup対象からはずすこと
        // https://light11.hatenadiary.com/entry/2019/10/07/031405
        static AssetBundleHubSettings BuildDefault()
        {
            return new AssetBundleHubSettings()
            {
                tempSavePath = Application.temporaryCachePath + "/AB",
                saveDataPath = Application.persistentDataPath + "/AB",
                localAssetBundleTablePath = Application.persistentDataPath + "/AB/LocalAssetBundleTable.json"
            };
        }

        public static void Load()
        {
            if (instance != null)
            {
                return;
            }
#if UNITY_EDITOR
            instance = EditorSettings();
#else
            instance = BuildDefault();
#endif
        }
    }
}
