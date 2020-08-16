using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using System.Linq;
using UtilityScripts;

public class ZombieBehaviour : CharacterBehaviourComponent {
    public ZombieBehaviour() {
        priority = 900;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += "Character is zombie, will just stroll";
        return character.jobComponent.PlanZombieStrollOutside(out producedJob); //currentStructure
    }
}
