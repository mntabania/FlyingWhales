using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class CleanseTileBehaviour : CharacterBehaviourComponent {
    
    public CleanseTileBehaviour() {
        priority = 630;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (StillHasPoisonedTile(character)) {
            //cleanse nearest poisoned tile
            producedJob = CleanseNearestTile(character);
            if (producedJob == null) {
                character.traitContainer.RemoveTrait(character, "Cleansing");
            }
        } else {
            producedJob = null;
            character.traitContainer.RemoveTrait(character, "Cleansing");
        }
        return true;
    }
    
    
    private bool StillHasPoisonedTile(Character character) {
        return character.behaviourComponent.cleansingTilesForSettlement.settlementJobTriggerComponent.poisonedTiles.Count > 0;
    }
    private JobQueueItem CleanseNearestTile(Character character) {
        LocationGridTile nearestTile = null;
        float nearest = 99999f;

        for (int i = 0; i < character.behaviourComponent.cleansingTilesForSettlement.settlementJobTriggerComponent.poisonedTiles.Count; i++) {
            LocationGridTile tile = character.behaviourComponent.cleansingTilesForSettlement.settlementJobTriggerComponent.poisonedTiles[i];
            Poisoned poisoned = tile.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
            if (poisoned != null) {
                if (poisoned.cleanser == null) {
                    float dist = Vector2.Distance(character.worldObject.transform.position, tile.worldLocation);
                    if (dist < nearest) {
                        nearestTile = tile;
                        nearest = dist;
                    }    
                }
            } else {
                //TODO: Find out why non poisoned tiles are still in list
                character.behaviourComponent.cleansingTilesForSettlement.settlementJobTriggerComponent.poisonedTiles.RemoveAt(i);
                i--;
            }
        }
        if (nearestTile != null) {
            Poisoned poisoned = nearestTile.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned"); 
            Assert.IsNotNull(poisoned, $"Poisoned of {nearestTile} is null.");
            poisoned.SetCleanser(character);
            GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CLEANSE_TILES,
                INTERACTION_TYPE.CLEANSE_TILE, nearestTile.tileObjectComponent.genericTileObject, character);
            // character.jobQueue.AddJobInQueue(goapPlanJob);
            return goapPlanJob;
        }
        return null;
    }

}