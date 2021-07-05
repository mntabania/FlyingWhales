using System;
namespace Traits {
    public class Cleansing : Status {

        private Character _owner;
        
        public Cleansing() {
            name = "Cleansing";
            description = "This is Cleansing tiles.";
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
                Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
                Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
            }
        }
        #endregion

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _owner = character;
                character.behaviourComponent.AddBehaviourComponent(typeof(CleanseTileBehaviour));
                Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
                Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                _owner = null;
                Log endLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Behaviour", "CleanseTileBehaviour", "end", null, LOG_TAG.Work);
                endLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                endLog.AddLogToDatabase(true);    
                
                character.behaviourComponent.RemoveBehaviourComponent(typeof(CleanseTileBehaviour));
                character.behaviourComponent.SetCleansingTilesForSettlement(null);
                Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
                Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
            }
        }
        #endregion
        
        private void OnCharacterCanNoLongerPerform(Character character) {
            if (character == _owner) {
                character.traitContainer.RemoveTrait(character, this);
            }
        }
        private void OnJobAddedToQueue(JobQueueItem job, Character character) {
            if (_owner == character && job.priority > JOB_TYPE.CLEANSE_TILES.GetJobTypePriority()) {
                //character took a job that is higher priority than cleansing tiles
                character.traitContainer.RemoveTrait(character, this);
            }
        }
    }
}