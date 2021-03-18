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
        //Messenger.AddListener<PLAYER_SKILL_TYPE>(SpellSignals.ADDED_PLAYER_MINION_SKILL, OnGainPlayerMinionSkill);
        Messenger.AddListener<MonsterAndDemonUnderlingCharges>(PlayerSignals.UPDATED_MONSTER_UNDERLING, OnUpdateMonsterUnderling);
    }
    public void UpdateList() {
        for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++) {
            Character character = PlayerManager.Instance.player.playerFaction.characters[i];
            if (character.minion != null && !character.isDead && !character.isPreplaced) {
                CreateNewActiveMinionItem(character.minion);
            }
        }
        for (int i = 0; i < PlayerManager.Instance.player.playerSkillComponent.minionsSkills.Count; i++) {
            
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
        
        //spellItem.AddHoverEnterAction(OnHoverEnterReserveMinion);
        //spellItem.AddHoverExitAction(OnHoverExitReserveMinion);
        
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
    private MonsterUnderlingQuantityNameplateItem CreateNewMonsterUnderlingQuantityItem(MonsterAndDemonUnderlingCharges p_underlingCharges) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(quantityMonsterItemPrefab.name, Vector3.zero, Quaternion.identity, quantityScrollView.content);
        MonsterUnderlingQuantityNameplateItem item = go.GetComponent<MonsterUnderlingQuantityNameplateItem>();
        item.SetObject(p_underlingCharges);
        item.SetAsDisplayOnly();
        item.AddHoverEnterAction(OnHoverEnterDemonUnderlingData);
        item.AddHoverExitAction(OnHoverExitDemonUnderlingData);
        go.SetActive(p_underlingCharges.hasMaxCharge);
        _monsterUnderlingQuantityNameplateItems.Add(item);
        return item;
    }
    private MonsterUnderlingQuantityNameplateItem GetMonsterUnderlingQuantityNameplateItem(MonsterAndDemonUnderlingCharges p_underlingCharges) {
        for (int i = 0; i < _monsterUnderlingQuantityNameplateItems.Count; i++) {
            MonsterUnderlingQuantityNameplateItem item = _monsterUnderlingQuantityNameplateItems[i];
            if (item.obj == p_underlingCharges) {
                return item;
            }
        }
        return null;
    }
    private void DeleteMonsterUnderlingItem(MonsterAndDemonUnderlingCharges p_underlingCharges) {
        MonsterUnderlingQuantityNameplateItem item = GetMonsterUnderlingQuantityNameplateItem(p_underlingCharges);
        if (item != null) {
            ObjectPoolManager.Instance.DestroyObject(item);
        }
    }

    public void UpdateMonsterUnderlingQuantityList() {
        Player player = PlayerManager.Instance.player;
        if (player != null) {
            Dictionary<SUMMON_TYPE, MonsterAndDemonUnderlingCharges> kvp = player.underlingsComponent.monsterUnderlingCharges;
            foreach (MonsterAndDemonUnderlingCharges item in kvp.Values) {
                CreateNewMonsterUnderlingQuantityItem(item);
            }
        }
    }
    private void OnUpdateMonsterUnderling(MonsterAndDemonUnderlingCharges p_underlingCharges) {
        if (p_underlingCharges.isDemon) {
            MonsterUnderlingQuantityNameplateItem nameplateItem = GetMonsterUnderlingQuantityNameplateItem(p_underlingCharges);
            if (nameplateItem != null) {
                nameplateItem.UpdateBasicData();
                nameplateItem.gameObject.SetActive(p_underlingCharges.hasMaxCharge);
            } else {
                CreateNewMonsterUnderlingQuantityItem(p_underlingCharges);
            }
        }
    }
    private void OnHoverEnterDemonUnderlingData(MonsterAndDemonUnderlingCharges p_data) {
        MinionPlayerSkill minionPlayerSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(p_data.minionType);
        if (minionPlayerSkill != null) {
            CharacterClassData data = CharacterManager.Instance.GetOrCreateCharacterClassData(minionPlayerSkill.className);
            if (data.combatBehaviourType != CHARACTER_COMBAT_BEHAVIOUR.None) {
                CharacterCombatBehaviour combatBehaviour = CombatManager.Instance.GetCombatBehaviour(data.combatBehaviourType);
                UIManager.Instance.ShowSmallInfo(combatBehaviour.description, _hoverPosition, combatBehaviour.name);
            }
        }
        PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(minionPlayerSkill, PlayerUI.Instance.minionListHoverPosition);
    }
    private void OnHoverExitDemonUnderlingData(MonsterAndDemonUnderlingCharges p_data) {
        UIManager.Instance.HideSmallInfo();
        PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
    }
    #endregion
}
