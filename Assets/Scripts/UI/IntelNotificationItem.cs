using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntelNotificationItem : PlayerNotificationItem {

    public IIntel intel { get; private set; }

    [SerializeField] private Button getIntelBtn;
    [SerializeField] private GameObject convertTooltip;
    [SerializeField] private GameObject effectPrefab;

    private string _intelHoverText;

    public void Initialize(IIntel intel, System.Action<PlayerNotificationItem> onDestroyAction = null) {
        this.intel = intel;
        base.Initialize(intel.log, onDestroyAction);
    }
    public void GetIntel() {
        Vector3 pos = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(getIntelBtn.transform.position);
        pos.z = 0f;
        GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(effectPrefab.name,
            pos, Quaternion.identity, InnerMapManager.Instance.transform, true);
        effectGO.transform.position = pos;

        Vector3 intelTabPos = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(PlayerUI.Instance.intelToggle.transform.position);

        Vector3 controlPointA = effectGO.transform.position;
        controlPointA.x -= 5f;
        controlPointA.z = 0f;
		
        Vector3 controlPointB = intelTabPos;
        controlPointB.y -= 5f;
        controlPointB.z = 0f;

        IIntel storedIntel = intel;
        effectGO.transform.DOPath(new[] {intelTabPos, controlPointA, controlPointB}, 0.7f, PathType.CubicBezier).SetEase(Ease.InSine).OnComplete(() => OnReachIntelTab(effectGO, storedIntel));
        
        DeleteNotification();
    }
    private void OnReachIntelTab(GameObject effectGO, IIntel intel) {
        PlayerUI.Instance.DoIntelTabPunchEffect();
        ObjectPoolManager.Instance.DestroyObject(effectGO);
        PlayerManager.Instance.player.AddIntel(intel);
    }

    #region Hover
    public void OnHoverEnter() {
        if(intel != null && string.IsNullOrEmpty(_intelHoverText)) {
            string blackmailText = intel.GetIntelInfoBlackmailText();
            string reactionText = intel.GetIntelInfoRelationshipText();
            _intelHoverText = string.Empty;

            _intelHoverText += blackmailText;
            if (!string.IsNullOrEmpty(_intelHoverText)) {
                _intelHoverText += "\n";
            }
            _intelHoverText += reactionText;
        }
        if (!string.IsNullOrEmpty(_intelHoverText)) {
            UIManager.Instance.ShowSmallInfo(_intelHoverText, pos: _hoverPosition, autoReplaceText: false, relayout:true);
        } else {
            UIManager.Instance.ShowSmallInfo("Doesn't seem very useful, but...", pos: _hoverPosition, autoReplaceText: false, relayout:true);   
        }
    }
    public void OnHoverExit() {
        _intelHoverText = string.Empty;
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    public override void DeleteOldestNotification() {
        intel.OnIntelRemoved(); //cleanup intel
        base.DeleteOldestNotification();
    }
    public override void Reset() {
        base.Reset();
        _intelHoverText = string.Empty;
        convertTooltip.SetActive(false);
    }
}
