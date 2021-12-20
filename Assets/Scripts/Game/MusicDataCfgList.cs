/********************************************************************
  created:  2020-06-05         
  author:    音乐配置结构       
  purpose:  游戏中控制背景Mgr,音乐播放，背景效果          
*********************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicDataCfgList:MonoBehaviour
{
    public List<MusicData> list;
}
[Serializable]
public class MusicData
{
    //id
    public int id;
    //音乐专辑图片
    public Sprite musicTexture;
    //音频文件
    public AudioClip audio;
    //困难度
    public GameDifficulty difficulty;
    //音乐名
    public string name;
}
