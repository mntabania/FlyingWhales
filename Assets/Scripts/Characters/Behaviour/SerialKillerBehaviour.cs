using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class SerialKillerBehaviour : CharacterBehaviourComponent {
    public SerialKillerBehaviour() {
        priority = 12;
        //attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a Serial Killer, 15% chance to Hunt Victim if there is one";
        int chance = UnityEngine.Random.Range(0, 100);
        log += $"\n  -RNG roll: {chance}";
        if (chance < 15) {
            Psychopath serialKiller = character.traitContainer.GetNormalTrait<Psychopath>("Psychopath");
            //serialKiller.CheckTargetVictimIfStillAvailable();
            if(serialKiller.targetVictim != null) {
                log += $"\n  -Target victim is {serialKiller.targetVictim.name}, will try to Hunt Victim";
                if (serialKiller.CreateHuntVictimJob(out producedJob)) {
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