﻿using UnityEngine;
using System.Collections;
using System;

namespace ECS {
    [System.Serializable]
    public class Skill {
        public string skillName;
        public int activationWeight;
        public float accuracy;
        public int range;
		public bool isEnabled;
        public float strengthPower;
        public float intellectPower;
        public float agilityPower;
        public SkillRequirement[] skillRequirements;
        public CHARACTER_ATTRIBUTES attributeModifier;
    }
}