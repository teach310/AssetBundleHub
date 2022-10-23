using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using AssetBundleHub;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AssetBundleHubTests
{
    public class MockDownload : IDownloadAsyncDecorator<IDownloadRequestContext, IDownloadResponseContext>
    {
        public async UniTask<IDownloadResponseContext> DownloadAsync(IDownloadRequestContext context, CancellationToken cancellationToken, Func<IDownloadRequestContext, CancellationToken, UniTask<IDownloadResponseContext>> next)
        {
            await UniTask.DelayFrame(10, cancellationToken: cancellationToken);
            return new DownloadResponseContext();
        }
    }

    public class BundlePullContextFixture : IBundlePullContext
    {
        public AssetBundleList AssetBundleList { get; set; }

        public List<string> AssetBundleNames { get; set; }

        public bool Shuffle { get; set; } = true;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10f);

        public IDownloadAsyncDecorator<IDownloadRequestContext, IDownloadResponseContext>[] DownloadAsyncDecorators { get; set; }

        public Exception Error { get; set; }
        public string baseUrl = "";
        Dictionary<string, float> downloadProgress = new Dictionary<string, float>();
        public List<string> downloadedAssetBundles = new List<string>();

        public string GetDestPath(string assetBundleName)
        {
            return "Test/BundlePullContextFixture/Dest/" + assetBundleName;
        }

        public string GetTempSavePath(string assetBundleName)
        {
            return "Test/BundlePullContextFixture/Temp/" + assetBundleName;
        }

        public string GetURL(string assetBundleName)
        {
            return baseUrl + assetBundleName;
        }

        public void SetDownloadedAssetBundle(string assetBundleName)
        {
            downloadedAssetBundles.Add(assetBundleName);
        }

        public void SetDownloadProgress(string assetBundleName, float progress)
        {
            downloadProgress[assetBundleName] = progress;
        }

        public static List<string> DefaultAssetBundleNames()
        {
            return new List<string>() { "Prefabs001", "Prefabs002", "Prefabs003" };
        }

        public static BundlePullContextFixture Load(params IDownloadAsyncDecorator<IDownloadRequestContext, IDownloadResponseContext>[] decorators)
        {
            var result = new BundlePullContextFixture()
            {
                AssetBundleList = AssetBundleListFixture.Load(),
                AssetBundleNames = DefaultAssetBundleNames()
            };

            result.DownloadAsyncDecorators = decorators.Length != 0 ? decorators : DefaultDecorators();

            return result;
        }

        static IDownloadAsyncDecorator<IDownloadRequestContext, IDownloadResponseContext>[] DefaultDecorators()
        {
            return new IDownloadAsyncDecorator<IDownloadRequestContext, IDownloadResponseContext>[]
            {
                new QueueRequestDecorator(runCapacity: 1), // テスト時には順番に実行したほうがテストがやりやすいためキャパを1にする
                new MockDownload()
            };
        }

        public IEnumerable<string> GetTempAssetBundles()
        {
            throw new NotImplementedException();
        }

        public void ReportBrokenAssetBundle(string assetBundleName)
        {
            throw new NotImplementedException();
        }

        public void SetMergedAssetBundle(string assetBundleName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetMergedAssetBundles()
        {
            throw new NotImplementedException();
        }
    }
}
