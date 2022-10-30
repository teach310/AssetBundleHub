using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using AssetBundleHub.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AssetBundleHub
{
    /// <summary>
    /// gitのLocalRepositoryをイメージしたクラス。
    /// ローカルのAssetBundleや状態を管理する。
    /// AssetBundleListの保持
    /// AssetBundleのロード、キャッシュ
    /// </summary>
    public class AssetBundleLocalRepository : IDownloadAssetBundleInfoStore, IPullAssetBundles
    {
        IUserAssetBundleTable userAssetBundleTable;
        AssetBundleList assetBundleList;
        // key: AssetPath value: assetBundleName
        ReadOnlyDictionary<string, string> assetPathToAssetBundleMap;

        IAssetBundlePath assetBundlePath;
        string assetBundleListPath => assetBundlePath.GetAssetBundleListPath();
        public AssetBundleInfo GetAssetBundleInfo(string assetBundleName) => assetBundleList.Infos[assetBundleName];
        public List<string> GetAllDependencies(string assetBundleName) => assetBundleList.GetAllDependencies(assetBundleName);
        public bool TryGetAssetBundleName(string assetPath, out string assetBundleName) => assetPathToAssetBundleMap.TryGetValue(assetPath, out assetBundleName);

        /// <summary>
        /// AssetBundleListをロード。初期化時に呼ぶ必要がある。
        /// </summary>
        public void LoadAndCacheAssetBundleList()
        {
            assetBundleList = AssetBundleList.LoadFromFile(assetBundleListPath); // TODO AssetBundleListが暗号化されることを考慮
            // Assetのフルパス : AssetBundle名のmapを作ってキャッシュ
            var assetToAssetBundleMap = new Dictionary<string, string>();
            foreach (var kvp in assetBundleList.Infos)
            {
                var assetBundleName = kvp.Key;
                foreach (var assetName in kvp.Value.AssetNames)
                {
                    if (assetToAssetBundleMap.ContainsKey(assetName))
                    {
                        Debug.LogWarning($"AssetBundleに含まれるAssetが重複しています。 Asset {assetName} AB {assetToAssetBundleMap[assetName]}, {assetBundleName}");
                        continue;
                    }
                    assetToAssetBundleMap[assetName] = assetBundleName;
                }
            }

            assetPathToAssetBundleMap = new ReadOnlyDictionary<string, string>(assetToAssetBundleMap);
        }

        public bool ExistsNewRelease(string assetBundleName)
        {
            // Listになければ対象外
            if (!assetBundleList.Infos.TryGetValue(assetBundleName, out AssetBundleInfo assetBundleInfo))
            {
                Debug.LogError($"AssetBundleInfo not found {assetBundleName}");
                return false;
            }

            // ローカルに対象のファイルが存在してなければ新規
            if (!userAssetBundleTable.TryGetHash(assetBundleName, out string userAssetBundleHash))
            {
                return true;
            }

            // ローカルのAssetBundleのバージョンが古かったら新規
            if (assetBundleInfo.Hash != userAssetBundleHash)
            {
                return true;
            }
            return false;
        }

        public async UniTask PullAssetBundles(IList<string> assetBundleNames, Action<ulong> reportDownloadedBytes = null)
        {
            if (assetBundleList == null)
            {
                throw new Exception("assetBundleList is null.");
            }

            var bundlePullContext = new BundlePullContext(AssetBundleHubSettings.Instance);
            bundlePullContext.AssetBundleList = assetBundleList;
            bundlePullContext.AssetBundleNames = assetBundleNames.ToList();
            // TODO: reportDownloadedBytesの実装
            var pullAssetBundles = new PullAssetBundles();
            using var cts = new CancellationTokenSource(); // TODO: より上流から伝搬
            await pullAssetBundles.Run(bundlePullContext, cts.Token);
        }
    }
}