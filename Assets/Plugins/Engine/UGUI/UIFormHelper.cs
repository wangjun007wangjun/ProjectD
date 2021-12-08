using Engine.Asset;
using UnityEngine;

namespace Engine.UGUI
{
    public static class UIFormHelper
    {
        public static T CreateFormClass<T>(System.Action<string,object> actionHandler, object parameter=null, bool pool=true ) where T : UIFormClass, new()
        {
            T prefabClass = (T)AssetService.GetInstance().LoadFormAsset<T>(pool ? LifeType.Trust : LifeType.Manual);
            if (prefabClass == null)
            {
                return null;
            }
            prefabClass.Create(actionHandler, parameter);
            return prefabClass;
        }

        public static void DisposeFormClass<T>(ref T prefabClass) where T : UIFormClass
        {
            DisposeFormClass(prefabClass);
            prefabClass = null;
        }

        public static void DisposeFormClass(UIFormClass prefabClass)
        {
            if (null == prefabClass)
            {
                return;
            }
            prefabClass.Destroy();
        }

        //创建子视图
        public static T CreateViewClass<T>(this UIFormClass formVC, Transform parentTf, System.Action<string, object> actionHandler=null, object parameter = null) where T : UIViewClass, new()
        {
            T prefabClass = (T)AssetService.GetInstance().LoadViewAsset<T>();
            if (prefabClass == null)
            {
                return null;
            }
            prefabClass.Create(formVC,parentTf,actionHandler, parameter);
            return prefabClass;
        }

        public static void DisposeViewClass<T>(this UIFormClass formVC,ref T prefabClass) where T : UIViewClass
        {
            formVC.DisposeViewClass(prefabClass);
            prefabClass = null;
        }

        public static void DisposeViewClass(this UIFormClass formVC,UIViewClass prefabClass)
        {
            if (null == prefabClass)
            {
                return;
            }
            prefabClass.Destroy();
        }

    }
}
