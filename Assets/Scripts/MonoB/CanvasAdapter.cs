/********************************************************************
*		作者： XH
*		时间： 2018-09-14
*		描述： UGUI Canvas 适配
*********************************************************************/
using UnityEngine;
using UnityEngine.UI;
namespace MonoB.Msic
{
    public class CanvasAdapter : MonoBehaviour
    {
        //Canvas设计宽度
        [SerializeField] private float _referenceWidth = 1280;
        //Canvas设计高度
        [SerializeField] private float _referencedevHeight = 720;

        private void Start()
        {
            CanvasScaler canvasScaler = transform.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                return;
            }
            //屏幕的宽度
            float screenWidth = Screen.width;
            //屏幕的高度
            float screenHeight = Screen.height;
            //适配方式
            float matchWidthOrHeight = 0;
            //设计宽高比
            float referenceAspectRatio = _referenceWidth / _referencedevHeight;
            //当前宽高比
            float curAspectRatio = screenWidth / screenHeight;
            //计算矫正比例
            if (curAspectRatio < referenceAspectRatio)
            {
                matchWidthOrHeight = referenceAspectRatio / curAspectRatio;
            }

            if (matchWidthOrHeight == 0)
            {
                canvasScaler.matchWidthOrHeight = 1;
            }
            else
            {
                canvasScaler.matchWidthOrHeight = 0;
            }
        }

    }
}