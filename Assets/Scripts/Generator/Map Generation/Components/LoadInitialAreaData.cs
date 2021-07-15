using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadInitialAreaData : MapGenerationComponent {
    public override void LoadSavedData(object state) {
        try {
			LoadThreadQueueItem threadItem = state as LoadThreadQueueItem;
			MapGenerationData mapData = threadItem.mapData;
			SaveDataCurrentProgress saveData = threadItem.saveData;

			SaveDataArea[,] savedMap = saveData.worldMapSave.GetSaveDataMap();
			Area[,] map = new Area[mapData.width, mapData.height];

			for (int x = 0; x < mapData.width; x++) {
				for (int y = 0; y < mapData.height; y++) {
					SaveDataArea savedHexTile = savedMap[x, y];

					Area area = savedHexTile.Load();
					DatabaseManager.Instance.areaDatabase.RegisterArea(area);
				}
			}
			for (int i = 0; i < DatabaseManager.Instance.areaDatabase.allAreas.Count; i++) {
				Area area = DatabaseManager.Instance.areaDatabase.allAreas[i];
				area.neighbourComponent.FindNeighbours(area, map);
			}
			GridMap.Instance.SetMap(map);
			threadItem.isDone = true;
		} catch (Exception e) {
            Debug.LogError(e.Message + "\n" + e.StackTrace);
        }
    }
}
