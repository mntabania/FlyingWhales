using System;
namespace Traits {
    public class Patrolling : Status {

        private Character _owner;
        
        public Patrolling() {
            name = "Patrolling";
            description = "This is Patrolling.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 24;;
            isHidden = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _owner = character;
                character.behaviourComponent.AddBehaviourComponent(typeof(PatrolBehaviour));
                Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
                Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
                Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_COMBAT, OnCharacterCanNoLongerCombat);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                _owner = null;
                Log endLog = new Log(GameManager.Instance.Today(), "Behaviour", "PatrolBehaviour", "end");
                endLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                endLog.AddLogToInvolvedObjects();    
                
                character.behaviourComponent.RemoveBehaviourComponent(typeof(PatrolBehaviour));
                Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
                Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
                Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_COMBAT, OnCharacterCanNoLongerCombat);
            }
        }
        #endregion
        
        private void OnCharacterCanNoLongerCombat(Character character) {
            if (character == _owner) {
                character.traitContainer.RemoveTrait(character, this);
            }
        }
        private void OnCharacterCanNoLongerPerform(Character character) {
            if (character == _owner) {
                character.traitContainer.RemoveTrait(character, this);
            }
        }
        private void OnJobAddedToQueue(JobQueueItem job, Character character) {
            if (_owner == character && job.priority > JOB_TYPE.PATROL.GetJobTypePriority()) {
                //character took a job that is higher priority than cleansing tiles
                character.traitContainer.RemoveTrait(character, this);
            }
        }
    }
}