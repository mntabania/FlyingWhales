using System;
namespace Traits {
    public class Drying : Status {

        private Character _owner;
        
        public Drying() {
            name = "Drying";
            description = "This is Drying tiles.";
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
                character.behaviourComponent.AddBehaviourComponent(typeof(DryTilesBehaviour));
                Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
                Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                _owner = null;
                Log endLog = new Log(GameManager.Instance.Today(), "Behaviour", "DryTilesBehaviour", "end");
                endLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                endLog.AddLogToInvolvedObjects();    
                
                character.behaviourComponent.RemoveBehaviourComponent(typeof(DryTilesBehaviour));
                character.behaviourComponent.SetDryingTilesForSettlement(null);
                Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
                Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
            }
        }
        #endregion
        
        private void OnCharacterCanNoLongerPerform(Character character) {
            if (character == _owner) {
                character.traitContainer.RemoveTrait(character, this);
            }
        }
        private void OnJobAddedToQueue(JobQueueItem job, Character character) {
            if (_owner == character && job.priority > JOB_TYPE.DRY_TILES.GetJobTypePriority()) {
                //character took a job that is higher priority than cleansing tiles
                character.traitContainer.RemoveTrait(character, this);
            }
        }
    }
}