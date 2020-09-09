using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.UI;

public class SummonListUI : PopupMenuBase {

    [Header("Minion List")]
    [SerializeField] private GameObject activeSummonItemPrefab;
    [SerializeField] private GameObject reserveSummonItemPrefab;
    [SerializeField] private ScrollRect summonListScrollView;
    [SerializeField] private RectTransform reserveHeader;

    [SerializeField] private UIHoverPosition _hoverPosition;
    [SerializeField] private Toggle _mainToggle;
    
    private List<SummonMinionPlayerSkillNameplateItem> _summonPlayerSkillItems;

    public override void Open() {
        base.Open();
        _mainToggle.SetIsOnWithoutNotify(true);
        UpdateSummonPlayerSkillItems();
    }
    public override void Close() {
        base.Close();
        _mainToggle.SetIsOnWithoutNotify(false);
    }
    public void Initialize() {
        _summonPlayerSkillItems = new List<SummonMinionPlayerSkillNameplateItem>();
        Messenger.AddListener<Summon>(Signals.PLAYER_GAINED_SUMMON, OnGainSummon);
        Messenger.AddListener<Summon>(Signals.PLAYER_LOST_SUMMON, OnLostSummon);
        Messenger.AddListener<SPELL_TYPE>(Signals.ADDED_PLAYER_SUMMON_SKILL, OnGainPlayerSummonSkill);
        Messenger.AddListener<SpellData>(Signals.CHARGES_ADJUSTED, OnChargesAdjusted);
    }
    public void UpdateList() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if (character is Summon summon && character.faction != null && character.faction.isPlayerFaction && !character.isDead && !character.isPreplaced) {
                CreateNewActiveSummonItem(summon);
            }
        }
    }
    private void OnChargesAdjusted(SpellData spellData) {
        if (spellData is SummonPlayerSkill summonPlayerSkill) {
            SummonMinionPlayerSkillNameplateItem nameplateItem = GetSummonMinionPlayerSkillNameplateItem(spellData);
            if (spellData.charges > 0) {
                if(nameplateItem == null) {
                    nameplateItem = CreateNewReserveSummonItem(spellData.type);
                }
                nameplateItem.UpdateData();
            } else {
                if(nameplateItem != null) {
                    DeleteSummonItem(nameplateItem);
                }
            }
        }
    }
    private void UpdateSummonPlayerSkillItems() {
        for (int i = 0; i < _summonPlayerSkillItems.Count; i++) {
            _summonPlayerSkillItems[i].UpdateData();
        }
    }
    private void CreateNewActiveSummonItem(Summon summon) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(activeSummonItemPrefab.name, Vector3.zero, Quaternion.identity, summonListScrollView.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        item.SetObject(summon);
        item.SetAsDefaultBehaviour();
        item.AddOnClickAction((c) => UIManager.Instance.ShowCharacterInfo(c, true));
        item.transform.SetSiblingIndex(reserveHeader.GetSiblingIndex());

        if (!string.IsNullOrEmpty(summon.bredBehaviour) && TraitManager.Instance.allTraits.ContainsKey(summon.bredBehaviour)) {
            Trait trait = TraitManager.Instance.allTraits[summon.bredBehaviour];
            item.AddHoverEnterAction(data => UIManager.Instance.ShowSmallInfo(trait.descriptionInUI, _hoverPosition, trait.name));
            item.AddHoverExitAction(data => UIManager.Instance.HideSmallInfo());    
        }
    }
    private SummonMinionPlayerSkillNameplateItem CreateNewReserveSummonItem(SPELL_TYPE summonPlayerSkillType) {
        SummonPlayerSkill summonPlayerSkill = PlayerSkillManager.Instance.GetSummonPlayerSkillData(summonPlayerSkillType);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(reserveSummonItemPrefab.name, Vector3.zero, Quaternion.identity, summonListScrollView.content);
        SummonMinionPlayerSkillNameplateItem item = go.GetComponent<SummonMinionPlayerSkillNameplateItem>();
        item.SetObject(summonPlayerSkill);
        
        if (!string.IsNullOrEmpty(summonPlayerSkill.bredBehaviour) && TraitManager.Instance.allTraits.ContainsKey(summonPlayerSkill.bredBehaviour)) {
            Trait trait = TraitManager.Instance.allTraits[summonPlayerSkill.bredBehaviour];
            item.AddHoverEnterAction(data => UIManager.Instance.ShowSmallInfo(trait.descriptionInUI, _hoverPosition, trait.name));
            item.AddHoverExitAction(data => UIManager.Instance.HideSmallInfo());    
        }
        
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
    private SummonMinionPlayerSkillNameplateItem GetSummonMinionPlayerSkillNameplateItem(SpellData spellData) {
        for (int i = 0; i < _summonPlayerSkillItems.Count; i++) {
            SummonMinionPlayerSkillNameplateItem item = _summonPlayerSkillItems[i];
            if (item.spellData == spellData) {
                return item;
            }
        }
        return null;
    }
    private void OnGainPlayerSummonSkill(SPELL_TYPE minionPlayerSkillType) {
        CreateNewReserveSummonItem(minionPlayerSkillType);
    }
    private void OnGainSummon(Summon summon) {
        CreateNewActiveSummonItem(summon);
        UpdateSummonPlayerSkillItems();
    }
    private void OnLostSummon(Summon summon) {
        DeleteSummonItem(summon);
    }
    private void OnHoverEnterReserveSummon(SpellData spellData) {
        PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(spellData);
    }
    private void OnHoverExitReserveSummon(SpellData spellData) {
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
