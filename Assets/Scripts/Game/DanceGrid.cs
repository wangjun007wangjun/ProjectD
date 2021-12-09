using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Engine.Asset;

public class DanceGrid
{
    private string assetPath = "Gaming/DanceItem";
    private BaseAsset curAsset = null;
    private DanceItem danceItem = null;

    public void Born(Vector2 pos, Transform parent)
    {
        curAsset = AssetService.GetInstance().LoadInstantiateAsset(assetPath, LifeType.Manual);
        curAsset.RootTf.SetParent(parent);
        curAsset.RootTf.localPosition = pos;
        curAsset.RootTf.localScale = Vector3.one;
        curAsset.RootGo.SetActive(true);

        danceItem = curAsset.RootGo.GetComponent<DanceItem>();
        danceItem.OnClickAction = Destroy;
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
