#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ECS {
	[CustomEditor(typeof(ItemComponent))]
    public class ItemCreator : Editor {
		ItemComponent itemComponent;

        public override void OnInspectorGUI() {
			if(itemComponent == null){
				itemComponent = (ItemComponent)target;
			}
            GUILayout.Label("Item Creator ", EditorStyles.boldLabel);
			itemComponent.itemType = (ITEM_TYPE)EditorGUILayout.EnumPopup("Item Type: ", itemComponent.itemType);
			itemComponent.itemName = EditorGUILayout.TextField("Item Name: ", itemComponent.itemName);
			itemComponent.description = EditorGUILayout.TextField("Description: ", itemComponent.description);

			switch (itemComponent.itemType) {
			case ITEM_TYPE.WEAPON:
				ShowWeaponFields();
				break;
			case ITEM_TYPE.ARMOR:
				ShowArmorFields();
				break;
			}

            //itemComponent.bonusActRate = EditorGUILayout.IntField("Bonus Act Rate: ", itemComponent.bonusActRate);
            //itemComponent.bonusStrength = EditorGUILayout.IntField("Bonus Strength: ", itemComponent.bonusStrength);
            //itemComponent.bonusIntelligence = EditorGUILayout.IntField("Bonus Intelligence: ", itemComponent.bonusIntelligence);
            //itemComponent.bonusAgility = EditorGUILayout.IntField("Bonus Agility: ",itemComponent.bonusAgility);
            //         itemComponent.bonusVitality = EditorGUILayout.IntField("Bonus Vitality: ", itemComponent.bonusVitality);
            //         itemComponent.bonusMaxHP = EditorGUILayout.IntField("Bonus Max HP: ", itemComponent.bonusMaxHP);
            //itemComponent.bonusDodgeRate = EditorGUILayout.IntField("Bonus Dodge Rate: ", itemComponent.bonusDodgeRate);
            //itemComponent.bonusParryRate = EditorGUILayout.IntField("Bonus Parry Rate: ", itemComponent.bonusParryRate);
            //itemComponent.bonusBlockRate = EditorGUILayout.IntField("Bonus Block Rate: ", itemComponent.bonusBlockRate);
            //itemComponent.durability = EditorGUILayout.IntField("Durability :", itemComponent.durability);
            //itemComponent.cost = EditorGUILayout.IntField("Cost :", itemComponent.cost);
            //         itemComponent.exploreWeight = EditorGUILayout.IntField("Explore Weight :", itemComponent.exploreWeight);
            //         itemComponent.collectChance = EditorGUILayout.IntField("Collect Chance :", itemComponent.collectChance);
            itemComponent.isUnlimited = EditorGUILayout.Toggle("Is Unlimited :", itemComponent.isUnlimited);
            itemComponent.isObtainable = EditorGUILayout.Toggle("Is Obtainable :", itemComponent.isObtainable);
            itemComponent.interactString = EditorGUILayout.TextField("Interaction text: ", itemComponent.interactString);

			//SerializedProperty statusEffectResistance = serializedObject.FindProperty("statusEffectResistances");
			//EditorGUILayout.PropertyField(statusEffectResistance, true);
			//serializedObject.ApplyModifiedProperties ();

            if (GUILayout.Button("Save Item")) {
				SaveItem(itemComponent.itemName);
            }
        }

        private void ShowWeaponFields() {
			itemComponent.weaponType = (WEAPON_TYPE)EditorGUILayout.EnumPopup("Weapon Type: ", itemComponent.weaponType);
			//itemComponent.weaponMaterial = (MATERIAL)EditorGUILayout.EnumPopup("Material: ", itemComponent.weaponMaterial);
			//itemComponent.weaponQuality = (QUALITY)EditorGUILayout.EnumPopup("Quality: ", itemComponent.weaponQuality);
			itemComponent.weaponPower = EditorGUILayout.FloatField("Weapon Power: ", itemComponent.weaponPower);
            itemComponent.weaponPrefix = (WEAPON_PREFIX) EditorGUILayout.EnumPopup("Prefix: ", itemComponent.weaponPrefix);
            itemComponent.weaponSuffix = (WEAPON_SUFFIX) EditorGUILayout.EnumPopup("Suffix: ", itemComponent.weaponSuffix);
            itemComponent.element = (ELEMENT) EditorGUILayout.EnumPopup("Element: ", itemComponent.element);

            //SerializedProperty weaponAttribute = serializedObject.FindProperty("weaponAttributes");
            //EditorGUILayout.PropertyField(weaponAttribute, true);
            //serializedObject.ApplyModifiedProperties ();

            //         SerializedProperty equipRequirements = serializedObject.FindProperty("equipRequirements");
            //         EditorGUILayout.PropertyField(equipRequirements, true);
            //         serializedObject.ApplyModifiedProperties();

            //serializedObject.FindProperty("itemComponent");
            //itemComponent.skillsFoldout = EditorGUILayout.Foldout(itemComponent.skillsFoldout, "Skills");

            //if (itemComponent.skillsFoldout && itemComponent.skills != null) {
            //	EditorGUI.indentLevel++;
            //	for (int i = 0; i < itemComponent.skills.Count; i++) {
            //		SerializedProperty currSkill = serializedObject.FindProperty("_skills").GetArrayElementAtIndex(i);
            //		EditorGUILayout.PropertyField(currSkill, true);
            //	}
            //	serializedObject.ApplyModifiedProperties();
            //	EditorGUI.indentLevel--;
            //}

            //Add Skill Area
            //GUILayout.Space(10);
            //GUILayout.BeginVertical(EditorStyles.helpBox);
            //GUILayout.Label("Add Skills ", EditorStyles.boldLabel);
            //itemComponent.skillTypeToAdd = (SKILL_TYPE)EditorGUILayout.EnumPopup("Skill Type To Add: ", itemComponent.skillTypeToAdd);
            //List<string> choices = GetAllSkillsOfType(SKILL_CATEGORY.WEAPON, itemComponent.skillTypeToAdd);
            //itemComponent.skillToAddIndex = EditorGUILayout.Popup("Skill To Add: ", itemComponent.skillToAddIndex, choices.ToArray());
            //GUI.enabled = choices.Count > 0;
            //if (GUILayout.Button("Add Skill")) {
            //	AddSkillToList(choices[itemComponent.skillToAddIndex]);
            //}
            //GUI.enabled = true;
            //GUILayout.EndHorizontal();
        }

        private void ShowArmorFields() {
			itemComponent.armorType = (ARMOR_TYPE)EditorGUILayout.EnumPopup("Armor Type: ", itemComponent.armorType);
			itemComponent.armorBodyType = EditorGUILayout.TextField("Body Armor Type: ", itemComponent.armorBodyType);
            itemComponent.pDef = EditorGUILayout.IntField("PDef: ", itemComponent.pDef);
            itemComponent.mDef = EditorGUILayout.IntField("MDef: ", itemComponent.mDef);
            itemComponent.armorPrefix = (ARMOR_PREFIX) EditorGUILayout.EnumPopup("Prefix: ", itemComponent.armorPrefix);
            itemComponent.armorSuffix = (ARMOR_SUFFIX) EditorGUILayout.EnumPopup("Suffix: ", itemComponent.armorSuffix);

            //itemComponent.armorMaterial = (MATERIAL)EditorGUILayout.EnumPopup("Material: ", itemComponent.armorMaterial);
            //itemComponent.armorQuality = (QUALITY)EditorGUILayout.EnumPopup("Quality: ", itemComponent.armorQuality);
            //itemComponent.baseDamageMitigation = EditorGUILayout.FloatField("Base Damage Mitigation: ", itemComponent.baseDamageMitigation);
            //itemComponent.damageNullificationChance = EditorGUILayout.FloatField("Damage Nullification: ", itemComponent.damageNullificationChance);

   //         SerializedProperty ineffectiveAttackType = serializedObject.FindProperty("ineffectiveAttackTypes");
			//EditorGUILayout.PropertyField(ineffectiveAttackType, true);
			//serializedObject.ApplyModifiedProperties ();

			//SerializedProperty effectiveAttackType = serializedObject.FindProperty("effectiveAttackTypes");
			//EditorGUILayout.PropertyField(effectiveAttackType, true);
			//serializedObject.ApplyModifiedProperties ();

			//SerializedProperty armorAttribute = serializedObject.FindProperty("armorAttributes");
			//EditorGUILayout.PropertyField(armorAttribute, true);
			//serializedObject.ApplyModifiedProperties ();
        }
			
        #region Saving
        private void SaveItem(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                EditorUtility.DisplayDialog("Error", "Please specify an Item Name", "OK");
                return;
            }
			string path = Utilities.dataPath + "Items/" + itemComponent.itemType.ToString() + "/" + fileName + ".json";
            if (Utilities.DoesFileExist(path)) {
                if (EditorUtility.DisplayDialog("Overwrite Item", "An item with name " + fileName + " already exists. Replace with this item?", "Yes", "No")) {
                    File.Delete(path);
					SaveItemJson(path);
                }
            } else {
				SaveItemJson(path);
            }
        }
        private void SaveItemJson(string path) {
			if(itemComponent.itemType == ITEM_TYPE.WEAPON) {
				SaveWeapon(path);
			} else if (itemComponent.itemType == ITEM_TYPE.ARMOR) {
                SaveArmor(path);
			} else{
				Save (path);
			}

            //Re-import the file to update the reference in the editor
            UnityEditor.AssetDatabase.ImportAsset(path);
            Debug.Log("Successfully saved item at " + path);
        }
		private void SetCommonData(Item newItem) {
			newItem.itemType = itemComponent.itemType;
			newItem.itemName = itemComponent.itemName;
			newItem.description = itemComponent.description;
            //newItem.bonusActRate = itemComponent.bonusActRate;
            //newItem.bonusStrength = itemComponent.bonusStrength;
            //newItem.bonusIntelligence = itemComponent.bonusIntelligence;
            //newItem.bonusAgility = itemComponent.bonusAgility;
            //newItem.bonusMaxHP = itemComponent.bonusMaxHP;
            //newItem.bonusDodgeRate = itemComponent.bonusDodgeRate;
            //newItem.bonusParryRate = itemComponent.bonusParryRate;
            //newItem.bonusBlockRate = itemComponent.bonusBlockRate;
            //newItem.durability = itemComponent.durability;
            //newItem.cost = itemComponent.cost;
            //newItem.exploreWeight = itemComponent.exploreWeight;
            //newItem.collectChance = itemComponent.collectChance;
            newItem.isUnlimited = itemComponent.isUnlimited;
            newItem._isObtainable = itemComponent.isObtainable;
            newItem.interactString = itemComponent.interactString;
        }
        private void SaveWeapon(string path) {
			Weapon weapon = new Weapon();

			SetCommonData(weapon);

			weapon.weaponType = itemComponent.weaponType;
			//weapon.material = itemComponent.weaponMaterial;
			//weapon.quality = itemComponent.weaponQuality;
			weapon.weaponPower = itemComponent.weaponPower;
            weapon.element = itemComponent.element;
            weapon.SetPrefix(itemComponent.weaponPrefix);
            weapon.SetSuffix(itemComponent.weaponSuffix);
            //weapon.attributes = itemComponent.weaponAttributes;
            //weapon.equipRequirements = itemComponent.equipRequirements;
            //for (int i = 0; i < itemComponent.skills.Count; i++) {
            //	weapon.AddSkill (itemComponent.skills [i]);
            //}

            SaveJson(weapon, path);
        }
		private void SaveArmor(string path) {
			Armor armor = new Armor();

			SetCommonData(armor);

			armor.armorType = itemComponent.armorType;
			armor.armorBodyType = itemComponent.armorBodyType;
            armor.pDef = itemComponent.pDef;
            armor.mDef = itemComponent.mDef;
            armor.SetPrefix(itemComponent.armorPrefix);
            armor.SetSuffix(itemComponent.armorSuffix);
            //armor.material = itemComponent.armorMaterial;
            //armor.quality = itemComponent.armorQuality;
            //armor.baseDamageMitigation = itemComponent.baseDamageMitigation;
            //armor.damageNullificationChance = itemComponent.damageNullificationChance;
            //armor.ineffectiveAttackTypes = itemComponent.ineffectiveAttackTypes;
            //armor.effectiveAttackTypes = itemComponent.effectiveAttackTypes;
            //armor.attributes = itemComponent.armorAttributes;

            SaveJson(armor, path);
		}
		private void Save(string path){
			Item item = new Item ();
			SetCommonData (item);
			SaveJson(item, path);
		}
		private void SaveJson(Item item, string path) {
			string jsonString = JsonUtility.ToJson(item);

            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(jsonString);
            writer.Close();
        }
        #endregion

        #region Skills
        private List<string> GetAllSkillsOfType(SKILL_CATEGORY category, SKILL_TYPE skillType) {
            List<string> allSkillsOfType = new List<string>();
            string path = Utilities.dataPath + "Skills/" + category.ToString() + "/" + skillType.ToString() + "/";
            foreach (string file in Directory.GetFiles(path, "*.json")) {
                allSkillsOfType.Add(Path.GetFileNameWithoutExtension(file));
            }
            return allSkillsOfType;
        }
        //private void AddSkillToList(string skillName) {
        //	string path = Utilities.dataPath + "Skills/" + itemComponent.itemType.ToString() + "/" + itemComponent.skillTypeToAdd.ToString() + "/" + skillName + ".json";
        //	string dataAsJson = File.ReadAllText(path);
        //	switch (itemComponent.skillTypeToAdd) {
        //	case SKILL_TYPE.ATTACK:
        //		AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill>(dataAsJson);
        //		itemComponent.AddSkill(attackSkill);
        //		break;
        //	case SKILL_TYPE.HEAL:
        //		HealSkill healSkill = JsonUtility.FromJson<HealSkill>(dataAsJson);
        //		itemComponent.AddSkill(healSkill);
        //		break;
        //	case SKILL_TYPE.OBTAIN_ITEM:
        //		ObtainSkill obtainSkill = JsonUtility.FromJson<ObtainSkill>(dataAsJson);
        //		itemComponent.AddSkill(obtainSkill);
        //		break;
        //	case SKILL_TYPE.FLEE:
        //		FleeSkill fleeSkill = JsonUtility.FromJson<FleeSkill>(dataAsJson);
        //		itemComponent.AddSkill(fleeSkill);
        //		break;
        //	case SKILL_TYPE.MOVE:
        //		MoveSkill moveSkill = JsonUtility.FromJson<MoveSkill>(dataAsJson);
        //		itemComponent.AddSkill(moveSkill);
        //		break;
        //	}
        //}
        #endregion
    }
}
#endif