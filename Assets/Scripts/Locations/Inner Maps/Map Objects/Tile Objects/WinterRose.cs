﻿using System.Collections.Generic;

public class WinterRose : TileObject {
    
    private AutoDestroyParticle _particleEffect;
    
    public WinterRose() {
        Initialize(TILE_OBJECT_TYPE.WINTER_ROSE, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
    }
    public WinterRose(SaveDataTileObject data) { }

    public void WinterRoseEffect() {
        if(gridTileLocation != null) {
            _particleEffect = GameManager.Instance.CreateParticleEffectAt(
                gridTileLocation.parentArea.GetCenterLocationGridTile(),
                PARTICLE_EFFECT.Winter_Rose).GetComponent<AutoDestroyParticle>();
            //gridTileLocation.hexTileOwner.ChangeBiomeType(BIOMES.SNOW);
            gridTileLocation.parentArea.GradualChangeBiomeType(BIOMES.SNOW, OnDoneChangingBiome);
            gridTileLocation.structure.RemovePOI(this);
        }
    }
    
    private void OnDoneChangingBiome() {
        _particleEffect.StopEmission();
    }
}
