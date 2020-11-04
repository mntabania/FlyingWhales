#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
	[CustomEditor(typeof(WeaponTypeComponent))]
	public class WeaponTypeCreator : Editor {

		WeaponTypeComponent weaponTypeComponent;

		public override void OnInspectorGUI() {
			if(weaponTypeComponent == null){
				weaponTypeComponent = (WeaponTypeComponent)target;
			}
			weaponTypeComponent.weaponType = (WEAPON_TYPE)EditorGUILayout.EnumPopup ("Weapon Type: ", weaponTypeComponent.weaponType);
			//weaponTypeComponent.powerModifier = EditorGUILayout.FloatField ("Power Modifier: ", weaponTypeComponent.powerModifier);
			weaponTypeComponent.damageRange = EditorGUILayout.FloatField ("Damage Range: ", weaponTypeComponent.damageRange);

			SerializedProperty equipRequirement = serializedObject.FindProperty("equipRequirements");
			EditorGUILayout.PropertyField(equipRequirement, true);
			serializedObject.ApplyModifiedProperties ();

//			SerializedProperty weaponMaterial = serializedObject.FindProperty("weaponMaterials");
//			EditorGUILayout.PropertyField(weaponMaterial, true);
//			serializedObject.ApplyModifiedProperties ();

			//weaponTypeComponent.skillsFoldout = EditorGUILayout.Foldout(weaponTypeComponent.skillsFoldout, "Skills");

			//if (weaponTypeComponent.skillsFoldout && weaponTypeComponent.skills != null) {
			//	EditorGUI.indentLevel++;
			//	for (int i = 0; i < weaponTypeComponent.skills.Count; i++) {
			//		SerializedProperty currSkill = serializedObject.FindProperty("skills").GetArrayElementAtIndex(i);
			//		EditorGUILayout.PropertyField(currSkill, true);
			//	}
			//	serializedObject.ApplyModifiedProperties();
			//	EditorGUI.indentLevel--;
			//}

			//Add Skill Area
			//GUILayout.Space(10);
			//GUILayout.BeginVertical(EditorStyles.helpBox);
			//GUILayout.Label("Add Skills ", EditorStyles.boldLabel);
			//weaponTypeComponent.skillTypeToAdd = (SKILL_TYPE)EditorGUILayout.EnumPopup("Skill Type To Add: ", weaponTypeComponent.skillTypeToAdd);
			//List<string> choices = GetAllSkillsOfType(SKILL_CATEGORY.WEAPON, weaponTypeComponent.skillTypeToAdd);
			//weaponTypeComponent.skillToAddIndex = EditorGUILayout.Popup("Skill To Add: ", weaponTypeComponent.skillToAddIndex, choices.ToArray());
			//GUI.enabled = choices.Count > 0;
			//if (GUILayout.Button("Add Skill")) {
			//	AddSkillToList(choices[weaponTypeComponent.skillToAddIndex]);
			//}
			//GUI.enabled = true;
			//GUILayout.EndHorizontal();

			if (GUILayout.Button("Save Weapon Type")) {
				SaveWeaponType(Utilities.NormalizeString(weaponTypeComponent.weaponType.ToString()));
			}
		}

		private void SaveWeaponType(string fileName) {
			if (string.IsNullOrEmpty(fileName)) {
				EditorUtility.DisplayDialog("Error", "Please specify a filename", "OK");
				return;
			}
			string path = Utilities.dataPath + "WeaponTypes/" + fileName + ".json";
			if (Utilities.DoesFileExist(path)) {
				if (EditorUtility.DisplayDialog("Overwrite File", "A file with name " + fileName + " already exists. Replace with this file?", "Yes", "No")) {
					File.Delete(path);
					Save(path);
				}
			} else {
				Save(path);
			}
		}
		private void Save(string path){
			WeaponType weaponType = new WeaponType ();
			weaponType.weaponType = weaponTypeComponent.weaponType;
			//weaponType.powerModifier = weaponTypeComponent.powerModifier;
			weaponType.damageRange = weaponTypeComponent.damageRange;
			weaponType.equipRequirements = weaponTypeComponent.equipRequirements;
//			weaponType.weaponMaterials = weaponTypeComponent.weaponMaterials;
			//for (int i = 0; i < weaponTypeComponent.skills.Count; i++) {
			//	weaponType.AddSkill (weaponTypeComponent.skills [i]);
			//}

			string jsonString = JsonUtility.ToJson(weaponType);

			System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
			writer.WriteLine(jsonString);
			writer.Close();
		}

		#region Skills
		//private List<string> GetAllSkillsOfType(SKILL_CATEGORY category, SKILL_TYPE skillType) {
		//	List<string> allSkillsOfType = new List<string>();
		//	string path = Utilities.dataPath + "Skills/" + category.ToString() + "/" + skillType.ToString() + "/";
		//	foreach (string file in Directory.GetFiles(path, "*.json")) {
		//		allSkillsOfType.Add(Path.GetFileNameWithoutExtension(file));
		//	}
		//	return allSkillsOfType;
		//}
		//private void AddSkillToList(string skillName) {
		//	string path = Utilities.dataPath + "Skills/WEAPON/" + weaponTypeComponent.skillTypeToAdd.ToString() + "/" + skillName + ".json";
		//	string dataAsJson = File.ReadAllText(path);
		//	switch (weaponTypeComponent.skillTypeToAdd) {
		//	case SKILL_TYPE.ATTACK:
		//		AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill>(dataAsJson);
		//		weaponTypeComponent.AddSkill(attackSkill);
		//		break;
		//	case SKILL_TYPE.HEAL:
		//		HealSkill healSkill = JsonUtility.FromJson<HealSkill>(dataAsJson);
		//		weaponTypeComponent.AddSkill(healSkill);
		//		break;
		//	case SKILL_TYPE.OBTAIN_ITEM:
		//		ObtainSkill obtainSkill = JsonUtility.FromJson<ObtainSkill>(dataAsJson);
		//		weaponTypeComponent.AddSkill(obtainSkill);
		//		break;
		//	case SKILL_TYPE.FLEE:
		//		FleeSkill fleeSkill = JsonUtility.FromJson<FleeSkill>(dataAsJson);
		//		weaponTypeComponent.AddSkill(fleeSkill);
		//		break;
		//	case SKILL_TYPE.MOVE:
		//		MoveSkill moveSkill = JsonUtility.FromJson<MoveSkill>(dataAsJson);
		//		weaponTypeComponent.AddSkill(moveSkill);
		//		break;
		//	}
		//}
		#endregion
	}
}
#endif