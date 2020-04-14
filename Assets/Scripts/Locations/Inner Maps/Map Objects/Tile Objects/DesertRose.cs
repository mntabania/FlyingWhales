using System.Collections.Generic;
using UnityEngine;

public class DesertRose : TileObject {

    private AutoDestroyParticle _particleEffect;
    
    public DesertRose() {
        Initialize(TILE_OBJECT_TYPE.DESERT_ROSE, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public DesertRose(SaveDataTileObject data) {
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }

    public void DesertRoseEffect() {
        if(gridTileLocation != null) {
            _particleEffect = GameManager.Instance.CreateParticleEffectAt(
                gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetCenterLocationGridTile(),
                PARTICLE_EFFECT.Desert_Rose).GetComponent<AutoDestroyParticle>();
            // gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.ChangeBiomeType(BIOMES.DESERT);
            gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GradualChangeBiomeType(BIOMES.DESERT, OnDoneChangingBiome);
            gridTileLocation.structure.RemovePOI(this);
        }
    }
    private void OnDoneChangingBiome() {
        _particleEffect.StopEmission();
    }
}
