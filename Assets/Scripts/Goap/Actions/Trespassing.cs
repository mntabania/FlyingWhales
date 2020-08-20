using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps.Location_Structures;

public class Trespassing : GoapAction {
    public Trespassing() : base(INTERACTION_TYPE.TRESPASSING) {
        actionIconString = GoapActionStateDB.No_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Trespass Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    //public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
    //    GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
    //    IPointOfInterest poiTarget = node.poiTarget;
    //    if (goapActionInvalidity.isInvalid == false) {
    //        if ((poiTarget as Character).isDead == false) {
    //            goapActionInvalidity.isInvalid = true;
    //        }
    //    }
    //    return goapActionInvalidity;
    //}
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);

        if (!actor.isDead) {
            LocationStructure trespassedStructure = actor.currentStructure;
            if (trespassedStructure != null && trespassedStructure.settlementLocation != null && trespassedStructure.settlementLocation.owner != null && trespassedStructure.settlementLocation.owner == witness.faction) {
                bool willReact = true;
                switch (trespassedStructure.structureType) {
                    case STRUCTURE_TYPE.TAVERN:
                    case STRUCTURE_TYPE.FARM:
                    case STRUCTURE_TYPE.APOTHECARY:
                    case STRUCTURE_TYPE.CEMETERY:
                    case STRUCTURE_TYPE.CITY_CENTER:
                        willReact = false;
                        break;
                }

                if (willReact) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                    CrimeManager.Instance.ReactToCrime(witness, actor, witness, witness.faction, node.crimeType, node, status);
                }
            }
        }
        return response;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Trespassing;
    }
    #endregion
}