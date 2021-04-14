using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TargetsListUI : PopupMenuBase, BookmarkableEventDispatcher.IListener {

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
        // Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
        // Messenger.AddListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchFromLimbo);
        UpdateItems();
    }
    // private void OnCharacterSwitchFromLimbo(Character p_inLimbo, Character p_outOfLimbo) {
    //     if (PlayerManager.Instance.player.storedTargetsComponent.allStoredTargets.Contains(p_inLimbo) || 
    //         PlayerManager.Instance.player.storedTargetsComponent.allStoredTargets.Contains(p_outOfLimbo)) {
    //         UpdateItems();
    //     }
    // }
    // private void OnCharacterChangedName(Character p_character) {
    //     UpdateItems();
    // }
    private void OnPlayerRemovedStoredTarget(IStoredTarget p_target) {
        p_target.bookmarkEventDispatcher.Unsubscribe(this, p_target);
        UpdateItems();
    }
    private void OnPlayerStoredTarget(IStoredTarget p_target) {
        p_target.bookmarkEventDispatcher.Subscribe(this, p_target);    
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
    private void UpdateSpecificItem(IStoredTarget p_target) {
        for (int i = 0; i < targetItems.Length; i++) {
            StoredTargetUIItem item = targetItems[i];
            if (item.target == p_target) {
                item.UpdateName(p_target);
            }
        }
    }
    public void OnBookmarkRemoved(IBookmarkable p_bookmarkable) {
        if (p_bookmarkable is IStoredTarget) {
            UpdateItems();    
        }
    }
    public void OnBookmarkChangedName(IBookmarkable p_bookmarkable) {
        if (p_bookmarkable is IStoredTarget storedTarget) {
            UpdateSpecificItem(storedTarget);    
        }
    }
}
