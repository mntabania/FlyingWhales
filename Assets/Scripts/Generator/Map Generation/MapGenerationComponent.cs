﻿using System.Collections;
using System.Collections.Generic;
using Scenario_Maps;
using UnityEngine;

/// <summary>
/// Base class for all map generation actions.
/// </summary>
public abstract class MapGenerationComponent {

	public bool succeess = true; //if generation component succeeded or not.
	public string log = "";
	
	public abstract IEnumerator ExecuteRandomGeneration(MapGenerationData data);
	public virtual IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) { yield return null; }
	public virtual IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) { yield return null; }
	

	public void AddLog(string str) {
		log += $"\t-{str}\n";
	}
}
