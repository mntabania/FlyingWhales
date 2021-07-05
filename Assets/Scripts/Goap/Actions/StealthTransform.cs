using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;
using Inner_Maps;
using Locations.Settlements;
using UtilityScripts;

public class StealthTransform : GoapAction {
    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public StealthTransform() : base(INTERACTION_TYPE.STEALTH_TRANSFORM) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.No_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Crimes, LOG_TAG.Life_Changes};
    }

    #region Overrides
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        OtherData[] otherData = node.otherData;
        if (otherData != null && otherData.Length == 1) {
            //if (otherData[0] is Dwelling) {
            //    return otherData[0] as Dwelling;
            //} else 
            if (otherData[0].obj is LocationStructure structure) {
                return structure;
            } else if (otherData[0].obj is Area area) {
                return area.primaryStructureInArea;
            } else if (otherData[0].obj is BaseSettlement settlement) {
                if (settlement.locationType == LOCATION_TYPE.VILLAGE) {
                    return settlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
                } else {
                    if (settlement.allStructures.Count > 0) {
                        return settlement.allStructures[0];
                    }
                }
            }
        }
        return null;
    }
    public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        OtherData[] otherData = goapNode.otherData;
        if (otherData != null && otherData.Length == 1) {
            //if (otherData[0] is Dwelling) {
            //    return otherData[0] as Dwelling;
            //} else 
            if (otherData[0].obj is LocationStructure structure) {
                if (structure.passableTiles.Count > 0) {
                    return structure.GetRandomPassableTile();
                }
                return structure.GetRandomTile();
            } else if (otherData[0].obj is Area area) {
                return area.GetRandomPassableTile();
            } else if (otherData[0].obj is BaseSettlement settlement) {
                //If settlement is passed and the settlement is a village, do stealth transform on the areas around village
                if (settlement.locationType == LOCATION_TYPE.VILLAGE) {
                    List<Area> areas = RuinarchListPool<Area>.Claim();
                    settlement.PopulateSurroundingAreas(areas);
                    Area chosenArea = areas[GameUtilities.RandomBetweenTwoNumbers(0, areas.Count - 1)];
                    return chosenArea.GetRandomPassableTile();
                } else {
                    return settlement.GetRandomPassableTile();
                }
            }
        }
        return base.GetTargetTileToGoTo(goapNode);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Transform Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 1;
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion

    #region State Effects
    //public void PreVisitSuccess(ActualGoapNode goapNode) {
    //goapNode.descriptionLog.AddToFillers(null, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    public void AfterTransformSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.trapStructure.SetStructureAndDuration(goapNode.targetStructure, GameManager.Instance.GetTicksBasedOnHour(2) + GameManager.Instance.GetTicksBasedOnMinutes(30));
        Lycanthrope lycanthrope = goapNode.actor.traitContainer.GetTraitOrStatus<Lycanthrope>("Lycanthrope");
        if(lycanthrope != null) {
            lycanthrope.CheckIfAlone();
        }
    }
    #endregion
}
