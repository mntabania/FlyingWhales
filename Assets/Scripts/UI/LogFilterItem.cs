using UnityEngine;
using UnityEngine.UI;

public class LogFilterItem : MonoBehaviour {

    [SerializeField] private LOG_TAG logTag;
    [SerializeField] private Toggle toggle;

    private System.Action<bool, LOG_TAG> onToggleAction;
    
    #region getters
    public bool isOn => toggle.isOn;
    public LOG_TAG filterType => logTag;
    #endregion

    public void SetOnToggleAction(System.Action<bool, LOG_TAG> onToggleAction) {
        this.onToggleAction = onToggleAction;
    }
    public void OnToggleFilter(bool state) {
        onToggleAction.Invoke(state, logTag);
    }
    public void SetIsOnWithoutNotify(bool state) {
        toggle.SetIsOnWithoutNotify(state);
    }
}
