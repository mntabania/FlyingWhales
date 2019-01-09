﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookOfTheDead : SpecialToken {

    public BookOfTheDead() : base(SPECIAL_TOKEN.BOOK_OF_THE_DEAD) {
        //quantity = 1;
        weight = 20;
        npcAssociatedInteractionType = INTERACTION_TYPE.USE_ITEM_ON_SELF;
    }

    #region Overrides
    public override void CreateJointInteractionStates(Interaction interaction, Character user, object target) {
        TokenInteractionState itemUsedState = new TokenInteractionState(Item_Used, interaction, this);
        TokenInteractionState stopFailState = new TokenInteractionState(Stop_Fail, interaction, this);
        itemUsedState.SetTokenUserAndTarget(user, target);
        stopFailState.SetTokenUserAndTarget(user, target);

        if (target != null) {
            //This means that the interaction is not from Use Item On Self, rather, it is from an interaction which a minion triggered
            itemUsedState.SetEffect(() => ItemUsedEffectMinion(itemUsedState));
        } else {
            itemUsedState.SetEffect(() => ItemUsedEffectNPC(itemUsedState));
        }
        stopFailState.SetEffect(() => StopFailEffect(stopFailState));

        interaction.AddState(itemUsedState);
        interaction.AddState(stopFailState);
    }
    public override bool CanBeUsedBy(Character sourceCharacter) {
        //return true;
        return (sourceCharacter.race == RACE.HUMANS || sourceCharacter.race == RACE.GOBLIN || sourceCharacter.race == RACE.ELVES) 
            && sourceCharacter.gender == GENDER.MALE && sourceCharacter.specificLocation.tileLocation.areaOfTile.name == "Gloomhollow Crypts" 
            && sourceCharacter.specificLocation.tileLocation.areaOfTile.owner == null;
    }
    #endregion

    private void ItemUsedEffectMinion(TokenInteractionState state) {
        Character targetCharacter = state.target as Character;
        targetCharacter.ChangeClass("Necromancer");
        targetCharacter.ChangeRace(RACE.SKELETON);
        targetCharacter.SetForcedInteraction(null);

        Faction oldFaction = targetCharacter.faction;
        targetCharacter.faction.RemoveCharacter(targetCharacter);

        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName("Ziranna");
        newFaction.SetLeader(targetCharacter);
        newFaction.AddNewCharacter(targetCharacter);
        FactionManager.Instance.neutralFaction.UnownArea(targetCharacter.specificLocation.tileLocation.areaOfTile);
        LandmarkManager.Instance.OwnArea(newFaction, newFaction.raceType, targetCharacter.specificLocation.tileLocation.areaOfTile);

        newFaction.GetRelationshipWith(oldFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.ENEMY);
        newFaction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.ALLY);
        newFaction.SetFactionActiveState(true);

        Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-minion" + "_description");
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.MINION_1);
        stateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.OverrideDescriptionLog(stateDescriptionLog);

        Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special1");
        log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.MINION_1);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.AddLogToInvolvedObjects(log);
    }
    private void ItemUsedEffectNPC(TokenInteractionState state) {
        state.tokenUser.ChangeClass("Necromancer");
        state.tokenUser.ChangeRace(RACE.SKELETON);
        state.tokenUser.SetForcedInteraction(null);
        state.tokenUser.MigrateTo(state.tokenUser.specificLocation as BaseLandmark);

        Faction oldFaction = state.tokenUser.faction;
        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName("Ziranna");
        newFaction.SetLeader(state.tokenUser);
        state.tokenUser.ChangeFactionTo(newFaction);
        FactionManager.Instance.neutralFaction.UnownArea(state.tokenUser.specificLocation.tileLocation.areaOfTile);
        LandmarkManager.Instance.OwnArea(newFaction, newFaction.raceType, state.tokenUser.specificLocation.tileLocation.areaOfTile);

        newFaction.GetRelationshipWith(oldFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.DISLIKED);
        newFaction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.DISLIKED);
        newFaction.SetFactionActiveState(true);

        Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-npc" + "_description");
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.OverrideDescriptionLog(stateDescriptionLog);

        Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special2");
        log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.AddLogToInvolvedObjects(log);

        Debug.LogWarning("[Day " + GameManager.Instance.continuousDays + "] " + state.tokenUser.name + " used " + name + " on " + Utilities.GetPronounString(state.tokenUser.gender, PRONOUN_TYPE.REFLEXIVE, false) + " and became a " + state.tokenUser.characterClass.className + " at " + state.tokenUser.specificLocation.tileLocation.areaOfTile.name);
    }
    private void StopFailEffect(TokenInteractionState state) {
        state.tokenUser.ChangeClass("Necromancer");
        state.tokenUser.ChangeRace(RACE.SKELETON);
        state.tokenUser.SetForcedInteraction(null);

        Faction oldFaction = state.tokenUser.faction;
        state.tokenUser.faction.RemoveCharacter(state.tokenUser);

        Faction newFaction = FactionManager.Instance.GetFactionBasedOnName("Ziranna");
        newFaction.SetLeader(state.tokenUser);
        newFaction.AddNewCharacter(state.tokenUser);
        FactionManager.Instance.neutralFaction.UnownArea(state.tokenUser.specificLocation.tileLocation.areaOfTile);
        LandmarkManager.Instance.OwnArea(newFaction, newFaction.raceType, state.tokenUser.specificLocation.tileLocation.areaOfTile);

        newFaction.GetRelationshipWith(oldFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.DISLIKED);
        newFaction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.DISLIKED);
        newFaction.SetFactionActiveState(true);

        state.descriptionLog.AddToFillers(state.interaction.investigatorMinion, state.interaction.investigatorMinion.name, LOG_IDENTIFIER.MINION_1);

        state.AddLogFiller(new LogFiller(state.interaction.investigatorMinion, state.interaction.investigatorMinion.name, LOG_IDENTIFIER.MINION_1));

        Debug.LogWarning("[Day " + GameManager.Instance.continuousDays + "] " + state.tokenUser.name + " used " + name + " on " + Utilities.GetPronounString(state.tokenUser.gender, PRONOUN_TYPE.REFLEXIVE, false) + " and became a " + state.tokenUser.characterClass.className + " at " + state.tokenUser.specificLocation.tileLocation.areaOfTile.name);
    }
}
