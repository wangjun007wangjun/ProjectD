/********************************************************************
  created:  2020-06-05         
  author:   大厅背景控制   
  purpose:  游戏中控制背景Mgr,音乐播放，背景效果          
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using Data;
using Engine.Audio;
using UnityEngine;

public class LobbyEnv : MonoBehaviour
{
    public GameObject temp;
    public Transform cubeParent;
    private List<GameObject> newCube = new List<GameObject>();
    public AudioClip clip;
    private List<MeshRenderer> materials = new List<MeshRenderer>();
    private AudioSource audio;

    private int colorPropertyId = Shader.PropertyToID("_Color");
    private bool gridColorChange = true;
    public GridOverlay gridOverlay;
    private float[] spectrumData = new float[64];

    void Awake()
    {
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        PlaySound();

        audio = AudioService.GetInstance().Worker.GetCurMusicSource();

        for (int i = 0; i < 10; i++)
        {
            GameObject cubeT = Instantiate(temp) as GameObject;
            cubeT.SetActive(true);
            cubeT.transform.SetParent(cubeParent);
            cubeT.transform.localPosition = new Vector3(i, 0, 0);
            cubeT.transform.localEulerAngles = new Vector3(0, 0, 0);

            newCube.Add(cubeT);

            materials.Add(cubeT.transform.GetChild(0).GetComponent<MeshRenderer>());
        }
        StartCoroutine(GridOn());
        // Debug.Log("大厅播放");
    }
    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {

    }
    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        StopCoroutine(GridOn());
        StopCoroutine(GridOff());
        if (DataService.GetInstance().Model == 2)
        {
            AudioService.GetInstance().StopChannel(1);
        }
    }
    // Update is called once per frame
    void Update()
    {
        Visulization();
        DynamicColor();
    }
    void Visulization()
    {
        audio.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        int i = 0;
        while (i < newCube.Count)
        {
            newCube[i].transform.localScale = new Vector3(0.5f, Mathf.Lerp(newCube[i].transform.localScale.y, spectrumData[i] * 1000, 0.5f), 0.5f);
            MaterialPropertyBlock block = new MaterialPropertyBlock();

            block.SetColor(colorPropertyId, new Vector4(Mathf.Lerp(materials[i].material.color.r, spectrumData[i] * 500f, 0.2f), 0.5f, 1f, 0.5f));

            materials[i].SetPropertyBlock(block);
            i++;
        }
    }
    public void PlaySound()
    {
        AudioService.GetInstance().Play(AudioChannelType.MUSIC, "Mp3/CloseToYou", true);
    }

    void DynamicColor()
    {
        if (gridColorChange)
        {
            float gridColor = Mathf.Lerp(gridOverlay.mainColor.r, spectrumData[50] * 1000, 0.5f);
            if (gridColor > 1)
            {
                gridColor = 1;
            }
            gridOverlay.mainColor = new Vector4(gridColor, 0.5f, 1f, 1f);
        }
    }
    IEnumerator GridOff()
    {
        for (int i = 0; i < 51; i++)
        {
            gridOverlay.largeStep += 10;
            yield return new WaitForSeconds(0.02f);
        }
        gridOverlay.showMain = false;

        yield return new WaitForSeconds(Random.Range(1f, 5f));
        yield return StartCoroutine(GridOn());
    }
    IEnumerator GridOn()
    {
        gridOverlay.showMain = true;
        gridColorChange = true;
        gridOverlay.largeStep = 500;
        for (int i = 0; i < 49; i++)
        {
            gridOverlay.largeStep -= 10;
            yield return new WaitForSeconds(0.02f);
        }
        yield return new WaitForSeconds(Random.Range(1f, 5f));
        yield return StartCoroutine(GridOff());
    }
}
