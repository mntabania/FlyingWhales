﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using UtilityScripts;

public class CharacterNameplateItem : NameplateItem<Character> {

    [Header("Character Nameplate Attributes")]
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private GameObject travellingIcon;
    [SerializeField] private GameObject arrivedIcon;
    [SerializeField] private GameObject restrainedIcon;
    [SerializeField] private GameObject leaderIcon;
    [SerializeField] private Image raceIcon;
    
    [Space(10)]
    [Header("Store Target")] 
    [SerializeField] private StoreTargetButton btnStoreTarget;

    public bool isActive { get; private set; }

    public Character character { get; private set; }

    private void OnEnable() {
        Messenger.AddListener(Signals.TICK_ENDED, UpdateAllTextsAndIcon);
        if (character != null) {
            UpdateAllTextsAndIcon();
            btnStoreTarget.UpdateInteractableState();
        }
    }
    private void OnDisable() {
        Messenger.RemoveListener(Signals.TICK_ENDED, UpdateAllTextsAndIcon);
    }
    
    #region Overrides
    public override void SetObject(Character character) {
        base.SetObject(character);
        this.character = character;
        portrait.GeneratePortrait(character);
        btnStoreTarget.SetTarget(character);
        UpdateAllTextsAndIcon();
    }
    public override void UpdateObject(Character character) {
        base.UpdateObject(character);
        this.character = character;
        portrait.GeneratePortrait(character);
        UpdateAllTextsAndIcon();
    }
    public override void OnHoverEnter() {
        portrait.SetHoverHighlightState(true);
        base.OnHoverEnter();
    }
    public override void OnHoverExit() {
        portrait.SetHoverHighlightState(false);
        base.OnHoverExit();
    }
    public override void Reset() {
        base.Reset(); ;
        SetPortraitInteractableState(true);
        character = null;
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

    public void SetPortraitInteractableState(bool state) {
        portrait.ignoreInteractions = !state;
    }

    #region Sub Text
    private void UpdateAllTextsAndIcon() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"Character Nameplate - Update All Texts and Icon");
#endif
        UpdateMainAndActionText();
        UpdateSubTextAndIcon();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void UpdateMainAndActionText() {
        mainLbl.text = $"<b>{character.firstNameWithColor}</b>";
        supportingLbl.text = character.visuals.GetThoughtBubble();
        SetSupportingLabelState(true);
    }
    private void UpdateSubTextAndIcon() {
        if(!character.isNormalCharacter) {
            subLbl.gameObject.SetActive(false);
        } else {
            subLbl.text = character.characterClass.className;
            raceIcon.sprite = character.raceSetting.nameplateIcon;
            raceIcon.gameObject.SetActive(character.raceSetting.nameplateIcon != null);
            subLbl.gameObject.SetActive(true);
        }
    }
#endregion

#region Leader Icon
    public void OnHoverLeaderIcon() {
        string message = string.Empty;
        if (character.isSettlementRuler) {
            message = $"<b>{character.name}</b> is the Settlement Ruler of <b>{character.homeSettlement.name}</b>\n";
        } 
        if (character.isFactionLeader) {
            message += $"<b>{character.name}</b> is the Faction Leader of <b>{character.faction.name}</b>";
        }
        UIManager.Instance.ShowSmallInfo(message);
    }
    public void OnHoverExitLeaderIcon() {
        UIManager.Instance.HideSmallInfo();
    }
#endregion

#region Race Icon
    public void OnHoverRaceIcon() {
        string message = GameUtilities.GetNormalizedSingularRace(character.race);
        if (character.talentComponent != null) {
            message += "\n" + character.talentComponent.GetTalentSummary();
        }
        UIManager.Instance.ShowSmallInfo(message);
    }
    public void OnHoverExitRaceIcon() {
        UIManager.Instance.HideSmallInfo();
    }
#endregion
}
