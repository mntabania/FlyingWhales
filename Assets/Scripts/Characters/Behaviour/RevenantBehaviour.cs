using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps.Location_Structures;
using UtilityScripts;
using Inner_Maps;

public class RevenantBehaviour : BaseMonsterBehaviour {
    public RevenantBehaviour() {
        priority = 8;
    }
    protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
#if DEBUG_LOG
        log += $"\n-{character.name} is a revenant";
#endif
        if (character.gridTileLocation != null && (character.isAtHomeStructure || character.IsInHomeSettlement() || character.IsInTerritory())) {
            TIME_IN_WORDS currentTimeOfDay = GameManager.Instance.GetCurrentTimeInWordsOfTick(character);
            if(currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT || currentTimeOfDay == TIME_IN_WORDS.LATE_NIGHT || currentTimeOfDay == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += $"\n-Early/Late Night or After Midnight, 10% chance to spawn a ghost";
                log += $"\n-Roll: " + roll;
#endif
                if (roll < 10) {
                    TrySpawnGhost(character, ref log);
                }
            }
            if (character.HasHome() && !character.IsAtHome()) {
#if DEBUG_LOG
                log += "\n-Return to territory";
#endif
                return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
            } else {
#if DEBUG_LOG
                log += "\n-Already in territory or has no territory, Roam";
#endif
                return character.jobComponent.TriggerRoamAroundTile(out producedJob);
            }
        }
        return false;
    }
    protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
        if (TryTakeSettlementJob(p_character, ref p_log, out p_producedJob)) {
            return true;
        } else {
            if (GameUtilities.RollChance(10)) {
                if (TrySpawnGhost(p_character, ref p_log)) {
                    return false;
                }
            }
            return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
        }
    }

    private bool TrySpawnGhost(Character p_character, ref string p_log) {
        if (p_character is Revenant revenant) {
            int numberOfGhosts = GetNumberOfGhostsInHome(revenant);
            if (numberOfGhosts < 5) {
#if DEBUG_LOG
                p_log += $"\n-Will spawn ghost";
#endif
                Character betrayer = revenant.GetRandomBetrayer();
                LocationGridTile tile = GetTileToSpawnGhostRelativeTo(revenant);
                var targetFaction = p_character.faction ?? FactionManager.Instance.undeadFaction;
                Summon ghost = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Ghost, targetFaction, homeLocation: revenant.homeSettlement, homeRegion: revenant.homeRegion, bypassIdeologyChecking: true);
                (ghost as Ghost).SetBetrayedBy(betrayer);
                CharacterManager.Instance.PlaceSummonInitially(ghost, tile);
                return true;
            } else {
#if DEBUG_LOG
                p_log += $"\n-Already reached maximum number of spawned ghosts: 5";
#endif
                return false;
            }
        }
        return false;
    }

    private int GetNumberOfGhostsInHome(Character character) {
        int ghostCount = 0;
        if(character.homeSettlement != null) {
            ghostCount = character.homeSettlement.GetNumberOfResidentsThatIsAliveMonsterAndMonsterTypeIs(SUMMON_TYPE.Ghost);
        } else if (character.homeStructure != null) {
            ghostCount = character.homeStructure.GetNumberOfResidentsThatIsAliveMonsterAndMonsterTypeIs(SUMMON_TYPE.Ghost);
        } else if (character.HasTerritory()) {
            ghostCount = CharacterManager.Instance.allCharacters.Count(c => !c.isDead && c.IsTerritory(character.territory) && c is Summon summon && summon.summonType == SUMMON_TYPE.Ghost);
        }
        return ghostCount;
    }
    private LocationGridTile GetTileToSpawnGhostRelativeTo(Character character) {
        LocationGridTile tile = null;
        if (character.homeSettlement != null) {
            tile = character.homeSettlement.GetRandomArea().GetRandomPassableTile();
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