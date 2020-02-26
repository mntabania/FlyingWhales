using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinionListUI : PopupMenuBase {
    
    [Header("Minion List")]
    [SerializeField] private GameObject minionItemPrefab;
    [SerializeField] private ScrollRect minionListScrollView;
    [SerializeField] private GameObject minionListGO;
    [SerializeField] private UIHoverPosition minionListCardTooltipPos;
    [SerializeField] private Toggle minionListToggle;
    public override void Close() {
        minionListToggle.isOn = false;
    }
    public override void Open() {
        minionListToggle.isOn = true;
    }
    public void Initialize() {
        Messenger.AddListener<Minion>(Signals.SUMMON_MINION, OnGainedMinion);
        Messenger.AddListener<Minion>(Signals.UNSUMMON_MINION, OnLostMinion);
    }
    
    private void UpdateMinionList() {
        UtilityScripts.Utilities.DestroyChildren(minionListScrollView.content);
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            Minion currMinion = PlayerManager.Instance.player.minions[i];
            CreateNewMinionItem(currMinion);
        }
    }
    private void CreateNewMinionItem(Minion minion) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(minionItemPrefab.name, Vector3.zero, Quaternion.identity, minionListScrollView.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        item.SetObject(minion.character);
        item.AddHoverEnterAction((character) => UIManager.Instance.ShowMinionCardTooltip(character.minion, minionListCardTooltipPos));
        item.AddHoverExitAction((character) => UIManager.Instance.HideMinionCardTooltip());
        item.SetAsDefaultBehaviour();
    }
    private void DeleteMinionItem(Minion minion) {
        CharacterNameplateItem item = GetMinionItem(minion);
        if (item != null) {
            ObjectPoolManager.Instance.DestroyObject(item);
        }
    }
    private CharacterNameplateItem GetMinionItem(Minion minion) {
        CharacterNameplateItem[] items = UtilityScripts.GameUtilities.GetComponentsInDirectChildren<CharacterNameplateItem>(minionListScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            CharacterNameplateItem item = items[i];
            if (item.character == minion.character) {
                return item;
            }
        }
        return null;
    }
    private void OnGainedMinion(Minion minion) {
        CreateNewMinionItem(minion);
    }
    private void OnLostMinion(Minion minion) {
        DeleteMinionItem(minion);
    }
    public void ToggleMinionList(bool isOn) {
        minionListGO.SetActive(isOn);
    }
    public void HideMinionList() {
        if (minionListToggle.isOn) {
            minionListToggle.isOn = false;
        }
    }
}
