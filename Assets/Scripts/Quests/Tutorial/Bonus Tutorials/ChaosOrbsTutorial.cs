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
        public ChaosOrbsTutorial() : base("Mana Orbs", TutorialManager.Tutorial.Chaos_Orbs_Tutorial) { }

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
            Messenger.AddListener(PlayerSignals.CHAOS_ORB_DESPAWNED, CheckForFailure);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener(PlayerSignals.CHAOS_ORB_DESPAWNED, CheckForFailure);
        }
        #endregion

        #region Steps
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new CollectChaosOrbStep()
                        .SetObjectsToCenter(() => 
                            PlayerManager.Instance.availableChaosOrbs.Select(x => x.gameObject).ToList(), 
                            IsChaosOrbSelected, CenterChaosOrb
                        ).SetCompleteAction(OnCompleteClickChaosOrb)
                ),
            };
        }
        #endregion

        #region Step Helpers
        private void OnCompleteClickChaosOrb() {
            TutorialManager.Instance.StartCoroutine(DelayedChaosOrbTooltip());
        }
        private IEnumerator DelayedChaosOrbTooltip() {
            yield return UtilityScripts.GameUtilities.waitFor1Second;
            PlayerUI.Instance.ShowGeneralConfirmation("Mana Orbs",
                $"A {UtilityScripts.Utilities.ColorizeAction("Mana Orb")} has been produced by a Villager! Hover over these magical orbs of chaotic energy to gain more Mana. " +
                $"These are produced by Villagers whenever they perform {UtilityScripts.Utilities.ColorizeAction("criminal acts")}. " +
                $"They also produce these {UtilityScripts.Utilities.ColorizeAction("when they cry")} so try to make them miserable."
            );
        }
        private bool IsChaosOrbSelected(GameObject gameObject) {
            return InnerMapCameraMove.Instance.target == gameObject.transform 
                   || InnerMapCameraMove.Instance.lastCenteredTarget == gameObject.transform;
        }
        private void CenterChaosOrb(GameObject gameObject) {
            ChaosOrb chaosOrb = gameObject.GetComponent<ChaosOrb>();
            if (chaosOrb.location != null) {
                if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != chaosOrb.location.innerMap) {
                    InnerMapManager.Instance.HideAreaMap();
                }
                if (chaosOrb.location.innerMap.isShowing == false) {
                    InnerMapManager.Instance.ShowInnerMap(chaosOrb.location);
                }
                InnerMapCameraMove.Instance.CenterCameraOn(gameObject);    
            }
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