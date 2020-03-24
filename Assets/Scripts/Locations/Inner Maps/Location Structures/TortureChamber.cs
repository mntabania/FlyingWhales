using System.Linq;
using Actionables;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
namespace Inner_Maps.Location_Structures {
    public class TortureChamber : DemonicStructure {
        public override Vector2 selectableSize { get; }
        private Character _currentTortureTarget;
        private TortureChamberStructureObject _tortureChamberStructureObject;
        
        public TortureChamber(Region location) : base(STRUCTURE_TYPE.TORTURE_CHAMBER, location){
            selectableSize = new Vector2(10f, 10f);
        }

        #region Initialization
        public override void Initialize() {
            base.Initialize();
            AddTortureAction();
        }
        #endregion
        
        #region Listeners
        protected override void SubscribeListeners() {
            base.SubscribeListeners();
            Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
        }
        protected override void UnsubscribeListeners() {
            base.UnsubscribeListeners();
            Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
        }
        #endregion

        #region Structure Object
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            _tortureChamberStructureObject = structureObj as TortureChamberStructureObject;
        }
        #endregion

        #region Torture
        private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
            if (structure == this && character.IsNPC()) {
                character.trapStructure.SetForcedStructure(this);
                character.DecreaseCanTakeJobs();
            }
        }
        private void OnCharacterLeftStructure(Character character, LocationStructure structure) {
            if (structure == this && character.IsNPC()) {
                character.trapStructure.SetForcedStructure(null);
                character.IncreaseCanTakeJobs();
            }
        }
        private void AddTortureAction() {
            PlayerAction tortureAction = new PlayerAction(PlayerDB.Torture_Action, () => _currentTortureTarget == null, null, ChooseTortureTarget);
            AddPlayerAction(tortureAction);
        }
        private void ChooseTortureTarget() {
            UIManager.Instance.ShowClickableObjectPicker(charactersHere, StartTorture, null, CanTorture, "Choose Torture Target", showCover: true);
        }
        private bool CanTorture(Character character) {
            return character.IsNPC();
        }
        private void StartTorture(object character) {
            Character target = character as Character;
            _currentTortureTarget = target;
            TileObject ironMaiden = GetTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.IRON_MAIDEN);
            Assert.IsNotNull(ironMaiden, $"Trying to activate torture for {target.name} but there was no iron maiden available!");
            target.marker.GoToPOI(ironMaiden, OnArriveAtTortureLocation);
            UIManager.Instance.HideObjectPicker();
            Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        private void OnArriveAtTortureLocation() {
            _currentTortureTarget.interruptComponent.TriggerInterrupt(INTERRUPT.Being_Tortured, _currentTortureTarget);
            Messenger.AddListener<INTERRUPT, Character>(Signals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
            _tortureChamberStructureObject.ActivateShroudParticles();
        }
        private void StopTorture() {
            _currentTortureTarget = null;
            _tortureChamberStructureObject.DeactivateShroudParticles();
        }
        private void CheckIfTortureInterruptFinished(INTERRUPT interrupt, Character character) {
            if (character == _currentTortureTarget && interrupt == INTERRUPT.Being_Tortured) {
                Messenger.RemoveListener<INTERRUPT, Character>(Signals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
                StopTorture();
            }
        }
        #endregion
    }
}