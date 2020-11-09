using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Ruinarch;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UtilityScripts;

public class LevelLoaderManager : MonoBehaviour {
    public static LevelLoaderManager Instance;

    [SerializeField] private GameObject loaderGO;
    [SerializeField] private Image loadingBG;
    [SerializeField] private TextMeshProUGUI loaderInfoText;
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TextMeshProUGUI _additionalLoadingText;

    public bool isLoadingNewScene;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }

    public void LoadLevel(string sceneName, bool updateSceneProgress = false) {
        _progressBar.value = 0f;
        Messenger.Broadcast(UISignals.STARTED_LOADING_SCENE, sceneName);
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        isLoadingNewScene = true;
        StartCoroutine(LoadLevelAsynchronously(sceneName, updateSceneProgress));
    }

    private IEnumerator LoadLevelAsynchronously(string sceneName, bool updateSceneProgress) {
        SetLoadingState(true);
        // UpdateLoadingInfo($"Loading {sceneName}...");
        var unloader = Resources.UnloadUnusedAssets();
        while (!unloader.isDone) {
            yield return null;
        }
        GC.Collect();
        yield return null;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;
        
        while (asyncOperation.progress < 0.9f) {
            // _progress = 0.5f * asyncOperation.progress / 0.9f;
            if (updateSceneProgress) {
                UpdateLoadingBar(asyncOperation.progress, 2f);    
            }
            
        }
        asyncOperation.allowSceneActivation = true;
        isLoadingNewScene = false;
    }

    public void UpdateLoadingBar(float value, float duration) {
        //loaderText.text = (100f * amount).ToString("F0") + " %";
        _progressBar.DOValue(value, duration);
        // _progressBar.value = amount;
    }
    public void UpdateLoadingInfo(string info) {
        loaderInfoText.text = info;
    }
    public bool IsLoadingScreenActive() {
        return loaderGO.activeInHierarchy;
    }
    public void SetLoadingState(bool state) {
        loaderGO.SetActive(state);
        // if (WorldConfigManager.Instance.isDemoBuild) {
        //     _additionalLoadingText.text = "This is a preview build. Only includes Tutorial.";
        // } else {
        //     _additionalLoadingText.text = "This is a preview build. Scenarios still have some missing features that will be added prior to launch.";
        // }
    }
}
