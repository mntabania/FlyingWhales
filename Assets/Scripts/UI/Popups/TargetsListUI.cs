using System;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.UI;

public class TargetsListUI : PopupMenuBase, BookmarkableEventDispatcher.IListener {

    [SerializeField] private Toggle tglTargets; 
    [SerializeField] private StoredTargetUIItem[] targetItems;
    [SerializeField] private UIHoverPosition tooltipPos;
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
        for (int i = 0; i < targetItems.Length; i++) {
            StoredTargetUIItem item = targetItems[i];
            item.Initialize(OnHoverOver, OnHoverOut);
        }
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
    private void OnHoverOver(IStoredTarget p_target) {
        if (p_target is Character character) {
            UIManager.Instance.ShowCharacterNameplateTooltip(character, tooltipPos);
        } else if (p_target is TileObject tileObject) {
            UIManager.Instance.ShowTileObjectNameplateTooltip(tileObject, tooltipPos);
        } else if (p_target is LocationStructure structure) {
            UIManager.Instance.ShowStructureNameplateTooltip(structure, tooltipPos);
        }
    }
    private void OnHoverOut(IStoredTarget p_target) {
        if (p_target is Character) {
            UIManager.Instance.HideCharacterNameplateTooltip();
        } else if (p_target is TileObject) {
            UIManager.Instance.HideTileObjectNameplateTooltip();
        } else if (p_target is LocationStructure) {
            UIManager.Instance.HideStructureNameplateTooltip();
        } else {
            UIManager.Instance.HideCharacterNameplateTooltip();
            UIManager.Instance.HideTileObjectNameplateTooltip();
            UIManager.Instance.HideStructureNameplateTooltip();
        }
    }
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
