﻿using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class Cry : Interrupt {
        public Cry() : base(INTERRUPT.Cry) {
            duration = 3;
            doesStopCurrentAction = true;
            //isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Sad_Icon;
            //isIntel = true;
            logTags = new[] {LOG_TAG.Social};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, Log overrideEffectLog, ActualGoapNode goapNode = null) {
            //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, interruptHolder.actor.marker.transform.position, 2, interruptHolder.actor.currentRegion.innerMap);
            overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "cry", null, logTags);
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(null, interruptHolder.identifier, LOG_IDENTIFIER.STRING_1);
            return true;
        }
        //public override string ReactionToActor(Character witness, Character actor, IPointOfInterest target, Interrupt interrupt, REACTION_STATUS status) {
        //    string response = base.ReactionToActor(witness, actor, target, interrupt, status);
        //    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
        //    if (opinionLabel == RelationshipManager.Acquaintance) {
        //        if (!witness.traitContainer.HasTrait("Psychopath")) {
        //            if (UnityEngine.Random.Range(0, 2) == 0) {
        //                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
        //            }
        //        }
        //    } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
        //        if (!witness.traitContainer.HasTrait("Psychopath")) {
        //            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
        //        }
        //    } else if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
        //        if (UnityEngine.Random.Range(0, 2) == 0) {
        //            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
        //        }
        //    }
        //    return response;
        //}
        #endregion
    }
}
