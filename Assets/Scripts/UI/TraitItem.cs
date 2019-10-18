﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TraitItem : MonoBehaviour {
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image iconImg;

    [SerializeField] private CharacterPortrait portrait;

    private Trait trait;

    public void SetCombatAttribute(Trait trait) {
        this.trait = trait;
        nameText.text = this.trait.nameInUI;

        if (this.trait is RelationshipTrait) {
            RelationshipTrait relTrait = this.trait as RelationshipTrait;
            portrait.GeneratePortrait(relTrait.targetCharacter);
            portrait.gameObject.SetActive(true);
            iconImg.gameObject.SetActive(false);
            //portrait.SetClickButton(UnityEngine.EventSystems.PointerEventData.InputButton.Left);
        } else {
            portrait.gameObject.SetActive(false);
            Sprite icon = AttributeManager.Instance.GetTraitIcon(trait.name);
            if (icon != null) {
                iconImg.sprite = icon;
                iconImg.gameObject.SetActive(true);
            } else {
                iconImg.gameObject.SetActive(false);
            }
        }


        descriptionText.text = this.trait.description;
        this.gameObject.SetActive(true);
    }

    public void OnHover() {
        if(trait != null) {
            string summary = trait.nameInUI;
            if (trait is RelationshipTrait) {
                RelationshipTrait t = trait as RelationshipTrait;
                for (int i = 0; i < t.targetCharacter.alterEgos.Values.Count; i++) {
                    AlterEgoData currAlterEgo = t.targetCharacter.alterEgos.Values.ElementAt(i);
                    if (UIManager.Instance.characterInfoUI.activeCharacter.HasRelationshipWith(currAlterEgo, true)) {
                        CharacterRelationshipData rel = UIManager.Instance.characterInfoUI.activeCharacter.relationships[currAlterEgo];
                        summary += "\n" + rel.GetSummary();
                        break;
                    }
                }
                //if (UIManager.Instance.characterInfoUI.activeCharacter.HasRelationshipWith(t.targetCharacter, true)) {
                //CharacterRelationshipData rel = UIManager.Instance.characterInfoUI.activeCharacter.relationships[t.targetCharacter.currentAlterEgo];
                //summary += "\n" + rel.GetSummary();
                //} else {
                //    summary = string.Empty;
                //}
            } else {
                summary += "\n" + trait.GetTestingData();
            }
            if(summary != string.Empty) {
                UIManager.Instance.ShowSmallInfo(summary);
            }
        }
    }
    public void OnHoverOut() {
        if (trait != null) {
            UIManager.Instance.HideSmallInfo();
        }
    }
}
