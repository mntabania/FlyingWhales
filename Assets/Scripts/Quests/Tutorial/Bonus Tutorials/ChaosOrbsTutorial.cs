using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class ChaosOrbsTutorial : BonusTutorial {
        public ChaosOrbsTutorial() : base("Chaos Orbs", TutorialManager.Tutorial.Chaos_Orbs_Tutorial) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new ChaosOrbSpawned(),
            };
        }
        #endregion

        #region Availability
        protected override void MakeAvailable() {
            if (isAvailable) {
                return;
            }
            isAvailable = true;
            ConstructSteps();
            TutorialManager.Instance.ActivateTutorial(this);
        }
        #endregion
        
        #region Activation
        public override void Activate() {
            base.Activate();
            Messenger.AddListener(Signals.CHAOS_ORB_DESPAWNED, CheckForFailure);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener(Signals.CHAOS_ORB_DESPAWNED, CheckForFailure);
        }
        #endregion

        #region Steps
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnChaosOrbStep()
                        .SetObjectsToCenter(() => 
                            PlayerManager.Instance.availableChaosOrbs.Select(x => x.gameObject).ToList(), 
                            IsChaosOrbSelected, CenterChaosOrb
                        )
                        .SetCompleteAction(OnCompleteClickChaosOrb)
                ),
            };
        }
        #endregion

        #region Step Helpers
        private void OnCompleteClickChaosOrb() {
            TutorialManager.Instance.StartCoroutine(DelayedChaosOrbTooltip());
        }
        private IEnumerator DelayedChaosOrbTooltip() {
            yield return new WaitForSeconds(1f);
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Chaos Orbs",
                "You start a playthrough with limited amount of Mana. " +
                "The primary way of gaining more Mana is by finding Chaos Orbs. " +
                $"These are Mana-filled orbs that are produced by {UtilityScripts.Utilities.VillagerIcon()}Villagers whenever they perform criminal acts. " +
                "They also produce these when they cry so try to make them miserable.",
                TutorialManager.Instance.afflictionsVideoClip
            );
        }
        private bool IsChaosOrbSelected(GameObject gameObject) {
            return InnerMapCameraMove.Instance.target == gameObject.transform 
                   || InnerMapCameraMove.Instance.lastCenteredTarget == gameObject.transform;
        }
        private void CenterChaosOrb(GameObject gameObject) {
            InnerMapCameraMove.Instance.CenterCameraOn(gameObject);
        }
        #endregion

        #region Failure
        private void CheckForFailure() {
            if (PlayerManager.Instance.availableChaosOrbs.Count == 0) {
                FailQuest();
            }
        }
        protected override void FailQuest() {
            base.FailQuest();
            //respawn this Quest.
            TutorialManager.Instance.InstantiateTutorial(TutorialManager.Tutorial.Chaos_Orbs_Tutorial);
        }
        #endregion
    }
}