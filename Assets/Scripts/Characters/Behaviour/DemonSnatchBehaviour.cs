using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

public class DemonSnatchBehaviour : CharacterBehaviourComponent {
    public DemonSnatchBehaviour () {
        priority = 200;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;
        log += $"\n-Character is snatching";
        Party party = character.partyComponent.currentParty;
        if (party.isActive && party.partyState == PARTY_STATE.Working) {
            log += $"\n-Party is working";
            DemonSnatchPartyQuest quest = party.currentQuest as DemonSnatchPartyQuest;
            if(quest.targetCharacter != null) {
                if (quest.targetCharacter.isDead) {
                    quest.EndQuest("Target is dead");
                    return true;
                }
                Prisoner prisoner = quest.targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                if (prisoner != null && prisoner.IsFactionPrisonerOf(character.faction)) {
                    party.GoBackHomeAndEndQuest();
                    quest.CreateSnatchJobFor(quest.targetCharacter, party);
                    return true;
                } else {
                    if (quest.targetCharacter.isBeingSeized) {
                        if(quest.targetCharacter.marker.previousGridTile == character.gridTileLocation) {
                            quest.EndQuest("Target is unavailable");
                        } else {
                            hasJob = character.jobComponent.CreateGoToSpecificTileJob(quest.targetCharacter.marker.previousGridTile, out producedJob);
                        }
                    } else {
                        hasJob = character.jobComponent.TriggerRestrainJob(quest.targetCharacter, JOB_TYPE.SNATCH_RESTRAIN, out producedJob);
                    }
                }
            }
        }
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        return hasJob;
    }

    private bool RoamAroundStructureOrHex(Character actor, IPartyQuestTarget target, out JobQueueItem producedJob) {
        if(target != null && target.currentStructure != null && target.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS) {
            if(target is Character targetCharacter && targetCharacter.gridTileLocation != null) {
                Area targetArea = targetCharacter.areaLocation;
                //Job type is Roam Around Structure because the Roam Around Tile job priority is less than the Rescue Behaviour
                return actor.jobComponent.TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_STRUCTURE, out producedJob, targetArea.gridTileComponent.GetRandomTile());
            }
        }
        //When roaming around structure or hex relative to the target and the target is not in a tile that we expect him to be, just roam aroung current structure to avoid null refs
        return actor.jobComponent.TriggerRoamAroundStructure(out producedJob);
    }
}
