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
                if (IsCharacterConsideredTargetOf(actor, target) && target.gridTileLocation.IsPartOfSettlement(actor.homeSettlement)) {
                    return target;
                }
            }
        } else if(actor.homeStructure != null) {
            for (int i = 0; i < actor.homeStructure.charactersHere.Count; i++) {
                Character target = actor.homeStructure.charactersHere[i];
                if(IsCharacterConsideredTargetOf(actor, target)) {
                    return target;
                }
            }
        } else {
            HexTile hex = actor.areaLocation;
            if(hex != null) {
                Character chosenTarget = hex.GetFirstCharacterInsideHexThatMeetCriteria<Character>(target => IsCharacterConsideredTargetOf(actor, target));
                if(chosenTarget != null) {
                    return chosenTarget;
                }
            }
        }
        return null;
    }
    private bool IsCharacterConsideredTargetOf(Character p_considerer, Character p_targetCharacter) {
        if (p_considerer != p_targetCharacter
            && p_targetCharacter.gridTileLocation != null
            && !p_targetCharacter.isDead 
            && !p_targetCharacter.isAlliedWithPlayer
            && p_targetCharacter.marker 
            && p_targetCharacter.marker.isMainVisualActive
            && p_considerer.movementComponent.HasPathTo(p_targetCharacter.gridTileLocation)
            && !p_targetCharacter.isInLimbo 
            && !p_targetCharacter.isBeingSeized 
            && p_targetCharacter.carryComponent.IsNotBeingCarried()) {
            if(!p_targetCharacter.traitContainer.HasTrait("Hibernating", "Indestructible")) {
                if (p_considerer.IsHostileWith(p_targetCharacter)) {
                    if (!IsCharacterConsideredPrisonerOf(p_considerer, p_targetCharacter)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    private bool IsCharacterConsideredPrisonerOf(Character p_considerer, Character p_targetCharacter) {
        Prisoner prisoner = p_targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
        if(prisoner != null) {
            return prisoner.IsConsideredPrisonerOf(p_considerer);
        }
        return false;
    }
}