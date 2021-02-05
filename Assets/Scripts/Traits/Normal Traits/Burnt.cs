﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Traits {
    public class Burnt : Status {

        private Color burntColor => Color.gray;
        private GameObject _burntEffect;
        
        public Burnt() {
            name = "Burnt";
            description = "Was ravaged by fire.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            isTangible = true;
            moodEffect = -10;
            //effects = new List<TraitEffect>();
        }

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is BaseMapObject mapObject && mapObject.baseMapObjectVisual != null) {
                if (mapObject is GenericTileObject genericTileObject) {
                    LocationGridTile tile = genericTileObject.gridTileLocation;
                    Sprite floorSprite = tile.parentMap.groundTilemap.GetSprite(tile.localPlace);
                    mapObject.baseMapObjectVisual.SetVisual(floorSprite);
                    tile.parentTileMap.SetColor(tile.localPlace, burntColor);
                    tile.parentMap.detailsTilemap.SetColor(tile.localPlace, burntColor);
                    tile.parentMap.northEdgeTilemap.SetColor(tile.localPlace, burntColor);
                    tile.parentMap.southEdgeTilemap.SetColor(tile.localPlace, burntColor);
                    tile.parentMap.eastEdgeTilemap.SetColor(tile.localPlace, burntColor);
                    tile.parentMap.westEdgeTilemap.SetColor(tile.localPlace, burntColor);
                }
                if (addTo is IPointOfInterest poi) {
                    _burntEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Burnt);
                }
                mapObject.baseMapObjectVisual.SetMaterial(InnerMapManager.Instance.assetManager.burntMaterial);
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is BaseMapObject mapObject && mapObject.baseMapObjectVisual != null) {
                if (mapObject is GenericTileObject genericTileObject) {
                    LocationGridTile tile = genericTileObject.gridTileLocation;
                    Sprite floorSprite = tile.parentMap.groundTilemap.
                        GetSprite(tile.localPlace);
                    mapObject.baseMapObjectVisual.SetVisual(floorSprite);
                    tile.parentTileMap.SetColor(tile.localPlace, burntColor);
                    tile.parentMap.detailsTilemap.SetColor(tile.localPlace, burntColor);
                    tile.parentMap.northEdgeTilemap.SetColor(tile.localPlace, burntColor);
                    tile.parentMap.southEdgeTilemap.SetColor(tile.localPlace, burntColor);
                    tile.parentMap.eastEdgeTilemap.SetColor(tile.localPlace, burntColor);
                    tile.parentMap.westEdgeTilemap.SetColor(tile.localPlace, burntColor);
                }
                if (addedTo is IPointOfInterest poi) {
                    _burntEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Burnt);
                }
                mapObject.baseMapObjectVisual.SetMaterial(InnerMapManager.Instance.assetManager.burntMaterial);
            }
            if (addedTo is TileObject obj) {
                obj.SetPOIState(POI_STATE.INACTIVE);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is BaseMapObject mapObject && mapObject.baseMapObjectVisual != null) {
                if (mapObject is GenericTileObject genericTileObject) {
                    LocationGridTile tile = genericTileObject.gridTileLocation;
                    tile.parentTileMap.SetColor(tile.localPlace, Color.white);
                    tile.parentMap.detailsTilemap.SetColor(tile.localPlace, Color.white);
                    tile.parentMap.northEdgeTilemap.SetColor(tile.localPlace, Color.white);
                    tile.parentMap.southEdgeTilemap.SetColor(tile.localPlace, Color.white);
                    tile.parentMap.eastEdgeTilemap.SetColor(tile.localPlace, Color.white);
                    tile.parentMap.westEdgeTilemap.SetColor(tile.localPlace, Color.white);
                }
                if (_burntEffect != null) {
                    ObjectPoolManager.Instance.DestroyObject(_burntEffect);
                    _burntEffect = null;
                }
                mapObject.baseMapObjectVisual.SetMaterial(InnerMapManager.Instance.assetManager.defaultObjectMaterial);
            }
            if (removedFrom is TileObject obj) {
                obj.SetPOIState(POI_STATE.ACTIVE);
            }
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is TileObject targetPOI && targetPOI.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                if (targetPOI.Advertises(INTERACTION_TYPE.REPAIR)) {
                    GoapPlanJob currentJob = targetPOI.GetJobTargetingThisCharacter(JOB_TYPE.REPAIR);
                    if (currentJob == null) {
                        //job.SetCanBeDoneInLocation(true);
                        if (InteractionManager.Instance.CanCharacterTakeRepairJob(characterThatWillDoJob, targetPOI)) {
                            GoapEffect effect = new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burnt", false, GOAP_EFFECT_TARGET.TARGET);
                            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPAIR, effect, targetPOI, characterThatWillDoJob);
                            UtilityScripts.JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(characterThatWillDoJob, job, INTERACTION_TYPE.TAKE_RESOURCE);
                            job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TileObjectDB.GetTileObjectData(targetPOI.tileObjectType).mainRecipe });
                            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                            return true;
                        }
                        //else {
                        //    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRepairJob);
                        //    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                        //    return false;
                        //}
                    } 
                    //else {
                    //    if (InteractionManager.Instance.CanCharacterTakeRepairJob(characterThatWillDoJob, currentJob)) {
                    //        return TryTransferJob(currentJob, characterThatWillDoJob);
                    //    }
                    //}
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        #endregion
    }
}

