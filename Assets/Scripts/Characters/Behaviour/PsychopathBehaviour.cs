using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class PsychopathBehaviour : CharacterBehaviourComponent {
    public PsychopathBehaviour() {
        priority = 12;
        //attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a Psychopath, 15% chance to Hunt Victim if there is one";
        int chance = UnityEngine.Random.Range(0, 100);
        log += $"\n  -RNG roll: {chance}";
        if (chance < 15) {
            Psychopath psychopath = character.traitContainer.GetNormalTrait<Psychopath>("Psychopath");
            //psychopath.CheckTargetVictimIfStillAvailable();
            if (psychopath.targetVictim != null) {
                log += $"\n  -Target victim is {psychopath.targetVictim.name}, will try to Hunt Victim";
                if (psychopath.CreateHuntVictimJob(out producedJob)) {
                    log += "\n  -Created Hunt Victim Job";
                    return true;
                } else {
                    log += "\n  -Cannot hunt victim, already has a Hunt Victim Job in queue";
                }
            } else {
                log += "\n  -No target victim";
            }
        }
        return false;
    }
}