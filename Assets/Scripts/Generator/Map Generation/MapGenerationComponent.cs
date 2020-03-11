using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all map generation actions.
/// </summary>
public abstract class MapGenerationComponent {

	public bool succeess = true; //if generation component succeeded or not.
	public string log = "";
	
	public abstract IEnumerator Execute(MapGenerationData data);

	public void AddLog(string str) {
		log += $"\t-{str}\n";
	}
}
