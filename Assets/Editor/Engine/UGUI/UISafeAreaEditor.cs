using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//*************************************************************************************************
/// <summary>
/// SafeAreaAdjuster Inspector extension class
/// </summary>
//*************************************************************************************************
[CustomEditor(typeof(UISafeArea))]
public class UISafeAreaEditor : Editor {
  private bool toggle;
  private UISafeArea comp;

  //*************************************************************************************************
  /// <summary>
  /// Notification on display
  /// </summary>
  //*************************************************************************************************
  void OnEnable() {
    comp = target as UISafeArea;
  }

  //*************************************************************************************************
  /// <summary>
  /// Inspector drawing
  /// </summary>
  //*************************************************************************************************
  public override void OnInspectorGUI() {
    base.OnInspectorGUI();

    EditorGUI.BeginChangeCheck();

    // toggle Click with the mouse to change the value
    EditorGUILayout.Space();
    toggle = EditorGUILayout.ToggleLeft("Editor Simulate immediately on", toggle);

    // toggle Becomes true whenever the value of is changed
    if (EditorGUI.EndChangeCheck()) {
      if (toggle) {
        comp.SimulateAtEditor();
      } else {
        comp.Setup();
        comp.Apply();
      }
    }
  }
}
