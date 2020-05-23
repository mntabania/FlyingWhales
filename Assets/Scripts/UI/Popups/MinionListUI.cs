using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinionListUI : PopupMenuBase {
    
    [Header("Minion List")]
    [SerializeField] private GameObject activeMinionItemPrefab;
    [SerializeField] private GameObject reserveMinionItemPrefab;
    [SerializeField] private ScrollRect minionListScrollView;
    [SerializeField] private GameObject minionListGO;
    [SerializeField] private UIHoverPosition minionListCardTooltipPos;
    [SerializeField] private Toggle minionListToggle;
    [SerializeField] private RectTransform activeHeader;
    [SerializeField] private RectTransform reserveHeader;

    private List<SummonMinionPlayerSkillNameplateItem> _minionPlayerSkillItems;

    public override void Open() {
        base.Open();
        UpdateMinionPlayerSkillItems();
    }
    public void Initialize() {
        _minionPlayerSkillItems = new List<SummonMinionPlayerSkillNameplateItem>();
        Messenger.AddListener<Minion>(Signals.PLAYER_GAINED_MINION, OnGainMinion);
        Messenger.AddListener<Minion>(Signals.PLAYER_LOST_MINION, OnLostMinion);
        Messenger.AddListener<SPELL_TYPE>(Signals.ADDED_PLAYER_MINION_SKILL, OnGainPlayerMinionSkill);
    }

    private void UpdateMinionPlayerSkillItems() {
        for (int i = 0; i < _minionPlayerSkillItems.Count; i++) {
            _minionPlayerSkillItems[i].SetCount(_minionPlayerSkillItems[i].spellData.charges, true);
        }
    }
    //private void UpdateMinionList() {
    //    UtilityScripts.Utilities.DestroyChildren(minionListScrollView.content);
    //    for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
    //        Minion currMinion = PlayerManager.Instance.player.minions[i];
    //        CreateNewMinionItem(currMinion);
    //    }
    //}
    private void CreateNewActiveMinionItem(Minion minion) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(activeMinionItemPrefab.name, Vector3.zero, Quaternion.identity, minionListScrollView.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        item.SetObject(minion.character);
        item.AddHoverEnterAction((character) => UIManager.Instance.ShowMinionCardTooltip(character.minion, minionListCardTooltipPos));
        item.AddHoverExitAction((character) => UIManager.Instance.HideMinionCardTooltip());
        item.SetAsDefaultBehaviour();
        item.transform.SetSiblingIndex(reserveHeader.GetSiblingIndex());
    }
    private void CreateNewReserveMinionItem(SPELL_TYPE minionPlayerSkillType) {
        MinionPlayerSkill minionPlayerSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillData(minionPlayerSkillType);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(reserveMinionItemPrefab.name, Vector3.zero, Quaternion.identity, minionListScrollView.content);
        SummonMinionPlayerSkillNameplateItem item = go.GetComponent<SummonMinionPlayerSkillNameplateItem>();
        item.SetObject(minionPlayerSkill);
        item.SetCount(minionPlayerSkill.charges, true);
        item.ClearAllOnClickActions();
        item.ClearAllHoverEnterActions();
        item.AddHoverEnterAction(OnHoverEnterReserveMinion);
        item.AddHoverExitAction(OnHoverExitReserveMinion);
        _minionPlayerSkillItems.Add(item);
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
    private void OnGainPlayerMinionSkill(SPELL_TYPE minionPlayerSkillType) {
        CreateNewReserveMinionItem(minionPlayerSkillType);
    }
    private void OnGainMinion(Minion minion) {
        CreateNewActiveMinionItem(minion);
        UpdateMinionPlayerSkillItems();
    }
    private void OnLostMinion(Minion minion) {
        DeleteMinionItem(minion);
    }
    private void OnHoverEnterReserveMinion(SpellData spellData) {
        PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(spellData);
    }
    private void OnHoverExitReserveMinion(SpellData spellData) {
        PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
    }
    public void ToggleMinionList(bool isOn) {
        if (isOn) {
            Open();
        } else {
            Close();
        }
    }
    public void HideMinionList() {
        if (minionListToggle.isOn) {
            minionListToggle.isOn = false;
        }
    }
}
