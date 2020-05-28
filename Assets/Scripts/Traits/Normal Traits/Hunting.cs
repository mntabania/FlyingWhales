namespace Traits {
    public class Hunting : Status {

        private Character _owner;
        public HexTile targetTile { get; private set; }
        
        public Hunting() {
            name = "Hunting";
            description = "This is Dousing fires.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(3);
            isHidden = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _owner = character;
                character.behaviourComponent.AddBehaviourComponent(typeof(HuntPreyBehaviour));
                Messenger.AddListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.behaviourComponent.RemoveBehaviourComponent(typeof(HuntPreyBehaviour));
                Messenger.RemoveListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
            }
        }
        #endregion

        public void SetTargetTile(HexTile hexTile) {
            targetTile = hexTile;
        }
        
        private void OnCharacterFinishedJobSuccessfully(Character character, GoapPlanJob goapPlanJob) {
            if (character == _owner && goapPlanJob.targetInteractionType == INTERACTION_TYPE.EAT_CORPSE) {
                character.traitContainer.RemoveTrait(character, this);
            }
        }
    }
}