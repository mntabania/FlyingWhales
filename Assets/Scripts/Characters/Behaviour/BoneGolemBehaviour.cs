using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using Locations.Settlements;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UtilityScripts;
using Traits;

public class BoneGolemBehaviour : CharacterBehaviourComponent {
    public BoneGolemBehaviour() {
        priority = 8;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.IsAtHome()) {
            Character hostile = GetFirstHostileIntruder(character);
            if(hostile != null) {
                character.combatComponent.Fight(hostile, CombatManager.Hostility);
                producedJob = null;
                return true;
            } else {
                //Roam around tile
                return character.jobComponent.TriggerRoamAroundTile(out producedJob);
            }
        } else {
            //character is not at home, go back.
            return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
        }
    }
    private Character GetFirstHostileIntruder(Character actor) {
        if(actor.homeSettlement != null && actor.homeSettlement.region != null) {
            for (int i = 0; i < actor.homeSettlement.region.charactersAtLocation.Count; i++) {
                Character target = actor.homeSettlement.region.charactersAtLocation[i];
                if (CharacterManager.Instance.IsCharacterConsideredTargetOfBoneGolem(actor, target) && target.gridTileLocation.IsPartOfSettlement(actor.homeSettlement)) {
                    return target;
                }
            }
        } else if(actor.homeStructure != null) {
            for (int i = 0; i < actor.homeStructure.charactersHere.Count; i++) {
                Character target = actor.homeStructure.charactersHere[i];
                if(CharacterManager.Instance.IsCharacterConsideredTargetOfBoneGolem(actor, target)) {
                    return target;
                }
            }
        } else {
            Area area = actor.areaLocation;
            if(area != null) {
                Character chosenTarget = area.locationCharacterTracker.GetFirstCharacterInsideHexForBoneGolemBehaviour(actor);
                if(chosenTarget != null) {
                    return chosenTarget;
                }
            }
        }
        return null;
    }
}