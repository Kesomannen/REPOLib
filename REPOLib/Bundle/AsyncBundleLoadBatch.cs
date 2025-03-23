using REPOLib.Objects.Sdk;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOLib.Bundle;

internal class AsyncBundleLoadBatch
{
    private readonly AssetBundleLoader _loader; // Reference to the parent loader

    private readonly List<string> _bundlePaths;
    private readonly Queue<AssetBundle> _processQueue = new();

    private bool _abort;

    public int TotalBundlesToLoad { get; }
    public int BundlesProcessed { get; private set; }

    public bool IsComplete { get; private set; }

    public AsyncBundleLoadBatch(AssetBundleLoader loader, List<string> bundlePaths)
    {
        _loader = loader;
        _bundlePaths = bundlePaths;
        TotalBundlesToLoad = _bundlePaths.Count;
    }

    // Start the batch loading process
    public void StartLoading()
    {
        _loader.StartCoroutine(BundleLoadingLoop());
        _loader.StartCoroutine(AssetLoadingLoop()); // Replace GameObject with your desired T
    }

    // Load bundles and enqueue them as they finish
    private IEnumerator BundleLoadingLoop()
    {
        List<(AssetBundleCreateRequest request, string filePath)> requests = [];

        //Start bundle loads
        foreach (string filePath in _bundlePaths)
        {
            var request = AssetBundle.LoadFromFileAsync(filePath);
            requests.Add((request, filePath));
        }

        // Poll and process bundles as they complete
        while (requests.Count > 0)
        {
            for (int i = requests.Count - 1; i >= 0; i--)
            {
                if (!requests[i].request.isDone) continue;

                //Record end time.
                var bundle = requests[i].request.assetBundle;
                if (bundle != null)
                {
                    _processQueue.Enqueue(bundle);
                }
                else
                {
                    Logger.LogError($"Failed to load bundle from {requests[i].filePath}");
                    BundlesProcessed++;
                }
                requests.RemoveAt(i);
            }

            yield return null;
        }

        Debug.Log("All bundles in batch loaded and enqueued.");
    }

    // Coroutine 2: Process bundles from the queue and load assets asynchronously
    private IEnumerator AssetLoadingLoop()
    {
        while (!_abort)
        {
            yield return null;
            
            if (!_processQueue.TryDequeue(out var bundle)) continue;
            if (bundle == null) continue;

            // Load all assets asynchronously from the bundle
            var assetRequest = bundle.LoadAllAssetsAsync();
            yield return assetRequest;

            Object[] allAssets = assetRequest.allAssets;
            Mod[] mods = allAssets.OfType<Mod>().ToArray();
            
            switch (mods.Length)
            {
                case 0:
                    Logger.LogError($"Bundle contains no mods.");
                    BundlesProcessed++;
                    continue;
                case > 1:
                    Logger.LogError($"Bundle contains more than one mod.");
                    BundlesProcessed++;
                    continue;
            }
            
            var mod = mods[0];
                
            //You can filter for mods and content now
            //Record mod load time here perhaps?
            foreach (var content in allAssets.OfType<Content>())
            {
                content.Initialize(mod);
            }
            
            // Unload bundle after all assets are loaded???
            bundle.Unload(false);

            BundlesProcessed++;

            if (BundlesProcessed >= TotalBundlesToLoad) break;
        }

        IsComplete = true;
        //Record total end time.
        Logger.LogInfo("All assets in batch processed. Batch complete.");
    }

    // Manually stop the batch
    public void Abort()
    {
        _abort = true;
    }
}
