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
    [SerializeField] private Image logBG;
    [SerializeField] private EnvelopContentUnityUI envelopContent;
    [SerializeField] private EventLabel eventLabel;
    private UIHoverPosition _hoverPosition;

    //private bool _isInspected;

    public override void SetLog(Log log) {
        base.SetLog(log);
        this.name = log.id.ToString();
        dateLbl.text = log.date.ConvertToContinuousDaysWithTime();
        this.logLbl.text = _log.fillers.Count > 0 ? UtilityScripts.Utilities.LogReplacer(_log) 
            : LocalizationManager.Instance.GetLocalizedValue(_log.category, _log.file, _log.key);
        eventLabel.SetOnClickAction(OnClickObjectInLog);
        EnvelopContentExecute();
    }
    private void EnvelopContentExecute() {
        envelopContent.Execute();
    }
    public void SetHoverPosition(UIHoverPosition hoverPosition) {
        _hoverPosition = hoverPosition;
    }

    private void OnClickObjectInLog(object obj) {
        IPointOfInterest pointOfInterest = UIManager.Instance.GetCurrentlySelectedPOI();
        if (pointOfInterest != null) {
            Messenger.Broadcast(Signals.LOG_HISTORY_OBJECT_CLICKED, obj, log, pointOfInterest);    
        }
        UIManager.Instance.OpenObjectUI(obj);
    }
    
    
    public void SetLogColor(Color color) {
        //logBG.color = color;
    }

    public void OnHoverOverLog(object obj) {
        if (obj is string indexText) {
            int index = Int32.Parse(indexText);
            LogFiller logFiller = log.fillers[index];
            if (logFiller.obj is Character character && _hoverPosition != null) {
                UIManager.Instance.ShowCharacterNameplateTooltip(character, _hoverPosition);
            }
        }
    }
    public void OnHoverOutLog() {
        UIManager.Instance.HideCharacterNameplateTooltip();
    }
    

}
