using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WinGameOverItem : MonoBehaviour {
    public GameObject go;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI experienceText;

    public void Open() {
        UpdateData();
        go.SetActive(true);
    }

    public void Close() {
        go.SetActive(false);
    }

    private void UpdateData() {
        int killCount = 0;
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if (character.faction.isMajorNonPlayerFriendlyNeutral && !(character is Summon) && character.minion == null) {
                if (character.isDead) {
                    killCount++;
                }
            }
        }

        int experienceGained = killCount * 100;
        PlayerManager.Instance.player.AdjustExperience(experienceGained);

        killsText.text = "x" + killCount;
        experienceText.text = "+" + experienceGained;
        SaveManager.Instance.SaveCurrentStateOfWorld();
    }

    public void BackToMainMenu() {
        LevelLoaderManager.Instance.LoadLevel("MainMenu");
    }
}
