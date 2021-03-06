using UnityEngine;
using System;
using Engine.Asset;

//气泡实体，加分执行
public class DanceGrid
{
    private string assetPath = "Gaming/DanceItem";
    private BaseAsset curAsset = null;
    private DanceItem danceItem = null;
    public Action<ItemDanceState> AddScoreAction;
    public void Born(Vector2 pos, Transform parent)
    {
        curAsset = AssetService.GetInstance().LoadInstantiateAsset(assetPath, LifeType.Manual);
        curAsset.RootTf.SetParent(parent);
        curAsset.RootTf.localPosition = pos;
        curAsset.RootTf.localScale = Vector3.one * 1.5f;
        curAsset.RootGo.SetActive(true);

        danceItem = curAsset.RootGo.GetComponent<DanceItem>();
        danceItem.OnClickAction = Destroy;
        danceItem.OnAddScoreAction = AddScoreAction;
    }

    public void Destroy()
    {
        if (curAsset != null)
        {
            AssetService.GetInstance().Unload(curAsset);
            curAsset = null;
            danceItem = null;
        }
    }
}
