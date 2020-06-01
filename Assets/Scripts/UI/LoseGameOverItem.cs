using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoseGameOverItem : MonoBehaviour {
    public GameObject go;
    public TextMeshProUGUI experienceText;

    public void Open() {
        UpdateData();
        go.SetActive(true);
    }

    public void Close() {
        go.SetActive(false);
    }

    private void UpdateData() {
        int experienceGained = 100;
        PlayerManager.Instance.player.AdjustExperience(experienceGained);
        experienceText.text = "+" + experienceGained;
        SaveManager.Instance.Save();
    }

    public void BackToMainMenu() {
        LevelLoaderManager.Instance.LoadLevel("MainMenu");
        Messenger.Cleanup();
    }
}
