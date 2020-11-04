//#if UNITY_EDITOR
//using UnityEditor;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.IO;
//using System.Linq;

//namespace ECS{
//	public class UpdateArmorData : EditorWindow{
//		[MenuItem ("Window/Update Armor Data")]
//		static void Init()
//		{
//			UpdateData ();
//		}

//		private static void UpdateData(){
//			string armorTypePath = Utilities.dataPath + "ArmorTypes/";
//			GameObject productionManagerGO = GameObject.Find("Production");
//			ProductionManager productionManager = productionManagerGO.GetComponent<ProductionManager> ();

//			foreach (string armorTypeFile in System.IO.Directory.GetFiles(armorTypePath, "*.json")) {
//				ArmorType armorType = JsonUtility.FromJson<ArmorType> (System.IO.File.ReadAllText (armorTypeFile));
//				if(productionManager.armorMaterials != null){
//					for (int i = 0; i < productionManager.armorMaterials.Count; i++) {
//						Armor armor = CreateArmor (productionManager.armorMaterials[i], armorType);
//						SaveArmor (armor);
//					}
//				}

//			}
//		}

//		private static Armor CreateArmor(MATERIAL materialType, ArmorType armorType){
//			string materialPath = Utilities.dataPath + "Materials/" + Utilities.NormalizeString(materialType.ToString()) + ".json";
//			Materials material = JsonUtility.FromJson<Materials> (System.IO.File.ReadAllText (materialPath));

//			Armor armor = new Armor ();
//			armor.armorType = armorType.armorType;
//			armor.armorBodyType = armorType.armorBodyType;
//			armor.material = materialType;
//			armor.quality = QUALITY.NORMAL;
//			armor.baseDamageMitigation = material.armorData.baseDamageMitigation;
//			armor.damageNullificationChance = material.armorData.damageNullificationChance;
//			armor.ineffectiveAttackTypes = new List<ATTACK_TYPE> (material.armorData.ineffectiveAttackTypes);
//			armor.effectiveAttackTypes = new List<ATTACK_TYPE> (material.armorData.effectiveAttackTypes);
//          armor.durability = material.armorData.durability;
//          armor.cost = material.armorData.cost;
//          armor.itemType = ITEM_TYPE.ARMOR;
//			armor.itemName = Utilities.NormalizeString (armor.material.ToString ()) + " " + Utilities.NormalizeString (armor.armorType.ToString ());
//			armor.description = armor.itemName;
//			return armor;
//		}

//		private static void SaveArmor(Armor armor){
//			string path = Utilities.dataPath + "Items/ARMOR/" + armor.itemName + ".json";
//			if (Utilities.DoesFileExist(path)) {
//				File.Delete(path);
//				Save(armor, path);
//			} else {
//				Save(armor, path);
//			}
//		}

//		private static void Save(Armor armor, string path){
//			string jsonString = JsonUtility.ToJson(armor);

//			System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
//			writer.WriteLine(jsonString);
//			writer.Close();
//		}
//	}
//}
//#endif