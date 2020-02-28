using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesiresIsolationBehaviour : CharacterBehaviourComponent {
    public DesiresIsolationBehaviour() {
        priority = 5;
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"{character.name} Desires isolation behaviour.";
        if (character.currentStructure == character.homeStructure) {
            log += $"\n{character.name} is at home structure.";   
            int roll = Random.Range(0, 100);
            int chance = 25;
            log += $"\n{character.name} will roll for idle sit: {roll}. Chance is {chance}";
            if (roll < chance) {
                TileObject deskOrTable = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                log += "\n-Sit if there is still an unoccupied Table or Desk in the current location";
                if (deskOrTable != null) {
                    log += $"\n  -{character.name} will do action Sit on {deskOrTable}";
                    character.PlanIdle(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable);
                    return true;
                }
                log += "\n-No available desk or table at location.";
            } 
            
            log += "\n-Otherwise, stand idle";
            log += $"\n  -{character.name} will do action Stand";
            character.PlanIdle(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character);
            
        } else {
            log += $"{character.name} is not at home. Plan Hide At Home.";
            (character.jobTriggerComponent as CharacterJobTriggerComponent).CreateHideAtHomeJob();
        }
        return true;
    }
}