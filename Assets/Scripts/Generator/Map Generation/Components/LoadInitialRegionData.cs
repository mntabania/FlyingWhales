using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class LoadInitialRegionData : MapGenerationComponent {
    public override void LoadSavedData(object state) {
        try {
			LoadThreadQueueItem threadItem = state as LoadThreadQueueItem;
			MapGenerationData mapData = threadItem.mapData;
			SaveDataCurrentProgress saveData = threadItem.saveData;
			//Region[] allRegions = new Region[saveData.worldMapSave.regionSaves.Count];

			//for (int i = 0; i < saveData.worldMapSave.regionSaves.Count; i++) {
			//	SaveDataRegion saveDataRegion = saveData.worldMapSave.regionSaves[i];
			//	Region region = CreateNewRegionFromSave(saveDataRegion);
			//	allRegions[i] = region;
			//}
			Region region = new Region(saveData.worldMapSave.regionSave);

			DatabaseManager.Instance.regionDatabase.RegisterRegion(region);

			threadItem.isDone = true;
		} catch (Exception e) {
            Debug.LogError(e.Message + "\n" + e.StackTrace);
        }
    }

	//private Region CreateNewRegionFromSave(SaveDataRegion saveDataRegion) {
	//	Region region = new Region(saveDataRegion);
	//	for (int x = startingX; x < maxX; x++) {
	//		for (int y = startingY; y < maxY; y++) {
	//			Area tile = GridMap.Instance.map[x, y];
	//			region.AddTile(tile);
	//		}
	//	}

	//	return region;
	//}
}
