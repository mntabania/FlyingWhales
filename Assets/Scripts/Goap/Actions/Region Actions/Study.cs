
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class Study : GoapAction {
    
    public Study() : base(INTERACTION_TYPE.STUDY) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Buff", false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Study Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        log.AddToFillers(node.poiTarget.gridTileLocation.parentMap.location.coreTile.region, node.poiTarget.gridTileLocation.parentMap.location.coreTile.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //**Requirements:** Region must not be corrupted and region must not be empty
            var region = poiTarget.gridTileLocation.parentMap.location.coreTile.region;
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && region.coreTile.isCorrupted == false && region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.NONE;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreStudySuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.parentMap.location.coreTile.region, goapNode.poiTarget.gridTileLocation.parentMap.location.coreTile.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterStudySuccess(ActualGoapNode goapNode) {
        List<string> buffs = TraitManager.Instance.GetAllBuffTraitsThatCharacterCanHave(goapNode.actor);
        if (buffs.Count > 0) {
            string chosenBuff = CollectionUtilities.GetRandomElement(buffs);
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, chosenBuff);
            goapNode.descriptionLog.AddToFillers(null, chosenBuff, LOG_IDENTIFIER.STRING_1);
        } else {
            Debug.LogWarning($"{goapNode.actor.name} could not find any buffs to gain by studying.");
        }
    }
    #endregion
}
