using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;

public class BlackmailUIItem : MonoBehaviour {

    public static System.Action<IIntel, bool> onChooseBlackmail;
    
    public TextMeshProUGUI txtLog;
    public RuinarchToggle tglChoose;

    private IIntel _intel;
    
    private void OnEnable() {
        tglChoose.onValueChanged.AddListener(OnToggleIntel);
    }
    private void OnDisable() {
        tglChoose.onValueChanged.RemoveListener(OnToggleIntel);
    }

    public void SetItemDetails(IIntel p_intel) {
        _intel = p_intel;
        txtLog.text = p_intel.log.logText;
    }
    
    #region Button Clicks
    private void OnToggleIntel(bool p_isOn) {
        onChooseBlackmail?.Invoke(_intel, p_isOn);
    }
    #endregion
}
