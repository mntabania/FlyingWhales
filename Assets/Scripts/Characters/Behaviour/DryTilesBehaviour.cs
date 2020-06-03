using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class DryTilesBehaviour : CharacterBehaviourComponent {
    
    public DryTilesBehaviour() {
        priority = 430;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if (StillHasWetTile(character)) {
            //dry nearest wet tile
            if (DryNearestTile(character, out producedJob) == false) {
                //could not find a tile to dry
                character.traitContainer.RemoveTrait(character, "Drying");
            }
        } else {
            //no more wet tiles
            character.traitContainer.RemoveTrait(character, "Drying");
        }
        return true;
    }
    
    private bool StillHasWetTile(Character character) {
        return character.behaviourComponent.dryingTilesForSettlement.settlementJobTriggerComponent.wetTiles.Count > 0;
    }
    
    private bool DryNearestTile(Character character, out JobQueueItem producedJob) {
        LocationGridTile nearestTile = null;
        float nearest = 99999f;

        for (int i = 0; i < character.homeSettlement.settlementJobTriggerComponent.wetTiles.Count; i++) {
            LocationGridTile wetTile = character.homeSettlement.settlementJobTriggerComponent.wetTiles[i];
            Wet wet = wetTile.genericTileObject.traitContainer.GetNormalTrait<Wet>("Wet");
            if (wet != null && wet.dryer == null) {
                //only consider dousing fire that is not yet assigned
                float dist = Vector2.Distance(character.worldObject.transform.position, wetTile.worldLocation);
                if (dist < nearest) {
                    nearestTile = wetTile;
                    nearest = dist;
                }    
            }
        }
        if (nearestTile != null) {
            Wet wet = nearestTile.genericTileObject.traitContainer.GetNormalTrait<Wet>("Wet"); 
            Assert.IsNotNull(wet, $"Wet of {nearestTile} is null.");
            wet.SetDryer(character);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DRY_TILES, INTERACTION_TYPE.DRY_TILE,
                nearestTile.genericTileObject, character);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }

}