using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;

public class UnlockStructureUIModel : MVCUIModel {
    public UnlockStructureItemUI[] structureItems;
    
    public System.Action onClickClose;
    public RuinarchButton btnClose;
    private void OnEnable() {
        btnClose.onClick.AddListener(OnClickClose);
    }
    private void OnDisable() {
        btnClose.onClick.RemoveListener(OnClickClose);
    }
    private void OnClickClose() {
        onClickClose?.Invoke();
    }
}
