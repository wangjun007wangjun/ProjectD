/********************************************************************
	created:	2020/02/12
	author:		OneJun
	
	purpose:	游戏启动窗口
*********************************************************************/
using Engine.UGUI;
using Engine.Event;
using UnityEngine;
using UnityEngine.UI;

public class UILaunchForm : UIFormClass
{
    public override string GetPath()
    {
        return "Fixed/UILaunchWnd";
    }


    /// <summary>
    /// 资源加载后回调
    /// </summary>
    protected override void OnResourceLoaded()
    {
      

    }

    /// <summary>
    /// 资源卸载后回调
    /// </summary>
    protected override void OnResourceUnLoaded()
    {


    }

    /// <summary>
    /// 逻辑初始化
    /// </summary>
    /// <param name="parameter">初始化参数</param>
    protected override void OnInitialize(object parameter)
    {
       
       
        
    }

    /// <summary>
    /// 逻辑反初始化
    /// </summary>
    protected override void OnUninitialize()
    {
        
    }

    /// <summary>
    /// 更新UI
    /// </summary>
    /// <param name="id">更新ID标识</param>
    /// <param name="param">更新参数</param>
    protected override void OnUpdateUI(string id, object param)
    {

    }

    /// <summary>
    /// 按钮动作处理
    /// </summary>
    /// <param name="id"></param>
    /// <param name="param"></param>
    protected override void OnAction(string id, object param)
    {

    }


}
