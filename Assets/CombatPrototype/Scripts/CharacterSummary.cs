using UnityEngine;
using System.Collections;

namespace ECS{
	public class CharacterSummary : MonoBehaviour {
        //public CharacterSummary Instance;

        public GameObject basicSummaryGO;
        public GameObject itemsSummaryGO;

		public UILabel basicInfoLbl;
		public UILabel classInfoLbl;
		public UILabel bodyPartsInfoLbl;

        public UILabel itemsInfoLbl;

		//void Awake(){
		//	Instance = this;
		//}

        public void ShowBasicSummary() {
            basicSummaryGO.SetActive(true);
            itemsSummaryGO.SetActive(false);
            UpdateCharacterSummary(CombatPrototypeUI.Instance.currSelectedCharacter);
        }

        public void ShowItemSummary() {
            basicSummaryGO.SetActive(false);
            itemsSummaryGO.SetActive(true);
            UpdateItemSummary(CombatPrototypeUI.Instance.currSelectedCharacter);
        }

        public void UpdateCharacterSummary(ECS.Character character){
            if (character == null) {
                return;
            }
            basicInfoLbl.text = string.Empty;
            basicInfoLbl.text += "[b]Name:[/b] " + character.name + "\n";
            basicInfoLbl.text += "[b]Class:[/b] " + character.characterClass.className + "\n";
            basicInfoLbl.text += "[b]Race:[/b] " + character.raceSetting.race.ToString() + "\n";
            basicInfoLbl.text += "[b]HP:[/b] " + character.currentHP.ToString() + "/" + character.maxHP.ToString() + "\n";
            basicInfoLbl.text += "[b]Strength:[/b] " + character.strength.ToString() + "\n";
            basicInfoLbl.text += "[b]Intelligence:[/b] " + character.intelligence.ToString() + "\n";
            basicInfoLbl.text += "[b]Agility:[/b] " + character.agility.ToString() + "\n";
            basicInfoLbl.text += "[b]Row:[/b] " + character.currentRow.ToString() + "\n";
            basicInfoLbl.text += "[b]Is Dead?:[/b] " + character.isDead.ToString() + "\n";

            classInfoLbl.text = string.Empty;
            classInfoLbl.text += "[b]Class:[/b] " + character.characterClass.className + "\n";
            classInfoLbl.text += "[b]Skills:[/b] ";
            for (int i = 0; i < character.skills.Count; i++) {
                Skill currSkill = character.skills[i];
                classInfoLbl.text += currSkill.skillName.ToString();
                if(i + 1 < character.skills.Count) {
                    classInfoLbl.text += ", ";
                }
            }
            //classInfoLbl.text += "\n";
            classInfoLbl.text += "[b]Strength %:[/b] " + character.characterClass.strWeightAllocation.ToString() + "\n";
            classInfoLbl.text += "[b]Intelligence %:[/b] " + character.characterClass.intWeightAllocation.ToString() + "\n";
            classInfoLbl.text += "[b]Agility %:[/b] " + character.characterClass.agiWeightAllocation.ToString() + "\n";
            classInfoLbl.text += "[b]Vitality %:[/b] " + character.characterClass.vitWeightAllocation.ToString() + "\n";
            //         classInfoLbl.text += "[b]Dodge Rate:[/b] " + character.characterClass.dodgeRate.ToString() + "\n";
            //         classInfoLbl.text += "[b]Parry Rate:[/b] " + character.characterClass.parryRate.ToString() + "\n";
            //         classInfoLbl.text += "[b]Block Rate:[/b] " + character.characterClass.blockRate.ToString();

            bodyPartsInfoLbl.text = string.Empty;
            bodyPartsInfoLbl.text += "[b]Body Parts:[/b]";
            for (int i = 0; i < character.bodyParts.Count; i++) {
                BodyPart currBodyPart = character.bodyParts[i];
                bodyPartsInfoLbl.text += "\n";
				bodyPartsInfoLbl.text += Utilities.NormalizeString(currBodyPart.name);
				if (currBodyPart.attributes.Count > 0) {
					bodyPartsInfoLbl.text += "[";
					for (int j = 0; j < currBodyPart.attributes.Count; j++) {
						BodyAttribute currAttribute = currBodyPart.attributes [j];
						bodyPartsInfoLbl.text += Utilities.NormalizeString (currAttribute.attribute.ToString ()) + "(" + currAttribute.isUsed.ToString () + ")";
					}
					bodyPartsInfoLbl.text += "]";
				}

				if(currBodyPart.secondaryBodyParts.Count > 0){
					bodyPartsInfoLbl.text += "\n    -";
					for (int j = 0; j < currBodyPart.secondaryBodyParts.Count; j++) {
						SecondaryBodyPart otherBodyPart = currBodyPart.secondaryBodyParts[j];
						if(j != 0){
							bodyPartsInfoLbl.text += ", ";
						}
						bodyPartsInfoLbl.text += "{";
						bodyPartsInfoLbl.text += Utilities.NormalizeString(otherBodyPart.name);
						if( otherBodyPart.attributes.Count > 0){
							bodyPartsInfoLbl.text += "[";
							for (int k = 0; k < otherBodyPart.attributes.Count; k++) {
								BodyAttribute currAttribute = otherBodyPart.attributes[k];
								bodyPartsInfoLbl.text += Utilities.NormalizeString(currAttribute.attribute.ToString()) + "(" + currAttribute.isUsed.ToString() + ")";
							}
							bodyPartsInfoLbl.text += "]";
						}
						bodyPartsInfoLbl.text += "}";
					}
				}
            }
            UpdateItemSummary(CombatPrototypeUI.Instance.currSelectedCharacter);
        }

        public void UpdateItemSummary(ECS.Character character) {
            if(character == null) {
                return;
            }
            itemsInfoLbl.text = "[b]ITEMS[/b]";
            for (int i = 0; i < character.equippedItems.Count; i++) {
				Item currItem = character.equippedItems[i];
				itemsInfoLbl.text += "\n[b]" + "[url= " + i.ToString() + "]" + currItem.itemName + "[/url]" + "[/b] ";
				itemsInfoLbl.text += " (";
				//itemsInfoLbl.text += "Durability: " + currItem.currDurability.ToString() + "/" + currItem.durability.ToString();
                if(currItem.itemType == ITEM_TYPE.ARMOR) {
                    Armor armor = (Armor)currItem;
                    itemsInfoLbl.text += ", Body part: " + armor.bodyPartAttached.name;
                    //for (int j = 0; j < armor.attributes.Count; j++) {
                    //    itemsInfoLbl.text += armor.attributes[j].ToString();
                    //    if(j + 1 < armor.attributes.Count) {
                    //        itemsInfoLbl.text += ", ";
                    //    }
                    //}
                } else if(currItem.itemType == ITEM_TYPE.WEAPON) {
                    Weapon weapon = (Weapon)currItem;
					itemsInfoLbl.text += ", Body part:";
					for (int j = 0; j < weapon.bodyPartsAttached.Count; j++) {
						itemsInfoLbl.text += " " + weapon.bodyPartsAttached[j].name;
					}
                    itemsInfoLbl.text += ", Weapon Power: " + weapon.weaponPower.ToString();
                    //for (int j = 0; j < weapon.attributes.Count; j++) {
                    //    itemsInfoLbl.text += weapon.attributes[j].ToString();
                    //    if (j + 1 < weapon.attributes.Count) {
                    //        itemsInfoLbl.text += ", ";
                    //    }
                    //}
                }
                    
            }
        }

//		private void BasicInfo(ECS.Character character){
//			summaryLbl 
//		}

	}
}

