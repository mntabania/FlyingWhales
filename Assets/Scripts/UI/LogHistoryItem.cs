using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LogHistoryItem : LogItem {

    [SerializeField] private TextMeshProUGUI logLbl;
    [SerializeField] private TextMeshProUGUI dateLbl;
    [SerializeField] private Image logBG;
    [SerializeField] private EnvelopContentUnityUI envelopContent;

    //private bool _isInspected;

    public override void SetLog(Log log) {
        base.SetLog(log);
        this.name = log.id.ToString();
        dateLbl.text = log.date.ConvertToContinuousDaysWithTime();
        if (_log.fillers.Count > 0) {
            this.logLbl.text = UtilityScripts.Utilities.LogReplacer(_log);
        } else {
            this.logLbl.text = LocalizationManager.Instance.GetLocalizedValue(_log.category, _log.file, _log.key);
        }
        //if (log.isInspected || GameManager.Instance.inspectAll) {
        //    dateLbl.text = "Day " + new GameDate((int) log.month, log.day, log.year, log.hour).GetDayAndTicksString();
        //    if (_log.fillers.Count > 0) {
        //        this.logLbl.text = Utilities.LogReplacer(_log);
        //    } else {
        //        this.logLbl.text = LocalizationManager.Instance.GetLocalizedValue(_log.category, _log.file, _log.key);
        //    }
        //} else {
        //    int numOfChars = LocalizationManager.Instance.GetLocalizedValue(_log.category, _log.file, _log.key).Length;
        //    dateLbl.text = "???";
        //    this.logLbl.text = "???";
        //    for (int i = 0; i < numOfChars; i++) {
        //        this.logLbl.text += " ";
        //    }
        //}

        //if (!this.gameObject.activeSelf) {
        //    throw new System.Exception("Log Item is not active!");
        //}
        EnvelopContentExecute();
    }
    public void EnvelopContentExecute() {
        envelopContent.Execute();
    }

    public void SetLogColor(Color color) {
        //logBG.color = color;
    }

    public void ShowLogDebugInfo() {
        //if (log.fromInteraction != null && log.fromInteraction.intel != null) {
        //    string text = log.GetLogDebugInfo();
        //    text += "\n\n<i>(Double Click to Obatin intel)</i>";
        //    UIManager.Instance.ShowSmallInfo(text);
        //}
    }
    public void HideLogDebugInfo() {
        UIManager.Instance.HideSmallInfo();
    }

    public void CheckForObtain(BaseEventData baseData) {
        PointerEventData pData = baseData as PointerEventData;
        if (pData.clickCount == 2) { //double click
            ObtainIntel();
        }
    }

    public void ObtainIntel() {
        //if (log.fromInteraction != null && log.fromInteraction.intel != null) {
        //    if (!PlayerManager.Instance.player.AlreadyHasIntel(log.fromInteraction.intel)) {
        //        log.fromInteraction.intel.SetLog(log);
        //        PlayerManager.Instance.player.AddIntel(log.fromInteraction.intel);
        //    }
        //}
    }
}
