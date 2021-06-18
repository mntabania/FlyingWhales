using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using BayatGames.SaveGameFree;
using Object_Pools;
using Ruinarch;
using Settings;
using TMPro;

public class MainMenuManager : MonoBehaviour {

    public static MainMenuManager Instance;

    //public ParentPlayerSkillTreeUI[] parentPlayerSkillTrees;

    //public TextMeshProUGUI expText;

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
        AudioManager.Instance.ResetAndPlayMainMenuMusic();
        AudioManager.Instance.TransitionToMainMenu();
        MainMenuUI.Instance.ShowMenuButtons();
        MainMenuUI.Instance.ShowEarlyAccessAnnouncement();
        LevelLoaderManager.Instance.SetLoadingState(false);
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
    }
    #endregion

    private void Initialize() {
        SaveManager.Instance.LoadSaveDataPlayer();
        LogPool.WarmUp(10000);
    }

    public void LoadMainGameScene() {
        //WorldConfigManager.Instance.SetDataToUse(newGameData); //Remove so that code will randomly generate world.
        LevelLoaderManager.Instance.LoadLevel("Game");
    }
    public void OnUnlockPlayerSkill() {
        //UpdateExp();
    }
    //private void UpdateExp() {
    //    expText.text = SaveManager.Instance.currentSaveDataPlayer.exp.ToString();
    //}
    public void StartGame() {
        LevelLoaderManager.Instance.SetLoadingState(true);
        AudioManager.Instance.TransitionToLoading();
        LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing Data...");
        LevelLoaderManager.Instance.UpdateLoadingBar(0.1f, 3f);
        LoadMainGameScene();
    }
}
