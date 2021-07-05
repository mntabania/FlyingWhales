namespace Traits {
    public class Dousing : Status {

        private Character _owner;
        
        public Dousing() {
            name = "Dousing";
            description = "This is Dousing fires.";
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
                Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
            }
        }
        #endregion

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _owner = character;
                character.behaviourComponent.AddBehaviourComponent(typeof(DouseFireBehaviour));
                Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
                Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
                Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.behaviourComponent.RemoveBehaviourComponent(typeof(DouseFireBehaviour));
                character.behaviourComponent.SetDouseFireSettlement(null);
                Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
                Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
                Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
            }
        }
        #endregion
        
        private void OnCharacterCanNoLongerPerform(Character character) {
            if (character == _owner) {
                character.traitContainer.RemoveTrait(character, this);
            }
        }
        private void OnJobAddedToQueue(JobQueueItem job, Character character) {
            if (_owner == character && job.priority > JOB_TYPE.DOUSE_FIRE.GetJobTypePriority()) {
                //character took a job that is higher priority than dousing fire
                character.traitContainer.RemoveTrait(character, this);
            }
        }
        private void OnJobRemovedFromQueue(JobQueueItem job, Character character) {
            if (_owner == character && job.jobType == JOB_TYPE.DOUSE_FIRE && !job.finishedSuccessfully && !character.isBeingSeized) {
                bool hasFireInVision = false;
                if (character.hasMarker) {
                    for (int i = 0; i < character.marker.inVisionPOIs.Count; i++) {
                        IPointOfInterest poi = character.marker.inVisionPOIs[i];
                        if (poi.traitContainer.HasTrait("Burning")) {
                            hasFireInVision = true;
                            break;
                        }
                    }    
                }
                
                //character failed to do a douse fire job
                character.traitContainer.RemoveTrait(character, this);
                if (hasFireInVision) {
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Panicking, character, reason: "FIRE!");    
                }
            }
        }
    }
}