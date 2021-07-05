using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeWardObjectGameObject : TileObjectGameObject {
	public override void Initialize(TileObject tileObject) {
		base.Initialize(tileObject);
		visionTrigger.SetVisionTriggerCollidersState(false);
	}
}
