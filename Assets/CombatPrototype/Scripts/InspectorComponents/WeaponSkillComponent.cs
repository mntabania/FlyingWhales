﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class WeaponSkillComponent : MonoBehaviour {
		public WEAPON_TYPE weaponType;
		public List<Skill> skills;
		public List<IBodyPart.ATTRIBUTE> equipRequirements;

		internal bool skillsFoldout;
		internal SKILL_TYPE skillTypeToAdd;
		internal int skillToAddIndex;

		public void AddSkill(Skill skillToAdd) {
			if(this.skills == null){
				this.skills = new List<Skill> ();	
			}
			this.skills.Add (skillToAdd);
		}
    }
}

