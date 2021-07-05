using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class NoxiousWandererBehaviour : CharacterBehaviourComponent {
    public NoxiousWandererBehaviour() {
        priority = 8;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
        log += $"\n-{character.name} is a noxious wanderer";
        log += $"\n-Character has 15% chance to spawn poison cloud";
        log += $"\n-Roll: " + roll;
#endif
        if (roll < 15) {
#if DEBUG_LOG
            log += $"\n-Character will spawn poison cloud";
#endif
            character.jobComponent.TriggerSpawnPoisonCloud(out producedJob);
        } else {
#if DEBUG_LOG
            log += $"\n-Character will roam";
#endif
            character.jobComponent.TriggerRoamAroundTile(out producedJob);
        }
        return true;
    }
}