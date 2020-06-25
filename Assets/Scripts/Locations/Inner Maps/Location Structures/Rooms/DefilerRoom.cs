using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class DefilerRoom : StructureRoom {
        
        public Character currentBrainwashTarget { get; private set; }
        
        private Summon _skeleton;

        public DefilerRoom(List<LocationGridTile> tilesInRoom) : base("Defiler Room", tilesInRoom) { }

        #region Overrides
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(SPELL_TYPE.BRAINWASH);
        }
        #endregion
        
        #region Seize
        public override bool CanUnseizeCharacterInRoom(Character character) {
            if (charactersInRoom.Count > 0) {
                return false;
            }
            return character.isNormalCharacter && character.isDead == false;
        }
        #endregion

        #region Brainwash
        public bool HasValidBrainwashTarget() {
            List<Character> characters = charactersInRoom;
            for (int i = 0; i < characters.Count; i++) {
                Character character = characters[i];
                //if there is a normal character that is not dead and is not a cultist
                if (IsValidBrainwashTarget(character)) {
                    return true;
                }
            }
            return false;
        }
        private bool IsValidBrainwashTarget(Character character) {
            return character.isNormalCharacter && character.isDead == false
                    && character.traitContainer.HasTrait("Cultist") == false;
        } 
        public void StartBrainwash() {
            DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
            door?.Close();
            Character chosenTarget = CollectionUtilities.GetRandomElement(charactersInRoom.Where(x => IsValidBrainwashTarget(x)));
            currentBrainwashTarget = chosenTarget;
            currentBrainwashTarget.interruptComponent.TriggerInterrupt(INTERRUPT.Being_Brainwashed, currentBrainwashTarget);
            Messenger.AddListener<INTERRUPT, Character>(Signals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
            Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        private void BrainwashDone() {
            currentBrainwashTarget = null;
            Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        private void CheckIfBrainwashFinished(INTERRUPT interrupt, Character chosenTarget) {
            if (interrupt == INTERRUPT.Being_Brainwashed && chosenTarget == currentBrainwashTarget) {
                Messenger.RemoveListener<INTERRUPT, Character>(Signals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
            
                DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
                door?.Open();

                if (chosenTarget.traitContainer.HasTrait("Cultist")) {
                    //successfully converted
                    GameDate dueDate = GameManager.Instance.Today();
                    dueDate.AddTicks(1);
                    SchedulingManager.Instance.AddEntry(dueDate, () => chosenTarget.jobComponent.PlanIdleReturnHome(), chosenTarget);
                    BrainwashDone();
                } else {
                    chosenTarget.traitContainer.AddTrait(chosenTarget, "Restrained");
                    //spawn skeleton to carry target
                    _skeleton = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton,
                        FactionManager.Instance.friendlyNeutralFaction, null, chosenTarget.currentRegion, className: "Archer");
                    _skeleton.SetShowNotificationOnDeath(false);
                    _skeleton.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
                    _skeleton.SetDestroyMarkerOnDeath(true);
                    _skeleton.ClearPlayerActions();

                    List<LocationGridTile> dropChoices = parentStructure.occupiedHexTile.hexTileOwner.locationGridTiles.Where(t => 
                        t.structure.structureType == STRUCTURE_TYPE.WILDERNESS).ToList();
                    
                    CharacterManager.Instance.PlaceSummon(_skeleton, CollectionUtilities.GetRandomElement(tilesInRoom));
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER,
                        INTERACTION_TYPE.DROP, chosenTarget, _skeleton);
                    job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {
                        _skeleton.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS), 
                        CollectionUtilities.GetRandomElement(dropChoices)
                    });
                    job.SetCannotBePushedBack(true);
                    _skeleton.jobQueue.AddJobInQueue(job);
                    
                    Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
                }                
            }
        }
        private void OnJobRemovedFromCharacter(JobQueueItem job, Character character) {
            if (character == _skeleton && job.jobType == JOB_TYPE.MOVE_CHARACTER) {
                Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
                //close door
                DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
                door?.Close();
                
                //kill skeleton
                _skeleton.Death();
                currentBrainwashTarget.traitContainer.RemoveTrait(currentBrainwashTarget, "Restrained");
                
                currentBrainwashTarget.jobComponent.DisableReportStructure();
                
                _skeleton = null;
                BrainwashDone();
            }
        }
        #endregion
    }
}