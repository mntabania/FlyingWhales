﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class SkillComponent : MonoBehaviour {
		public SKILL_TYPE skillType;
		public string skillName;
		public string description;
		public int activationWeight;
		public float accuracy;
		public int range;
        public float strengthPower;
        public float intellectPower;
        public float agilityPower;
        public SkillRequirement[] skillRequirements;
		public CHARACTER_ATTRIBUTES attributeModifier;
		public int levelRequirement;

		//Attack Skill Fields
		public ATTACK_TYPE attackType;
        public int durabilityDamage;
		public List<StatusEffectRate> statusEffectRates;

		//Heal Skill Fields
		public int healPower;

        //Shared Fields
        public int durabilityCost;
    }
}

