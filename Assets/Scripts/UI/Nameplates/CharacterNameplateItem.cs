﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterNameplateItem : NameplateItem<Character> {

    [Header("Character Nameplate Attributes")]
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private GameObject travellingIcon;
    [SerializeField] private GameObject arrivedIcon;
    [SerializeField] private GameObject restrainedIcon;

    public Character character { get; private set; }

    public override void SetObject(Character character) {
        base.SetObject(character);
        this.character = character;
        mainLbl.text = character.name;
        subLbl.text = character.raceClassName;
        portrait.GeneratePortrait(character);
        UpdateStatusIcons();
    }

    public override void Reset() {
        base.Reset();
        SetInteractableState(true);
    }

    /// <summary>
    /// Set this nameplate to behave in the default settings (button, onclick shows character UI, etc.)
    /// </summary>
    public void SetAsDefaultBehaviour() {
        SetAsButton();
        ClearAllOnClickActions();
        AddOnClickAction(UIManager.Instance.ShowCharacterInfo);
        SetSupportingLabelState(false);
    }

    private void UpdateStatusIcons() {
        if (character.currentParty.icon.isTravellingOutside) {
            //character is travelling outside
            travellingIcon.SetActive(true);
            arrivedIcon.SetActive(false);
            restrainedIcon.SetActive(false);
        } else if (!character.isAtHomeRegion) {
            //character is at another location other than his/her home region
            travellingIcon.SetActive(false);
            arrivedIcon.SetActive(true);
            restrainedIcon.SetActive(false);
        } else if (character.GetNormalTrait("Restrained") != null) {
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
}
