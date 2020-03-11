using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using BayatGames.SaveGameFree;

public class MainMenuManager : MonoBehaviour {

    public static MainMenuManager Instance;

    [ContextMenu("Get Combinations")]
    public void GetCombinations() {
        List<int> sample = new List<int> { 1, 2 };
        List<List<int>> result = UtilityScripts.Utilities.ItemCombinations(sample, 3, 3);
        for (int i = 0; i < result.Count; i++) {
            string log = "\n{";
            for (int j = 0; j < result[i].Count(); j++) {
                log += $" {result[i][j]},";
            }
            log += " }";
            Debug.Log(log);
        }
    }

    #region Monobehaviours
    public void Awake() {
        Instance = this;
    }
    private void Start() {
        Initialize();
        AudioManager.Instance.PlayFade("Main Menu", 5, () => MainMenuUI.Instance.ShowMenuButtons());
        LevelLoaderManager.Instance.SetLoadingState(false);
    }
    #endregion
    private void Initialize() {
        SaveManager.Instance.LoadSaveData();
        //loadGameButton.interactable = SaveManager.Instance.currentSave != null;
    }
    
    public void LoadMainGameScene() {
        //WorldConfigManager.Instance.SetDataToUse(newGameData); //Remove so that code will randomly generate world.
        LevelLoaderManager.Instance.LoadLevel("Game");
    }
}
