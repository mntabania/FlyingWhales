using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookmarkTextItemUI : PooledObject {
    [SerializeField] private TextMeshProUGUI lblName;
    [SerializeField] private Button btnMain;
    [SerializeField] private Button btnRemove;
    
    public void SetBookmark(IBookmarkable p_bookmarkable) {
        lblName.text = p_bookmarkable.bookmarkName;
        btnMain.onClick.AddListener(p_bookmarkable.OnSelectBookmark);
        btnRemove.onClick.AddListener(p_bookmarkable.RemoveBookmark);
    }
    public override void Reset() {
        base.Reset();
        btnMain.onClick.RemoveAllListeners();
        btnRemove.onClick.RemoveAllListeners();
    }
}
