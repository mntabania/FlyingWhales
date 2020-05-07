namespace Traits {
    public class Tending : Status {

        private Character _owner;
        private bool _hasTendedAtLeastOnce;
        
        public Tending() {
            name = "Tending";
            description = "This is Tending.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _owner = character;
                character.behaviourComponent.AddBehaviourComponent(typeof(TendFarmBehaviour));
                Messenger.AddListener<Character, ActualGoapNode>(Signals.CHARACTER_DOING_ACTION, OnActionStarted);
                Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                if (_hasTendedAtLeastOnce) {
                    Log endLog = new Log(GameManager.Instance.Today(), "Behaviour", "TendFarmBehaviour", "end");
                    endLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    endLog.AddLogToInvolvedObjects();    
                }

                character.behaviourComponent.RemoveBehaviourComponent(typeof(TendFarmBehaviour));
                Messenger.RemoveListener<Character, ActualGoapNode>(Signals.CHARACTER_DOING_ACTION, OnActionStarted);
                Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
                character.homeSettlement.settlementJobTriggerComponent.CheckIfFarmShouldBeTended(false);
            }
        }
        #endregion

        private void OnActionStarted(Character character, ActualGoapNode goapNode) {
            if (_owner == character) {
                if (goapNode.action.goapType != INTERACTION_TYPE.TEND && 
                    goapNode.action.goapType != INTERACTION_TYPE.START_TEND) {
                    _owner.traitContainer.RemoveTrait(_owner, this);
                } else if (goapNode.action.goapType == INTERACTION_TYPE.TEND) {
                    _hasTendedAtLeastOnce = true;
                }    
            }
        }
        private void OnCharacterStartedState(Character character, CharacterState state) {
            if (character == _owner) {
                _owner.traitContainer.RemoveTrait(_owner, this);
            }
        }
    }
}