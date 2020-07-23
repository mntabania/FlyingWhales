using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class CharacterNameplateItem : NameplateItem<Character> {

    [Header("Character Nameplate Attributes")]
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private GameObject travellingIcon;
    [SerializeField] private GameObject arrivedIcon;
    [SerializeField] private GameObject restrainedIcon;
    [SerializeField] private GameObject leaderIcon;

    public Character character { get; private set; }
    public bool isActive { get; private set; }

    private void OnEnable() {
        Messenger.AddListener(Signals.TICK_ENDED, UpdateText);
        Messenger.AddListener<Character, ILeader>(Signals.ON_SET_AS_FACTION_LEADER, OnSetAsFactionLeader);
        Messenger.AddListener<Faction, ILeader>(Signals.ON_FACTION_LEADER_REMOVED, OnFactionLeaderRemoved);
        Messenger.AddListener<Character, Character>(Signals.ON_SET_AS_SETTLEMENT_RULER, OnSetAsSettlementRuler);
        Messenger.AddListener<NPCSettlement, Character>(Signals.ON_SETTLEMENT_RULER_REMOVED, OnSettlementRulerRemoved);
        if (character != null) {
            UpdateText();
            UpdateLeaderIcon();
        }
    }
    private void OnDisable() {
        Messenger.RemoveListener(Signals.TICK_ENDED, UpdateText);
        Messenger.RemoveListener<Character, ILeader>(Signals.ON_SET_AS_FACTION_LEADER, OnSetAsFactionLeader);
        Messenger.RemoveListener<Faction, ILeader>(Signals.ON_FACTION_LEADER_REMOVED, OnFactionLeaderRemoved);
        Messenger.RemoveListener<Character, Character>(Signals.ON_SET_AS_SETTLEMENT_RULER, OnSetAsSettlementRuler);
        Messenger.RemoveListener<NPCSettlement, Character>(Signals.ON_SETTLEMENT_RULER_REMOVED, OnSettlementRulerRemoved);
    }
    
    #region Overrides
    public override void SetObject(Character character) {
        base.SetObject(character);
        this.character = character;
        mainLbl.text = character.visuals.GetNameplateName();
        subLbl.text = character.raceClassName;
        UpdateLeaderIcon();
        portrait.GeneratePortrait(character);
        UpdateStatusIcons();
        UpdateText();
    }
    public override void UpdateObject(Character character) {
        base.UpdateObject(character);
        this.character = character;
        mainLbl.text = character.visuals.GetNameplateName();
        subLbl.text = character.raceClassName;
        UpdateLeaderIcon();
        portrait.GeneratePortrait(character);
        UpdateStatusIcons();
        UpdateText();
    }
    public override void OnHoverEnter() {
        portrait.SetHoverHighlightState(true);
        //if (character != null && character.minion != null) {
        //    UIManager.Instance.ShowMinionCardTooltip(character.minion);
        //}
        base.OnHoverEnter();
    }
    public override void OnHoverExit() {
        portrait.SetHoverHighlightState(false);
        //if (character != null && character.minion != null) {
        //    UIManager.Instance.HideMinionCardTooltip();
        //}
        base.OnHoverExit();
    }
    public override void Reset() {
        base.Reset();
        SetPortraitInteractableState(true);
    }
    #endregion

    public void SetPosition(UIHoverPosition position) {
        UIManager.Instance.PositionTooltip(position, gameObject, this.transform as RectTransform);
    }

    public void SetIsActive(bool state) {
        isActive = state;
    }

    /// <summary>
    /// Set this nameplate to behave in the default settings (button, onclick shows character UI, etc.)
    /// </summary>
    public void SetAsDefaultBehaviour() {
        SetAsButton();
        ClearAllOnClickActions();
        AddOnClickAction((character) => UIManager.Instance.ShowCharacterInfo(character));
        SetSupportingLabelState(false);
    }

    private void UpdateStatusIcons() {
        if (character.carryComponent.masterCharacter.avatar.isTravellingOutside) {
            //character is travelling outside
            travellingIcon.SetActive(true);
            arrivedIcon.SetActive(false);
            restrainedIcon.SetActive(false);
        } else if (!character.isAtHomeRegion) {
            //character is at another location other than his/her home region
            travellingIcon.SetActive(false);
            arrivedIcon.SetActive(true);
            restrainedIcon.SetActive(false);
        } else if (character.traitContainer.HasTrait("Restrained")) {
            //character is restrained
            travellingIcon.SetActive(false);
            arrivedIcon.SetActive(false);
            restrainedIcon.SetActive(true);
        } else {
            travellingIcon.SetActive(false);
            arrivedIcon.SetActive(false);
            restrainedIcon.SetActive(false);
        }
    }

    public void SetPortraitInteractableState(bool state) {
        portrait.ignoreInteractions = !state;
    }

    #region Sub Text
    private void UpdateText() {
        mainLbl.text = character.visuals.GetNameplateName();
        supportingLbl.text = character.visuals.GetThoughtBubble(out _);
        SetSupportingLabelState(true);
    }
    #endregion

    #region Leader Icon
    private void UpdateLeaderIcon() {
        leaderIcon.SetActive(character.isFactionLeader || character.isSettlementRuler);
    }
    private void OnSetAsFactionLeader(Character character, ILeader previousLeader) {
        if (character == this.character || previousLeader == this.character) {
            UpdateLeaderIcon();
        }
    }
    private void OnFactionLeaderRemoved(Faction faction, ILeader previousLeader) {
        if (previousLeader == this.character) {
            UpdateLeaderIcon();
        }
    }
    private void OnSetAsSettlementRuler(Character character, Character previousRuler) {
        if (character == this.character || previousRuler == this.character) {
            UpdateLeaderIcon();
        }
    }
    private void OnSettlementRulerRemoved(NPCSettlement settlement, Character previousLeader) {
        if (previousLeader == this.character) {
            UpdateLeaderIcon();
        }
    }
    #endregion
}
