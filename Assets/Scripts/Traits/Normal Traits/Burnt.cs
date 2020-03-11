using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Burnt : Status {

        private Color burntColor {
            get {
                return Color.gray;
            }
        }

        public Burnt() {
            name = "Burnt";
            description = "This is burnt.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is TileObject) {
                TileObject obj = addedTo as TileObject;
                obj.SetPOIState(POI_STATE.INACTIVE);
                //obj.SetSlotColor(burntColor);
                //obj.mapVisual?.SetColor(burntColor);
                //if (obj is GenericTileObject) {
                //    LocationGridTile tile = obj.gridTileLocation;
                //    tile.parentTileMap.SetColor(tile.localPlace, burntColor);
                //    tile.SetDefaultTileColor(burntColor);
                //    tile.parentMap.detailsTilemap.SetColor(tile.localPlace, burntColor);
                //    tile.parentMap.northEdgeTilemap.SetColor(tile.localPlace, burntColor);
                //    tile.parentMap.southEdgeTilemap.SetColor(tile.localPlace, burntColor);
                //    tile.parentMap.eastEdgeTilemap.SetColor(tile.localPlace, burntColor);
                //    tile.parentMap.westEdgeTilemap.SetColor(tile.localPlace, burntColor);
                //} 
            } 
            // else if (addedTo is SpecialToken) {
            //     SpecialToken token = addedTo as SpecialToken;
            //     token.SetPOIState(POI_STATE.INACTIVE);
            //     token.mapVisual.SetColor(burntColor);
            // }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is TileObject) {
                TileObject obj = removedFrom as TileObject;
                obj.SetPOIState(POI_STATE.ACTIVE);
                //obj.SetSlotColor(Color.white);
                //obj.mapVisual.SetColor(Color.white);
                //if (obj is GenericTileObject) {
                //    LocationGridTile tile = obj.gridTileLocation;
                //    tile.parentTileMap.SetColor(tile.localPlace, Color.white);
                //    tile.SetDefaultTileColor(Color.white);
                //    tile.parentMap.detailsTilemap.SetColor(tile.localPlace, Color.white);
                //    tile.parentMap.northEdgeTilemap.SetColor(tile.localPlace, Color.white);
                //    tile.parentMap.southEdgeTilemap.SetColor(tile.localPlace, Color.white);
                //    tile.parentMap.eastEdgeTilemap.SetColor(tile.localPlace, Color.white);
                //    tile.parentMap.westEdgeTilemap.SetColor(tile.localPlace, Color.white);
                //}
            } 
            // else if (removedFrom is SpecialToken) {
            //     SpecialToken token = removedFrom as SpecialToken;
            //     token.SetPOIState(POI_STATE.ACTIVE);
            //     token.mapVisual.SetColor(Color.white);
            // }
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is TileObject) {
                TileObject targetPOI = traitOwner as TileObject;
                if (targetPOI.advertisedActions.Contains(INTERACTION_TYPE.REPAIR)) {
                    GoapPlanJob currentJob = targetPOI.GetJobTargetingThisCharacter(JOB_TYPE.REPAIR);
                    if (currentJob == null) {
                        //job.SetCanBeDoneInLocation(true);
                        if (InteractionManager.Instance.CanCharacterTakeRepairJob(characterThatWillDoJob)) {
                            GoapEffect effect = new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burnt", false, GOAP_EFFECT_TARGET.TARGET);
                            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPAIR, effect, targetPOI, characterThatWillDoJob);
                            job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { (int)(TileObjectDB.GetTileObjectData(targetPOI.tileObjectType).constructionCost * 0.5f) });
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

