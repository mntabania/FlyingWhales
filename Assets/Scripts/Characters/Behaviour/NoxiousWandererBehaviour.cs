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
        log += $"\n-{character.name} is a noxious wanderer";
        log += $"\n-Character has 15% chance to spawn poison cloud";
        int roll = UnityEngine.Random.Range(0, 100);
        log += $"\n-Roll: " + roll;
        if (roll < 15) {
            log += $"\n-Character will spawn poison cloud";
            character.jobComponent.TriggerSpawnPoisonCloud(out producedJob);
        } else {
            log += $"\n-Character will roam";
            character.jobComponent.TriggerRoamAroundTile(out producedJob);
        }
        return true;
    }
}