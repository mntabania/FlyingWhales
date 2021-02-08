using Characters.Components;
namespace Traits {
    public class Tending : Status, CharacterEventDispatcher.ICarryListener {

        private Character _owner;
        private bool _hasTendedAtLeastOnce;

        #region getters
        public bool hasTendedAtLeastOnce => _hasTendedAtLeastOnce;
        #endregion

        public Tending() {
            name = "Tending";
            description = "This is Tending.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                _owner = character;
                Messenger.AddListener<Character, ActualGoapNode>(JobSignals.CHARACTER_DOING_ACTION, OnActionStarted);
                Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
                character.eventDispatcher.SubscribeToCharacterCarried(this);
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _owner = character;
                character.behaviourComponent.AddBehaviourComponent(typeof(TendFarmBehaviour));
                Messenger.AddListener<Character, ActualGoapNode>(JobSignals.CHARACTER_DOING_ACTION, OnActionStarted);
                Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
                character.eventDispatcher.SubscribeToCharacterCarried(this);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                if (_hasTendedAtLeastOnce) {
                    Log endLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Behaviour", "TendFarmBehaviour", "end", null, LOG_TAG.Work);
                    endLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    endLog.AddLogToDatabase(true);    
                }

                character.behaviourComponent.RemoveBehaviourComponent(typeof(TendFarmBehaviour));
                Messenger.RemoveListener<Character, ActualGoapNode>(JobSignals.CHARACTER_DOING_ACTION, OnActionStarted);
                Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
                character.homeSettlement?.settlementJobTriggerComponent.CheckIfFarmShouldBeTended(false);
                character.eventDispatcher.UnsubscribeToCharacterCarried(this);
            }
        }
        public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to) {
            base.OnCopyStatus(statusToCopy, from, to);
            if (statusToCopy is Tending status) {
                _hasTendedAtLeastOnce = status.hasTendedAtLeastOnce;
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
        public void OnCharacterCarried(Character p_character, Character p_carriedBy) {
            p_character.traitContainer.RemoveTrait(p_character, this);
        }
    }
}