﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class GhostBehaviour : BaseMonsterBehaviour {
    public GhostBehaviour() {
        priority = 8;
    }
    protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a ghost";
        if (character.gridTileLocation != null) {
            if (character.HasTerritory()) {
                //Only get the first territory because right now even though the territory is a list, only one territory is being assigned at a time
                Area territory = character.territory;
                if (territory.HasAliveVillagerResident()) {
                    character.ClearTerritory();
                }
            }
            if (!character.HasTerritory()) {
                log += "\n-No territory, will set nearest hex tile as territory";
                Area area = character.areaLocation?.neighbourComponent.GetNearestPlainAreaWithNoResident();
                if(area != null) {
                    character.SetTerritory(area);
                }
            }
            TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);
            if(currentTimeOfDay == TIME_IN_WORDS.LATE_NIGHT || currentTimeOfDay == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                if (TryDoRevenge(character, ref log, out producedJob)) return true;
            }

            if ((character.HasTerritory() && !character.IsInTerritory()) || (character.homeStructure != null && !character.isAtHomeStructure)) {
                log += "\n-Return to territory";
                return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
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
            TIME_IN_WORDS currentTime = GameManager.GetCurrentTimeInWordsOfTick();
            p_log = $"{p_log}\n-Will check if can do Revenge, current time is {currentTime.ToString()}";
            if ((currentTime == TIME_IN_WORDS.LATE_NIGHT || currentTime == TIME_IN_WORDS.AFTER_MIDNIGHT) && GameUtilities.RollChance(5, ref p_log)) {
                if (TryDoRevenge(p_character, ref p_log, out p_producedJob)) {
                    return true;
                }
            }
            return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
        }
    }
    
    private bool TryDoRevenge(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
        if (p_character is Ghost ghost) {
            if (ghost.betrayedBy != null) {
                p_log = $"{p_log}\n-Will try to attack a source of betrayal if still alive and currently in the region";
                if (!ghost.betrayedBy.isDead && ghost.betrayedBy.currentRegion == ghost.currentRegion) {
                    if (ghost.jobComponent.CreateDemonKillJob(ghost.betrayedBy, out p_producedJob)) {
                        p_log = $"{p_log}\n-Will attack {ghost.betrayedBy.name}";
                        return true;
                    }
                }
                else {
                    p_log = $"{p_log}\n-Betrayer is either dead or not in current region";
                }
            }
        }
        p_producedJob = null;
        return false;
    }
}