using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class RevenantBehaviour : BaseMonsterBehaviour {
    public RevenantBehaviour() {
        priority = 8;
    }
    protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a revenant";
        if (character.gridTileLocation != null && (character.isAtHomeStructure || character.IsInHomeSettlement() || character.IsInTerritory())) {
            TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);
            if(currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT || currentTimeOfDay == TIME_IN_WORDS.LATE_NIGHT || currentTimeOfDay == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                log += $"\n-Early/Late Night or After Midnight, 9% chance to spawn a ghost";
                int roll = UnityEngine.Random.Range(0, 100);
                log += $"\n-Roll: " + roll;
                if(roll < 10) {
                    TrySpawnGhost(character, ref log);
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
            if(revenant.numOfSummonedGhosts < 5) {
                p_log = $"{p_log}\n-Will spawn ghost";
                Character betrayer = revenant.GetRandomBetrayer();
                Summon ghost = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Ghost, FactionManager.Instance.undeadFaction, homeLocation: revenant.homeSettlement, homeRegion: revenant.homeRegion);
                (ghost as Ghost).SetBetrayedBy(betrayer);
                CharacterManager.Instance.PlaceSummon(ghost, revenant.homeSettlement.GetRandomHexTile().GetRandomTile());
                revenant.AdjustNumOfSummonedGhosts(1);
                return true;
            } else {
                p_log = $"{p_log}\n-Already reached maximum number of spawned ghosts: 5";
                return false;
            }
        }
        return false;
    }
}