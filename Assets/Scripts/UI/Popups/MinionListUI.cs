using System.Collections;
using System.Collections.Generic;
using TMPro;
using Traits;
using UnityEngine;
using UnityEngine.UI;

public class MinionListUI : PopupMenuBase {
    
    [Header("Minion List")]
    [SerializeField] private GameObject activeMinionItemPrefab;
    [SerializeField] private GameObject spellItemPrefab;
    [SerializeField] private ScrollRect minionListScrollView;
    [SerializeField] private Toggle minionListToggle;
    [SerializeField] private RectTransform reserveHeader;
    [SerializeField] private UIHoverPosition _hoverPosition;

    private List<SummonMinionPlayerSkillNameplateItem> _minionItems;

    public override void Open() {
        base.Open();
        UpdateMinionPlayerSkillItems();
    }
    public override void Close() {
        base.Close();
        HideMinionList();
    }
    public void Initialize() {
        _minionItems = new List<SummonMinionPlayerSkillNameplateItem>();
        Messenger.AddListener<Minion>(Signals.PLAYER_GAINED_MINION, OnGainMinion);
        Messenger.AddListener<Minion>(Signals.PLAYER_LOST_MINION, OnLostMinion);
        Messenger.AddListener<SPELL_TYPE>(Signals.ADDED_PLAYER_MINION_SKILL, OnGainPlayerMinionSkill);
    }

    private void UpdateMinionPlayerSkillItems() {
        for (int i = 0; i < _minionItems.Count; i++) {
            _minionItems[i].UpdateData();
        }
    }
    private void CreateNewActiveMinionItem(Minion minion) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(activeMinionItemPrefab.name, Vector3.zero, Quaternion.identity, minionListScrollView.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        item.SetObject(minion.character);
        item.SetAsDefaultBehaviour();
        item.transform.SetSiblingIndex(reserveHeader.GetSiblingIndex());

        if (TraitManager.Instance.allTraits.ContainsKey(minion.character.characterClass.traitNameOnTamedByPlayer)) {
            Trait trait = TraitManager.Instance.allTraits[minion.character.characterClass.traitNameOnTamedByPlayer];
            item.AddHoverEnterAction(data => UIManager.Instance.ShowSmallInfo(trait.description, PlayerUI.Instance.minionListHoverPosition, trait.name));
            item.AddHoverExitAction(data => UIManager.Instance.HideSmallInfo());    
        }
    }
    private void CreateNewReserveMinionItem(SPELL_TYPE minionPlayerSkillType) {
        MinionPlayerSkill minionPlayerSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillData(minionPlayerSkillType);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(spellItemPrefab.name, Vector3.zero, Quaternion.identity, minionListScrollView.content);
        SummonMinionPlayerSkillNameplateItem spellItem = go.GetComponent<SummonMinionPlayerSkillNameplateItem>();
        spellItem.SetObject(minionPlayerSkill);
        
        spellItem.ClearAllHoverEnterActions();
        spellItem.ClearAllHoverExitActions();
        
        spellItem.AddHoverEnterAction(OnHoverEnterReserveMinion);
        spellItem.AddHoverExitAction(OnHoverExitReserveMinion);
        
        _minionItems.Add(spellItem);
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
        if (spellData is MinionPlayerSkill minionPlayerSkill) {
            CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(minionPlayerSkill.className);
            if (TraitManager.Instance.allTraits.ContainsKey(characterClass.traitNameOnTamedByPlayer)) {
                Trait trait = TraitManager.Instance.allTraits[characterClass.traitNameOnTamedByPlayer];
                UIManager.Instance.ShowSmallInfo(trait.description, _hoverPosition, trait.name);
            }
        }
        PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(spellData, PlayerUI.Instance.minionListHoverPosition);
    }
    private void OnHoverExitReserveMinion(SpellData spellData) {
        UIManager.Instance.HideSmallInfo();
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
