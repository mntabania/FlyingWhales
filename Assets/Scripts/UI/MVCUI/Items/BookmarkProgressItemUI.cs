using EZObjectPools;
using TMPro;
using UnityEngine;
using UtilityScripts;
using UnityEngine.UI;

public class BookmarkProgressItemUI : PooledObject, RuinarchProgressable.IListener, BookmarkableEventDispatcher.IListener {
    [SerializeField] private TextMeshProUGUI lblName;
    [SerializeField] private Slider sliderProgress;
    [SerializeField] private Button btnMain;
    [SerializeField] private HoverHandler hoverHandler;
    [SerializeField] private UIHoverPosition hoverPosition;

    private System.Action _onResetAction;
    
    public void SetProgressable(RuinarchProgressable p_progressable) {
        p_progressable.ListenToProgress(this);
        btnMain.onClick.AddListener(p_progressable.OnSelect);
        _onResetAction += () => OnResetActions(p_progressable);
        lblName.text = p_progressable.progressableName;
        UpdateProgressBar(p_progressable);
        p_progressable.bookmarkEventDispatcher.Subscribe(this, p_progressable);
        hoverHandler.AddOnHoverOverAction(() => p_progressable.OnHoverOverBookmarkItem(hoverPosition));
        hoverHandler.AddOnHoverOutAction(p_progressable.OnHoverOutBookmarkItem);
        
    }
    public void OnCurrentProgressChanged(RuinarchProgressable p_progressable) {
        UpdateProgressBar(p_progressable);
    }
    private void UpdateProgressBar(RuinarchProgressable p_progressable) {
        sliderProgress.value = p_progressable.GetCurrentProgressPercent();
    }
    private void OnResetActions(RuinarchProgressable p_progressable) {
        btnMain.onClick.RemoveListener(p_progressable.OnSelect);
        p_progressable.StopListeningToProgress(this);
    }
    public void OnBookmarkRemoved(IBookmarkable p_bookmarkable) {
        RectTransform rect = null;
        RectTransform parentOfParent = null;
        if (transform.parent is RectTransform rectTransform) {
            rect = rectTransform;
            parentOfParent = transform.parent.parent.parent as RectTransform;
        }
        p_bookmarkable.bookmarkEventDispatcher.Unsubscribe(this, p_bookmarkable);
        ObjectPoolManager.Instance.DestroyObject(this);
        if (rect) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);    
        }
        if (parentOfParent) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentOfParent);
        }
    }
    public void OnBookmarkChangedName(IBookmarkable p_bookmarkable) {
        if (p_bookmarkable is RuinarchProgressable progressable) {
            lblName.text = progressable.progressableName;    
        }
    }
    public override void Reset() {
        base.Reset();
        _onResetAction?.Invoke();
        _onResetAction = null;
        btnMain.onClick.RemoveAllListeners();
        hoverHandler.ClearHoverActions();
    }
}
