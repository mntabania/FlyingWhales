using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Pathfinding;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Traits;
public class SlaveBehaviour : CharacterBehaviourComponent {

    public SlaveBehaviour() {
        priority = 9;
    }

    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        bool isInHome = character.IsInHomeSettlement() || character.isAtHomeStructure || character.IsInTerritory();
        if (isInHome) {
            if (character.behaviourComponent.PlanWorkActions(out producedJob)) {
                //Ratmen can do work actions
                return true;
            }
        }
        if (!isInHome) {
            return character.jobComponent.TriggerReturnTerritory(out producedJob);
        }
        return character.jobComponent.TriggerRoamAroundTile(out producedJob);
    }
}