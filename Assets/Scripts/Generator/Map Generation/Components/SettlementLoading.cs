using System;
using System.Collections;
using Locations.Settlements;
using UnityEngine;
namespace Generator.Map_Generation.Components {
    public class SettlementLoading : MapGenerationComponent {
        public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
            LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Settlements...");
            AddLog($"Loading settlements");
            for (int i = 0; i < saveData.worldMapSave.settlementSaves.Count; i++) {
                SaveDataBaseSettlement saveDataBaseSettlement = saveData.worldMapSave.settlementSaves[i];
                BaseSettlement settlement = saveDataBaseSettlement.Load();
                AddLog($" - Loaded settlement {settlement.name} with {settlement.areas.Count} tiles.");
                yield return null;
            }
        }
		public override void LoadSavedData(object state) {
            try {
			    LoadThreadQueueItem threadItem = state as LoadThreadQueueItem;
			    MapGenerationData mapData = threadItem.mapData;
			    SaveDataCurrentProgress saveData = threadItem.saveData;
                AddLog($"Loading settlements");
                for (int i = 0; i < saveData.worldMapSave.settlementSaves.Count; i++) {
                    SaveDataBaseSettlement saveDataBaseSettlement = saveData.worldMapSave.settlementSaves[i];
                    BaseSettlement settlement = saveDataBaseSettlement.Load();
                    AddLog($" - Loaded settlement {settlement.name} with {settlement.areas.Count} tiles.");
                }
                threadItem.isDone = true;
            } catch (Exception e) {
			    Debug.LogError(e.Message + "\n" + e.StackTrace);
            }
        }
	}
}