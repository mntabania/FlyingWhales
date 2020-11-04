using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class ItemComponent : MonoBehaviour {
		public ITEM_TYPE itemType;
		public string itemName;
		public string description;
        //public int bonusActRate;
        //public int bonusStrength;
        //public int bonusIntelligence;
        //public int bonusAgility;
        //      public int bonusVitality;
        //      public int bonusMaxHP;
        //public int bonusDodgeRate;
        //public int bonusParryRate;
        //public int bonusBlockRate;
        //public int durability;
        //public int cost;
        //      public int exploreWeight;
        //      public int collectChance;
        public bool isUnlimited;
        public bool isObtainable;
        public string interactString;
        //public List<StatusEffectRate> statusEffectResistances = new List<StatusEffectRate>();

        //Weapon Fields
        public float weaponPower;
        public WEAPON_TYPE weaponType;
        public WEAPON_PREFIX weaponPrefix;
        public WEAPON_SUFFIX weaponSuffix;
        public ELEMENT element;
        //public MATERIAL weaponMaterial;
        //public QUALITY weaponQuality;

		//public List<IBodyPart.ATTRIBUTE> equipRequirements = new List<IBodyPart.ATTRIBUTE>();
		//public List<IBodyPart.ATTRIBUTE> weaponAttributes = new List<IBodyPart.ATTRIBUTE>();
		//public List<Skill> _skills = new List<Skill> ();

		//internal bool skillsFoldout;
		//internal SKILL_TYPE skillTypeToAdd;
		//internal int skillToAddIndex;

		//#region getters/setters
		//public List<Skill> skills {
		//	get { return _skills; }
		//}
		//#endregion

//		public void AddSkill(Skill skillToAdd) {
////			switch (skillType) {
////			case SKILL_TYPE.ATTACK:
////				attackSkills.Add (skillToAdd.skillName);
////				break;
////			}
////			if(this._skills == null){
////				this._skills = new List<Skill> ();
////			}
//			if(this._skills == null){
//				this._skills = new List<Skill> ();	
//			}
//			this._skills.Add (skillToAdd);
//		}


		//Armor Fields
		public ARMOR_TYPE armorType;
		public string armorBodyType;
        public int pDef;
        public int mDef;
        public ARMOR_PREFIX armorPrefix;
        public ARMOR_SUFFIX armorSuffix;
        //public MATERIAL armorMaterial;
        //public QUALITY armorQuality;
        //public float baseDamageMitigation;
        //public float damageNullificationChance;
        //public List<ATTACK_TYPE> ineffectiveAttackTypes;
        //public List<ATTACK_TYPE> effectiveAttackTypes;
        //public List<IBodyPart.ATTRIBUTE> armorAttributes = new List<IBodyPart.ATTRIBUTE>();
    }

}
