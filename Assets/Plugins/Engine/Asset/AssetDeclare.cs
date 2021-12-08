/********************************************************************
  created:  2020-03-31         
  author:    OneJun           
  purpose:  资产相关说明                
*********************************************************************/
using Engine.Base;
using Engine.PLink;
using Engine.Res;
using UnityEngine;
using UnityEngine.U2D;

namespace Engine.Asset
{
    /// <summary>
    /// 加载优先级
    /// </summary>
    public static class LoadPriority
    {
        /// <summary>
        /// 阻塞 最高
        /// </summary>
        public const int Wait = 0;
        /// <summary>
        /// 推荐UI 模型 最高级异步
        /// </summary>
        public const int ImmediateShow = 1;
        //异步中 推荐 特效
        public const int SpareShow = 10000;
        //异步低 推荐当前不需要的
        public const int Silent = 20000;
        //异步的形式加载预处理的资源 最低
        public const int Preprocess = 30000;
    }

    /// <summary>
    /// 资产加载结果
    /// </summary>
    public static class AssetLoadResult
    {
        public const int Success = 0;

        public const int BundleFail = 1;

        public const int ResourceFail = 2;
    }

    /// <summary>
    /// 资产加载回调
    /// </summary>
    public interface IAssetLoadCallback
    {
        /// <summary>
        /// 资产加载完成
        /// </summary>
        /// <param name="assetName">资产名</param>
        /// <param name="result">加载结果，取值为AssetLoadResult</param>
        /// <param name="resource">资源</param>
        void OnLoadAssetCompleted(string assetName, int result, ResObj resource);
    }

    /// <summary>
    /// Asset生命周期
    /// </summary>
    public class LifeType
    {
        // 常驻 一直存在
        public const int Resident = 0;

        //托管 根据调用unloadunused释放
        public const int Trust = 1;

        //手动管理 各自负责 unload 时候会释放
        public const int Manual = 2;

    }

    /// <summary>
    /// 资产类型定义
    /// </summary>
    public class AssetType
    {
        // 特效
        public const int Particle = 0;

        // 3D模型
        public const int Model = 1;

        // 纹理 散图
        public const int Texture = 2;

        // 实例化资源 
        public const int Instantiate = 3;

        //不实例化资源 
        public const int Primitive = 4;

        //音效
        public const int Audio = 5;

        //2D精灵
        public const int Sprite = 6;

        //精灵图集
        public const int SAtlas = 7;

        //spine模型
        public const int Spine = 8;

        //基础资源总数
        public const int BaseAssetTypeCount = 9;

        // UI预制件不带canvas 嵌入Form 小视图View
        public const int UIView = 20;

        //UIFormClass对应的窗口实例预制
        public const int UIForm = 50;

        // Unity场景
        public const int UnityScene = 99;

        // 外部资源
        public const int External = 100;

    }

    /// <summary>
    /// 资产数据信息
    /// </summary>
    public struct AssetData
    {
        public IAssetLoadCallback Callback;
        public int Type;
        public string Name;
        public string Filename;
        public int Life;
        public int Priority;
        public IObjectFactory<BaseAsset> Factory;
        public int Count;
    }

    /// <summary>
    /// 基础资产定义
    /// </summary>
    public class BaseAsset
    {
        //内部使用的加载信息 数据结构，外部不要修改也不要访问
        public AssetData BaseData;

        //对应的资源对象
        public ResObj Resource;

        public GameObject RootGo;
        public Transform RootTf;

        public PrefabLink RootPLink;

        public Vector3 OrignalPosition;
        public Quaternion OrignalRotate;
        public Vector3 OrignalScale;

        public GameObject Go
        {
            get
            {
                return RootGo;
            }
        }

        public Transform Tf
        {
            get
            {
                return RootTf;
            }
        }
    }

    /// <summary>
    /// UI窗口资产
    /// </summary>
    public abstract class UIFormAsset : BaseAsset
    {
        public abstract void OnFormAssetCreate();
        public abstract void OnFormAssetDestroy();
        public abstract string GetPath();
    }
    
    /// <summary>
    ///  UI视图资产 
    /// </summary>
    public abstract class UIViewAsset : BaseAsset
    {
        public abstract void OnViewAssetCreate();
        public abstract void OnViewAssetDestroy();
        public abstract string GetPath();
    }
    
    /// <summary>
    /// 模型资产
    /// </summary>
    public class ModelAsset : BaseAsset
    {
        public Renderer[] Render;
        public Animation AnimationCpt;
        public Animator AnimatorCtrl;
    }

    public class SpriteAsset : BaseAsset
    {
        public Sprite SpriteObj;
    }


    public class SpriteAtlasAsset : BaseAsset
    {
        public SpriteAtlas AtlasObj;

        public Sprite GetSpriteByName(string name)
        {
            if (AtlasObj != null)
            {
                return AtlasObj.GetSprite(name);
            }
            return null;
        }

    }

    /// 声音资源
    /// </summary>
    public class AudioAsset : BaseAsset
    {
        public AudioClip Clip;
    }

    //Spine 资源
    public class SpineAsset : BaseAsset
    {
        public Spine.Unity.SkeletonGraphic Sg;
        public Spine.Unity.SkeletonAnimation Sa;
    }
}