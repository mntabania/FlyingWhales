using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class GhostBehaviour : CharacterBehaviourComponent {
    public GhostBehaviour() {
        priority = 8;
        // attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a ghost";
        if (character.gridTileLocation != null) {
            if (character.HasTerritory()) {
                //Only get the first territory because right now even though the territory is a list, only one territory is being assigned at a time
                HexTile territory = character.territorries[0];
                if (territory.HasAliveVillagerResident()) {
                    character.ClearTerritory();
                }
            }
            if (!character.HasTerritory()) {
                log += "\n-No territory, will set nearest hex tile as territory";
                HexTile hex = character.gridTileLocation.collectionOwner.GetNearestPlainHexTileWithNoResident();
                if(hex != null) {
                    character.AddTerritory(hex);
                }
            }
            TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);
            if(currentTimeOfDay == TIME_IN_WORDS.LATE_NIGHT || currentTimeOfDay == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                if (character is Ghost ghost) {
                    if (ghost.betrayedBy != null) {
                        log += $"\n-15% chance to attack a source of betrayal if still alive and currently in the region";
                        if(!ghost.betrayedBy.isDead && ghost.betrayedBy.currentRegion == ghost.currentRegion) {
                            log += $"\n-Will attack";
                            if(ghost.jobComponent.CreateDemonKillJob(ghost.betrayedBy, out producedJob)) {
                                return true;
                            }
                        } else {
                            log += $"\n-Betrayer is either dead or not in current region";
                        }
                    }
                }
            }

            if ((character.HasTerritory() && !character.IsInTerritory()) || (character.homeStructure != null && !character.isAtHomeStructure)) {
                log += "\n-Return to territory";
                return character.jobComponent.PlanIdleReturnHome(out producedJob);
            } else {
                log += "\n-Already in territory or has no territory, Roam";
                return character.jobComponent.TriggerRoamAroundTile(out producedJob);
            }
        }
        return false;
    }
}