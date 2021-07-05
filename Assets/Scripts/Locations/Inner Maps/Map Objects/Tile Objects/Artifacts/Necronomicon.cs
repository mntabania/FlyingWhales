using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using UtilityScripts;
public class Necronomicon : Artifact {

    public Necronomicon() : base(ARTIFACT_TYPE.Necronomicon) {
        maxHP = 700;
        currentHP = maxHP;
        traitContainer.AddTrait(this, "Treasure");
    }
    public Necronomicon(SaveDataArtifact data) : base(data) { }

    #region Overrides
    public override void SetInventoryOwner(Character p_newOwner) {
        if(isBeingCarriedBy != p_newOwner) {
            base.SetInventoryOwner(p_newOwner);
            if (isBeingCarriedBy != null) {
                isBeingCarriedBy.interruptComponent.NecromanticTransform();
            }
        }
    }
    public override void ActivateTileObject() {
        if (gridTileLocation != null) {
            base.ActivateTileObject();
            List<LocationGridTile> tilesInRange = RuinarchListPool<LocationGridTile>.Claim();
            gridTileLocation.PopulateTilesInRadius(tilesInRange, 1);
            LocationGridTile tile1 = null;
            LocationGridTile tile2 = null;
            LocationGridTile tile3 = null;

            int index1 = UnityEngine.Random.Range(0, tilesInRange.Count);
            tile1 = tilesInRange[index1];
            tilesInRange.RemoveAt(index1);

            tile2 = tile1;
            tile3 = tile1;

            if(tilesInRange.Count > 0) {
                int index2 = UnityEngine.Random.Range(0, tilesInRange.Count);
                tile2 = tilesInRange[index2];
                tilesInRange.RemoveAt(index2);
            }
            if (tilesInRange.Count > 0) {
                int index3 = UnityEngine.Random.Range(0, tilesInRange.Count);
                tile3 = tilesInRange[index3];
            }

            Summon skeleton1 = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, FactionManager.Instance.undeadFaction, homeRegion: gridTileLocation.parentMap.region, className: "Marauder");
            Summon skeleton2 = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, FactionManager.Instance.undeadFaction, homeRegion: gridTileLocation.parentMap.region, className: "Archer");
            Summon skeleton3 = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, FactionManager.Instance.undeadFaction, homeRegion: gridTileLocation.parentMap.region, className: "Mage");
            CharacterManager.Instance.PlaceSummonInitially(skeleton1, tile1);
            CharacterManager.Instance.PlaceSummonInitially(skeleton2, tile2);
            CharacterManager.Instance.PlaceSummonInitially(skeleton3, tile3);

            GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Necronomicon_Activate);
            RuinarchListPool<LocationGridTile>.Release(tilesInRange);
            //gridTileLocation.structure.RemovePOI(this);
        }
    }
    //public override void OnTileObjectAddedToInventoryOf(Character inventoryOwner) {
    //    base.OnTileObjectAddedToInventoryOf(inventoryOwner);
    //    if(CharacterManager.Instance.necromancerInTheWorld == null) {
    //        if (inventoryOwner.traitContainer.HasTrait("Evil") || inventoryOwner.traitContainer.HasTrait("Treacherous")) { //|| (inventoryOwner.traitContainer.HasTrait("Treacherous") && inventoryOwner.traitContainer.HasTrait("Betrayed", "Heartbroken", "Griefstricken"))
    //            //Necromantic Transformation
    //            inventoryOwner.interruptComponent.TriggerInterrupt(INTERRUPT.Necromantic_Transformation, inventoryOwner);
    //        }
    //    }
    //}
    #endregion
}
