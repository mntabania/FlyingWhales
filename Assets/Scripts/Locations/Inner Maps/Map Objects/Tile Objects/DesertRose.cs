using System.Collections.Generic;
using UnityEngine;

public class DesertRose : TileObject {

    private AutoDestroyParticle _particleEffect;
    
    public DesertRose() {
        Initialize(TILE_OBJECT_TYPE.DESERT_ROSE, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
    }
    public DesertRose(SaveDataTileObject data) : base(data) { }

    public void DesertRoseWaterEffect() {
        if(gridTileLocation != null) {
            // _particleEffect = GameManager.Instance.CreateParticleEffectAt(
            //     gridTileLocation.hexTileOwner.GetCenterLocationGridTile(),
            //     PARTICLE_EFFECT.Desert_Rose).GetComponent<AutoDestroyParticle>();
            // // gridTileLocation.hexTileOwner.ChangeBiomeType(BIOMES.DESERT);
            // gridTileLocation.hexTileOwner.GradualChangeBiomeType(BIOMES.DESERT, OnDoneChangingBiome);
            // gridTileLocation.structure.RemovePOI(this);
            for (int i = 0; i < FactionManager.Instance.undeadFaction.characters.Count; i++) {
                Character character = FactionManager.Instance.undeadFaction.characters[i];
                if (!character.isDead) {
                    character.behaviourComponent.AddBehaviourComponent(typeof(PangatLooVillageInvaderBehaviour));
                    character.jobQueue.CancelAllJobs();
                }
            }
            
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Tile Object", "DesertRose", "activated_village", providedTags: LOG_TAG.Player);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
            gridTileLocation.structure.RemovePOI(this);
        }
    }
    public void DesertRoseOtherDamageEffect() {
        if(gridTileLocation != null) {
            for (int i = 0; i < FactionManager.Instance.undeadFaction.characters.Count; i++) {
                Character character = FactionManager.Instance.undeadFaction.characters[i];
                if (!character.isDead) {
                    character.behaviourComponent.AddBehaviourComponent(typeof(PangatLooPortalAttackerBehaviour));
                    character.jobQueue.CancelAllJobs();
                }
            }
            
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Tile Object", "DesertRose", "activated_portal", providedTags: LOG_TAG.Player);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
            gridTileLocation.structure.RemovePOI(this);
        }
    }
    private void OnDoneChangingBiome() {
        _particleEffect.StopEmission();
    }
}
