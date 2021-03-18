using EZObjectPools;
using TMPro;
using UnityEngine;
using UtilityScripts;
using UnityEngine.UI;

public class BookmarkProgressItemUI : PooledObject, RuinarchProgressable.IListener, BookmarkableEventDispatcher.IListener {
    [SerializeField] private TextMeshProUGUI lblName;
    [SerializeField] private Slider sliderProgress;
    [SerializeField] private Button btnMain;

    private System.Action _onResetAction;
    
    public void SetProgressable(RuinarchProgressable p_progressable) {
        p_progressable.ListenToProgress(this);
        btnMain.onClick.AddListener(p_progressable.OnSelect);
        _onResetAction += () => OnResetActions(p_progressable);
        lblName.text = p_progressable.name;
        UpdateProgressBar(p_progressable);
        p_progressable.eventDispatcher.Subscribe(this);
        
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
        p_bookmarkable.eventDispatcher.Unsubscribe(this);
        ObjectPoolManager.Instance.DestroyObject(this);
    }
    public override void Reset() {
        base.Reset();
        _onResetAction?.Invoke();
        _onResetAction = null;
        btnMain.onClick.RemoveAllListeners();
    }
}
