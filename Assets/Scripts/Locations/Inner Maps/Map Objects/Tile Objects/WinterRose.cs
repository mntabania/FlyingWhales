using System.Collections.Generic;

public class WinterRose : TileObject {
    
    private AutoDestroyParticle _particleEffect;
    
    public WinterRose() {
        Initialize(TILE_OBJECT_TYPE.WINTER_ROSE, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public WinterRose(SaveDataTileObject data) {
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }

    public void WinterRoseEffect() {
        if(gridTileLocation != null) {
            _particleEffect = GameManager.Instance.CreateParticleEffectAt(
                gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetCenterLocationGridTile(),
                PARTICLE_EFFECT.Winter_Rose).GetComponent<AutoDestroyParticle>();
            //gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.ChangeBiomeType(BIOMES.SNOW);
            gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GradualChangeBiomeType(BIOMES.SNOW, OnDoneChangingBiome);
            gridTileLocation.structure.RemovePOI(this);
        }
    }
    
    private void OnDoneChangingBiome() {
        _particleEffect.StopEmission();
    }
}
