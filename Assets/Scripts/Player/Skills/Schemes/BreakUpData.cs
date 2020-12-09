﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class BreakUpData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BREAK_UP;
    public override string name => "Break Up";
    public override string description => "Break Up";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

    public BreakUpData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character targetCharacter) {
            int loverAffairCount = targetCharacter.relationshipContainer.GetRelatablesWithRelationshipCount(RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
            if (loverAffairCount > 1) {
                List<Character> choices = ObjectPoolManager.Instance.CreateNewCharactersList();
                foreach (KeyValuePair<int, IRelationshipData> kvp in targetCharacter.relationshipContainer.relationships) {
                    if (kvp.Value.HasRelationship(RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR)) {
                        Character character = CharacterManager.Instance.GetCharacterByID(kvp.Key);
                        if(character != null) {
                            choices.Add(character);
                        }
                    }
                }
                UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseCharacter(o, targetCharacter), validityChecker: t => CanBeBrokenUp(targetCharacter, t), showCover: true,
                    shouldShowConfirmationWindowOnPick: false, layer: 25);
                ObjectPoolManager.Instance.ReturnCharactersListToPool(choices);
            } else {
                //If only 1 lover or affair do not pick anymore
                //Show Scheme UI
                Character loverOrAffair = targetCharacter.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
                UIManager.Instance.ShowSchemeUI(targetCharacter, loverOrAffair, this);
            }
        }
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character character) {
            return character.isNormalCharacter && character.relationshipContainer.HasRelationship(RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
        }
        return base.IsValid(target);
    }
    protected override void OnSuccessScheme(Character character, object target) {
        base.OnSuccessScheme(character, target);
        if(target is Character targetCharacter) {
            character.interruptComponent.TriggerInterrupt(INTERRUPT.Break_Up, targetCharacter);
        }
    }
    protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful) {
        if (target is Character targetCharacter) {
            ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData($"You must break up with {targetCharacter.name}.", null, DialogItem.Position.Right);
            conversationList.Add(data);
        }
        base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
    }
    public override float GetSuccessRateMultiplier(Character p_targetCharacter) {
        if (p_targetCharacter.traitContainer.HasTrait("Unfaithful")) {
            return 2f;
        }
        return base.GetSuccessRateMultiplier(p_targetCharacter);
    }
    public override string GetSuccessRateMultiplierText(Character p_targetCharacter) {
        if (p_targetCharacter.traitContainer.HasTrait("Unfaithful")) {
            return $"{p_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} is Unfaithful";
        }
        return base.GetSuccessRateMultiplierText(p_targetCharacter);
    }
    #endregion

    private bool CanBeBrokenUp(Character source, Character target) {
        return true;
    }
    private void OnChooseCharacter(object obj, Character source) {
        if (obj is Character targetCharacter) {
            UIManager.Instance.HideObjectPicker();

            //Show Scheme UI
            UIManager.Instance.ShowSchemeUI(source, targetCharacter, this);
        }
    }
}
