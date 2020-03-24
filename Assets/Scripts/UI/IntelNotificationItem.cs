using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntelNotificationItem : PlayerNotificationItem {

    public IIntel intel { get; private set; }

    [SerializeField] private Button getIntelBtn;
    [SerializeField] private GameObject convertTooltip;

    public void Initialize(IIntel intel, bool hasExpiry = true, System.Action<PlayerNotificationItem> onDestroyAction = null) {
        this.intel = intel;
        base.Initialize(intel.log, hasExpiry, onDestroyAction);
    }
   
    public void GetIntel() {
        PlayerManager.Instance.player.AddIntel(intel);
        // PlayerManager.Instance.player.ShowNotificationFrom(new Log(GameManager.Instance.Today(), "Character", "Generic", "intel_stored"));
        DeleteNotification();
    }

    //protected override void OnExpire() {
    //    base.OnExpire();
    //    intel.OnIntelExpire();
    //}
    public override void Reset() {
        base.Reset();
        convertTooltip.SetActive(false);
    }
}
