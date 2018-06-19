﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hostile : Trait {

    //#region International Incidents
    //internal override WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> GetInternationalIncidentReactionWeight(INTERNATIONAL_INCIDENT_TYPE incidentType,
    //    FactionRelationship rel, Faction aggressor) {
    //    WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> actionWeights = new WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION>();
    //    actionWeights.AddElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 5); //Add 5 Weight to Declare War

    //    Relationship chieftainRel = CharacterManager.Instance.GetRelationshipBetween(_ownerOfTrait, aggressor.leader);
    //    if (chieftainRel != null) {
    //        if (chieftainRel.totalValue < 0) {
    //            //Add 1 Weight to Declare per Negative Opinion the Chieftain has towards the other Chieftain (if they have a relationship)
    //            actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, Mathf.Abs(chieftainRel.totalValue));
    //        }
    //    }

    //    return actionWeights;
    //}
    //#endregion

    // internal override int GetWarOfConquestWeightModification(Kingdom otherKingdom) {
    //     Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
    //     int weight = 0;
    //     //if i am not at war, loop through non-ally adjacent kingdoms i am not at war with
    //     int warCount = sourceKingdom.GetWarCount();
    //     if(warCount <= 0) {
    //         KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
    //if (currRel.sharedRelationship.isAdjacent && !currRel.sharedRelationship.isAtWar && !currRel.AreAllies()) {
    //             KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
    //             if (currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {
    //                 //5 weight per 1% of my theoretical power over his
    //                 float theoreticalPowerPercent = currRel.GetTheoreticalPowerAdvantageOverTarget();
    //                 if (theoreticalPowerPercent > 0) {
    //                     weight += 5 * (int)theoreticalPowerPercent;
    //                 }
    //                 if(currRel.totalLike < 0) {
    //                     //add 2 weight per negative opinion
    //                     weight += Mathf.Abs(currRel.totalLike * 2);
    //                 }
    //             }
    //         }
    //     }
    //     return weight;
    // }

 //   #region Leave Alliance
 //   internal override int GetLeaveAllianceWeightModification(AlliancePool alliance) {
 //       int weight = 0;
 //       Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
 //       for (int i = 0; i < alliance.kingdomsInvolved.Count; i++) {
 //           Kingdom ally = alliance.kingdomsInvolved[i];
 //           if (ally.id != sourceKingdom.id) {
 //               KingdomRelationship sourceRelWithAlly = sourceKingdom.GetRelationshipWithKingdom(ally);
 //               if(sourceRelWithAlly.totalLike < 0) {
 //                   weight += Mathf.Abs(2 * sourceRelWithAlly.totalLike); //add 2 weight to leave alliance for every negative opinion I have towards the king
 //               }
 //           }
 //       }
 //       return weight;
 //   }
 //   internal override int GetKeepAllianceWeightModification(AlliancePool alliance) {
 //       int weight = 0;
 //       Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
 //       //List<Warfare> activeWars = sourceKingdom.GetAllActiveWars();
 //       for (int i = 0; i < alliance.kingdomsInvolved.Count; i++) {
 //           Kingdom ally = alliance.kingdomsInvolved[i];
 //           if (ally.id != sourceKingdom.id) {
 //               weight += 20 * ally.GetWarCount(); //add 20 weight to keep alliance for every active war of other kingdoms withinin alliance

 //               //List<Warfare> activeWarsOfAlly = ally.GetAllActiveWars();
 //               //for (int j = 0; j < activeWarsOfAlly.Count; j++) {
 //               //    Warfare currWar = activeWarsOfAlly[j];
 //               //    if (activeWars.Contains(currWar)) {
 //               //        weight += 20; //add 20 weight to keep alliance for every active war of other kingdoms withinin alliance
 //               //    }
 //               //}
                
 //           }
 //       }
 //       return weight;
 //   }
 //   #endregion

 //   internal override int GetLeaveTradeDealWeightModification(Kingdom otherKingdom) {
 //       return -30; //add 30 to Default Weight
 //   }

	//internal override int GetInternationalIncidentReactionWeight (InternationalIncident.INCIDENT_ACTIONS incidentAction, KingdomRelationship kr){
	//	int weight = 0;
	//	if(incidentAction == InternationalIncident.INCIDENT_ACTIONS.INCREASE_TENSION){
	//		weight += 20;
	//		if(kr.totalLike < 0){
	//			weight -= (kr.totalLike * 2);
	//		}
	//	}
	//	return weight;
	//}

	//internal override int GetRefugeeGovernorDecisionWeight(Refuge.GOVERNOR_DECISION decision){
	//	if(decision == Refuge.GOVERNOR_DECISION.REJECT){
	//		return 100;
	//	}
	//	return 0;
	//}

	//internal override int GetRandomInternationalIncidentWeight(){
	//	return 20;
	//}

	//internal override int GetMaxGeneralsModifier(){
	//	return 1;
	//}
}
