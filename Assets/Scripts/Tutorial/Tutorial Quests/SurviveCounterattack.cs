using System;
using System.Collections.Generic;
namespace Tutorial {
    public class SurviveCounterattack : TutorialQuest {
        
        public SurviveCounterattack() : base("Survive Counterattack", TutorialManager.Tutorial.Survive_Counterattack) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<TutorialQuestCriteria>() {
                new ThreatMaxedOut()
            };
        }
        #endregion
        
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(new ClickOnCharacterStep("Find the heroic party", validityChecker: IsCharacterPartOfHeroicParty)),
                new TutorialQuestStepCollection(new EliminateCharacterStep("Eliminate all threats", targets: PlayerManager.Instance.player.threatComponent.attackingCharacters.ToArray())) 
            };
        }

        #region Step Helpers
        private bool IsCharacterPartOfHeroicParty(Character character) {
            if (PlayerManager.Instance.player.threatComponent.attackingCharacters.Contains(character)) {
                return true;
            }
            return false;
        }
        #endregion
    }
}