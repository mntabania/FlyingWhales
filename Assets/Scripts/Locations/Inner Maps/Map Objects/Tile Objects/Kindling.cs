using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kindling : TileObject{
	public Kindling() {
		Initialize(TILE_OBJECT_TYPE.KINDLING);
	}
	public Kindling(SaveDataTileObject data) {
		Initialize(data);
	}
}
