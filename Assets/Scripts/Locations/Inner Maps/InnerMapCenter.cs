using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class InnerMapCenter : MonoBehaviour {
    public ParticleSystem bigFogOfWarParticleEffect;

    public void ResizeFogOfWarBasedOnTileMapSize(InnerTileMap map) {
        ParticleSystem.ShapeModule newShape = bigFogOfWarParticleEffect.shape;
        newShape.scale = new Vector3(map.width + 18f, map.height + 18f, 1f);
        bigFogOfWarParticleEffect.Play();
    }
}
