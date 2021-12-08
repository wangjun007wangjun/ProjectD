/********************************************************************
	created:	2018-06-07   
	filename: 	JWMenuRoot
	author:		OneJun
	
	purpose:	菜单根 禁止分散项目扩展菜单
*********************************************************************/
using UnityEngine;
using UnityEditor;
using System.IO;
public static class AppMenuRoot
{

    [MenuItem("出档/构建/输出AndroidProj")]
    static void BuildAndroidProj()
    {
        Debug.Log("----BuildAndroidProj Begin!-----");
        //输出EXE
        CommandBuild.BuildAndroidProj();
        //
        Debug.Log("----BuildAndroidProj Done!-----");
    }

    [MenuItem("出档/构建/输出IosProj")]
    static void BuildIosProj()
    {
        Debug.Log("----BuildIosProj Begin!-----");
        //输出EXE
        CommandBuild.BuildIOSProject();
        Debug.Log("----BuildIosProj Done!-----");
    }

    [MenuItem("出档/清理PlayerPrefs")]
    static void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

}

