﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using UtilityScripts;
namespace Interrupts {
    public class TotalOrganFailure : Interrupt {
        public TotalOrganFailure() : base(INTERRUPT.Total_Organ_Failure) {
            duration = 5;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Death_Icon;
            isIntel = true;
            logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Player};
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            if (GameUtilities.RollChance(15) && interruptHolder.actor.homeSettlement != null && 
                Locations.Settlements.Settlement_Events.PlaguedEvent.HasMinimumAmountOfPlaguedVillagersForEvent(interruptHolder.actor.homeSettlement) &&
                !interruptHolder.actor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event) && interruptHolder.actor.homeSettlement.eventManager.CanHaveEvents()) {
                interruptHolder.actor.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Plagued_Event);
            }
            interruptHolder.actor.Death("Total Organ Failure", _deathLog: interruptHolder.effectLog, interrupt: this);
            return true;
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target,
            Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            
            if (status == REACTION_STATUS.WITNESSED && actor.homeSettlement != null && actor.homeSettlement is NPCSettlement settlement) {
                //When a resident has been witnessed to die due to Total Organ Failure, the Settlement will be flagged as Plagued
                settlement.SetIsPlagued(true);
            }
            
            return response;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);

            reactions.Add(EMOTION.Shock);

            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend ||
                opinionLabel == RelationshipManager.Close_Friend) {
                reactions.Add(EMOTION.Concern);
            } else if ((witness.relationshipContainer.IsFamilyMember(actor) ||
                        witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR)) &&
                       !witness.relationshipContainer.HasOpinionLabelWithCharacter(actor, RelationshipManager.Rival)) {
                reactions.Add(EMOTION.Concern);
            } else if (opinionLabel == RelationshipManager.Rival) {
                reactions.Add(EMOTION.Scorn);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                reactions.Add(EMOTION.Fear);
            }

        }
        #endregion
    }
}