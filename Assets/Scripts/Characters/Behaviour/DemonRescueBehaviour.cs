using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class DemonRescueBehaviour : CharacterBehaviourComponent {
    public DemonRescueBehaviour() {
        priority = 200;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;
#if DEBUG_LOG
        log += $"\n-Character is rescuing";
#endif
        Party party = character.partyComponent.currentParty;
        if (party.isActive && party.partyState == PARTY_STATE.Working) {
#if DEBUG_LOG
            log += $"\n-Party is working";
#endif
            DemonRescuePartyQuest quest = party.currentQuest as DemonRescuePartyQuest;
            DemonicStructure targetStructure = quest.targetDemonicStructure;

            if (targetStructure.hasBeenDestroyed || targetStructure.objectsThatContributeToDamage.Count <= 0) {
                if (IsInTargetDemonicStructure(character, quest)) {
                    if (character.hasMarker && IsInTargetDemonicStructure(quest.targetCharacter, quest)) {
                        if (quest.targetCharacter.isDead) {
                            quest.SetIsSuccessful(true);
                            quest.EndQuest("Target is dead");
                        } else {
                            if (quest.targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared", "Enslaved")) {
                                hasJob = character.jobComponent.TriggerReleaseJob(quest.targetCharacter, out producedJob);
                                if (hasJob) {
                                    quest.SetIsReleasing(true);
                                }
                                return hasJob;
                            } else {
                                quest.SetIsSuccessful(true);
                                quest.EndQuest("Target is safe");
                                //if target is paralyzed carry back home
                                if (quest.targetCharacter.traitContainer.HasTrait("Paralyzed")) {
                                    if (!quest.targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.RESCUE_MOVE_CHARACTER)) {
                                        //Do not set this as a party job
                                        character.jobComponent.TryTriggerRescueMoveCharacter(quest.targetCharacter, out producedJob, false);
                                    }
                                }
                                return true;
                            }
                        }
                    } else {
                        quest.SetIsSuccessful(true);
                        quest.EndQuest("Target is nowhere to be found");
                        return true;
                    }
                } else {
                    LocationGridTile tileToGoTo = GetRandomPassableTileThatHasPathToFor(character, quest.targetDemonicStructureTiles);
                    if (tileToGoTo != null) {
                        hasJob = character.jobComponent.CreatePartyGoToSpecificTileJob(tileToGoTo, out producedJob);
                        return hasJob;
                    }
                }
            } else {
#if DEBUG_LOG
                log += "\n-Has tile object that contribute damage";
                log += "\n-Adding tile object as hostile";
#endif
                TileObject chosenTileObject = null;
                IDamageable nearestDamageableObject = targetStructure.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
                if (nearestDamageableObject != null && nearestDamageableObject is TileObject tileObject) {
                    chosenTileObject = tileObject;
                }
                if (chosenTileObject != null) {
                    character.combatComponent.Fight(chosenTileObject, CombatManager.Clear_Demonic_Intrusion);
                    return true;
                }
            }
            hasJob = RoamAroundStructureOrHex(character, party.currentQuest.target, out producedJob);
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
    private bool IsInTargetDemonicStructure(Character p_character, DemonRescuePartyQuest p_quest) {
        if (!p_quest.targetDemonicStructure.hasBeenDestroyed) {
            return p_character.currentStructure == p_quest.targetDemonicStructure;
        } else {
            return p_quest.targetDemonicStructureTiles.Contains(p_character.gridTileLocation);
        }
    }
    private LocationGridTile GetRandomPassableTileThatHasPathToFor(Character p_character, List<LocationGridTile> p_tiles) {
        LocationGridTile chosenTile = null;
        if (p_character.gridTileLocation != null) {
            List<LocationGridTile> tilesToGoTo = UtilityScripts.RuinarchListPool<LocationGridTile>.Claim();
            for (int i = 0; i < p_tiles.Count; i++) {
                LocationGridTile tile = p_tiles[i];
                if (p_character.movementComponent.HasPathToEvenIfDiffRegion(tile) && tile.IsPassable()) {
                    tilesToGoTo.Add(tile);
                }
            }
            if (tilesToGoTo.Count > 0) {
                chosenTile = tilesToGoTo[UtilityScripts.GameUtilities.RandomBetweenTwoNumbers(0, tilesToGoTo.Count - 1)];
            }
            UtilityScripts.RuinarchListPool<LocationGridTile>.Release(tilesToGoTo);
        }
        return chosenTile;
    }
}
