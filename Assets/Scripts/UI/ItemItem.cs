using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZObjectPools;
using TMPro;
using UnityEngine.UI;

public class ItemItem : PooledObject {
    public TextMeshProUGUI itemButtonText;
    public Toggle itemToggle;

    public TILE_OBJECT_TYPE item { get; private set; }

    public void SetItem(TILE_OBJECT_TYPE item) {
        this.item = item;
        UpdateData();
        Messenger.AddListener<TILE_OBJECT_TYPE>(Signals.PLAYER_NO_ACTIVE_ITEM, OnPlayerNoActiveItem);
    }

    private void UpdateData() {
        itemButtonText.text = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(item.ToString());
    }
    private void OnPlayerNoActiveItem(TILE_OBJECT_TYPE item) {
        if(this.item == item) {
            if (itemToggle.isOn) {
                itemToggle.isOn = false;
            }
        }
    }
    public void OnToggleItem(bool state) {
        PlayerManager.Instance.player.SetCurrentlyActiveItem(TILE_OBJECT_TYPE.NONE);
        if (state) {
            PlayerManager.Instance.player.SetCurrentlyActiveItem(item);
        } 
    }

    public override void Reset() {
        base.Reset();
        item = TILE_OBJECT_TYPE.NONE;
        Messenger.RemoveListener<TILE_OBJECT_TYPE>(Signals.PLAYER_NO_ACTIVE_ITEM, OnPlayerNoActiveItem);
    }
}
