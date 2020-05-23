using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SummonListUI : PopupMenuBase {

    [Header("Minion List")]
    [SerializeField] private GameObject activeSummonItemPrefab;
    [SerializeField] private GameObject reserveSummonItemPrefab;
    [SerializeField] private ScrollRect summonListScrollView;
    [SerializeField] private GameObject summonListGO;
    //[SerializeField] private UIHoverPosition summonListCardTooltipPos;
    [SerializeField] private Toggle summonListToggle;
    [SerializeField] private RectTransform activeHeader;
    [SerializeField] private RectTransform reserveHeader;

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
    }

    private void UpdateSummonPlayerSkillItems() {
        for (int i = 0; i < _summonPlayerSkillItems.Count; i++) {
            _summonPlayerSkillItems[i].SetCount(_summonPlayerSkillItems[i].spellData.charges, true);
        }
    }
    //private void UpdateMinionList() {
    //    UtilityScripts.Utilities.DestroyChildren(minionListScrollView.content);
    //    for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
    //        Minion currMinion = PlayerManager.Instance.player.minions[i];
    //        CreateNewMinionItem(currMinion);
    //    }
    //}
    private void CreateNewActiveSummonItem(Summon summon) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(activeSummonItemPrefab.name, Vector3.zero, Quaternion.identity, summonListScrollView.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        item.SetObject(summon);
        //item.AddHoverEnterAction((character) => UIManager.Instance.ShowMinionCardTooltip(character.minion, summonListCardTooltipPos));
        //item.AddHoverExitAction((character) => UIManager.Instance.HideMinionCardTooltip());
        item.SetAsDefaultBehaviour();
        item.transform.SetSiblingIndex(reserveHeader.GetSiblingIndex());
    }
    private void CreateNewReserveSummonItem(SPELL_TYPE summonPlayerSkillType) {
        SummonPlayerSkill summonPlayerSkill = PlayerSkillManager.Instance.GetSummonPlayerSkillData(summonPlayerSkillType);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(reserveSummonItemPrefab.name, Vector3.zero, Quaternion.identity, summonListScrollView.content);
        SummonMinionPlayerSkillNameplateItem item = go.GetComponent<SummonMinionPlayerSkillNameplateItem>();
        item.SetObject(summonPlayerSkill);
        item.SetCount(summonPlayerSkill.charges, true);
        item.ClearAllOnClickActions();
        item.ClearAllHoverEnterActions();
        item.AddHoverEnterAction(OnHoverEnterReserveSummon);
        item.AddHoverExitAction(OnHoverExitReserveSummon);
        _summonPlayerSkillItems.Add(item);
    }
    private void DeleteSummonItem(Summon summon) {
        CharacterNameplateItem item = GetSummonItem(summon);
        if (item != null) {
            ObjectPoolManager.Instance.DestroyObject(item);
        }
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
