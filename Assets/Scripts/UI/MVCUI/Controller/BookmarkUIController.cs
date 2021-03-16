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
        });
    }
    private void Awake() {
        InstantiateUI();
    }
    public void OnClickHide() {
        m_bookmarkUIView.Hide();
    }
    public void OnClickShow() {
        m_bookmarkUIView.Show();
    }
}
