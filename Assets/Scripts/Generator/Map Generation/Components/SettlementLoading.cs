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
    }
}