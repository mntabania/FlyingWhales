using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentDataHandler : MonoBehaviour
{
    public static EquipmentDataHandler Instance = null;

	public List<EquipmentData> allEquipmentsData = new List<EquipmentData>();

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

	public EquipmentData GetEquipmentDataBaseOnName(string p_name) { 
		for(int x = 0; x < allEquipmentsData.Count; ++x) {
			if(string.Equals(p_name.Replace(" ", "").ToLower(), allEquipmentsData[x].name.Replace(" ", "").ToLower())) {
				return allEquipmentsData[x];
			}
		}
		return null;
	}
}
