using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class EquipmentDataHandler : MonoBehaviour
{
    public static EquipmentDataHandler Instance = null;

	public List<EquipmentData> allEquipmentsData = new List<EquipmentData>();

	private Dictionary<string, EquipmentData> _equipmentDataDictionary = new Dictionary<string, EquipmentData>();
	
	private void OnEnable() {
		if (Instance == null) {
			Instance = this;
		}
	}

	private void OnDisable() {
		if(Instance == this) {
			Instance = null;
		}
	}
	private void Awake() {
		for (int i = 0; i < allEquipmentsData.Count; i++) {
			EquipmentData equipmentData = allEquipmentsData[i];
			_equipmentDataDictionary.Add(equipmentData.name, equipmentData);
		}
	}
	public EquipmentData GetEquipmentDataBaseOnName(string p_name) {
		if (_equipmentDataDictionary.ContainsKey(p_name)) {
			return _equipmentDataDictionary[p_name];
		}
		
		
		// for(int x = 0; x < allEquipmentsData.Count; ++x) {
		// 	EquipmentData equipmentData = allEquipmentsData[x];
		// 	if(string.Equals(p_name.Replace(" ", "").ToLower(), equipmentData.name.Replace(" ", "").ToLower())) {
		// 		return equipmentData;
		// 	}
		// }
		return null;
	}

	public List<CONCRETE_RESOURCES> GetResourcesNeeded(TILE_OBJECT_TYPE p_equipment) {
		string equipmentName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(p_equipment.ToString());
		EquipmentData equipmentData = GetEquipmentDataBaseOnName(equipmentName);
		Assert.IsNotNull(equipmentData, $"Could not find equipment data for {p_equipment}");
		return equipmentData.specificResource;
	}

	public RESOURCE GetGeneralResourcesNeeded(TILE_OBJECT_TYPE p_equipment) {
		string equipmentName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(p_equipment.ToString());
		EquipmentData equipmentData = GetEquipmentDataBaseOnName(equipmentName);
		Assert.IsNotNull(equipmentData, $"Could not find equipment data for {p_equipment}");
		return equipmentData.resourceType;
	}

	public int GetResourcesNeededAmount(TILE_OBJECT_TYPE p_equipment) {
		string equipmentName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(p_equipment.ToString());
		EquipmentData equipmentData = GetEquipmentDataBaseOnName(equipmentName);
		Assert.IsNotNull(equipmentData, $"Could not find equipment data for {p_equipment}");
		return equipmentData.resourceAmount;
	}
}
