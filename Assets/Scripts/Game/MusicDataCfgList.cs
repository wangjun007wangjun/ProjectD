using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicDataCfgList:MonoBehaviour
{
    public List<MusicData> list;
}
[Serializable]
public class MusicData
{
    public Texture2D musicTexture;
    public AudioClip audio;
    public GameDifficulty difficulty;
    public string name;
}
