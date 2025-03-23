using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace REPOLib.Bundle;

internal class AssetBundleLoader : MonoBehaviour
{
    private TMP_Text _text;
    private Action _textCleanup;
    private bool _justCreated = true;
    
    private readonly List<AsyncBundleLoadBatch> _batches = [];
    
    private IEnumerator Start()
    {
        yield return null;
        (_text, _textCleanup) = BundleLoader.SetupLoadingUI();
        _justCreated = false;
    }

    // Add a new batch to the loader
    public AsyncBundleLoadBatch AddBatch(List<string> bundleAssetPaths)
    {
        var batch = new AsyncBundleLoadBatch(this, bundleAssetPaths);
        _batches.Add(batch);
        return batch;
    }
 
    // Start all batches
    public void StartAllBatches()
    {
        foreach (var batch in _batches)
        {
            batch.StartLoading();
        }
    }
 
    // Check if all batches are complete
    public bool AreAllBatchesComplete()
    {
        return _batches.All(batch => batch.IsComplete);
    }
 
    // Stop all batches
    public void Abort()
    {
        foreach (var batch in _batches)
        {
            batch.Abort();
        }
    }

    private void Update()
    {
        if (_justCreated) return;
        if (AreAllBatchesComplete())
        {
            enabled = false;
            _textCleanup();
        }
        else
        {
            int total = _batches.Sum(batch => batch.TotalBundlesToLoad);
            int completed = _batches.Sum(batch => batch.BundlesProcessed);
            _text.text = $"REPOLib is loading bundles: {completed}/{total}...";
        }
    }
}
