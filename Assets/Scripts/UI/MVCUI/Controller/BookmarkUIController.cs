using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class BookmarkUIController : MVCUIController, BookmarkUIView.IListener {
    [SerializeField]
    private BookmarkUIModel m_bookmarkUIModel;
    private BookmarkUIView m_bookmarkUIView;
    
    //Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
    [ContextMenu("Instantiate UI")]
    public override void InstantiateUI() {
        BookmarkUIView.Create(_canvas, m_bookmarkUIModel, (p_ui) => {
            m_bookmarkUIView = p_ui;
            m_bookmarkUIView.Subscribe(this);
            InitUI(p_ui.UIModel, p_ui);
            m_bookmarkUIView.Show();
            p_ui.UIModel.transform.SetSiblingIndex(siblingIndex);
        });
    }
    private void Awake() {
        InstantiateUI();
        m_bookmarkUIView.Hide();
        Messenger.AddListener<BookmarkCategory>(PlayerSignals.BOOKMARK_CATEGORY_ADDED, OnBookmarkCategoryAdded);
        Messenger.AddListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, OnLoadoutSelected);
    }
    private void OnLoadoutSelected() {
        m_bookmarkUIView.Show();
        UIManager.Instance.OnBookmarkMenuShow();
    }
    private void OnBookmarkCategoryAdded(BookmarkCategory p_category) {
        m_bookmarkUIView.CreateBookmarkCategoryItem(p_category);
    }
    public void OnClickHide() {
        m_bookmarkUIView.Hide();
        UIManager.Instance.OnBookmarkMenuHide();
    }
    public void OnClickShow() {
        m_bookmarkUIView.Show();
        UIManager.Instance.OnBookmarkMenuShow();
    }
    public UIHoverPosition GetHoverPosition() {
        return m_bookmarkUIView.UIModel.tooltipHoverPosition;
    }
    public Transform GetUIModelTransform() {
        return m_bookmarkUIView.UIModel.transform;
    }
}
