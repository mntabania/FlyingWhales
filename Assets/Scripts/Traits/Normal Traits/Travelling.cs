using System;
namespace Traits {
    public class Travelling : Status {
        
        public override bool isSingleton => true;
        
        public Travelling() {
            name = "Travelling";
            description = "This character is travelling.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                Messenger.Broadcast(CharacterSignals.STARTED_TRAVELLING, character);
            }
        }
        #endregion
    }
}