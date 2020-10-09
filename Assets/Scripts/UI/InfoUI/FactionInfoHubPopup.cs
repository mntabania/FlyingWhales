using UnityEngine;
using UnityEngine.UI;

public class FactionInfoHubPopup : PopupMenuBase {

    [SerializeField] private Toggle villagersToggle;
    public override void Close() {
        base.Close();
        villagersToggle.SetIsOnWithoutNotify(false);
    }
    public override void Open() {
        base.Open();
        villagersToggle.SetIsOnWithoutNotify(true);
    }
}
