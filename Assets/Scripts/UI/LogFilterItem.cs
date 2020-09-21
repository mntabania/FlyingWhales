using UnityEngine;
using UnityEngine.UI;

public class LogFilterItem : MonoBehaviour {

    [SerializeField] private LOG_TAG logTag;
    [SerializeField] private LogsWindow _logsWindow;
    [SerializeField] private Toggle toggle;

    #region getters
    public bool isOn => toggle.isOn;
    public LOG_TAG filterType => logTag;
    #endregion
    
    public void OnToggleFilter(bool state) {
        _logsWindow.OnToggleFilter(state, logTag);
    }
    public void SetIsOnWithoutNotify(bool state) {
        toggle.SetIsOnWithoutNotify(state);
    }
}
