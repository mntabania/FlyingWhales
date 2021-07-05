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
        int chance = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
        log += $"\n-{character.name} is a Psychopath, 15% chance to Hunt Victim if there is one";
        log += $"\n  -RNG roll: {chance}";
#endif
        if (chance < 15) {
            Psychopath psychopath = character.traitContainer.GetTraitOrStatus<Psychopath>("Psychopath");
            psychopath.CheckTargetVictimIfStillAvailable();
            if (psychopath.targetVictim != null) {
#if DEBUG_LOG
                log += $"\n  -Target victim is {psychopath.targetVictim.name}, will try to Hunt Victim";
#endif
                if (psychopath.targetVictim.isAtHomeStructure || psychopath.targetVictim.IsInHomeSettlement()) {
                    if (psychopath.CreateHuntVictimJob(out producedJob)) {
#if DEBUG_LOG
                        log += "\n  -Created Hunt Victim Job";
#endif
                        return true;
                    } else {
#if DEBUG_LOG
                        log += "\n  -Cannot hunt victim, already has a Hunt Victim Job in queue";
#endif
                    }
                } else {
#if DEBUG_LOG
                    log += "\n  -Cannot hunt victim, target is not in his home structure/settlement";
#endif
                }
            } else {
#if DEBUG_LOG
                log += "\n  -No target victim";
#endif
            }
        }
        return false;
    }
}