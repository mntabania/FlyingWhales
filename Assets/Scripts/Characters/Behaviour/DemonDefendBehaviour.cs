﻿using System.Collections;
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
            if (party.targetDestination.IsAtTargetDestination(character)) {
                log += $"\n-Character is at target destination, do work";
                DemonDefendPartyQuest quest = party.currentQuest as DemonDefendPartyQuest;
                LocationStructure targetStructure = quest.targetStructure;
                Area targetArea = quest.targetArea;
                if (targetStructure != null && !targetStructure.hasBeenDestroyed) {
                    Character target = GetFirstHostileIntruderOf(character, targetStructure);
                    if (target != null) {
                        log += $"\n-Chosen target is {target.name}";
                        character.combatComponent.Fight(target, CombatManager.Hostility);
                    } else {
                        log += $"\n-Roam around";
                        hasJob = character.jobComponent.TriggerRoamAroundTile(out producedJob);
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
        }
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        return hasJob;
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
}