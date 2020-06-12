using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class TortureRoom : StructureRoom {
        
        public Character currentTortureTarget { get; private set; }
        private Summon _skeleton;
        private AutoDestroyParticle _particleEffect;

        public TortureRoom(List<LocationGridTile> tilesInRoom) : base("Torture Chamber", tilesInRoom) { }
        
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(SPELL_TYPE.TORTURE);
        }

        #region Seize
        public bool CanUnseizeCharacterInRoom(Character character) {
            if (charactersInRoom.Count > 0) {
                return false;
            }
            return character.isNormalCharacter;
        }
        #endregion

        #region Torture
        public void BeginTorture() {
            List<Character> characters = charactersInRoom;
            Character chosenTarget = CollectionUtilities.GetRandomElement(characters);
            StartTorture(chosenTarget);
        }
        public bool HasValidTortureTarget() {
            List<Character> characters = charactersInRoom;
            for (int i = 0; i < characters.Count; i++) {
                Character character = characters[i];
                if (character.isNormalCharacter) {
                    return true;
                }
            }
            return false;
        }
        private void StartTorture(Character target) {
            currentTortureTarget = target;
            currentTortureTarget.interruptComponent.TriggerInterrupt(INTERRUPT.Being_Tortured, currentTortureTarget);
            Messenger.AddListener<INTERRUPT, Character>(Signals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
            Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
            LocationGridTile centerTile = GetCenterTile();
            _particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile.worldLocation, centerTile.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
        }
        private void StopTorture() {
            currentTortureTarget = null;
            Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        private void CheckIfTortureInterruptFinished(INTERRUPT interrupt, Character character) {
            if (character == currentTortureTarget && interrupt == INTERRUPT.Being_Tortured) {
                _particleEffect.StopEmission();
                _particleEffect = null;
                
                TortureChambers tortureChamber = parentStructure as TortureChambers;
                Assert.IsNotNull(tortureChamber, $"Parent structure of torture room is not torture chamber! {parentStructure?.ToString() ?? "Null"}");
                
                Messenger.RemoveListener<INTERRUPT, Character>(Signals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
                character.traitContainer.AddTrait(character, "Restrained");

                //open door
                DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
                door?.Open();
                
                _skeleton = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton,
                    FactionManager.Instance.friendlyNeutralFaction, null, character.currentRegion, className: "Archer");
                _skeleton.SetShowNotificationOnDeath(false);
                _skeleton.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
                _skeleton.SetDestroyMarkerOnDeath(true);
                _skeleton.ClearPlayerActions();

                // GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Zombie_Transformation);

                int modifiedX = tortureChamber.entrance.localPlace.x - 2;
                modifiedX = Mathf.Max(modifiedX, 0);
                LocationGridTile outsideTile = tortureChamber.location.innerMap.map[modifiedX, tortureChamber.entrance.localPlace.y];

                List<LocationGridTile> dropChoices = outsideTile
                    .GetTilesInRadius(7, includeCenterTile: true, includeTilesInDifferentStructure: false).Where(t =>
                         t.objHere == null && t.structure.structureType == STRUCTURE_TYPE.WILDERNESS).ToList(); //&& t.collectionOwner.partOfHextile != parentStructure.occupiedHexTile
                
                CharacterManager.Instance.PlaceSummon(_skeleton, CollectionUtilities.GetRandomElement(tilesInRoom));
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER,
                    INTERACTION_TYPE.DROP, character, _skeleton);
                job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {
                    _skeleton.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS), 
                    CollectionUtilities.GetRandomElement(dropChoices)
                });
                _skeleton.jobQueue.AddJobInQueue(job);
                
                Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            }
        }
        private void OnJobRemovedFromCharacter(JobQueueItem job, Character character) {
            if (character == _skeleton && job.jobType == JOB_TYPE.MOVE_CHARACTER) {
                Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
                //close door
                DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
                door?.Close();
                
                //kill skeleton
                // GameManager.Instance.CreateParticleEffectAt(_skeleton.gridTileLocation, PARTICLE_EFFECT.Zombie_Transformation);
                _skeleton.Death();
                currentTortureTarget.traitContainer.RemoveTrait(currentTortureTarget, "Restrained");
                
                currentTortureTarget.jobComponent.DisableReportStructure();
                
                _skeleton = null;
                StopTorture();
            }
        }
        #endregion
    }
}