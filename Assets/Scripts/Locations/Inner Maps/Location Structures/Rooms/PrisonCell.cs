using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Traits;
namespace Inner_Maps.Location_Structures {
    public class PrisonCell : StructureRoom {
        
        public Character currentTortureTarget { get; private set; }
        public Summon skeleton { get; private set; }
        private AutoDestroyParticle _particleEffect;

        public PrisonCell(List<LocationGridTile> tilesInRoom) : base("Prison Cell", tilesInRoom) { }
        
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.TORTURE);
        }

        #region Loading
        public override void LoadReferences(SaveDataStructureRoom saveDataStructureRoom) {
            SaveDataPrisonCell saveData = saveDataStructureRoom as SaveDataPrisonCell;
            if (!string.IsNullOrEmpty(saveData.tortureID)) {
                currentTortureTarget = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveData.tortureID);
            }
            if (!string.IsNullOrEmpty(saveData.skeletonID)) {
                skeleton = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveData.skeletonID) as Summon;
            }
            if (currentTortureTarget != null && skeleton == null) {
                Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
                Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
                LocationGridTile centerTile = GetCenterTile();
                _particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile.worldLocation, centerTile.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
            }
            if (skeleton != null) {
                //if skeleton is not null then, listen for drop job to be finished
                Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            }
        }
        public override void LoadAdditionalReferences(SaveDataStructureRoom saveDataStructureRoom) {
            base.LoadAdditionalReferences(saveDataStructureRoom);
            if (skeleton != null) {
                //if skeleton is not null then open door, just in case skeleton was saved inside room
                DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
                door?.Open();
            }
        }
        #endregion
        
        #region Seize
        public override bool CanUnseizeCharacterInRoom(Character character) {
            if (HasAnyAliveCharacterInRoom()) {
                return false;
            }
            return character.isNormalCharacter && character.isDead == false;
        }
        #endregion

        #region Torture
        public void BeginTorture() {
            List<Character> characters = charactersInRoom;
            Character chosenTarget = CollectionUtilities.GetRandomElement(characters);
            StartTorture(chosenTarget);
        }
        public void BeginTorture(Character p_character) {
            StartTorture(p_character);
        }
        public bool HasValidTortureTarget() {
            List<Character> characters = charactersInRoom;
            for (int i = 0; i < characters.Count; i++) {
                Character character = characters[i];
                if (IsValidTortureTarget(character)) {
                    return true;
                }
            }
            return false;
        }
        public bool IsValidTortureTarget(Character p_character) {
            return p_character.isNormalCharacter && p_character.isDead == false;
        } 
        private void StartTorture(Character target) {
            currentTortureTarget = target;
            currentTortureTarget.interruptComponent.TriggerInterrupt(INTERRUPT.Being_Tortured, currentTortureTarget);
            Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
            Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
            LocationGridTile centerTile = GetCenterTile();
            _particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile.worldLocation, centerTile.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
        }
        private void StopTorture() {
            currentTortureTarget = null;
            Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        private void CheckIfTortureInterruptFinished(INTERRUPT interrupt, Character character) {
            if (character == currentTortureTarget && interrupt == INTERRUPT.Being_Tortured) {
                _particleEffect.StopEmission();
                _particleEffect = null;
                
                TortureChambers tortureChamber = parentStructure as TortureChambers;
                Assert.IsNotNull(tortureChamber, $"Parent structure of torture room is not torture chamber! {parentStructure?.ToString() ?? "Null"}");
                
                Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);

                character.traitContainer.RestrainAndImprison(character, null, PlayerManager.Instance.player.playerFaction);
                //character.traitContainer.AddTrait(character, "Restrained");
                //Prisoner prisonerTrait = character.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                //if (prisonerTrait != null) {
                //    prisonerTrait.SetPrisonerOfFaction(PlayerManager.Instance.player.playerFaction);
                //}
                //open door
                DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
                door?.Open();
                
                skeleton = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, FactionManager.Instance.vagrantFaction, null, character.currentRegion, className: "Archer");
                skeleton.SetIsVolatile(true);
                skeleton.SetShowNotificationOnDeath(false);
                skeleton.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
                skeleton.SetDestroyMarkerOnDeath(true);
                skeleton.ClearPlayerActions();
                
                int modifiedX = tortureChamber.entrance.localPlace.x - 2;
                modifiedX = Mathf.Max(modifiedX, 0);
                LocationGridTile outsideTile = tortureChamber.region.innerMap.map[modifiedX, tortureChamber.entrance.localPlace.y];

                List<LocationGridTile> dropChoices = outsideTile
                    .GetTilesInRadius(7, includeCenterTile: true, includeTilesInDifferentStructure: false).Where(t =>
                         t.objHere == null && t.structure.structureType == STRUCTURE_TYPE.WILDERNESS).ToList(); //&& t.collectionOwner.partOfHextile != parentStructure.occupiedHexTile
                
                CharacterManager.Instance.PlaceSummon(skeleton, CollectionUtilities.GetRandomElement(tilesInRoom));
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, character, skeleton);
                job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {
                    skeleton.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS), 
                    CollectionUtilities.GetRandomElement(dropChoices)
                });
                skeleton.jobQueue.AddJobInQueue(job);
                
                Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            }
        }
        private void OnJobRemovedFromCharacter(JobQueueItem job, Character character) {
            if (character == skeleton && job.jobType == JOB_TYPE.MOVE_CHARACTER) {
                Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
                //close door
                DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
                door?.Close();
                
                //kill skeleton
                // GameManager.Instance.CreateParticleEffectAt(_skeleton.gridTileLocation, PARTICLE_EFFECT.Zombie_Transformation);
                LocationStructure deathLocation = skeleton.currentStructure;
                skeleton.Death();
                deathLocation?.RemoveCharacterAtLocation(skeleton);
                currentTortureTarget.traitContainer.RemoveRestrainAndImprison(currentTortureTarget);
                currentTortureTarget.jobComponent.DisableReportStructure();
                if (!currentTortureTarget.traitContainer.HasTrait("Paralyzed")) {
                    //No need to daze paralyzed characters, because we expect that characters than cannot perform should not be dazed.
                    currentTortureTarget.traitContainer.AddTrait(currentTortureTarget, "Dazed");    
                }
                
                
                skeleton = null;
                StopTorture();
            }
        }
        #endregion
        
        #region Destruction
        public override void OnParentStructureDestroyed() {
            base.OnParentStructureDestroyed();
            Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
            if (currentTortureTarget != null && currentTortureTarget.interruptComponent.isInterrupted && 
                currentTortureTarget.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured) {
                currentTortureTarget.interruptComponent.ForceEndNonSimultaneousInterrupt();
                currentTortureTarget.traitContainer.RemoveRestrainAndImprison(currentTortureTarget);
            }
            if (_particleEffect != null) {
                _particleEffect.StopEmission();
                _particleEffect = null;
            }
        }
        #endregion
    }
}

#region Save Data
public class SaveDataPrisonCell : SaveDataStructureRoom {
    public string tortureID;
    public string skeletonID;
    public override void Save(StructureRoom data) {
        base.Save(data);
        PrisonCell prisonCell = data as PrisonCell;
        tortureID = prisonCell.currentTortureTarget == null ? string.Empty : prisonCell.currentTortureTarget.persistentID;
        skeletonID = prisonCell.skeleton == null ? string.Empty : prisonCell.skeleton.persistentID;
    }
}
#endregion