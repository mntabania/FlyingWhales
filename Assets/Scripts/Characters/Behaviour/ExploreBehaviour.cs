using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class ExploreBehaviour : CharacterBehaviourComponent {
    public ExploreBehaviour() {
        priority = 200;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;
#if DEBUG_LOG
        log += $"\n-Character is exploring";
#endif
        Party party = character.partyComponent.currentParty;
        if (party.isActive && party.partyState == PARTY_STATE.Working) {
#if DEBUG_LOG
            log += $"\n-Party is working";
#endif
            if (party.targetDestination.IsAtTargetDestination(character)) {
#if DEBUG_LOG
                log += $"\n-Character is at target destination, do work";
                log += $"\n-Character is at target destination, will try to combat residents";
#endif
                LocationStructure targetStructure = (party.currentQuest as ExplorationPartyQuest).targetStructure;
                Character target = targetStructure.GetRandomResidentForInvasionTargetThatIsInsideStructureAndHostileWithFaction(character.faction, character);
                if (target != null) {
                    character.combatComponent.Fight(target, CombatManager.Hostility);
                    producedJob = null;
                    return true;
                }

                Character memberInCombat = party.GetMemberInCombatExcept(character);
                if (memberInCombat != null && party.targetDestination.IsAtTargetDestination(memberInCombat)) {
#if DEBUG_LOG
                    log += $"\n-{memberInCombat.name} is in combat, will try to combat also";
#endif
                    bool hasFought = false;
                    CombatState combatState = memberInCombat.stateComponent.currentState as CombatState;
                    if (combatState.currentClosestHostile != null) {
                        CombatData combatData = memberInCombat.combatComponent.GetCombatData(combatState.currentClosestHostile);
                        character.combatComponent.Fight(combatState.currentClosestHostile, combatData.reasonForCombat, combatData.connectedAction, combatData.isLethal);
                        hasFought = true;
                    }
                    if (hasFought) {
                        producedJob = null;
                        return true;
                    }
#if DEBUG_LOG
                    log += $"\n-Roam around";
#endif
                    hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                } else {
#if DEBUG_LOG
                    log += $"\n-Roam around";
#endif
                    hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                }
            } 
        }
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        return hasJob;
    }
}
