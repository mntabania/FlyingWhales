using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterPickerItem : ObjectPickerItem<Character>, IPointerClickHandler {

    public Action<Character> onClickAction;

    private Character character;

    [SerializeField] private CharacterPortrait portrait;
    public GameObject portraitCover;

    public override Character obj { get { return character; } }

    public void SetCharacter(Character character) {
        this.character = character;
        UpdateVisuals();
    }

    public override void SetButtonState(bool state) {
        base.SetButtonState(state);
        portraitCover.SetActive(!state);
    }

    private void UpdateVisuals() {
        portrait.GeneratePortrait(character);
        mainLbl.text = character.name;
        subLbl.text =
            $"{UtilityScripts.GameUtilities.GetNormalizedSingularRace(character.race)} {character.characterClass.className}";
    }

    private void OnClick() {
        if (onClickAction != null) {
            onClickAction.Invoke(character);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            //Debug.Log("Right clicked character portrait!");
            portrait.OnClick();
        } else {
            OnClick();
        }
    }
}
