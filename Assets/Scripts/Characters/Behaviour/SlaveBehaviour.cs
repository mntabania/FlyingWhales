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
        bool isInHome = character.IsAtHome();
        if (isInHome) {
            if (character.behaviourComponent.PlanSettlementOrFactionWorkActions(out producedJob)) {
                //Slaves can do work actions
                return true;
            }
        }
        if (character.movementComponent.isStationary) {
            return character.jobComponent.PlanIdleLongStandStill(out producedJob);
        } else {
            if (!isInHome) {
                return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
            }
            return character.jobComponent.TriggerRoamAroundTile(out producedJob);
        }
    }
}