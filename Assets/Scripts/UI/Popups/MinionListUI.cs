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

    [Header("Monster Quantity Column")]
    [SerializeField] private GameObject quantityMonsterItemPrefab;
    [SerializeField] private ScrollRect quantityScrollView;

    private List<SummonMinionPlayerSkillNameplateItem> _minionItems;
    private List<MonsterUnderlingQuantityNameplateItem> _monsterUnderlingQuantityNameplateItems;

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
        _monsterUnderlingQuantityNameplateItems = new List<MonsterUnderlingQuantityNameplateItem>();
        Messenger.AddListener<Minion>(PlayerSignals.PLAYER_GAINED_MINION, OnGainMinion);
        Messenger.AddListener<Minion>(PlayerSignals.PLAYER_LOST_MINION, OnLostMinion);
        Messenger.AddListener<PLAYER_SKILL_TYPE>(SpellSignals.ADDED_PLAYER_MINION_SKILL, OnGainPlayerMinionSkill);
    }
    public void UpdateList() {
        for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++) {
            Character character = PlayerManager.Instance.player.playerFaction.characters[i];
            if (character.minion != null && !character.isDead && !character.isPreplaced) {
                CreateNewActiveMinionItem(character.minion);
            }
        }
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
        item.AddOnClickAction((c) => UIManager.Instance.ShowCharacterInfo(c, true));
        item.transform.SetSiblingIndex(reserveHeader.GetSiblingIndex());

        if (TraitManager.Instance.allTraits.ContainsKey(minion.character.characterClass.traitNameOnTamedByPlayer)) {
            Trait trait = TraitManager.Instance.allTraits[minion.character.characterClass.traitNameOnTamedByPlayer];
            item.AddHoverEnterAction(data => UIManager.Instance.ShowSmallInfo(trait.descriptionInUI, PlayerUI.Instance.minionListHoverPosition, trait.name));
            item.AddHoverExitAction(data => UIManager.Instance.HideSmallInfo());    
        }
    }
    private void CreateNewReserveMinionItem(PLAYER_SKILL_TYPE minionPlayerSkillType) {
        //Should no longer have reserve items
        return;
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
    private void OnGainPlayerMinionSkill(PLAYER_SKILL_TYPE minionPlayerSkillType) {
        CreateNewReserveMinionItem(minionPlayerSkillType);
    }
    private void OnGainMinion(Minion minion) {
        CreateNewActiveMinionItem(minion);
        UpdateMinionPlayerSkillItems();
    }
    private void OnLostMinion(Minion minion) {
        DeleteMinionItem(minion);
    }
    private void OnHoverEnterReserveMinion(SkillData spellData) {
        if (spellData is MinionPlayerSkill minionPlayerSkill) {
            CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(minionPlayerSkill.className);
            if (TraitManager.Instance.allTraits.ContainsKey(characterClass.traitNameOnTamedByPlayer)) {
                Trait trait = TraitManager.Instance.allTraits[characterClass.traitNameOnTamedByPlayer];
                UIManager.Instance.ShowSmallInfo(trait.descriptionInUI, _hoverPosition, trait.name);
            }
        }
        PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(spellData, PlayerUI.Instance.minionListHoverPosition);
    }
    private void OnHoverExitReserveMinion(SkillData spellData) {
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

    #region Monster Underlings
    private MonsterUnderlingQuantityNameplateItem CreateNewMonsterUnderlingQuantityItem(MonsterAndMinionUnderlingCharges p_underlingCharges) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(quantityMonsterItemPrefab.name, Vector3.zero, Quaternion.identity, quantityScrollView.content);
        MonsterUnderlingQuantityNameplateItem item = go.GetComponent<MonsterUnderlingQuantityNameplateItem>();
        item.SetObject(p_underlingCharges);
        item.SetAsDisplayOnly();
        go.SetActive(p_underlingCharges.hasMaxCharge);
        _monsterUnderlingQuantityNameplateItems.Add(item);
        return item;
    }
    private MonsterUnderlingQuantityNameplateItem GetMonsterUnderlingQuantityNameplateItem(MonsterAndMinionUnderlingCharges p_underlingCharges) {
        for (int i = 0; i < _monsterUnderlingQuantityNameplateItems.Count; i++) {
            MonsterUnderlingQuantityNameplateItem item = _monsterUnderlingQuantityNameplateItems[i];
            if (item.obj == p_underlingCharges) {
                return item;
            }
        }
        return null;
    }
    private void DeleteMonsterUnderlingItem(MonsterAndMinionUnderlingCharges p_underlingCharges) {
        MonsterUnderlingQuantityNameplateItem item = GetMonsterUnderlingQuantityNameplateItem(p_underlingCharges);
        if (item != null) {
            ObjectPoolManager.Instance.DestroyObject(item);
        }
    }

    public void UpdateMonsterUnderlingQuantityList() {
        Player player = PlayerManager.Instance.player;
        if (player != null) {
            Dictionary<SUMMON_TYPE, MonsterAndMinionUnderlingCharges> kvp = player.underlingsComponent.monsterUnderlingCharges;
            foreach (MonsterAndMinionUnderlingCharges item in kvp.Values) {
                CreateNewMonsterUnderlingQuantityItem(item);
            }
        }
    }
    #endregion
}
