using UnityEngine;
using System.Collections;
using ECS;
using System.Collections.Generic;

public class HuntMagicUser : CharacterTask {

    private ECS.Character _targetCharacter;
    private BaseLandmark _targetLandmark;

	public HuntMagicUser(TaskCreator createdBy, STANCE stance = STANCE.STEALTHY) : base(createdBy, TASK_TYPE.HUNT_MAGIC_USER, stance) {
        _specificTargetClassification = "character";
        _needsSpecificTarget = true;
        _filters = new TaskFilter[] {
            new MustBeClass(new List<CHARACTER_CLASS>(){ CHARACTER_CLASS.ARCANIST, CHARACTER_CLASS.MAGE, CHARACTER_CLASS.BATTLEMAGE,}),
        };
        _states = new Dictionary<STATE, State> {
            {STATE.MOVE, new MoveState(this)},
            {STATE.ATTACK, new AttackState(this, null)}
        };
		SetCombatPriority (30);
    }

    #region overrides
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
        if (_specificTarget == null) {
            _specificTarget = GetCharacterTarget(character);
        }
		if(_specificTarget != null && _specificTarget is ECS.Character){
			_targetCharacter = (ECS.Character)_specificTarget;
			if (_targetLocation == null) {
				_targetLocation = _targetCharacter.specificLocation;
			}
			if (_targetLocation != null) {
                ChangeStateTo(STATE.MOVE);
				_assignedCharacter.GoToLocation(_targetLocation, PATHFINDING_MODE.USE_ROADS, () => ChangeStateTo(STATE.ATTACK));
			}else{
				EndTask (TASK_STATUS.FAIL);
			}
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
    }
    public override bool CanBeDone(Character character, ILocation location) {
        if(location.tileLocation.landmarkOnTile != null) {
            for (int i = 0; i < location.tileLocation.landmarkOnTile.charactersAtLocation.Count; i++) {
                ECS.Character currChar = location.tileLocation.landmarkOnTile.charactersAtLocation[i];
				if (currChar.id != character.id && CanMeetRequirements(currChar)) {
                    return true;
                }
            }
        }
        return base.CanBeDone(character, location);
    }
    public override bool AreConditionsMet(Character character) {
        //If there are any magic users in the region (Characters that use staves, or belong to the mage/battlemage class)
        for (int i = 0; i < character.specificLocation.tileLocation.region.charactersInRegion.Count; i++) {
            ECS.Character currChar = character.specificLocation.tileLocation.region.charactersInRegion[i];
            if (currChar.id != character.id) {
                if (CanMeetRequirements(currChar)) {
                    return true;
                }
            }
        }
        return base.AreConditionsMet(character);
    }
    public override int GetSelectionWeight(Character character) {
        return 100;
    }
    protected override Character GetCharacterTarget(Character character) {
        base.GetCharacterTarget(character);
        for (int i = 0; i < character.specificLocation.tileLocation.region.charactersInRegion.Count; i++) {
            ECS.Character currChar = character.specificLocation.tileLocation.region.charactersInRegion[i];
            if (currChar.id != character.id) {
                if (CanMeetRequirements(currChar)) {
                    int weight = 20; //Each magic user in region has base weight: 20
                    if (currChar.remainingHPPercent < 50) {
                        weight += 100; //If the magic user has less than 50% hp: +100
                    }
                    if (currChar.isFollower) {
                        weight += 200; //If the magic user is a follower: +200
                    }
                    _characterWeights.AddElement(currChar, weight);
                }
            }
        }
        LogTargetWeights(_characterWeights);
        if (_characterWeights.GetTotalOfWeights() > 0) {
            return _characterWeights.PickRandomElementGivenWeights();
        }
        return null;
    }
    #endregion
}
