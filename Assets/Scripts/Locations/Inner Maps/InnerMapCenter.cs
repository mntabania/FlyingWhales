using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class InnerMapCenter : MonoBehaviour {
    public ParticleSystem[] bigFogOfWarParticleEffect;

    public void ResizeFogOfWarBasedOnTileMapSize(InnerTileMap map) {
        for (int i = 0; i < bigFogOfWarParticleEffect.Length; i++) {
            ParticleSystem.ShapeModule newShape = bigFogOfWarParticleEffect[i].shape;
            newShape.scale = new Vector3(map.width + 3f, map.height + 3f, 1f);
            bigFogOfWarParticleEffect[i].Play();
        }

    }
}
