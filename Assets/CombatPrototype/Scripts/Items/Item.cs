using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Item : EntityComponent {
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
        //      public int currDurability;
        //public int cost;
        //      public int exploreWeight;
        //      public int collectChance;
        public bool isUnlimited;
        public string interactString;
        public bool _isObtainable;
        //public List<StatusEffectRate> statusEffectResistances = new List<StatusEffectRate>();

        protected ECS.Character _owner; //Not included in CreateNewCopy
		protected TaskCreator _possessor; //Not included in CreateNewCopy

		private bool _isEquipped;
        

        public bool isEquipped{
			get { return _isEquipped; }
		}
		public ECS.Character owner{
			get { return _owner; }
		}
		public TaskCreator possessor{
			get { return _possessor; }
		}

        //public string nameWithQuality{
        //	get{
        //		if(itemType == ITEM_TYPE.ARMOR){
        //			return Utilities.NormalizeString (((Armor)this).quality.ToString ()) + " " + itemName;
        //		}else if(itemType == ITEM_TYPE.WEAPON){
        //			return Utilities.NormalizeString (((Weapon)this).quality.ToString ()) + " " + itemName;
        //		}
        //		return itemName;
        //	}
        //}
        public bool isObtainable {
            get { return (itemType != ITEM_TYPE.ITEM ? true : _isObtainable); }
        }

        public int id;
        public Item() {
            id = Utilities.SetID(this);
        }

    //    public void AdjustDurability(int adjustment) {
    //        currDurability += adjustment;
    //        currDurability = Mathf.Clamp(currDurability, 0, durability);
    //        if (currDurability == 0) {
    //            //Item Destroyed! Unequip item if armor or weapon
				//_owner.UnequipItem(this);
    //        }
    //    }
    //    public void ResetDurability() {
    //        currDurability = durability;
    //    }
        public void SetOwner(ECS.Character owner) {
            _owner = owner;
        }
		public void SetPossessor(TaskCreator possessor) {
			_possessor = possessor;
		}
		public void SetEquipped(bool state){
			this._isEquipped = state;
		}

        //public void SetExploreWeight(int weight) {
        //    exploreWeight = weight;
        //}
        //public void SetcollectChance(int weight) {
        //    collectChance = weight;
        //}
        public void SetIsUnlimited(bool state) {
            isUnlimited = state;
        }

        public void OnItemPlacedOnLandmark(BaseLandmark landmark) {
            Messenger.Broadcast(Signals.ITEM_PLACED_LANDMARK, this, landmark);
        }
        public void OnItemPutInInventory(ECS.Character character) {
            Messenger.Broadcast(Signals.ITEM_PLACED_INVENTORY, this, character);
        }

        #region virtuals
        public virtual Item CreateNewCopy() {
            Item copy = new Item();
            SetCommonData(copy);
            return copy;
        }
        #endregion

        protected void SetCommonData(Item item) {
            item.itemType = itemType;
            item.itemName = itemName;
            item.description = description;
            //         item.bonusActRate = bonusActRate;
            //         item.bonusStrength = bonusStrength;
            //         item.bonusIntelligence = bonusIntelligence;
            //         item.bonusAgility = bonusAgility;
            //         item.bonusVitality = bonusVitality;
            //         item.bonusMaxHP = bonusMaxHP;
            //         item.bonusDodgeRate = bonusDodgeRate;
            //         item.bonusParryRate = bonusParryRate;
            //         item.bonusBlockRate = bonusBlockRate;
            //         item.durability = durability;
            //         item.currDurability = currDurability;
            //item.cost = cost;
            //item.exploreWeight = exploreWeight;
            //item.collectChance = collectChance;
            item.isUnlimited = isUnlimited;
            item._isObtainable = isObtainable;
            item.interactString = interactString;
            //item.statusEffectResistances = new List<StatusEffectRate>(statusEffectResistances);
        }
    }
}

