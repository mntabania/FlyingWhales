using System;
using System.Collections.Generic;
namespace Tutorial {
    public class SurviveCounterattack : TutorialQuest {
        private List<Character> _attackingCharacters;
        
        public SurviveCounterattack() : base("Survive Counterattack", TutorialManager.Tutorial.Survive_Counterattack) { }
        public override void WaitForAvailability() {
            Messenger.AddListener<List<Character>>(Signals.THREAT_MAXED_OUT, OnThreatMaxedOut);
        }
        protected override void StopWaitingForAvailability() {
            Messenger.RemoveListener<List<Character>>(Signals.THREAT_MAXED_OUT, OnThreatMaxedOut);
        }
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(new ClickOnCharacterStep("Find the heroic party", validityChecker: IsCharacterPartOfHeroicParty)),
                new TutorialQuestStepCollection(new EliminateCharacterStep("Eliminate all threats", targets: _attackingCharacters.ToArray())) 
            };
        }

        #region Listeners
        private void OnThreatMaxedOut(List<Character> attackingCharacters) {
            _attackingCharacters = attackingCharacters;
            MakeAvailable();
        }
        #endregion

        #region Step Helpers
        private bool IsCharacterPartOfHeroicParty(Character character) {
            if (_attackingCharacters.Contains(character)) {
                return true;
            }
            return false;
        }
        #endregion
    }
}