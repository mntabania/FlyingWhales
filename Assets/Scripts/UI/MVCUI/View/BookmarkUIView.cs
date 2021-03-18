using System;
using System.Linq;
using Ruinarch.MVCFramework;
using UnityEngine;

public class BookmarkUIView : MVCUIView{
    #region interface for listener
    public interface IListener {
        void OnClickHide();
        void OnClickShow();
    }
    #endregion
    
    #region MVC Properties and functions to override
    /*
     * this will be the reference to the model 
     * */
    public BookmarkUIModel UIModel => _baseAssetModel as BookmarkUIModel;
    /*
     * Call this Create method to Initialize and instantiate the UI.
     * There's a callback on the controller if you want custom initialization
     * */
    public static void Create(Canvas p_canvas, BookmarkUIModel p_assets, Action<BookmarkUIView> p_onCreate) {
        var go = new GameObject(typeof(BookmarkUIView).ToString());
        var gui = go.AddComponent<BookmarkUIView>();
        var assetsInstance = Instantiate(p_assets);
        gui.Init(p_canvas, assetsInstance);
        if (p_onCreate != null)
        {
            p_onCreate.Invoke(gui);
        }
    }
    #endregion
    
    #region Subscribe/Unsubscribe for IListener
    public void Subscribe(IListener p_listener) {
        UIModel.onClickHide += p_listener.OnClickHide;
        UIModel.onClickShow += p_listener.OnClickShow;
    }
    public void Unsubscribe(IListener p_listener) {
        UIModel.onClickHide -= p_listener.OnClickHide;
        UIModel.onClickShow -= p_listener.OnClickShow;
    }
    #endregion

    #region User defined functions
    public void Hide() {
        UIModel.rtWindow.anchoredPosition = UIModel.posHidden;
        UIModel.btnShow.gameObject.SetActive(true);
        UIModel.btnHide.gameObject.SetActive(false);
    }
    public void Show() {
        UIModel.rtWindow.anchoredPosition = UIModel.posShowing;
        UIModel.btnHide.gameObject.SetActive(true);
        UIModel.btnShow.gameObject.SetActive(false);
    }
    public void CreateBookmarkCategoryItem(BookmarkCategory p_category) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(UIModel.goBookmarkCategoryPrefab.name, Vector3.zero, Quaternion.identity, UIModel.scrollRectBookmarks.content);
        BookmarkCategoryItemUI categoryItemUI = go.GetComponent<BookmarkCategoryItemUI>();
        categoryItemUI.Initialize(p_category);

        //order items by order in category enum
        BookmarkCategoryItemUI[] items = UIModel.scrollRectBookmarks.content.GetComponentsInChildren<BookmarkCategoryItemUI>().OrderBy(i => i.category).ToArray();
        for (int i = 0; i < items.Length; i++) {
            BookmarkCategoryItemUI item = items[i];
            item.transform.SetAsLastSibling();
        }
    }
    #endregion
}
