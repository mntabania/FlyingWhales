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
    
    private List<SummonMinionPlayerSkillNameplateItem> _summonPlayerSkillItems;

    public override void Open() {
        base.Open();
        UpdateSummonPlayerSkillItems();
    }
    public void Initialize() {
        _summonPlayerSkillItems = new List<SummonMinionPlayerSkillNameplateItem>();
        Messenger.AddListener<Summon>(Signals.PLAYER_GAINED_SUMMON, OnGainSummon);
        Messenger.AddListener<Summon>(Signals.PLAYER_LOST_SUMMON, OnLostSummon);
        Messenger.AddListener<SPELL_TYPE>(Signals.ADDED_PLAYER_SUMMON_SKILL, OnGainPlayerSummonSkill);
        Messenger.AddListener<SpellData>(Signals.CHARGES_ADJUSTED, OnChargesAdjusted);
    }
    private void OnChargesAdjusted(SpellData spellData) {
        if (spellData is SummonPlayerSkill summonPlayerSkill) {
            for (int i = 0; i < _summonPlayerSkillItems.Count; i++) {
                SummonMinionPlayerSkillNameplateItem item = _summonPlayerSkillItems[i];
                if (item.spellData == spellData && spellData.charges <= 0) {
                    DeleteSummonItem(item);
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
        item.transform.SetSiblingIndex(reserveHeader.GetSiblingIndex());

        if (TraitManager.Instance.allTraits.ContainsKey(summon.characterClass.traitNameOnTamedByPlayer)) {
            Trait trait = TraitManager.Instance.allTraits[summon.characterClass.traitNameOnTamedByPlayer];
            item.AddHoverEnterAction(data => UIManager.Instance.ShowSmallInfo(trait.description, _hoverPosition, trait.name));
            item.AddHoverExitAction(data => UIManager.Instance.HideSmallInfo());    
        }
    }
    private void CreateNewReserveSummonItem(SPELL_TYPE summonPlayerSkillType) {
        SummonPlayerSkill summonPlayerSkill = PlayerSkillManager.Instance.GetSummonPlayerSkillData(summonPlayerSkillType);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(reserveSummonItemPrefab.name, Vector3.zero, Quaternion.identity, summonListScrollView.content);
        SummonMinionPlayerSkillNameplateItem item = go.GetComponent<SummonMinionPlayerSkillNameplateItem>();
        item.SetObject(summonPlayerSkill);
        
        CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(summonPlayerSkill.className);
        if (TraitManager.Instance.allTraits.ContainsKey(characterClass.traitNameOnTamedByPlayer)) {
            Trait trait = TraitManager.Instance.allTraits[characterClass.traitNameOnTamedByPlayer];
            item.AddHoverEnterAction(data => UIManager.Instance.ShowSmallInfo(trait.description, _hoverPosition, trait.name));
            item.AddHoverExitAction(data => UIManager.Instance.HideSmallInfo());    
        }
        
        _summonPlayerSkillItems.Add(item);
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
