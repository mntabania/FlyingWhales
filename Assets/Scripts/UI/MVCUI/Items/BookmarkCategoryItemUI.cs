using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookmarkCategoryItemUI : PooledObject {
    [SerializeField] private TextMeshProUGUI lblHeaderName;
    [SerializeField] private Button btnHeader;
    [SerializeField] private GameObject goContent;
    [SerializeField] private Transform contentParent;

    public void Initialize(BookmarkCategory p_category) {
        lblHeaderName.text = p_category.displayName;
    }
}
