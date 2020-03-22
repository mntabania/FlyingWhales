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
            if (addedTo is BaseMapObject mapObject && mapObject.baseMapObjectVisual != null) {
                mapObject.baseMapObjectVisual.SetMaterial(InnerMapManager.Instance.assetManager.burntMaterial);
            }
            if (addedTo is TileObject obj) {
                obj.SetPOIState(POI_STATE.INACTIVE);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is BaseMapObject mapObject && mapObject.baseMapObjectVisual != null) {
                mapObject.baseMapObjectVisual.SetMaterial(InnerMapManager.Instance.assetManager.defaultObjectMaterial);
            }
            if (removedFrom is TileObject obj) {
                obj.SetPOIState(POI_STATE.ACTIVE);
            }
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is TileObject) {
                TileObject targetPOI = traitOwner as TileObject;
                if (targetPOI.Advertises(INTERACTION_TYPE.REPAIR)) {
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

