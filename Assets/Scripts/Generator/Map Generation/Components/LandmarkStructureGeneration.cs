﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cellular_Automata;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class LandmarkStructureGeneration : MapGenerationComponent {
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Creating structures...");
		List<BaseLandmark> landmarks = LandmarkManager.Instance.GetAllLandmarks();
		for (int i = 0; i < landmarks.Count; i++) {
			BaseLandmark landmark = landmarks[i];
			if (landmark.specificLandmarkType == LANDMARK_TYPE.MONSTER_LAIR) {
				LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(landmark.tileLocation.region, landmark.specificLandmarkType.GetStructureType());
				landmark.tileLocation.settlementOnTile.GenerateStructures(structure);
				yield return MapGenerator.Instance.StartCoroutine(GenerateMonsterLair(landmark.tileLocation, structure));
			} else if (landmark.specificLandmarkType != LANDMARK_TYPE.VILLAGE) {
				yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceBuiltLandmark(landmark.tileLocation.settlementOnTile, 
						landmark.tileLocation.region.innerMap, RESOURCE.NONE, landmark.specificLandmarkType.GetStructureType()));
			}
		}
		yield return null;
	}
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}

	#region Cellular Automata
	private IEnumerator GenerateMonsterLair(HexTile hexTile, LocationStructure structure) {
		List<LocationGridTile> locationGridTiles = new List<LocationGridTile>(hexTile.locationGridTiles);

		LocationStructure wilderness = hexTile.region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
		
		InnerMapManager.Instance.MonsterLairCellAutomata(locationGridTiles, structure, hexTile.region, wilderness);
		
		structure.SetOccupiedHexTile(hexTile);
		yield return null;
	}
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}
	#endregion
}
