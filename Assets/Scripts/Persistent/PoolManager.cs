using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;

    internal DynamicObjectPool mapObjectPool = new DynamicObjectPool();
    public List<PrewarmData> prewarmDatas = new List<PrewarmData>();

    [SerializeField] Slider loadSlider;
    [SerializeField] private GameObject canvas;

    private void Awake()
    {
        instance = this;
        StartCoroutine(PreWarmAsync(UpdateLoadingProgress));
    }

    private void UpdateLoadingProgress(float percent)
    {
        loadSlider.value = percent;
    }

    public IEnumerator PreWarmAsync(System.Action<float> onProgress)
    {
        int total = prewarmDatas.Sum(p => p.count);
        int done = 0;

        foreach (var prewarm in prewarmDatas)
        {
            for (int i = 0; i < prewarm.count; i++)
            {
                mapObjectPool.PreWarm(prewarm.prefab, 1);
                done++;

                float progress = (float)done / total;
                onProgress?.Invoke(progress);

                if (done % 5 == 0)
                    yield return null;
            }
        }

        onProgress?.Invoke(1f);
        yield return new WaitForSeconds(0.1f);
        LoadSceneManager.Init();
        canvas.SetActive(false);
    }
}



[Serializable]
public class PrewarmData
{
    public GameObject prefab;
    public int count;
}
