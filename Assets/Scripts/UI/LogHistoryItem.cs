using System;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LogHistoryItem : LogItem {

    [SerializeField] private TextMeshProUGUI logLbl;
    [SerializeField] private TextMeshProUGUI dateLbl;
    [SerializeField] private EnvelopContentUnityUI envelopContent;
    [SerializeField] private EventLabel eventLabel;
    [SerializeField] private LogsTagButton logsTagButton;
    private UIHoverPosition _hoverPosition;
    
    private void OnEnable() {
        EnvelopContentExecute();
    }
    
    public void SetLog(Log log) {
        name = log.persistentID;
        dateLbl.text = log.gameDate.ConvertToTime();
        logLbl.text = log.logText;
        // log.fillers.Count > 0 ? UtilityScripts.Utilities.LogReplacer(log) : LocalizationManager.Instance.GetLocalizedValue(log.category, log.file, log.key);
        logsTagButton.SetTags(log.tags);
        eventLabel.SetOnLeftClickAction(OnLeftClickObjectInLog);
        eventLabel.SetOnRightClickAction(OnRightClickObjectInLog);
        EnvelopContentExecute();
    }
    private void EnvelopContentExecute() {
        envelopContent.Execute();
    }
    public void SetHoverPosition(UIHoverPosition hoverPosition) {
        _hoverPosition = hoverPosition;
    }
    private void OnLeftClickObjectInLog(object obj) {
        IPointOfInterest pointOfInterest = UIManager.Instance.GetCurrentlySelectedPOI();
        if (pointOfInterest != null) {
            Messenger.Broadcast(UISignals.LOG_HISTORY_OBJECT_CLICKED, obj, logLbl.text, pointOfInterest);    
        }
        UIManager.Instance.OpenObjectUI(obj);
    }
    private void OnRightClickObjectInLog(object obj) {
        if (obj is IPlayerActionTarget playerActionTarget) {
            if (playerActionTarget is Character character) {
                if(character.isLycanthrope) {
                    playerActionTarget = character.lycanData.activeForm;
                }
            }
            UIManager.Instance.ShowPlayerActionContextMenu(playerActionTarget, Input.mousePosition, true);
        }
    }
    public void OnHoverOverLog(object obj) {
        if (obj is Character character && _hoverPosition != null) {
            Character characterToShow = character;
            if(character.isLycanthrope) {
                characterToShow = character.lycanData.activeForm;
            }
            UIManager.Instance.ShowCharacterNameplateTooltip(characterToShow, _hoverPosition);
        }
    }
    public void OnHoverOutLog() {
        UIManager.Instance.HideCharacterNameplateTooltip();
    }
    public void ManualReset() {
        logsTagButton.Reset();
    }
    

}
