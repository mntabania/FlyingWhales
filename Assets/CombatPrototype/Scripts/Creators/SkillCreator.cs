#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
using System;

namespace ECS {
	[CustomEditor(typeof(SkillComponent))]
    public class SkillCreator : Editor {
		SkillComponent skillComponent;
        public override void OnInspectorGUI() {
			if(skillComponent == null){
				skillComponent = (SkillComponent)target;
			}

            GUILayout.Label("Skill Creator ", EditorStyles.boldLabel);
			skillComponent.skillType = (SKILL_TYPE)EditorGUILayout.EnumPopup("Skill Type: ", skillComponent.skillType);
			skillComponent.skillCategory = (SKILL_CATEGORY)EditorGUILayout.EnumPopup("Skill Category: ", skillComponent.skillCategory);
			skillComponent.skillName = EditorGUILayout.TextField("Skill Name: ", skillComponent.skillName);
			skillComponent.description = EditorGUILayout.TextField("Description: ", skillComponent.description);
			//skillComponent.actWeightType = (ACTIVATION_WEIGHT_TYPE)EditorGUILayout.EnumPopup("Activation Weight Type: ", skillComponent.actWeightType);
			skillComponent.activationWeight = EditorGUILayout.IntField("Activation Weight: ", skillComponent.activationWeight);
			skillComponent.range = EditorGUILayout.IntField("Range: ", skillComponent.range);
			//skillComponent.accuracy = EditorGUILayout.Slider("Accuracy: ", skillComponent.accuracy, 0f, 100f);

            switch (skillComponent.skillType) {
                case SKILL_TYPE.ATTACK:
                    ShowAttackSkillFields();
                    break;
                case SKILL_TYPE.HEAL:
                    ShowHealSkillFields();
                    break;
                case SKILL_TYPE.OBTAIN_ITEM:
                    ShowObtainItemFields();
                    break;
                case SKILL_TYPE.FLEE:
                    ShowFleeItemFields();
                    break;
                case SKILL_TYPE.MOVE:
                    ShowMoveItemFields();
                    break;
            }
            switch (skillComponent.skillCategory) {
                case SKILL_CATEGORY.BODY_PART:
                ShowBodyPartFields();
                break;
                case SKILL_CATEGORY.WEAPON:
                ShowWeaponFields();
                break;
            }

            if (GUILayout.Button("Save Skill")) {
				SaveSkill(skillComponent.skillName);
            }
        }

        private void ShowBodyPartFields() {
            SerializedProperty serializedProperty = serializedObject.FindProperty("skillRequirements");
            EditorGUILayout.PropertyField(serializedProperty, true);
            serializedObject.ApplyModifiedProperties();
        }

        private void ShowWeaponFields() {
            SerializedProperty serializedProperty = serializedObject.FindProperty("allowedWeaponTypes");
            EditorGUILayout.PropertyField(serializedProperty, true);
            serializedObject.ApplyModifiedProperties();
        }

        private void ShowAttackSkillFields() {
            skillComponent.power = EditorGUILayout.IntField("Power: ", skillComponent.power);
            skillComponent.spCost = EditorGUILayout.IntField("SP Cost: ", skillComponent.spCost);
            skillComponent.attackCategory = (ATTACK_CATEGORY) EditorGUILayout.EnumPopup("Attack Category: ", skillComponent.attackCategory);
            skillComponent.element = (ELEMENT) EditorGUILayout.EnumPopup("Element: ", skillComponent.element);

            //skillComponent.durabilityDamage = EditorGUILayout.IntField("Durability Damage: ", skillComponent.durabilityDamage);
            //skillComponent.attackType = (ATTACK_TYPE)EditorGUILayout.EnumPopup("Attack Type: ", skillComponent.attackType);
            //         skillComponent.durabilityDamage = EditorGUILayout.IntField("Durability Damage: ", skillComponent.durabilityDamage);
            //         skillComponent.durabilityCost = EditorGUILayout.IntField("Durability Cost: ", skillComponent.durabilityCost);

            //SerializedProperty serializedProperty = serializedObject.FindProperty("statusEffectRates");
            //EditorGUILayout.PropertyField(serializedProperty, true);
            //serializedObject.ApplyModifiedProperties();
        }
        private void ShowHealSkillFields() {
			skillComponent.healPower = EditorGUILayout.IntField("Heal Power: ",skillComponent. healPower);
            //skillComponent.durabilityCost = EditorGUILayout.IntField("Durability Cost: ", skillComponent.durabilityCost);
        }
        private void ShowObtainItemFields() {
            //Nothing yet
        }
        private void ShowFleeItemFields() {
            //Nothing yet
        }
        private void ShowMoveItemFields() {
            //Nothing yet
        }

        #region Saving
        private void SaveSkill(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                EditorUtility.DisplayDialog("Error", "Please specify a Skill Name", "OK");
                return;
            }
			string path = Utilities.dataPath + "Skills/" + skillComponent.skillCategory.ToString() + "/"+ skillComponent.skillType.ToString() + "/" + fileName + ".json";
            if (Utilities.DoesFileExist(path)) {
                if (EditorUtility.DisplayDialog("Overwrite Skill", "A skill with name " + fileName + " already exists. Replace with this skill?", "Yes", "No")) {
                    File.Delete(path);
                    SaveSkillJson(path);
                }
            } else {
                SaveSkillJson(path);
            }
        }
        private void SaveSkillJson(string path) {
			if(skillComponent.skillType == SKILL_TYPE.ATTACK) {
                SaveAttackSkill(path);
			} else if (skillComponent.skillType == SKILL_TYPE.HEAL) {
                SaveHealSkill(path);
			} else if (skillComponent.skillType == SKILL_TYPE.OBTAIN_ITEM) {
                SaveObtainSkill(path);
			} else if (skillComponent.skillType == SKILL_TYPE.MOVE) {
                SaveMoveSkill(path);
            } else {
                SaveFleeSkill(path);
            }

            //Re-import the file to update the reference in the editor
            UnityEditor.AssetDatabase.ImportAsset(path);
            Debug.Log("Successfully saved skill at " + path);
        }
        private void SetCommonData(Skill newSkill) {
			newSkill.skillType = skillComponent.skillType;
			newSkill.skillName = skillComponent.skillName;
			newSkill.skillCategory = skillComponent.skillCategory;
			newSkill.description = skillComponent.description;
			//newSkill.actWeightType = skillComponent.actWeightType;
			newSkill.activationWeight = skillComponent.activationWeight;
			//newSkill.accuracy = skillComponent.accuracy;
			newSkill.range = skillComponent.range;
			newSkill.skillRequirements = skillComponent.skillRequirements;
            newSkill.allowedWeaponTypes = skillComponent.allowedWeaponTypes;
        }
        private void SaveAttackSkill(string path) {
            AttackSkill newSkill = new AttackSkill();

            SetCommonData(newSkill);

            newSkill.power = skillComponent.power;
            newSkill.spCost = skillComponent.spCost;
            newSkill.attackCategory = skillComponent.attackCategory;
            newSkill.element = skillComponent.element;

            SaveJson(newSkill, path);
        }
        private void SaveHealSkill(string path) {
            HealSkill newSkill = new HealSkill();
            SetCommonData(newSkill);
			newSkill.healPower = skillComponent.healPower;
            //newSkill.durabilityCost = skillComponent.durabilityCost;
            SaveJson(newSkill, path);
        }
        private void SaveFleeSkill(string path) {
            FleeSkill newSkill = new FleeSkill();
            SetCommonData(newSkill);
            SaveJson(newSkill, path);
        }
        private void SaveObtainSkill(string path) {
            ObtainSkill newSkill = new ObtainSkill();
            SetCommonData(newSkill);
            SaveJson(newSkill, path);
        }
        private void SaveMoveSkill(string path) {
            MoveSkill newSkill = new MoveSkill();
            SetCommonData(newSkill);
            SaveJson(newSkill, path);
        }
        private void SaveJson(Skill skill, string path) {
            string jsonString = JsonUtility.ToJson(skill);

            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(jsonString);
            writer.Close();
        }
        #endregion
    }
}
#endif