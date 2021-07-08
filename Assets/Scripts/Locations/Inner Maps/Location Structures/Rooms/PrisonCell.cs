using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Traits;
using Tutorial;
namespace Inner_Maps.Location_Structures {
    public class PrisonCell : StructureRoom {
        
        public Character currentTortureTarget { get; private set; }
        public Character currentBrainwashTarget { get; private set; }
        // public Summon skeleton { get; private set; }
        private AutoDestroyParticle _particleEffect;

        public PrisonCell(List<LocationGridTile> tilesInRoom) : base("Prison Cell", tilesInRoom) {
            var worldLocation = GetCenterTile().centeredWorldLocation;
            worldLocation.x += 0.5f;
            worldPosition = worldLocation;
        }
        
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.TORTURE);
            AddPlayerAction(PLAYER_SKILL_TYPE.BRAINWASH);
        }

        #region Loading
        public override void LoadReferences(SaveDataStructureRoom saveDataStructureRoom) {
            SaveDataPrisonCell saveData = saveDataStructureRoom as SaveDataPrisonCell;
            if (!string.IsNullOrEmpty(saveData.tortureID)) {
                currentTortureTarget = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveData.tortureID);
            } else if (!string.IsNullOrEmpty(saveData.brainwashTargetID)) {
                currentBrainwashTarget = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveData.brainwashTargetID);
            }
            // if (!string.IsNullOrEmpty(saveData.skeletonID)) {
            //     skeleton = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveData.skeletonID) as Summon;
            // }
            // if (skeleton == null) {
            //     if (currentTortureTarget != null) {
            //         Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
            //         Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
            //         LocationGridTile centerTile = GetCenterTile();
            //         _particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile.worldLocation, centerTile.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
            //     } else if (currentBrainwashTarget != null) {
            //         Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
            //     }    
            // } else {
            //     //if skeleton is not null then, listen for drop job to be finished
            //     Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            // }
            if (currentTortureTarget != null) {
                Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
                    Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, parentStructure as IPlayerActionTarget);
                LocationGridTile centerTile = GetCenterTile();
                _particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile.worldLocation, centerTile.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
            } else if (currentBrainwashTarget != null) {
                Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
                LocationGridTile centerTile = GetCenterTile();
                _particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile.worldLocation, centerTile.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
            }
        }
        public override void LoadAdditionalReferences(SaveDataStructureRoom saveDataStructureRoom) {
            base.LoadAdditionalReferences(saveDataStructureRoom);
            // if (skeleton != null) {
            //     //if skeleton is not null then open door, just in case skeleton was saved inside room
            //     DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
            //     door?.Open();
            // }
        }
        #endregion
        
        #region Seize
        public override bool CanUnseizeCharacterInRoom(Character character) {
            if (HasAnyAliveCharacterInRoom()) {
                return false;
            }
            return IsValidOccupant(character);
        }
        public void OnHarpyDroppedCharacterHere(Character character) {
            if (IsValidOccupant(character)) {
                //automatically restrain and imprison dropped monsters
                //Reference: https://trello.com/c/AlvDm0U6/4251-kennel-and-prison-updates
                character.traitContainer.RestrainAndImprison(character, factionThatImprisoned: PlayerManager.Instance.player.playerFaction);
            }
        }
        public bool IsValidOccupant(Character character) {
            return character.isNormalCharacter && character.isDead == false && (character.faction == null || !character.faction.isPlayerFaction);
        }
        public bool HasValidOccupant() {
            for (int i = 0; i < tilesInRoom.Count; i++) {
                LocationGridTile t = tilesInRoom[i];
                for (int j = 0; j < t.charactersHere.Count; j++) {
                    Character c = t.charactersHere[j];
                    if (IsValidOccupant(c)) {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool HasOccupants() {
            for (int i = 0; i < parentStructure.charactersHere.Count; i++) {
                Character character = parentStructure.charactersHere[i];
                if (character.gridTileLocation != null && character.gridTileLocation.structure.IsTilePartOfARoom(character.gridTileLocation, out var room) && room == this) {
                    return true;
                }
            }
            // for (int i = 0; i < tilesInRoom.Count; i++) {
            //     LocationGridTile t = tilesInRoom[i];
            //     if (t.charactersHere.Count > 0) {
            //         return true;
            //     }
            // }
            return false;
        }
        public void PopulateValidOccupants(List<Character> p_characters) {
            for (int i = 0; i < tilesInRoom.Count; i++) {
                LocationGridTile t = tilesInRoom[i];
                for (int j = 0; j < t.charactersHere.Count; j++) {
                    Character c = t.charactersHere[j];
                    if (IsValidOccupant(c)) {
                        p_characters.Add(c);
                    }
                }
            }
        }
        #endregion

        #region Torture
        public void BeginTorture() {
            List<Character> characters = RuinarchListPool<Character>.Claim();
            for (int i = 0; i < tilesInRoom.Count; i++) {
                LocationGridTile t = tilesInRoom[i];
                for (int j = 0; j < t.charactersHere.Count; j++) {
                    Character c = t.charactersHere[j];
                    if (IsValidTortureTarget(c)) {
                        characters.Add(c);
                    }
                }
            }
            if (characters.Count > 0) {
                Character chosenTarget = CollectionUtilities.GetRandomElement(characters);
                StartTorture(chosenTarget);    
            }
            RuinarchListPool<Character>.Release(characters);
        }
        public void BeginTorture(Character p_character) {
            StartTorture(p_character);
        }
        public bool HasValidTortureTarget() {
            for (int i = 0; i < tilesInRoom.Count; i++) {
                LocationGridTile t = tilesInRoom[i];
                for (int j = 0; j < t.charactersHere.Count; j++) {
                    Character c = t.charactersHere[j];
                    if (IsValidTortureTarget(c)) {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IsValidTortureTarget(Character p_character) {
            return p_character.isNormalCharacter && !p_character.isDead && !p_character.traitContainer.HasTrait("Being Drained") && !p_character.interruptComponent.isInterrupted;;
        } 
        private void StartTorture(Character target) {
            currentTortureTarget = target;
            currentTortureTarget.interruptComponent.TriggerInterrupt(INTERRUPT.Being_Tortured, currentTortureTarget);
            Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, parentStructure as IPlayerActionTarget);
            LocationGridTile centerTile = GetCenterTile();
            _particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile.worldLocation, centerTile.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
        }
        private void StopTorture() {
            currentTortureTarget = null;
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, parentStructure as IPlayerActionTarget);
        }
        private void CheckIfTortureInterruptFinished(INTERRUPT interrupt, Character character) {
            if (character == currentTortureTarget && interrupt == INTERRUPT.Being_Tortured) {
                _particleEffect.StopEmission();
                _particleEffect = null;
                
                TortureChambers tortureChamber = parentStructure as TortureChambers;
                Assert.IsNotNull(tortureChamber, $"Parent structure of torture room is not torture chamber! {parentStructure?.ToString() ?? "Null"}");
                
                Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);

                character.traitContainer.RestrainAndImprison(character, null, PlayerManager.Instance.player.playerFaction);
                StopTorture();
                // //open door
                // DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
                // door?.Open();
                //
                // skeleton = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, FactionManager.Instance.vagrantFaction, null, character.currentRegion, className: "Archer");
                // skeleton.SetIsVolatile(true);
                // skeleton.SetShowNotificationOnDeath(false);
                // skeleton.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
                // skeleton.SetDestroyMarkerOnDeath(true);
                // skeleton.ClearPlayerActions();
                // skeleton.movementComponent.SetEnableDigging(true);

                // List<LocationGridTile> dropChoices = ObjectPoolManager.Instance.CreateNewGridTileList();
                // for (int i = 0; i < parentStructure.occupiedArea.gridTileComponent.gridTiles.Count; i++) {
                //     LocationGridTile tile = parentStructure.occupiedArea.gridTileComponent.gridTiles[i];
                //     if(tile.structure.structureType == STRUCTURE_TYPE.WILDERNESS) {
                //         dropChoices.Add(tile);
                //     }
                // }
                // LocationGridTile chosenDropTile = CollectionUtilities.GetRandomElement(dropChoices);
                // ObjectPoolManager.Instance.ReturnGridTileListToPool(dropChoices);
                //
                // CharacterManager.Instance.PlaceSummonInitially(skeleton, CollectionUtilities.GetRandomElement(tilesInRoom));
                // GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, character, skeleton);
                // job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {
                //     skeleton.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS), 
                //     chosenDropTile
                // });
                // skeleton.jobQueue.AddJobInQueue(job);
                //
                // Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            }
        }
        #endregion
        
        #region Brainwash
        private bool wasBrainwashStartedInTutorial;
        public bool WasBrainwashSuccessful(Character actor) {
            WeightedDictionary<bool> brainwashWeightedDictionary = new WeightedDictionary<bool>();

            int failWeight;
            int successWeight;
            
            if (wasBrainwashStartedInTutorial) {
                //if create a cultist tutorial is currently active then make sure that the brainwashing always succeeds
                failWeight = 0;
                successWeight = 100;
            } else {
                
                GetBrainwashSuccessAndFailWeights(actor, out successWeight, out failWeight);
                //failWeight = 0;
                //successWeight = 100;
            }

            // successWeight = 100;
            // failWeight = 0;
            
            brainwashWeightedDictionary.AddElement(true, successWeight);
            brainwashWeightedDictionary.AddElement(false, failWeight);

            brainwashWeightedDictionary.LogDictionaryValues($"{GameManager.Instance.TodayLogString()}{actor.name} brainwash weights:");
            
            return brainwashWeightedDictionary.PickRandomElementGivenWeights();
        }
        private static void GetBrainwashSuccessAndFailWeights(Character actor, out int successWeight, out int failWeight) {
            failWeight = 100;
            successWeight = 50;

            if (actor.moodComponent.moodState == MOOD_STATE.Normal) {
                if (actor.traitContainer.HasTrait("Evil")) {
                    successWeight += 100;
                }
                if (actor.traitContainer.HasTrait("Treacherous")) {
                    successWeight += 100;
                }
            } else if (actor.moodComponent.moodState == MOOD_STATE.Bad || actor.moodComponent.moodState == MOOD_STATE.Critical) {
                if (actor.moodComponent.moodState == MOOD_STATE.Bad) {
                    successWeight += 100;
                } else if (actor.moodComponent.moodState == MOOD_STATE.Critical) {
                    successWeight += 200;
                }

                if (actor.traitContainer.HasTrait("Evil")) {
                    successWeight += 150;
                }
                if (actor.traitContainer.HasTrait("Treacherous")) {
                    successWeight += 300;
                }
            }
            
            if (actor.traitContainer.HasTrait("Betrayed")) {
                successWeight += 100;
            }
            if (actor.isFactionLeader) {
                failWeight += 600;
            }
            if (actor.isSettlementRuler) {
                failWeight += 600;
            }

            if (actor.characterClass.className == "Hero" || actor.traitContainer.IsBlessed()) {
                successWeight = 0;
                failWeight = 100;
            }
        }
        public static float GetBrainwashSuccessRate(Character character) {
            GetBrainwashSuccessAndFailWeights(character, out int successWeight, out int failWeight);
            return ((float)successWeight / (successWeight + failWeight)) * 100f;
        }
        public bool HasValidBrainwashTarget() {
            for (int i = 0; i < tilesInRoom.Count; i++) {
                LocationGridTile t = tilesInRoom[i];
                for (int j = 0; j < t.charactersHere.Count; j++) {
                    Character c = t.charactersHere[j];
                    if (IsValidBrainwashTarget(c)) {
                        return true;
                    }
                }
            }
            //List<Character> characters = charactersInRoom;
            //for (int i = 0; i < characters.Count; i++) {
            //    Character character = characters[i];
            //    //if there is a normal character that is not dead and is not a cultist
            //    if (IsValidBrainwashTarget(character)) {
            //        return true;
            //    }
            //}
            return false;
        }
        public void PopulateBrainwashTargets(List<Character> p_characters) {
            for (int i = 0; i < tilesInRoom.Count; i++) {
                LocationGridTile t = tilesInRoom[i];
                for (int j = 0; j < t.charactersHere.Count; j++) {
                    Character c = t.charactersHere[j];
                    if (IsValidBrainwashTarget(c)) {
                        p_characters.Add(c);
                    }
                }
            }
        }
        public bool IsValidBrainwashTarget(Character p_character) {
            return p_character.isNormalCharacter && !p_character.isDead && !p_character.traitContainer.HasTrait("Cultist") && !p_character.traitContainer.IsBlessed() && 
                   !p_character.traitContainer.HasTrait("Being Drained") && !p_character.interruptComponent.isInterrupted;
        } 
        public void StartBrainwash() {
            wasBrainwashStartedInTutorial = TutorialManager.Instance.IsTutorialCurrentlyActive(TutorialManager.Tutorial.Create_A_Cultist);
            DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
            door?.Close();
            Character chosenTarget = null;
            List<Character> targets = RuinarchListPool<Character>.Claim();
            PopulateBrainwashTargets(targets);
            chosenTarget = CollectionUtilities.GetRandomElement(targets);
            RuinarchListPool<Character>.Release(targets);
            //CollectionUtilities.GetRandomElement(charactersInRoom.Where(IsValidBrainwashTarget));
            currentBrainwashTarget = chosenTarget;
            currentBrainwashTarget.interruptComponent.ForceEndNonSimultaneousInterrupt();
            currentBrainwashTarget.interruptComponent.TriggerInterrupt(INTERRUPT.Being_Brainwashed, currentBrainwashTarget);
            Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, parentStructure as IPlayerActionTarget);
            LocationGridTile centerTile = GetCenterTile();
            _particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile.worldLocation, centerTile.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
        }
        public void StartBrainwash(Character p_target) {
            wasBrainwashStartedInTutorial = TutorialManager.Instance.IsTutorialCurrentlyActive(TutorialManager.Tutorial.Create_A_Cultist);
            DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
            door?.Close();
            currentBrainwashTarget = p_target;
            currentBrainwashTarget.interruptComponent.ForceEndNonSimultaneousInterrupt();
            currentBrainwashTarget.interruptComponent.TriggerInterrupt(INTERRUPT.Being_Brainwashed, currentBrainwashTarget);
            Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, parentStructure as IPlayerActionTarget);
            LocationGridTile centerTile = GetCenterTile();
            _particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile.worldLocation, centerTile.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
        }
        private void BrainwashDone() {
            currentBrainwashTarget = null;
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, parentStructure as IPlayerActionTarget);
        }
        private void CheckIfBrainwashFinished(INTERRUPT interrupt, Character chosenTarget) {
            if (interrupt == INTERRUPT.Being_Brainwashed && chosenTarget == currentBrainwashTarget) {
                Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
            
                _particleEffect.StopEmission();
                _particleEffect = null;
                
                // DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
                // door?.Open();

                if (chosenTarget.traitContainer.HasTrait("Cultist")) {
                    //successfully converted
                    // GameDate dueDate = GameManager.Instance.Today();
                    // dueDate.AddTicks(1);
                    // SchedulingManager.Instance.AddEntry(dueDate, () => chosenTarget.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME), chosenTarget);
                    // chosenTarget.traitContainer.RemoveRestrainAndImprison(chosenTarget);
                    chosenTarget.traitContainer.RemoveTrait(chosenTarget, "Unconscious");
                } else {
                    chosenTarget.traitContainer.RestrainAndImprison(chosenTarget, null, PlayerManager.Instance.player.playerFaction);
                    // //spawn skeleton to carry target
                    // skeleton = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, FactionManager.Instance.vagrantFaction, null, chosenTarget.currentRegion, className: "Archer");
                    // skeleton.SetIsVolatile(true);
                    // skeleton.SetShowNotificationOnDeath(false);
                    // skeleton.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
                    // skeleton.SetDestroyMarkerOnDeath(true);
                    // skeleton.ClearPlayerActions();
                    // skeleton.movementComponent.SetEnableDigging(true);

                    // List<LocationGridTile> dropChoices = ObjectPoolManager.Instance.CreateNewGridTileList();
                    // for (int i = 0; i < parentStructure.occupiedArea.gridTileComponent.gridTiles.Count; i++) {
                    //     LocationGridTile tile = parentStructure.occupiedArea.gridTileComponent.gridTiles[i];
                    //     if (tile.structure.structureType == STRUCTURE_TYPE.WILDERNESS) {
                    //         dropChoices.Add(tile);
                    //     }
                    // }
                    // LocationGridTile chosenDropTile = CollectionUtilities.GetRandomElement(dropChoices);
                    // ObjectPoolManager.Instance.ReturnGridTileListToPool(dropChoices);
                    //
                    // CharacterManager.Instance.PlaceSummonInitially(skeleton, CollectionUtilities.GetRandomElement(tilesInRoom));
                    // GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, chosenTarget, skeleton);
                    // job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {
                    //     skeleton.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS),
                    //     chosenDropTile
                    // });
                    // job.SetCannotBePushedBack(true);
                    // skeleton.jobQueue.AddJobInQueue(job);
                    
                    // Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
                }
                BrainwashDone();
            }
        }
        #endregion

        // #region Shared Functions
        // private void OnJobRemovedFromCharacter(JobQueueItem job, Character character) {
        //     if (character == skeleton && job.jobType == JOB_TYPE.MOVE_CHARACTER) {
        //         Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
        //         //close door
        //         DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
        //         door?.Close();
        //         
        //         //kill skeleton
        //         LocationStructure deathLocation = skeleton.currentStructure;
        //         skeleton.Death();
        //         deathLocation?.RemoveCharacterAtLocation(skeleton);
        //
        //         bool isTorture = currentTortureTarget != null;
        //         var targetCharacter = isTorture ? currentTortureTarget : currentBrainwashTarget;
        //         
        //         targetCharacter.traitContainer.RemoveRestrainAndImprison(targetCharacter);
        //         targetCharacter.jobComponent.DisableReportStructure();
        //         if (!targetCharacter.traitContainer.HasTrait("Paralyzed")) {
        //             //No need to daze paralyzed characters, because we expect that characters than cannot perform should not be dazed.
        //             targetCharacter.traitContainer.AddTrait(targetCharacter, "Dazed");    
        //         }
        //         
        //         
        //         skeleton = null;
        //         if (isTorture) {
        //             StopTorture();    
        //         } else {
        //             BrainwashDone();    
        //         }
        //     }
        // }
        // #endregion

        #region Destruction
        public override void OnParentStructureDestroyed() {
            base.OnParentStructureDestroyed();
            Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
            if (currentTortureTarget != null && currentTortureTarget.interruptComponent.isInterrupted && 
                currentTortureTarget.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured) {
                currentTortureTarget.interruptComponent.ForceEndNonSimultaneousInterrupt();
            }
            if (_particleEffect != null) {
                _particleEffect.StopEmission();
                _particleEffect = null;
            }
            
            Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
            if (currentBrainwashTarget != null && currentBrainwashTarget.interruptComponent.isInterrupted && 
                currentBrainwashTarget.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed) {
                currentBrainwashTarget.interruptComponent.ForceEndNonSimultaneousInterrupt();
            }
        }
        #endregion

        #region Selectable
        public override bool CanBeSelected() {
            return false;
        }
        #endregion
    }
}

#region Save Data
public class SaveDataPrisonCell : SaveDataStructureRoom {
    public string tortureID;
    public string brainwashTargetID;
    // public string skeletonID;
    public override void Save(StructureRoom data) {
        base.Save(data);
        PrisonCell prisonCell = data as PrisonCell;
        tortureID = prisonCell.currentTortureTarget == null ? string.Empty : prisonCell.currentTortureTarget.persistentID;
        brainwashTargetID = prisonCell.currentBrainwashTarget == null ? string.Empty : prisonCell.currentBrainwashTarget.persistentID;
        // skeletonID = prisonCell.skeleton == null ? string.Empty : prisonCell.skeleton.persistentID;
    }
}
#endregion