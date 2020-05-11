using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
        StartCoroutine(LoadLevelAsynchronously(sceneName, updateSceneProgress));
    }

    IEnumerator LoadLevelAsynchronously(string sceneName, bool updateSceneProgress) {
        SetLoadingState(true);
        // UpdateLoadingInfo($"Loading {sceneName}...");
        yield return null;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;
        
        while (asyncOperation.progress < 0.9f) {
            // _progress = 0.5f * asyncOperation.progress / 0.9f;
            if (updateSceneProgress) {
                UpdateLoadingBar(asyncOperation.progress, 2f);    
            }
            
        }

        //asyncOperation.allowSceneActivation = true;
        //float newProg = _progress;
        
        // while (_progress < 1f) {
        //     yield return null;
        //     _progress += 0.05f;
        //     if(_progress > 1f) {
        //         _progress = 1f;
        //     }
        //     UpdateLoadingBar(_progress);
        // }
        asyncOperation.allowSceneActivation = true;
    }

    public void UpdateLoadingBar(float value, float duration) {
        //loaderText.text = (100f * amount).ToString("F0") + " %";
        _progressBar.DOValue(value, duration);
        // _progressBar.value = amount;
    }
    public void UpdateLoadingInfo(string info) {
        loaderInfoText.text = info;
    }
    public void SetLoadingState(bool state) {
        loaderGO.SetActive(state);
        // if (state) {
        //     StartSlideShow();
        // } else {
        //     StopSlideShow();
        // }
    }

    #region Slideshow
    [SerializeField] private Sprite[] _loadingBackgrounds;
    private Sequence _slideShowSequence;
    private void StartSlideShow() {
        _slideShowSequence = DOTween.Sequence();
        _slideShowSequence.Append(loadingBG.DOFade(0f, 1f).OnComplete(SetNextBackground));
        _slideShowSequence.Append(loadingBG.DOFade(1f, 1f));
        _slideShowSequence.AppendInterval(5f);
        _slideShowSequence.SetLoops(-1);
        _slideShowSequence.Play();
    }
    private void SetNextBackground() {
        Sprite nextBG = CollectionUtilities.GetNextElementCyclic(_loadingBackgrounds,
            Array.IndexOf(_loadingBackgrounds, loadingBG.sprite));
        loadingBG.sprite = nextBG;
    }
    private void StopSlideShow() {
        _slideShowSequence?.Kill();
    }
    #endregion
}
