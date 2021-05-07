using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using Locations.Settlements;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UtilityScripts;
using Inner_Maps.Location_Structures;
public class DemonDefendBehaviour : CharacterBehaviourComponent {
    public DemonDefendBehaviour() {
        priority = 200;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;
        log += $"\n-Character is defending";
        Party party = character.partyComponent.currentParty;
        if (party.isActive && party.partyState == PARTY_STATE.Working) {
            log += $"\n-Party is working";
            DemonDefendPartyQuest quest = party.currentQuest as DemonDefendPartyQuest;
            LocationStructure targetStructure = quest.targetStructure;
            Area targetArea = quest.targetArea;

            Character memberInCombat = party.GetMemberInCombatExcept(character);
             if (memberInCombat != null) {
                log += $"\n-{memberInCombat.name} is in combat, will try to combat also";
                bool hasFought = false;
                CombatState combatState = memberInCombat.stateComponent.currentState as CombatState;
                if (combatState.currentClosestHostile != null) {
                    if (IsPOIInPlayerSettlementOrCorruptedTile(combatState.currentClosestHostile) || IsPOIInPlayerSettlementOrCorruptedTile(memberInCombat)) {
                        CombatData combatData = memberInCombat.combatComponent.GetCombatData(combatState.currentClosestHostile);
                        character.combatComponent.Fight(combatState.currentClosestHostile, combatData.reasonForCombat, combatData.connectedAction, combatData.isLethal);
                        hasFought = true;
                    }
                }
                if (hasFought) {
                    producedJob = null;
                    return true;
                }
            }
             
             //check if any demonic structures are being attacked
             for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.allStructures.Count; i++) {
                 LocationStructure structure = PlayerManager.Instance.player.playerSettlement.allStructures[i];
                 if (structure is DemonicStructure demonicStructure && demonicStructure.currentAttackers.Count > 0) {
                     Character attacker = demonicStructure.currentAttackers.First();
                     character.combatComponent.Fight(attacker, CombatManager.Defending_Home, null, true);
                     producedJob = null;
                     return true;
                 }
             }

            if (targetStructure != null && !targetStructure.hasBeenDestroyed) {
                if(character.currentStructure != targetStructure) {
                    LocationGridTile tile = targetStructure.GetRandomPassableTile();
                    if (tile != null) {
                        hasJob = character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
                    } else {
                        hasJob = character.jobComponent.TriggerRoamAroundTile(out producedJob);
                    }
                } else {
                    quest.SetIsSuccessful(true);
                    Character target = GetFirstHostileIntruderOf(character, targetStructure);
                    if (target != null) {
                        log += $"\n-Chosen target is {target.name}";
                        character.combatComponent.Fight(target, CombatManager.Hostility);
                    } else {
                        log += $"\n-Roam around";
                        hasJob = character.jobComponent.TriggerRoamAroundTile(out producedJob);
                    }
                }
            } else if (targetArea != null) {
                Character target = GetFirstHostileIntruderOf(character, targetArea);
                if (target != null) {
                    log += $"\n-Chosen target is {target.name}";
                    character.combatComponent.Fight(target, CombatManager.Hostility);
                } else {
                    log += $"\n-Roam around";
                    hasJob = character.jobComponent.TriggerRoamAroundTile(out producedJob);
                }
            }
        }
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        return hasJob;
    }
    private bool IsPOIInPlayerSettlementOrCorruptedTile(IPointOfInterest p_poi) {
        LocationGridTile gridTile = p_poi.gridTileLocation;
        if (gridTile != null) {
            BaseSettlement settlement;
            gridTile.IsPartOfSettlement(out settlement);
            return settlement == PlayerManager.Instance.player.playerSettlement || gridTile.corruptionComponent.isCorrupted;
        }
        return false;
    }
    private Character GetFirstHostileIntruderOf(Character actor, LocationStructure p_structure) {
        for (int i = 0; i < p_structure.charactersHere.Count; i++) {
            Character target = p_structure.charactersHere[i];
            if (actor != target && actor.IsHostileWith(target) && !target.isDead && !target.isAlliedWithPlayer
                && target.marker && target.marker.isMainVisualActive && actor.movementComponent.HasPathTo(target.gridTileLocation)
                && !target.isInLimbo && !target.isBeingSeized && target.carryComponent.IsNotBeingCarried()
                && !target.traitContainer.HasTrait("Hibernating", "Indestructible")) {
                return target;
            }
        }
        return null;
    }
    private Character GetFirstHostileIntruderOf(Character actor, Area p_area) {
        Character chosenTarget = p_area.locationCharacterTracker.GetFirstCharacterInsideHexThatMeetCriteria<Character>(target => actor != target && actor.IsHostileWith(target) && !target.isDead && !target.isAlliedWithPlayer
            && target.marker && target.marker.isMainVisualActive && actor.movementComponent.HasPathTo(target.gridTileLocation) && !target.isInLimbo && !target.isBeingSeized && target.carryComponent.IsNotBeingCarried()
            && !target.traitContainer.HasTrait("Hibernating", "Indestructible"));
        return chosenTarget;
    }
    public override void OnAddBehaviourToCharacter(Character character) {
        base.OnAddBehaviourToCharacter(character);
        character.behaviourComponent.OnBecomeDemonicDefender();
    }
    public override void OnRemoveBehaviourFromCharacter(Character character) {
        base.OnRemoveBehaviourFromCharacter(character);
        character.behaviourComponent.OnNoLongerDemonicDefender();
    }
    public override void OnLoadBehaviourToCharacter(Character character) {
        base.OnLoadBehaviourToCharacter(character);
        character.behaviourComponent.OnBecomeDemonicDefender();
    }
}