using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.UI;

public class SummonListUI : PopupMenuBase {

    [Header("Minion List")]
    [SerializeField] private GameObject monsterCapacityItemPrefab;
    [SerializeField] private ScrollRect summonListScrollView;

    [SerializeField] private UIHoverPosition _hoverPosition;
    [SerializeField] private Toggle _mainToggle;
    
    private List<MonsterCapacityNameplateItem> _summonPlayerSkillItems;

    public override void Open() {
        base.Open();
        _mainToggle.SetIsOnWithoutNotify(true);
        UpdateAndCreateNewItems();
    }
    public override void Close() {
        base.Close();
        _mainToggle.SetIsOnWithoutNotify(false);
    }
    public void Initialize() {
        _summonPlayerSkillItems = new List<MonsterCapacityNameplateItem>();
        Messenger.AddListener<MonsterCapacity>(PlayerSignals.PLAYER_GAINED_NEW_MONSTER, CreateMonsterCapacityItem);
        Messenger.AddListener<MonsterCapacity>(PlayerSignals.PLAYER_REMOVED_MONSTER, RemoveMonsterCapacityItem);
        Messenger.AddListener<MonsterCapacity>(PlayerSignals.PLAYER_UPDATED_MONSTER_CHARGES_OR_CAPACITY, OnPlayerUpdatedMonsterChargesOrCapacity);
    }
    private void UpdateAndCreateNewItems() {
        foreach (var monsterCharge in PlayerManager.Instance.player.monsterCharges) {
            MonsterCapacityNameplateItem nameplateItem = GetMonsterCapacityItem(monsterCharge.Value);
            if (nameplateItem == null) {
                //create new item
                CreateMonsterCapacityItem(monsterCharge.Value);
            } else {
                nameplateItem.UpdateData(monsterCharge.Value);
            }
        }
    }
    private void OnPlayerUpdatedMonsterChargesOrCapacity(MonsterCapacity p_monsterCapacity) {
        UpdateItemData();
    }
    public void UpdateList() {
        UpdateAndCreateNewItems();
    }
    private void UpdateItemData() {
        int index = 0;
        foreach (var monsterCharge in PlayerManager.Instance.player.monsterCharges) {
            MonsterCapacityNameplateItem nameplateItem = _summonPlayerSkillItems[index];
            nameplateItem.UpdateData(monsterCharge.Value);
            index++;
        }
    }
    private void CreateMonsterCapacityItem(MonsterCapacity p_monsterCapacity) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(monsterCapacityItemPrefab.name, Vector3.zero, Quaternion.identity, summonListScrollView.content);
        MonsterCapacityNameplateItem nameplateItem = go.GetComponent<MonsterCapacityNameplateItem>();
        nameplateItem.SetObject(p_monsterCapacity);
        _summonPlayerSkillItems.Add(nameplateItem);
    }
    private void RemoveMonsterCapacityItem(MonsterCapacity p_monsterCapacity) {
        MonsterCapacityNameplateItem nameplateItem = GetMonsterCapacityItem(p_monsterCapacity);
        if (nameplateItem != null) {
            _summonPlayerSkillItems.Remove(nameplateItem);
            ObjectPoolManager.Instance.DestroyObject(nameplateItem);
        }
    }
    private MonsterCapacityNameplateItem GetMonsterCapacityItem(MonsterCapacity p_monsterCapacity) {
        for (int i = 0; i < _summonPlayerSkillItems.Count; i++) {
            MonsterCapacityNameplateItem nameplateItem = _summonPlayerSkillItems[i];
            if (nameplateItem.summonType == p_monsterCapacity.summonType) {
                return nameplateItem;
            }            
        }
        return null;
    }
    public void ToggleSummonList(bool isOn) {
        if (isOn) {
            Open();
        } else {
            Close();
        }
    }
}
