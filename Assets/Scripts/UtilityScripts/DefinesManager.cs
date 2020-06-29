
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
[ExecuteInEditMode]
#endif
public class DefinesManager : MonoBehaviour {

    public string[] mainSceneDefines;
    public string[] mainMenuSceneDefines;
    public string[] worldCreationDefines;

#if UNITY_EDITOR
    // private void Awake() {
        // if (!EditorApplication.isPlaying) { //only do this when scene is not playing!
        //     string activeScene = EditorSceneManager.GetActiveScene().name;
        //     string defines = string.Empty;
        //     if (activeScene.Equals("Main")) {
        //         for (int i = 0; i < mainSceneDefines.Length; i++) {
        //             string currDefine = mainSceneDefines[i];
        //             defines += $"{currDefine};";
        //         }
        //     } else if (activeScene.Equals("WorldCreationTool")) {
        //         for (int i = 0; i < worldCreationDefines.Length; i++) {
        //             string currDefine = worldCreationDefines[i];
        //             defines += $"{currDefine};";
        //         }
        //     } else if (activeScene.Equals("MainMenu")) {
        //         for (int i = 0; i < mainMenuSceneDefines.Length; i++) {
        //             string currDefine = mainMenuSceneDefines[i];
        //             defines += $"{currDefine};";
        //         }
        //     }
        //     PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
        //     Debug.Log($"Set Defines to: {defines}");
        // }
    // }
#endif
}

