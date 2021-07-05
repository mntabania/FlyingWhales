using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class DesiresIsolationBehaviour : CharacterBehaviourComponent {
    public DesiresIsolationBehaviour() {
        priority = 5;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
#if DEBUG_LOG
        log += $"{character.name} Desires isolation behaviour.";
#endif
        if (character.currentStructure == character.homeStructure) {
            int roll = Random.Range(0, 100);
            int chance = 25;
#if DEBUG_LOG
            log += $"\n{character.name} is at home structure.";
            log += $"\n{character.name} will roll for idle sit: {roll}. Chance is {chance}";
#endif
            if (roll < chance) {
                TileObject deskOrTable = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
#if DEBUG_LOG
                log += "\n-Sit if there is still an unoccupied Table or Desk in the current location";
#endif
                if (deskOrTable != null) {
#if DEBUG_LOG
                    log += $"\n  -{character.name} will do action Sit on {deskOrTable}";
#endif
                    character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable, out producedJob);
                    return true;
                }
#if DEBUG_LOG
                log += "\n-No available desk or table at location.";
#endif
            }
#if DEBUG_LOG
            log += "\n-Otherwise, stand idle";
            log += $"\n  -{character.name} will do action Stand";
#endif
            character.PlanIdle(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character);
            
        } else {
#if DEBUG_LOG
            log += $"{character.name} is not at home. Plan Hide At Home.";
#endif
            if (!(character.jobTriggerComponent as CharacterJobTriggerComponent).CreateHideAtHomeJob()) {
#if DEBUG_LOG
                log += $"{character.name} cannot hide at home because he does not have a home";
                log += $"{character.name} will roam to a tile outside settlement";
#endif
                LocationGridTile tileToGoTo = GetRandomTileOutsideSettlement(character.currentRegion, character) ?? character.gridTileLocation; //failsafe if character cannot find a tile to go to, stay in place.
                character.jobComponent.TriggerRoamAroundTile(out producedJob, tileToGoTo);
            }
        }
        return true;
    }

    private LocationGridTile GetRandomTileOutsideSettlement(Region p_region, Character p_character) {
        List<Area> areaChoices = p_region.areas.Where(x => x.elevationType == ELEVATION.PLAIN && x.settlementOnArea == null).ToList();
        if (areaChoices.Count > 0) {
            Area chosenArea = CollectionUtilities.GetRandomElement(areaChoices);
            return chosenArea.gridTileComponent.GetRandomTileThatCharacterCanReach(p_character);
        }
        return null;
    }
}