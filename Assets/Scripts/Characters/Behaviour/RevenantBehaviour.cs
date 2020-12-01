using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Inner_Maps;

public class RevenantBehaviour : CharacterBehaviourComponent {
    public RevenantBehaviour() {
        priority = 8;
        // attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a revenant";
        if (character.gridTileLocation != null && (character.isAtHomeStructure || character.IsInHomeSettlement() || character.IsInTerritory())) {
            TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);
            if(currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT || currentTimeOfDay == TIME_IN_WORDS.LATE_NIGHT || currentTimeOfDay == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                log += $"\n-Early/Late Night or After Midnight, 10% chance to spawn a ghost";
                int roll = UnityEngine.Random.Range(0, 100);
                log += $"\n-Roll: " + roll;
                if(roll < 10) {
                    if (character is Revenant revenant) {
                        int numberOfGhosts = GetNumberOfGhostsInHome(revenant);
                        if (numberOfGhosts < 5) {
                            log += $"\n-Will spawn ghost";
                            Character betrayer = revenant.GetRandomBetrayer();
                            LocationGridTile tile = GetTileToSpawnGhostRelativeTo(revenant);
                            Summon ghost = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Ghost, FactionManager.Instance.undeadFaction, homeLocation: revenant.homeSettlement, homeRegion: revenant.homeRegion);
                            (ghost as Ghost).SetBetrayedBy(betrayer);
                            CharacterManager.Instance.PlaceSummon(ghost, tile);
                            revenant.AdjustNumOfSummonedGhosts(1);
                        } else {
                            log += $"\n-Already reached maximum number of spawned ghosts: 5";
                        }
                    }
                }
            }

            if (character.HasHome() && !character.IsAtHome()) {
                log += "\n-Return to territory";
                return character.jobComponent.PlanIdleReturnHome(out producedJob);
            } else {
                log += "\n-Already in territory or has no territory, Roam";
                return character.jobComponent.TriggerRoamAroundTile(out producedJob);
            }
        }
        return false;
    }

    private int GetNumberOfGhostsInHome(Character character) {
        int ghostCount = 0;
        if(character.homeSettlement != null) {
            ghostCount = character.homeSettlement.GetNumOfResidentsThatMeetCriteria(c => !c.isDead && c is Summon summon && summon.summonType == SUMMON_TYPE.Ghost);
        } else if (character.homeStructure != null) {
            ghostCount = character.homeStructure.GetNumberOfReidentsThatMeetCriteria(c => !c.isDead && c is Summon summon && summon.summonType == SUMMON_TYPE.Ghost);
        } else if (character.HasTerritory()) {
            ghostCount = CharacterManager.Instance.allCharacters.Count(c => !c.isDead && c.IsTerritory(character.territory) && c is Summon summon && summon.summonType == SUMMON_TYPE.Ghost);
        }
        return ghostCount;
    }
    private LocationGridTile GetTileToSpawnGhostRelativeTo(Character character) {
        LocationGridTile tile = null;
        if (character.homeSettlement != null) {
            tile = character.homeSettlement.GetRandomHexTile().GetRandomPassableTile();
        } else if (character.homeStructure != null) {
            tile = character.homeStructure.GetRandomPassableTile();
        } else if (character.HasTerritory()) {
            tile = character.territory.GetRandomPassableTile();
        }
        if(tile == null) {
            tile = character.gridTileLocation;
        }
        return tile;
    }
}