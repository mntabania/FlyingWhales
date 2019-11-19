﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Traits {
    public class AccidentProne : Trait {

        public static INTERACTION_TYPE[] excludedActionsFromAccidentProneTrait = new INTERACTION_TYPE[] {
            INTERACTION_TYPE.STUMBLE, INTERACTION_TYPE.PUKE, INTERACTION_TYPE.SEPTIC_SHOCK, INTERACTION_TYPE.ACCIDENT
        };

        public Character owner { get; private set; }
        public CharacterState storedState { get; private set; }

        public AccidentProne() {
            name = "Accident Prone";
            description = "Accident Prone characters often gets injured.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ACCIDENT, INTERACTION_TYPE.STUMBLE };
            
            daysDuration = 0;
            canBeTriggered = true;

        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                owner = sourceCharacter as Character;
            }
        }
        public override bool PerTickOwnerMovement() {
            int stumbleChance = UnityEngine.Random.Range(0, 100);
            bool hasCreatedJob = false;
            if (stumbleChance < 2) {
                if (owner.currentActionNode == null || (owner.currentActionNode.action.goapType != INTERACTION_TYPE.STUMBLE && owner.currentActionNode.action.goapType != INTERACTION_TYPE.ACCIDENT)) {
                    DoStumble();
                    hasCreatedJob = true;
                }
            }
            return hasCreatedJob;
        }
        public override bool OnStartPerformGoapAction(ActualGoapNode goapNode, ref bool willStillContinueAction) {
            int accidentChance = UnityEngine.Random.Range(0, 100);
            bool hasCreatedJob = false;
            if (accidentChance < 10) {
                if (goapNode != null && !excludedActionsFromAccidentProneTrait.Contains(goapNode.action.goapType)) {
                    DoAccident(goapNode);
                    hasCreatedJob = true;
                    willStillContinueAction = false;
                }
            }
            return hasCreatedJob;
        }
        public override string TriggerFlaw(Character character) {
            if (character.marker.isMoving) {
                //If moving, the character will stumble and get injured.
                DoStumble();
            } else if (character.currentActionNode.action != null && !excludedActionsFromAccidentProneTrait.Contains(character.currentActionNode.action.goapType)) {
                //If doing something, the character will fail and get injured.
                DoAccident(character.currentActionNode);
            }
            return base.TriggerFlaw(character);
        }
        #endregion

        private void DoStumble() {
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.STUMBLE, owner, owner);
            owner.jobQueue.AddJobInQueue(job);
        }

        private void DoAccident(ActualGoapNode goapNode) {
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.ACCIDENT, owner, new Dictionary<INTERACTION_TYPE, object[]>() {
                { INTERACTION_TYPE.ACCIDENT, new object[] { goapNode.action }}
            },  owner);
            owner.jobQueue.AddJobInQueue(job);
        }
    }
}

