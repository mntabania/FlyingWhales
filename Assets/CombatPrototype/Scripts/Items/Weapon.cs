using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Weapon : Item {
		[SerializeField] private WEAPON_TYPE _weaponType;
        //public MATERIAL material;
        //public QUALITY quality;
        [SerializeField] private float _weaponPower;
        [SerializeField] private WEAPON_PREFIX _prefix;
        [SerializeField] private WEAPON_SUFFIX _suffix;
        [SerializeField] private ELEMENT _element;
		//public float damageRange;
		//public List<IBodyPart.ATTRIBUTE> attributes;
        //public List<IBodyPart.ATTRIBUTE> equipRequirements;
		internal List<IBodyPart> bodyPartsAttached = new List<IBodyPart>();

        //public List<string> attackSkills = new List<string>();
        //public List<string> healSkills = new List<string>();

        //private List<Skill> _skills;

        #region getters/setters
        public WEAPON_TYPE weaponType {
            get { return _weaponType; }
            set { _weaponType = value; }
        }
        public ELEMENT element {
            get { return _element; }
            set { _element = value; }
        }
        public WEAPON_PREFIX prefixType {
            get { return _prefix; }
        }
        public WEAPON_SUFFIX suffixType {
            get { return _suffix; }
        }
        public float weaponPower {
            get { return _weaponPower; }
            set { _weaponPower = value; }
        }
        public float attackPower {
            get { return (_weaponPower + prefix.flatModifier) * (1f + (prefix.percentModifier / 100f)); }
        }
        public WeaponPrefix prefix {
            get { return ItemManager.Instance.weaponPrefixes[_prefix]; }
        }
        public WeaponSuffix suffix {
            get { return ItemManager.Instance.weaponSuffixes[_suffix]; }
        }
        #endregion

        public void SetPrefix(WEAPON_PREFIX prefix) {
            _prefix = prefix;
        }
        public void SetSuffix(WEAPON_SUFFIX suffix) {
            _suffix = suffix;
        }
        //		public Weapon(){
        //			ConstructAllSkillsList ();
        //		}

        //		public void AddSkill(Skill skillToAdd) {
        //			if(skillToAdd is AttackSkill){
        //				attackSkills.Add(skillToAdd.skillName);
        //			}else if(skillToAdd is HealSkill){
        //				healSkills.Add(skillToAdd.skillName);
        //			}
        ////			switch (skillToAdd) {
        ////			case SKILL_TYPE.ATTACK:
        ////				attackSkills.Add(skillToAdd.skillName);
        ////				break;
        ////			}
        //		}

        /*
         This is for the main game that uses the SkillManager to create skills.
             */
        //public void ConstructSkillsList() {
        //    _skills = new List<Skill>();
        //    if (SkillManager.Instance.weaponTypeSkills.ContainsKey(this.weaponType)) {
        //        List<Skill> weaponTypeSkills = SkillManager.Instance.weaponTypeSkills[weaponType];
        //        for (int i = 0; i < weaponTypeSkills.Count; i++) {
        //            Skill newSkill = weaponTypeSkills[i].CreateNewCopy();
        //            //newSkill.weapon = this;
        //            _skills.Add(newSkill);
        //        }
        //    }

        //    for (int i = 0; i < attackSkills.Count; i++) {
        //        string skillName = attackSkills[i];
        //        AttackSkill currSkill = (AttackSkill)SkillManager.Instance.CreateNewSkillInstance(skillName);
        //        //currSkill.weapon = this;
        //        _skills.Add(currSkill);
        //    }
        //    for (int i = 0; i < healSkills.Count; i++) {
        //        string skillName = healSkills[i];
        //        HealSkill currSkill = (HealSkill)SkillManager.Instance.CreateNewSkillInstance(skillName);
        //        //currSkill.weapon = this;
        //        _skills.Add(currSkill);
        //    }
        //}

        #region overrides
        public override Item CreateNewCopy() {
            Weapon copy = new Weapon();
            copy.weaponType = weaponType;
            //copy.material = material;
            //copy.quality = quality;
            copy.weaponPower = weaponPower;
            copy._prefix = _prefix;
            copy._suffix = _suffix;
            copy.element = element;
			//copy.damageRange = damageRange;
   //         copy.attributes = new List<IBodyPart.ATTRIBUTE>(attributes);
   //         copy.equipRequirements = new List<IBodyPart.ATTRIBUTE>(equipRequirements);
            copy.bodyPartsAttached = new List<IBodyPart>(bodyPartsAttached);

            //copy.attackSkills = new List<string>(attackSkills);
            //copy.healSkills = new List<string>(healSkills);
            SetCommonData(copy);
            //copy.ConstructSkillsList();
            return copy;
        }
        #endregion

   //     public void SetQuality(QUALITY quality) {
   //         this.quality = quality;
			//if(quality == QUALITY.CRUDE){
			//	this.weaponPower += ItemManager.Instance.crudeWeaponPowerModifier;
			//	this.durability += ItemManager.Instance.crudeWeaponDurabilityModifier;
			//}else if(quality == QUALITY.EXCEPTIONAL){
			//	this.weaponPower += ItemManager.Instance.exceptionalWeaponPowerModifier;
			//	this.durability += ItemManager.Instance.exceptionalWeaponDurabilityModifier;
			//}
			//this.currDurability = this.durability;
   //     }
    }
}
