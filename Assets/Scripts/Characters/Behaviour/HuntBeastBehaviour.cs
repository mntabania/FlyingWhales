using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class HuntBeastBehaviour : CharacterBehaviourComponent {
    public HuntBeastBehaviour() {
        priority = 200;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
#if DEBUG_LOG
        log += $"\n-Character is hunting beasts";
#endif
        Party party = character.partyComponent.currentParty;
        if (party.isActive && party.partyState == PARTY_STATE.Working) {
#if DEBUG_LOG
            log += $"\n-Party is working";
#endif
            if (party.targetDestination.IsAtTargetDestination(character)) {
#if DEBUG_LOG
                log += $"\n-Character is at target destination, do work";
#endif
                HuntBeastPartyQuest quest = party.currentQuest as HuntBeastPartyQuest;
                LocationStructure targetStructure = quest.targetStructure;
                if (!targetStructure.hasBeenDestroyed) {
#if DEBUG_LOG
                    log += $"\n-Will do hunt";
#endif
                    LocationGridTile firstTile = targetStructure.GetFirstTileWithObject();
                    AnimalBurrow burrow = firstTile.tileObjectComponent.objHere as AnimalBurrow;
                    if (burrow != null) {
                        Summon targetMonster = burrow.GetRandomAliveSpawnedMonster();
                        if (targetMonster != null) {
#if DEBUG_LOG
                            log += $"\n-Will hunt: {targetMonster.nameWithID}";
#endif
                            character.combatComponent.Fight(targetMonster, CombatManager.Hostility);
                            return true;
                        }
#if DEBUG_LOG
                        log += $"\n-No longer alive monster, will try haul corpses";
#endif
                        targetMonster = burrow.GetRandomDeadSpawnedMonster();
                        if (targetMonster != null) {
#if DEBUG_LOG
                            log += $"\n-Will haul {targetMonster.nameWithID}";
#endif
                            character.jobComponent.TryTriggerHaulAnimalCorpse(targetMonster, out producedJob);
                            if (producedJob != null) {
                                producedJob.SetIsThisAPartyJob(true);
                                return true;
                            }
                        }
#if DEBUG_LOG
                        log += $"\n-No alive or dead spawned monster, End Hunt";
#endif
                        party.currentQuest.EndQuest("Finished quest");
                        return true;
                    }

                    //if(character.jobComponent.TriggerRoamAroundTile(out producedJob)) {
                    //    producedJob.SetIsThisAPartyJob(true);
                    //    return true;
                    //}
#if DEBUG_LOG
                    log += $"\n-No burrow, End Hunt";
#endif
                    party.currentQuest.EndQuest("Structure is destroyed");
                    return true;

                } else {
#if DEBUG_LOG
                    log += $"\n-Structure is destroyed, End Hunt";
#endif
                    party.currentQuest.EndQuest("Structure is destroyed");
                    return true;
                }
            } else {
                LocationGridTile tile = party.targetDestination.GetRandomPassableTile();
                character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
                if (producedJob != null) {
                    producedJob.SetIsThisAPartyJob(true);
                    return true;
                }
            }
        }
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        return false;
    }


    private Summon GetRandomDeadBeastToHaul(Area p_area) {
        Summon chosenBeast = null;
        List<Character> pool = RuinarchListPool<Character>.Claim();
        for (int i = 0; i < p_area.locationCharacterTracker.charactersAtLocation.Count; i++) {
            Character c = p_area.locationCharacterTracker.charactersAtLocation[i];
            if (c.isDead && c.hasMarker && c is Summon summon) {
                if (summon.summonType.IsAnimalBeast()) {
                    pool.Add(c);
                }
            }
        }
        if (pool.Count > 0) {
            chosenBeast = pool[GameUtilities.RandomBetweenTwoNumbers(0, pool.Count - 1)] as Summon;
        }
        RuinarchListPool<Character>.Release(pool);
        return chosenBeast;
    }
}