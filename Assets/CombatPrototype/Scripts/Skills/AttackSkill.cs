using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    public class AttackSkill : Skill {
        public int power;
        public int spCost;

        public ATTACK_CATEGORY attackCategory;
        public ELEMENT element;
    }
}