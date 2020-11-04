using UnityEngine;
using System.Collections;
using ECS;

public class ProclaimSuccessor : CharacterTask {
	public ProclaimSuccessor(TaskCreator createdBy, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.PROCLAIM_SUCCESSOR, stance) {
        _needsSpecificTarget = true;
        _specificTargetClassification = "character";
        _filters = new TaskFilter[] {
            new MustNotHaveTags(CHARACTER_TAG.SUCCESSOR),
            new MustBeFaction((createdBy as ECS.Character).faction)
        };
        _states = new System.Collections.Generic.Dictionary<STATE, State>() {
            { STATE.PROCLAIM_SUCCESSOR, new ProclaimSuccessorState(this) }
        };
    }

    #region overrides
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
        ChangeStateTo(STATE.PROCLAIM_SUCCESSOR);
    }
    public override bool CanBeDone(Character character, ILocation location) {
        if (character.faction != null && character.faction is Tribe) { //If character is part of a faction and there are no Successor tag for any character of the faction
            if ((character.faction as Tribe).successor == null) {
                for (int i = 0; i < location.charactersAtLocation.Count; i++) {
                    ECS.Character currCharacter = location.charactersAtLocation[i];
                    if (CanMeetRequirements(currCharacter)) {
                        return true;
                    }
                }            
            }
        }
        return base.CanBeDone(character, location);
    }
    public override bool AreConditionsMet(Character character) {
        if (character.faction != null && character.faction is Tribe) { //If character is part of a faction and there are no Successor tag for any character of the faction
            if ((character.faction as Tribe).successor == null) {
                for (int i = 0; i < character.faction.characters.Count; i++) {
                    ECS.Character currCharacter = character.faction.characters[i];
                    if (CanMeetRequirements(currCharacter)) {
                        return true;
                    }
                }
            }
        }
        return base.AreConditionsMet(character);
    }
    public override int GetSelectionWeight(Character character) {
        return 20;
    }
    #endregion
}
