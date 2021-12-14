using Engine.Base;
using Engine.PLink;
using UnityEngine;
using UnityEngine.U2D;

namespace Engine.Asset
{
    public static class AssetProcessor
    {
        public static BaseAsset CreateAssetClass(int type)
        {
            switch (type)
            {
                case AssetType.Model:
                    return new ModelAsset();
                case AssetType.Instantiate:
                    return new BaseAsset();
                case AssetType.Audio:
                    return new AudioAsset();
                case AssetType.Sprite:
                    return new SpriteAsset();
                case AssetType.Primitive:
                    return new BaseAsset();
                case AssetType.SAtlas:
                    return new SpriteAtlasAsset();
                case AssetType.Spine:
                    return new SpineAsset();
            }
            GGLog.LogE("AssetProcessor.CreateAssetClass : invalid type - {0}", type);
            return null;
        }

        public static bool ProcessCreate(BaseAsset ba)
        {
            if (ba == null || ba.Resource == null || ba.Resource.Content == null)
            {
                GGLog.LogE("AssetProcessor.ProcessCreate : invalid parameter");
                return false;
            }

            switch (ba.BaseData.Type)
            {
                case AssetType.UIForm:
                    {
                        UIFormAsset ast = ba as UIFormAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessCreate : type is not object - {0}", ba.BaseData.Name);
                            return false;
                        }

                        if (!InstantiateAsset(ast))
                        {
                            return false;
                        }
                        ast.OnFormAssetCreate();
                    }
                    break;

                case AssetType.Model:
                    {
                        ModelAsset ast = ba as ModelAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessCreate : type is not model - {0}", ba.BaseData.Name);
                            return false;
                        }

                        if (!InstantiateAsset(ast))
                        {
                            return false;
                        }

                        ast.Render = ast.RootGo.ExtGetComponentsInChildren<Renderer>(true);
                        ast.AnimationCpt = ast.RootGo.ExtGetComponentInChildren<Animation>();
                        ast.AnimatorCtrl = ast.RootGo.ExtGetComponentInChildren<Animator>();
                    }
                    break;
                case AssetType.UIView:
                    {
                        UIViewAsset ast = ba as UIViewAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessCreate : type is not uiasset - {0}", ba.BaseData.Name);
                            return false;
                        }
                        if (!InstantiateAsset(ast))
                        {
                            return false;
                        }
                        ast.OnViewAssetCreate();
                    }
                    break;
                case AssetType.Instantiate:
                    {
                        if (!InstantiateAsset(ba))
                        {
                            return false;
                        }
                    }
                    break;
                case AssetType.Audio:
                    {
                        AudioAsset ast = ba as AudioAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessCreate : type is not audioclip - {0}", ba.BaseData.Name);
                            return false;
                        }

                        ast.Clip = ba.Resource.Content as AudioClip;
                        if (ast.Clip == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessCreate : resource is not audioclip - {0}", ba.BaseData.Name);
                            return false;
                        }
                    }
                    break;
                case AssetType.Sprite:
                    {
                        SpriteAsset ast = ba as SpriteAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessCreate : type is not SpriteAsset - {0}", ba.BaseData.Name);
                            return false;
                        }

#if UNITY_EDITOR
                        Texture2D tt = ba.Resource.Content as Texture2D;
                        if (tt == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessCreate : Content Error - {0}", ba.BaseData.Name);
                            return false;
                        }
                        ast.SpriteObj = Sprite.Create(tt, new Rect(0, 0, tt.width, tt.height), new Vector2(0.5f, 0.5f));
#else
                        Sprite ss = ba.Resource.Content as Sprite;
                        if (ss == null)
                        {
                            GGLog.LogD("SpriteAsset Load From AB Is Texture2D Type");
                            Texture2D tt1 = ba.Resource.Content as Texture2D;
                            ast.SpriteObj = Sprite.Create(tt1, new Rect(0, 0, tt1.width, tt1.height), new Vector2(0.5f, 0.5f));
                        }
                        else
                        {
                            ast.SpriteObj = ss;
                        }
#endif
                        if (ast.SpriteObj == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessCreate : resource is not Sprite - {0}", ba.BaseData.Name);
                            return false;
                        }
                    }
                    break;

                case AssetType.SAtlas:
                    {
                        SpriteAtlasAsset ast = ba as SpriteAtlasAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessCreate : type is not SpriteAtlasAsset - {0}", ba.BaseData.Name);
                            return false;
                        }

                        ast.AtlasObj = ba.Resource.Content as SpriteAtlas;
                        if (ast.AtlasObj == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessCreate : resource is not SpriteAtlas - {0}", ba.BaseData.Name);
                            return false;
                        }
                    }
                    break;
                case AssetType.Spine:
                    {
                        SpineAsset ast = ba as SpineAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessCreate : type is not SpineAsset - {0}", ba.BaseData.Name);
                            return false;
                        }
                        if (!InstantiateAsset(ba))
                        {
                            return false;
                        }
                        ast.Sg = ast.RootGo.ExtGetComponent<Spine.Unity.SkeletonGraphic>();
                        ast.Sa = ast.RootGo.ExtGetComponent<Spine.Unity.SkeletonAnimation>();
                    }
                    break;
            }

            return true;
        }

        public static void ProcessRestore(BaseAsset ba)
        {
            if (ba == null)
            {
                GGLog.LogE("AssetProcessor.ProcessRestore : invalid parameter");
                return;
            }
            if (ba.RootTf != null)
            {
                ba.RootTf.localPosition = ba.OrignalPosition;
                ba.RootTf.localRotation = ba.OrignalRotate;
                ba.RootTf.localScale = ba.OrignalScale;
            }
            //
            switch (ba.BaseData.Type)
            {
                case AssetType.Model:
                    {
                        //ModelAsset ma = ba as ModelAsset;
                    }
                    break;
                case AssetType.UIForm:
                    {

                    }
                    break;
                case AssetType.UIView:
                    {

                    }
                    break;
                case AssetType.Spine:
                    {
                        SpineAsset aa = ba as SpineAsset;
                        aa.Sg = null;
                        aa.Sa = null;
                    }
                    break;
            }
        }

        public static bool ProcessDestroy(BaseAsset ba)
        {
            if (ba == null)
            {
                GGLog.LogE("AssetProcessor.ProcessDestroy : invalid parameter");
                return false;
            }

            switch (ba.BaseData.Type)
            {
                case AssetType.UIForm:
                    {
                        UIFormAsset ast = ba as UIFormAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessDestroy : type is not object - {0}", ba.BaseData.Name);
                            return false;
                        }

                        ast.OnFormAssetDestroy();
                        DestroyAsset(ast);
                    }
                    break;
                case AssetType.Model:
                    {
                        ModelAsset ast = ba as ModelAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessDestroy : type is not model - {0}", ba.BaseData.Name);
                            return false;
                        }

                        ast.AnimationCpt = null;
                        ast.AnimatorCtrl = null;
                        DestroyAsset(ast);
                    }
                    break;
                case AssetType.UIView:
                    {
                        UIViewAsset ast = ba as UIViewAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessDestroy : type is not ui - {0}", ba.BaseData.Name);
                            return false;
                        }
                        ast.OnViewAssetDestroy();
                        DestroyAsset(ast);
                    }
                    break;
                case AssetType.Spine:
                    {
                        SpineAsset ast = ba as SpineAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessDestroy : type is not SpineAsset - {0}", ba.BaseData.Name);
                            return false;
                        }

                        ast.Sg = null;
                        ast.Sa = null;
                        DestroyAsset(ast);
                    }
                    break;
                case AssetType.Instantiate:
                    {
                        DestroyAsset(ba);
                    }
                    break;
                case AssetType.Audio:
                    {
                        AudioAsset ast = ba as AudioAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessDestroy : type is not audio - {0}", ba.BaseData.Name);
                            return false;
                        }
                        ast.Clip = null;
                    }

                    break;
                case AssetType.Sprite:
                    {
                        SpriteAsset ast = ba as SpriteAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessDestroy : type is not SpriteAsset - {0}", ba.BaseData.Name);
                            return false;
                        }
                        ast.SpriteObj = null;
                    }
                    break;
                case AssetType.SAtlas:
                    {
                        SpriteAtlasAsset ast = ba as SpriteAtlasAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessDestroy : type is not SpriteAtlasAsset - {0}", ba.BaseData.Name);
                            return false;
                        }
                        ast.AtlasObj = null;
                    }
                    break;
            }
            return true;
        }

        public static BaseAsset ProcessClone(BaseAsset s)
        {
            if (s == null)
            {
                GGLog.LogE("ProcessClone Error Param");
                return null;
            }
            BaseAsset ba = s.BaseData.Factory.CreateObject();
            if (ba == null)
            {
                GGLog.LogE("ProcessClone : failed to create asset - {0}", s.BaseData.Name);
                return null;
            }
            //
            ba.BaseData = s.BaseData;
            ba.Resource = s.Resource;
            //��Դ��������++
            ba.Resource.RefCnt++;
            //
            if (ba == null || ba.Resource == null || ba.Resource.Content == null)
            {
                GGLog.LogE("AssetProcessor.ProcessCreate : invalid parameter");
                return null;
            }
            //
            switch (ba.BaseData.Type)
            {
                case AssetType.UIForm:
                    {
                        //���ڽ�ֹclone
                        return null;
                    }
                case AssetType.Model:
                    {
                        ModelAsset ast = ba as ModelAsset;
                        if (ast == null)
                        {
                            GGLog.LogE("AssetProcessor.ProcessCreate : type is not model - {0}", ba.BaseData.Name);
                            return null;
                        }
                        if (!InstantiateAsset(ast))
                        {
                            return null;
                        }
                        ast.Render = ast.RootGo.ExtGetComponentsInChildren<Renderer>(true);
                        ast.AnimationCpt = ast.RootGo.ExtGetComponentInChildren<Animation>();
                        ast.AnimatorCtrl = ast.RootGo.ExtGetComponentInChildren<Animator>();
                    }
                    break;
                //����ʵ��
                case AssetType.Instantiate:
                    {
                        if (!InstantiateAsset(ba))
                        {
                            return null;
                        }
                    }
                    break;
            }
            return ba;
        }

        private static bool InstantiateAsset(BaseAsset ba)
        {
            if (ba.RootGo == null)
            {
                ba.RootGo = (GameObject)ba.Resource.Content.ExtInstantiate();
            }

            if (ba.RootGo == null)
            {
                GGLog.LogE("AssetProcessor.Instantiate : failed to instantiate - {0}", ba.BaseData.Name);
                return false;
            }

            ba.RootGo.ExtSetActive(false);
            ba.RootTf = ba.RootGo.transform;
            ba.OrignalPosition = ba.RootTf.localPosition;
            ba.OrignalRotate = ba.RootTf.localRotation;
            ba.OrignalScale = ba.RootTf.localScale;
            ba.RootPLink = ba.RootGo.GetComponent<PrefabLink>();
            //
            return true;
        }

        private static void DestroyAsset(BaseAsset ba)
        {
            if (ba.RootGo != null)
            {
                ba.RootGo.ExtSetActive(false);

                ba.RootGo.ExtDestroy();
            }

            ba.RootGo = null;
            ba.RootTf = null;
            ba.RootTf = null;
            ba.RootPLink = null;
        }
    }
}