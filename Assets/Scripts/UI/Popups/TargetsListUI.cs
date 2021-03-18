using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TargetsListUI : PopupMenuBase {

    [SerializeField] private Toggle tglTargets; 
    [SerializeField] private StoredTargetUIItem[] targetItems;
    public override void Open() {
        base.Open();
        tglTargets.SetIsOnWithoutNotify(true);
    }
    public override void Close() {
        base.Close();
        tglTargets.SetIsOnWithoutNotify(false);
    }

    public void Initialize() {
        Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_STORED_TARGET, OnPlayerStoredTarget);
        Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, OnPlayerRemovedStoredTarget);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
        UpdateItems();
    }
    private void OnCharacterChangedName(Character arg1) {
        UpdateItems();
    }
    private void OnPlayerRemovedStoredTarget(IStoredTarget p_target) {
        UpdateItems();
    }
    private void OnPlayerStoredTarget(IStoredTarget p_target) {
        UpdateItems();
    }
    private void UpdateItems() {
        for (int i = 0; i < targetItems.Length; i++) {
            StoredTargetUIItem item = targetItems[i];
            IStoredTarget target = PlayerManager.Instance.player.storedTargetsComponent.allStoredTargets.ElementAtOrDefault(i);
            if (target == null) {
                item.gameObject.SetActive(false);
            } else {
                item.gameObject.SetActive(true);
                item.SetTarget(target);
            }
        }
    }
}
