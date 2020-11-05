using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RenameCharacterWindowUI : MonoBehaviour {
    public TextMeshProUGUI titleLbl;
    public TMP_InputField inputField;
    public GameObject mainWindow;

    private string _characterPersistentID;
    private string _characterCurrentName;

    void Start() {
        Messenger.AddListener<string, string>(Signals.EDIT_CHARACTER_NAME, OnListenedEditCharacterName);
    }

    #region Listeners
    private void OnListenedEditCharacterName(string characterPersistentID, string characterCurrentName) {
        RenameCharacterProcess(characterPersistentID, characterCurrentName);
    }
    #endregion

    #region General
    private void RenameCharacterProcess(string characterPersistentID, string characterCurrentName) {
        _characterPersistentID = characterPersistentID;
        _characterCurrentName = characterCurrentName;
        UpdateTitleLbl();
        SetInitialInputFieldText();
        ShowHideWindow(true);
    }
    private void ShowHideWindow(bool state) {
        mainWindow.SetActive(state);
    }
    private void UpdateTitleLbl() {
        titleLbl.text = "Rename " + _characterCurrentName;
    }
    private void SetInitialInputFieldText() {
        inputField.text = _characterCurrentName;
    }
    #endregion

    #region Button Clicks
    public void OnClickConfirmButton() {
        Messenger.Broadcast(Signals.RENAME_CHARACTER, _characterPersistentID, inputField.text);
        ShowHideWindow(false);
    }
    #endregion
}
