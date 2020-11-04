using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class WeaponTypeComponent : MonoBehaviour {
		public WEAPON_TYPE weaponType;
		//public float powerModifier;
		public float damageRange;
		//public List<Skill> skills;
		public List<IBodyPart.ATTRIBUTE> equipRequirements;
//		public List<MATERIAL> weaponMaterials;

		//internal bool skillsFoldout;
		//internal SKILL_TYPE skillTypeToAdd;
		//internal int skillToAddIndex;

		//public void AddSkill(Skill skillToAdd) {
		//	if(this.skills == null){
		//		this.skills = new List<Skill> ();	
		//	}
		//	this.skills.Add (skillToAdd);
		//}
    }
}

