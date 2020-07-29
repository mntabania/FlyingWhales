using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Ruinarch;
using UnityEngine;
using Inner_Maps;
using UtilityScripts;

public class PlayerDataGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
        PlayerManager.Instance.InitializePlayer(data.portal, data.portalStructure, PLAYER_ARCHETYPE.Normal);
        yield return null;
	}
}
