using System;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UtilityScripts;

public class BookmarkCategoryItemUI : PooledObject, BookmarkCategory.IListener {
    [SerializeField] private TextMeshProUGUI lblHeaderName;
    [SerializeField] private Button btnHeader;
    [SerializeField] private GameObject goContent;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject goExpand;
    [SerializeField] private GameObject goCollapse;

    public BOOKMARK_CATEGORY category { get; private set; }
    
    private System.Action _onResetAction;
    private void Awake() {
        btnHeader.onClick.AddListener(ToggleContent);
    }
    public void Initialize(BookmarkCategory p_category) {
        lblHeaderName.text = p_category.displayName;
        category = p_category.bookmarkCategory;
        p_category.SubscribeToEvents(this);
        _onResetAction += () => p_category.UnsubscribeToEvents(this);
        for (int i = 0; i < p_category.bookmarked.Count; i++) {
            IBookmarkable bookmarkable = p_category.bookmarked[i];
            CreateNewBookmarkItem(bookmarkable);
        }
        UpdateExpandCollapseVisual();
    }

    #region Interaction
    private void ToggleContent() {
        goContent.SetActive(!goContent.activeSelf);
        UpdateExpandCollapseVisual();
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent as RectTransform);
    }
    private void UpdateExpandCollapseVisual() {
        bool isContentActive = goContent.activeSelf;
        if (isContentActive) {
            goExpand.SetActive(false);
            goCollapse.SetActive(true);
        }
        else {
            goExpand.SetActive(true);
            goCollapse.SetActive(false);
        }
    }
    #endregion
    
    #region Listeners
    public void OnBookmarkAdded(IBookmarkable p_bookmarkable) {
        gameObject.SetActive(true);
        CreateNewBookmarkItem(p_bookmarkable);
    }
    public void OnBookmarkCategoryEmptiedOut(BookmarkCategory p_category) {
        gameObject.SetActive(false);
    }
    #endregion

    #region Content
    private void CreateNewBookmarkItem(IBookmarkable p_bookmarkable) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(GetPrefabName(p_bookmarkable.bookmarkType), Vector3.zero, Quaternion.identity, contentParent);
        switch (p_bookmarkable.bookmarkType) {
            case BOOKMARK_TYPE.Progress_Bar:
                RuinarchProgressable progressable = p_bookmarkable as RuinarchProgressable;
                Assert.IsNotNull(progressable, $"{p_bookmarkable.bookmarkName} is set as a progress bar type but is not a RuinarchProgressable. At time of creation, only RuinarchProgressables can be timers. Refactor this if that is no longer the case.");
                BookmarkProgressItemUI progressItemUI = go.GetComponent<BookmarkProgressItemUI>();
                progressItemUI.SetProgressable(progressable);
                break;
            case BOOKMARK_TYPE.Text:
            case BOOKMARK_TYPE.Text_With_Cancel:
                BookmarkTextItemUI textItemUI = go.GetComponent<BookmarkTextItemUI>();
                textItemUI.SetBookmark(p_bookmarkable);
                break;
            case BOOKMARK_TYPE.Special:
                SpecialBookmarkTextItemUI specialTextItemUI = go.GetComponent<SpecialBookmarkTextItemUI>();
                specialTextItemUI.SetBookmark(p_bookmarkable);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent as RectTransform);
    }
    private string GetPrefabName(BOOKMARK_TYPE p_bookmarkType) {
        switch (p_bookmarkType) {
            case BOOKMARK_TYPE.Text:
            case BOOKMARK_TYPE.Text_With_Cancel:
                return $"Bookmark_Item_Text_Prefab";
            case BOOKMARK_TYPE.Special:
                return $"Special_Bookmark_Item_Text_Prefab";
            default:
                return $"Bookmark_Item_{p_bookmarkType.ToString()}_Prefab";        
        }
    }
    #endregion
    
    public override void Reset() {
        base.Reset();
        _onResetAction?.Invoke();
        _onResetAction = null;
        category = BOOKMARK_CATEGORY.None;
    }
}
