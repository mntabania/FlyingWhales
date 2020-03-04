using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kindling : TileObject{
	public Kindling() {
		Initialize(TILE_OBJECT_TYPE.KINDLING);
		advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
	}
	public Kindling(SaveDataTileObject data) {
		advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
		Initialize(data);
	}
}
