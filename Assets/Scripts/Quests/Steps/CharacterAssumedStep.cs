using System;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using UnityEngine.Assertions;
namespace Quests.Steps {
    public class CharacterAssumedStep : QuestStep {
        
        private readonly Func<IPointOfInterest, bool> _poiValidityChecker;
        private readonly Func<Character, bool> _assumerValidityChecker;
        
        public CharacterAssumedStep(Func<IPointOfInterest, bool> poiValidityChecker, 
            Func<Character, bool> assumerValidityChecker, string stepDescription) : base(stepDescription) {
            _poiValidityChecker = poiValidityChecker;
            _assumerValidityChecker = assumerValidityChecker;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<Character, Character, IPointOfInterest>(Signals.CHARACTER_ASSUMED, CheckCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Character, Character, IPointOfInterest>(Signals.CHARACTER_ASSUMED, CheckCompletion);
        }

        #region Listeners
        private void CheckCompletion(Character assumer, Character target, IPointOfInterest targetObject) {
            if (_assumerValidityChecker.Invoke(assumer) && _poiValidityChecker.Invoke(targetObject)) {
                Complete();
            }
        }
        #endregion
    }
}