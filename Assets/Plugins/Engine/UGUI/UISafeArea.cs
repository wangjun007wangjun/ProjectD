using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

//*************************************************************************************************
/// <summary>
/// Safe area adjustment class
/// </summary>
//*************************************************************************************************
public class UISafeArea : MonoBehaviour
{

    private RectTransform panelRectTrans;
    /// <summary>RectTransfrom Target of adjustment</summary>
    public RectTransform PanelRectTrans
    {
        get
        {
            if (panelRectTrans == null)
            {
                panelRectTrans = GetComponent<RectTransform>();
            }
            return panelRectTrans;
        }
    }

    [Header("自动调整缩放")]
    public bool isAutoScale;

    private Rect safeArea;
    private Vector2Int screenSize;

    //*************************************************************************************************
    /// <summary>
    /// Startup process
    /// </summary>
    //*************************************************************************************************
    void Awake()
    {
#if UNITY_EDITOR
        // For debugging at runtime
        if (simulateOnPlay)
        {
            orientationType = OrientationType.Auto;
            simulateType = getSimulateTypeFromCurrentResolution();
            safeArea = getSimulationSafeArea(simulateType);
            screenSize = getSimulationResolution(simulateType);
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Apply();
            }
            return;
        }
#endif
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Setup();
            Apply();
        }
    }

    //*************************************************************************************************
    /// <summary>
    /// Initialization processing
    /// </summary>
    //*************************************************************************************************
    public void Setup()
    {
        safeArea = UnityEngine.Screen.safeArea;
        // NOTE: Solution to the problem that the Anchor of the Canvas shifts（http://appleorbit.hatenablog.com/entry/2018/05/29/235021）
        var display = Display.displays[0];
        screenSize = new Vector2Int(display.systemWidth, display.systemHeight);
    }

    //*************************************************************************************************
    /// <summary>
    /// Adaptation
    /// </summary>
    //*************************************************************************************************
    public void Apply()
    {
        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= screenSize.x;
        anchorMin.y /= screenSize.y;
        anchorMax.x /= screenSize.x;
        anchorMax.y /= screenSize.y;

        PanelRectTrans.anchorMin = anchorMin;
        PanelRectTrans.anchorMax = anchorMax;

        if (isAutoScale)
        {
            var oldSize = PanelRectTrans.rect;
            adjustScale();
            var heightRate = getHeightRate();
            var newSize = new Rect(PanelRectTrans.rect.x * heightRate, PanelRectTrans.rect.y * heightRate, PanelRectTrans.rect.width * heightRate, PanelRectTrans.rect.height * heightRate);
            // Increase the size of the frame by the reduced scale
            PanelRectTrans.sizeDelta = new Vector2((oldSize.width - newSize.width), (oldSize.height - newSize.height));
        }
    }

    //*************************************************************************************************
    /// <summary>
    /// Returns the ratio of SafeArea to height
    /// </summary>
    /// <returns>Ratio of height to SafeArea</returns>
    //*************************************************************************************************
    private float getHeightRate()
    {
        return Mathf.Clamp01(safeArea.height / screenSize.y);
    }

    //*************************************************************************************************
    /// <summary>
    /// Scale specified Transform to fit the size of SafeArea
    /// </summary>
    //*************************************************************************************************
    private void adjustScale()
    {
        var heightRate = getHeightRate();
        PanelRectTrans.localScale = new Vector3(heightRate, heightRate, 1.0f);
    }

    #region For debugging
#if UNITY_EDITOR
    // Model for simulation
    private enum SimulateType
    {
        None = 0,
        iPhoneXAndXs,
        iPhoneXR,
        iPhoneXsMax
    }
    // Terminal orientation
    private enum OrientationType
    {
        Auto = 0,
        Portrait,
        Landscape
    }
    private OrientationType orientationType = OrientationType.Auto;

    [SerializeField, Header("Debug: Reflect on the run time on PC")]
    private bool simulateOnPlay = true;
    [SerializeField, Header("Debug: The model you want to simulate"), Tooltip("Ignored at runtime")]
    private SimulateType simulateType;
    [Header("Debug: Vertical"), Tooltip("Ignored at runtime")]
    public bool isPortrait;

    // Terminal resolution. Index matches SimulateType
    private Vector2Int[] resolutions = new Vector2Int[]
    {
        Vector2Int.zero,
        new Vector2Int(1125, 2436),
        new Vector2Int(828, 1792),
        new Vector2Int(1242, 2688)
    };

    // Sideways
    private bool isLandscape
    {
        get
        {
            // At the time of operation on Editor, it is judged from isPortrait
            if (orientationType != OrientationType.Auto)
            {
                return !isPortrait;
            }

            // It is judged from the resolution at the time of execution
            var width = UnityEngine.Screen.width;
            var height = UnityEngine.Screen.height;

            var iPhoneX = resolutions[(int)SimulateType.iPhoneXAndXs];
            var iPhoneXR = resolutions[(int)SimulateType.iPhoneXR];
            var iPhoneXsMax = resolutions[(int)SimulateType.iPhoneXsMax];
            return width == iPhoneX.y && height == iPhoneX.x || width == iPhoneXR.y && height == iPhoneXR.x || width == iPhoneXsMax.y && height == iPhoneXsMax.x;
        }
    }

    //*************************************************************************************************
    /// <summary>
    /// Safe area simulation on iPhone X and later
    /// </summary>
    //*************************************************************************************************
    public void SimulateAtEditor()
    {
        if (simulateType == SimulateType.None)
        {
            return;
        }
        orientationType = isPortrait ? OrientationType.Portrait : OrientationType.Landscape;
        safeArea = getSimulationSafeArea(simulateType);
        screenSize = getSimulationResolution(simulateType);
        Apply();
    }

    //*************************************************************************************************
    /// <summary>
    /// Get resolution for simulation
    /// </summary>
    /// <param name="type"Model for simulation></param>
    /// <returns>Simulated resolution</returns>
    //*************************************************************************************************
    private Vector2Int getSimulationResolution(SimulateType type)
    {
        var index = (int)type;
        var width = isLandscape ? resolutions[index].y : resolutions[index].x;
        var height = isLandscape ? resolutions[index].x : resolutions[index].y;
        return new Vector2Int(width, height);
    }

    //*************************************************************************************************
    /// <summary>
    /// Get a safe area for simulation
    /// </summary>
    /// <param name="type">Model for simulation</param>
    /// <returns>Safe area for simulation</returns>
    //*************************************************************************************************
    private Rect getSimulationSafeArea(SimulateType type)
    {
        // NOTE: Resolution is tripled from physical resolution
        switch (type)
        {
            case SimulateType.iPhoneXAndXs:
                return isLandscape ? new Rect(132, 63, 2172, 1062) : new Rect(0, 102, 1125, 2202);
            case SimulateType.iPhoneXR:
                return isLandscape ? new Rect(132, 63, 1528, 765) : new Rect(0, 102, 1242, 2454);
            case SimulateType.iPhoneXsMax:
                return isLandscape ? new Rect(132, 63, 2424, 1179) : new Rect(0, 102, 1242, 2454);
            case SimulateType.None:
            default:
                return new Rect(0, 0, UnityEngine.Screen.width, UnityEngine.Screen.height);
        }
    }

    //*************************************************************************************************
    /// <summary>
    /// Identify the model to simulate from the resolution on the current editor
    /// </summary>
    /// <returns>Model to simulate</returns>
    //*************************************************************************************************
    private SimulateType getSimulateTypeFromCurrentResolution()
    {
        var width = UnityEngine.Screen.width;
        var height = UnityEngine.Screen.height;
        if (width == 2436 && height == 1125 || width == 1125 && height == 2436)
        {
            return SimulateType.iPhoneXAndXs;
        }
        else if (width == 1792 && height == 828 || width == 828 && height == 1792)
        {
            return SimulateType.iPhoneXR;
        }
        else if (width == 2688 && height == 1242 || width == 1242 && height == 2688)
        {
            return SimulateType.iPhoneXsMax;
        }
        else
        {
            return SimulateType.None;
        }
    }
#endif
    #endregion
}