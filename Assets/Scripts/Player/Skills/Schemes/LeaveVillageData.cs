﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Inner_Maps;
using Logs;

public class LeaveVillageData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LEAVE_VILLAGE;
    public override string name => "Leave Village";
    public override string description => "Convince a Villager to leave their current Village.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

    public LeaveVillageData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            UIManager.Instance.ShowSchemeUI(targetCharacter, null, this);
        }
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character character) {
            bool isValid = base.IsValid(target);
            return isValid && character.homeSettlement != null && !character.isConsideredRatman;
        }
        return false;
    }
    protected override void OnSuccessScheme(Character character, object target) {
        base.OnSuccessScheme(character, target);
        character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Village, character);
        Area chosenArea = character.currentRegion.GetRandomHexThatMeetCriteria(a => a.elevationType != ELEVATION.WATER && a.elevationType != ELEVATION.MOUNTAIN && a.structureComponent.HasStructureInArea() == false && !a.IsNextToOrPartOfVillage() && !a.gridTileComponent.HasCorruption());
        if (chosenArea != null) {
            LocationGridTile chosenTile = chosenArea.gridTileComponent.GetRandomPassableTile();
            if (chosenTile != null) {
                character.jobComponent.CreateGoToJob(chosenTile);
            }
        }
    }
    protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character targetCharacter, object target, bool isSuccessful) {
        ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData("I want you to abandon your current Village.", null, DialogItem.Position.Right);
        conversationList.Add(data);
        base.PopulateSchemeConversation(conversationList, targetCharacter, target, isSuccessful);
    }
    #endregion
}
