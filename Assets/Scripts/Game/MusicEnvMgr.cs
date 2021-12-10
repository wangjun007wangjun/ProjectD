using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MusicEnvMgr : MonoBehaviour
{
    public GameObject envCube;
    public Transform cubePar;
    public Transform cameraPosPar;
    public Transform cameraTrans;
    private List<GameObject> newCube = new List<GameObject>();
    public AudioClip clip;
    private List<MeshRenderer> materials = new List<MeshRenderer>();
    public Slider musicSlider;
    [Range(0, 10)]
    public float colorMultiplayer = 1;
    [Range(0, 1)]
    public float s = 1;
    [Range(0, 1)]
    public float v = 1;
    private float musicLength;
    private AudioSource audio;

    private int colorPropertyId = Shader.PropertyToID("_Color");
    private bool gridColorChange = true;
    public GridOverlay gridOverlay;
    private float[] spectrumData = new float[64];

    private List<Transform> cameraPos = new List<Transform>();

    private bool isCameraMove = false;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        audio = GetComponent<AudioSource>();
        for (int i = 0; i < cameraPosPar.childCount; i++)
        {
            cameraPos.Add(cameraPosPar.GetChild(i).transform);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        float temp = 5.625f;
        for (int i = 0; i < 64; i++)
        {
            GameObject cubeT = Instantiate(envCube) as GameObject;
            cubeT.SetActive(true);
            cubeT.transform.SetParent(cubePar);
            cubeT.transform.localPosition = Vector3.zero;
            cubeT.transform.localEulerAngles = new Vector3(0, temp * i, 0);

            newCube.Add(cubeT);

            materials.Add(cubeT.transform.GetChild(0).GetComponent<MeshRenderer>());
        }

    }
    //传入音乐
    public void OnSetAudioClip(AudioClip clipT)
    {
        audio.clip = clipT;
    }

    //相机开始移动
    public void CameraStartMove()
    {
        isCameraMove = true;
        StartCoroutine(CameraMove());
        StartCoroutine(GridOn());
    }
    //相机停止
    public void StopCameraMove()
    {
        isCameraMove = false;
        StopCoroutine(CameraMove());
        StopCoroutine(GridOn());
        StopCoroutine(GridOff());
    }

    // Update is called once per frame
    void Update()
    {
        Visulization();
        if (Input.GetMouseButtonDown(0))
        {
            // PlaySound();
        }
        // MusicSlider();
        DynamicColor();
    }
    void Visulization()
    {
        audio.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        int i = 0;
        while (i < newCube.Count)
        {
            newCube[i].transform.localScale = new Vector3(0.5f, Mathf.Lerp(newCube[i].transform.localScale.y, spectrumData[i] * 5000, 0.5f), 0.5f);
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            // block.SetColor(colorPropertyId, HSVtoRGB(spectrumData[i] * colorMultiplayer, s, v, 1));

            block.SetColor(colorPropertyId, new Vector4(Mathf.Lerp(materials[i].material.color.r, spectrumData[i] * 500f, 0.2f), 0.5f, 1f, 1f));

            materials[i].SetPropertyBlock(block);
            i++;
        }
    }
    void MusicSlider()
    {
        musicLength = audio.time;
        musicSlider.value = musicLength / audio.clip.length;
    }
    public void PlaySound(AudioClip clip)
    {
        audio.clip = clip;

        audio.Play();
        //TODO
        CameraStartMove();
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
        if (isCameraMove)
        {
            yield return new WaitForSeconds(Random.Range(1f, 5f));
            yield return StartCoroutine(GridOff());
        }
        
    }
    private IEnumerator CameraMove()
    {
        yield return new WaitForSeconds(Random.Range(2f, 3.5f));
        int index = Random.Range(0, cameraPos.Count - 1);
        int index2 = Random.Range(0, cameraPos.Count - 1);
        float time1 =  Random.Range(3f, 15f);
        float time2 = Random.Range(3f, 10f);
        cameraTrans.DOMove(cameraPos[index].transform.position, time1);
        cameraTrans.DOLookAt(cameraPos[index2].transform.position, time2, AxisConstraint.None, Vector3.up);
        if (isCameraMove)
        {
            yield return new WaitForSeconds(Mathf.Max(time1, time2));
            yield return StartCoroutine(CameraMove());
        }
        else
        {
            StopCoroutine(CameraMove());
            StopCoroutine(GridOff());
            StopCoroutine(GridOn());
        }
    }
    public static Color HSVtoRGB(float hue, float saturation, float value, float alpha)
    {
        while (hue > 1f)
        {
            hue -= 1f;
        }
        while (hue < 0f)
        {
            hue += 1f;
        }
        while (saturation > 1f)
        {
            saturation -= 1f;
        }
        while (saturation < 0f)
        {
            saturation += 1f;
        }
        while (value > 1f)
        {
            value -= 1f;
        }
        while (value < 0f)
        {
            value += 1f;
        }
        if (hue > 0.999f)
        {
            hue = 0.999f;
        }
        if (hue < 0.001f)
        {
            hue = 0.001f;
        }
        if (saturation > 0.999f)
        {
            saturation = 0.999f;
        }
        if (saturation < 0.001f)
        {
            return new Color(value * 255f, value * 255f, value * 255f);
        }
        if (value > 0.999f)
        {
            value = 0.999f;
        }
        if (value < 0.001f)
        {
            value = 0.001f;
        }
        float h6 = hue * 6f;
        if (h6 == 6f)
        {
            h6 = 0f;
        }
        int ihue = (int)h6;
        float p = value * (1f - saturation);
        float q = value * (1f - (saturation * (h6 - (float)ihue)));
        float t = value * (1f - (saturation * (1f - (h6 - (float)ihue))));
        switch (ihue)
        {
            case 0:
                return new Color(value, t, p, alpha);
            case 1:
                return new Color(q, value, p, alpha);
            case 2:
                return new Color(p, value, t, alpha);
            case 3:
                return new Color(p, q, value, alpha);
            case 4:
                return new Color(t, p, value, alpha);
            default:
                return new Color(value, p, q, alpha);
        }
    }
}
