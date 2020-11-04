using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
	public class WeaponType {
		public WEAPON_TYPE weaponType;
		public float damageRange;
		public List<IBodyPart.ATTRIBUTE> equipRequirements;
		//public List<Skill> skills;
		//public List<string> attackSkills = new List<string>();
		//public List<string> healSkills = new List<string>();

		//public void AddSkill(Skill skillToAdd) {
		//	if(skillToAdd.skillType == SKILL_TYPE.ATTACK){
		//		attackSkills.Add(skillToAdd.skillName);
		//	}else if(skillToAdd.skillType == SKILL_TYPE.HEAL){
		//		healSkills.Add(skillToAdd.skillName);
		//	}
		//}

        /*
         This is for the main game that uses the SkillManager to create skills.
             */
  //      public void ConstructSkillList() {
  //          for (int i = 0; i < attackSkills.Count; i++) {
  //              string skillName = attackSkills[i];
  //              skills.Add(SkillManager.Instance.CreateNewSkillInstance(skillName));
  //          }
  //          for (int i = 0; i < healSkills.Count; i++) {
  //              string skillName = healSkills[i];
  //              skills.Add(SkillManager.Instance.CreateNewSkillInstance(skillName));
  //          }
  //      }

  //      public void ConstructWeaponSkillsList() {
		//	skills = new List<Skill>();
		//	for (int i = 0; i < attackSkills.Count; i++) {
		//		string skillName = attackSkills[i];
		//		AttackSkill currSkill = SkillManager.Instance.allSkills[skillName] as AttackSkill;
		//		skills.Add(currSkill);
		//	}
		//	for (int i = 0; i < healSkills.Count; i++) {
		//		string skillName = healSkills[i];
		//		HealSkill currSkill = SkillManager.Instance.allSkills[skillName] as HealSkill;
  //              skills.Add(currSkill);
		//	}
		//}
	}
}
