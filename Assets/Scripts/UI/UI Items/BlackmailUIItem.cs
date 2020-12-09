using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;

public class BlackmailUIItem : MonoBehaviour {

    public static System.Action<IIntel> onChooseBlackmail;
    
    public TextMeshProUGUI txtLog;
    public RuinarchButton btnChoose;

    private IIntel _intel;
    
    private void OnEnable() {
        btnChoose.onClick.AddListener(OnClickIntel);
    }
    private void OnDisable() {
        btnChoose.onClick.RemoveListener(OnClickIntel);
    }

    public void SetItemDetails(IIntel p_intel) {
        _intel = p_intel;
        txtLog.text = p_intel.log.logText;
    }
    
    #region Button Clicks
    private void OnClickIntel() {
        onChooseBlackmail?.Invoke(_intel);
    }
    #endregion
}
