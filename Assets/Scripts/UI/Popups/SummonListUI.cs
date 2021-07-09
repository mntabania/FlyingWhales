using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.UI;

public class SummonListUI : PopupMenuBase {

    [Header("Summoned Monsters Column")]
    [SerializeField] private GameObject activeSummonItemPrefab;
    [SerializeField] private GameObject reserveSummonItemPrefab;
    [SerializeField] private ScrollRect summonListScrollView;
    [SerializeField] private RectTransform reserveHeader;
    [SerializeField] private UIHoverPosition _hoverPosition;
    [SerializeField] private Toggle _mainToggle;

    [Header("Monster Quantity Column")]
    [SerializeField] private GameObject quantityMonsterItemPrefab;
    [SerializeField] private ScrollRect quantityScrollView;

    [SerializeField] private MonsterToolTipUI monsterToolTipUI;

    private List<SummonMinionPlayerSkillNameplateItem> _summonPlayerSkillItems;
    private List<MonsterUnderlingQuantityNameplateItem> _monsterUnderlingQuantityNameplateItems;


    public override void Open() {
        base.Open();
        _mainToggle.SetIsOnWithoutNotify(true);
        UpdateSummonPlayerSkillItems();
        UpdateUnderlingItems();
    }
    public override void Close() {
        base.Close();
        monsterToolTipUI.HideToolTip();
        _mainToggle.SetIsOnWithoutNotify(false);
    }
    public void Initialize() {
        _summonPlayerSkillItems = new List<SummonMinionPlayerSkillNameplateItem>();
        _monsterUnderlingQuantityNameplateItems = new List<MonsterUnderlingQuantityNameplateItem>();
        Messenger.AddListener<Summon>(PlayerSignals.PLAYER_GAINED_SUMMON, OnGainSummon);
        Messenger.AddListener<Summon>(PlayerSignals.PLAYER_LOST_SUMMON, OnLostSummon);
        Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.ADDED_PLAYER_SUMMON_SKILL, OnGainPlayerSummonSkill);
        Messenger.AddListener<SkillData>(PlayerSkillSignals.CHARGES_ADJUSTED, OnChargesAdjusted);
        Messenger.AddListener<MonsterAndDemonUnderlingCharges>(PlayerSignals.UPDATED_MONSTER_UNDERLING, OnUpdateMonsterUnderling);
    }
    public void UpdateList() {
        for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++) {
            Character character = PlayerManager.Instance.player.playerFaction.characters[i];
            if (character is Summon summon && !character.isDead && !character.isPreplaced) {
                CreateNewActiveSummonItem(summon);
            }
        }
    }
    private void UpdateSummonPlayerSkillItems() {
        for (int i = 0; i < _summonPlayerSkillItems.Count; i++) {
            _summonPlayerSkillItems[i].UpdateData();
        }
    }
    private void UpdateUnderlingItems() {
        for (int i = 0; i < _monsterUnderlingQuantityNameplateItems.Count; i++) {
            _monsterUnderlingQuantityNameplateItems[i].UpdateBasicData();
        }
    }
    private void CreateNewActiveSummonItem(Summon summon) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(activeSummonItemPrefab.name, Vector3.zero, Quaternion.identity, summonListScrollView.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        //CharacterClassData summonData = CharacterManager.Instance.GetOrCreateCharacterClassData(summon.characterClass.className);
        //CharacterCombatBehaviour combatBehaviour = CombatManager.Instance.GetCombatBehaviour(summonData.combatBehaviourType);
        item.SetObject(summon);
        item.SetAsDefaultBehaviour();
        item.AddOnClickAction((c) => UIManager.Instance.ShowCharacterInfo(c, true));
        item.transform.SetSiblingIndex(reserveHeader.GetSiblingIndex());
        
        item.AddHoverEnterAction(data => OnHoverEnterActiveSummon(data));
        item.AddHoverExitAction(OnHoverExitActiveSummon);    
        // if (!string.IsNullOrEmpty(summon.bredBehaviour) && TraitManager.Instance.allTraits.ContainsKey(summon.bredBehaviour)) {
        //     Trait trait = TraitManager.Instance.allTraits[summon.bredBehaviour];
        //     item.AddHoverEnterAction(data => UIManager.Instance.ShowSmallInfo(trait.descriptionInUI, _hoverPosition, trait.name));
        //     item.AddHoverExitAction(data => UIManager.Instance.HideSmallInfo());    
        // }
    }
    private SummonMinionPlayerSkillNameplateItem CreateNewReserveSummonItem(PLAYER_SKILL_TYPE summonPlayerSkillType) {
        //Should no longer have reserve items
        return null;
        SummonPlayerSkill summonPlayerSkill = PlayerSkillManager.Instance.GetSummonPlayerSkillData(summonPlayerSkillType);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(reserveSummonItemPrefab.name, Vector3.zero, Quaternion.identity, summonListScrollView.content);
        SummonMinionPlayerSkillNameplateItem item = go.GetComponent<SummonMinionPlayerSkillNameplateItem>();
        item.SetObject(summonPlayerSkill);
        
        // if (!string.IsNullOrEmpty(summonPlayerSkill.bredBehaviour) && TraitManager.Instance.allTraits.ContainsKey(summonPlayerSkill.bredBehaviour)) {
        //     Trait trait = TraitManager.Instance.allTraits[summonPlayerSkill.bredBehaviour];
        //     item.AddHoverEnterAction(data => UIManager.Instance.ShowSmallInfo(trait.descriptionInUI, _hoverPosition, trait.name));
        //     item.AddHoverExitAction(data => UIManager.Instance.HideSmallInfo());    
        // }
        
        _summonPlayerSkillItems.Add(item);
        return item;
    }
    private void DeleteSummonItem(Summon summon) {
        CharacterNameplateItem item = GetSummonItem(summon);
        if (item != null) {
            ObjectPoolManager.Instance.DestroyObject(item);
        }
    }
    private void DeleteSummonItem(SummonMinionPlayerSkillNameplateItem item) {
        _summonPlayerSkillItems.Remove(item);
        ObjectPoolManager.Instance.DestroyObject(item);
    }
    private CharacterNameplateItem GetSummonItem(Summon summon) {
        CharacterNameplateItem[] items = UtilityScripts.GameUtilities.GetComponentsInDirectChildren<CharacterNameplateItem>(summonListScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            CharacterNameplateItem item = items[i];
            if (item.character == summon) {
                return item;
            }
        }
        return null;
    }
    private SummonMinionPlayerSkillNameplateItem GetSummonMinionPlayerSkillNameplateItem(SkillData spellData) {
        for (int i = 0; i < _summonPlayerSkillItems.Count; i++) {
            SummonMinionPlayerSkillNameplateItem item = _summonPlayerSkillItems[i];
            if (item.spellData == spellData) {
                return item;
            }
        }
        return null;
    }
    private void OnHoverEnterActiveSummon(Character p_character) {
        CharacterCombatBehaviour combatBehaviour = p_character.combatComponent.combatBehaviourParent.currentCombatBehaviour;
        if (combatBehaviour != null) {
            //UIManager.Instance.ShowSmallInfo(combatBehaviour.description, PlayerUI.Instance.minionListHoverPosition, combatBehaviour.name);
            monsterToolTipUI.DisplayToolTipWithoutCharge(combatBehaviour.name, combatBehaviour.description);
        }
    }
    private void OnHoverExitActiveSummon(Character p_character) {
        monsterToolTipUI.HideToolTip();
        UIManager.Instance.HideSmallInfo();
    }

    #region Listeners
    private void OnGainPlayerSummonSkill(PLAYER_SKILL_TYPE summonPlayerSkillType) {
        CreateNewReserveSummonItem(summonPlayerSkillType);
    }
    private void OnGainSummon(Summon summon) {
        CreateNewActiveSummonItem(summon);
        UpdateSummonPlayerSkillItems();
    }
    private void OnLostSummon(Summon summon) {
        DeleteSummonItem(summon);
    }
    private void OnChargesAdjusted(SkillData spellData) {
        if (spellData is SummonPlayerSkill) {
            SummonMinionPlayerSkillNameplateItem nameplateItem = GetSummonMinionPlayerSkillNameplateItem(spellData);
            if (spellData.charges > 0) {
                if (nameplateItem == null) {
                    nameplateItem = CreateNewReserveSummonItem(spellData.type);
                }
                nameplateItem.UpdateData();
            } else {
                if (nameplateItem != null) {
                    DeleteSummonItem(nameplateItem);
                }
            }
        }
    }
    private void OnUpdateMonsterUnderling(MonsterAndDemonUnderlingCharges p_underlingCharges) {
        if (!p_underlingCharges.isDemon) {
            MonsterUnderlingQuantityNameplateItem nameplateItem = GetMonsterUnderlingQuantityNameplateItem(p_underlingCharges);
            if (nameplateItem != null) {
                nameplateItem.UpdateBasicData();
                nameplateItem.gameObject.SetActive(p_underlingCharges.hasMaxCharge);
            } else {
                CreateNewMonsterUnderlingQuantityItem(p_underlingCharges);
            }
        }
    }
    #endregion

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
    private void OnHoverEnterDemonUnderlingData(MonsterAndDemonUnderlingCharges p_data) {
        string txtChargeTime = string.Empty;
        CharacterClassData cData = CharacterManager.Instance.GetOrCreateCharacterClassData(p_data.characterClassName);
        CharacterCombatBehaviour combatBehaviour = CombatManager.Instance.GetCombatBehaviour(cData.combatBehaviourType);
        //UIManager.Instance.ShowSmallInfo(combatBehaviour.description, PlayerUI.Instance.minionListHoverPosition, combatBehaviour.name);
        txtChargeTime = PlayerUI.Instance.OnHoverSpellChargeRemainingForSummon(cData, p_data);
        //PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(summonPlayerSkill, PlayerUI.Instance.minionListHoverPosition);
        if (string.IsNullOrEmpty(txtChargeTime)) {
            monsterToolTipUI.DisplayToolTipWithoutCharge(combatBehaviour.name, combatBehaviour.description);
        } else {
            monsterToolTipUI.DisplayToolTipWithCharge(combatBehaviour.name, combatBehaviour.description, txtChargeTime);
        }
    }
    private void OnHoverExitDemonUnderlingData(MonsterAndDemonUnderlingCharges p_data) {
        monsterToolTipUI.HideToolTip();
        Tooltip.Instance.HideSmallInfo();
        UIManager.Instance.HideSmallInfo();
        //PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
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
    #endregion

    private void OnHoverEnterReserveSummon(SkillData spellData, MonsterAndDemonUnderlingCharges m_underling) {
        string txtRecharge = PlayerUI.Instance.OnHoverSpellChargeRemaining(spellData, m_underling);
        if (string.IsNullOrEmpty(txtRecharge)) {
            monsterToolTipUI.DisplayToolTipWithoutCharge(spellData.localizedName, spellData.localizedDescription);
        } else {
            monsterToolTipUI.DisplayToolTipWithCharge(spellData.localizedName, spellData.localizedDescription, txtRecharge);
        }
        //PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(spellData);
    }
    private void OnHoverExitReserveSummon(SkillData spellData) {
        monsterToolTipUI.HideToolTip();
        Tooltip.Instance.HideSmallInfo();
        PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
    }
    public void ToggleSummonList(bool isOn) {
        if (isOn) {
            Open();
        } else {
            Close();
        }
    }
}
