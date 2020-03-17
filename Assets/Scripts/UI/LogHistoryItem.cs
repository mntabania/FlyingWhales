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
        this.logLbl.text = _log.fillers.Count > 0 ? UtilityScripts.Utilities.LogReplacer(_log) 
            : LocalizationManager.Instance.GetLocalizedValue(_log.category, _log.file, _log.key);
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

}
