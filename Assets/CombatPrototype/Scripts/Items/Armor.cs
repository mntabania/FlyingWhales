using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Armor : Item {
        [SerializeField] private ARMOR_TYPE _armorType;
        [SerializeField] private string _armorBodyType;
        [SerializeField] private int _pDef;
        [SerializeField] private int _mDef;
        [SerializeField] private ARMOR_PREFIX _prefix;
        [SerializeField] private ARMOR_SUFFIX _suffix;
        //public MATERIAL material;
        //public QUALITY quality;
        //public float baseDamageMitigation;
        //public float damageNullificationChance;
        //public List<ATTACK_TYPE> ineffectiveAttackTypes;
        //public List<ATTACK_TYPE> effectiveAttackTypes;
        //public List<IBodyPart.ATTRIBUTE> attributes;
        internal IBodyPart bodyPartAttached;

        #region getters/setters
        public ARMOR_TYPE armorType {
            get { return _armorType; }
            set { _armorType = value; }
        }
        public ARMOR_PREFIX prefixType {
            get { return _prefix; }
        }
        public ARMOR_SUFFIX suffixType {
            get { return _suffix; }
        }
        public string armorBodyType {
            get { return _armorBodyType; }
            set { _armorBodyType = value; }
        }
        public int pDef {
            get { return _pDef; }
            set { _pDef = value; }
        }
        public int mDef {
            get { return _mDef; }
            set { _mDef = value; }
        }
        public ArmorPrefix prefix {
            get { return ItemManager.Instance.armorPrefixes[_prefix]; }
        }
        public ArmorSuffix suffix {
            get { return ItemManager.Instance.armorSuffixes[_suffix]; }
        }
        #endregion

        #region overrides
        public override Item CreateNewCopy() {
            Armor copy = new Armor();
            copy.armorType = armorType;
            copy.armorBodyType = armorBodyType;
            copy.pDef = pDef;
            copy.mDef = mDef;
            copy._prefix = _prefix;
            copy._suffix = _suffix;
            //copy.material = material;
            //copy.quality = quality;
            //copy.baseDamageMitigation = baseDamageMitigation;
            //copy.damageNullificationChance = damageNullificationChance;
            
            //copy.ineffectiveAttackTypes = new List<ATTACK_TYPE>(ineffectiveAttackTypes);
            //copy.effectiveAttackTypes = new List<ATTACK_TYPE>(effectiveAttackTypes);
            //copy.attributes = new List<IBodyPart.ATTRIBUTE>(attributes);
            copy.bodyPartAttached = null;
            SetCommonData(copy);
            return copy;
        }
        #endregion

        public void SetPrefix(ARMOR_PREFIX prefix) {
            _prefix = prefix;
        }
        public void SetSuffix(ARMOR_SUFFIX suffix) {
            _suffix = suffix;
        }

        //     public void SetQuality(QUALITY quality) {
        //         this.quality = quality;
        //if(quality == QUALITY.CRUDE){
        //	this.baseDamageMitigation += ItemManager.Instance.crudeArmorMitigationModifier;
        //	this.durability += ItemManager.Instance.crudeArmorDurabilityModifier;
        //}else if(quality == QUALITY.EXCEPTIONAL){
        //	this.baseDamageMitigation += ItemManager.Instance.exceptionalArmorMitigationModifier;
        //	this.durability += ItemManager.Instance.exceptionalArmorDurabilityModifier;
        //}
        //this.currDurability = this.durability;
        //     }
    }
}
