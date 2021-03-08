// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Inner_Maps.Location_Structures;
// using Tutorial;
// using UnityEngine;
// using UtilityScripts;
// using Traits;
// namespace Inner_Maps.Location_Structures {
//     public class DefilerRoom : StructureRoom {
//         
//         public Character currentBrainwashTarget { get; private set; }
//         
//         public Summon skeleton { get; private set; }
//
//         public DefilerRoom(List<LocationGridTile> tilesInRoom) : base("Defiler Room", tilesInRoom) {
//             var worldLocation = GetCenterTile().centeredWorldLocation;
//             worldLocation.x += 0.5f;
//             worldPosition = worldLocation;
//         }
//
//         #region Loading
//         public override void LoadReferences(SaveDataStructureRoom saveDataStructureRoom) {
//             SaveDataDefilerRoom saveDataDefilerRoom = saveDataStructureRoom as SaveDataDefilerRoom;
//             if (!string.IsNullOrEmpty(saveDataDefilerRoom.brainwashTargetID)) {
//                 currentBrainwashTarget = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataDefilerRoom.brainwashTargetID);
//             }
//             if (!string.IsNullOrEmpty(saveDataDefilerRoom.skeletonID)) {
//                 skeleton = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataDefilerRoom.skeletonID) as Summon;
//             }
//             if (currentBrainwashTarget != null && skeleton == null) {
//                 Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
//             }
//             if (skeleton != null) {
//                 //if skeleton is not null then, listen for drop job to be finished
//                 Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
//             }
//         }
//         public override void LoadAdditionalReferences(SaveDataStructureRoom saveDataStructureRoom) {
//             base.LoadAdditionalReferences(saveDataStructureRoom);
//             if (skeleton != null) {
//                 //if skeleton is not null then open door, just in case skeleton was saved inside room
//                 DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
//                 door?.Open();
//             }
//         }
//         #endregion
//         
//         #region Overrides
//         public override void ConstructDefaultActions() {
//             base.ConstructDefaultActions();
//             AddPlayerAction(PLAYER_SKILL_TYPE.BRAINWASH);
//         }
//         #endregion
//         
//         #region Seize
//         public override bool CanUnseizeCharacterInRoom(Character character) {
//             if (HasAnyAliveCharacterInRoom()) {
//                 return false;
//             }
//             return character.isNormalCharacter && character.isDead == false && !character.traitContainer.HasTrait("Cultist");
//         }
//         #endregion
//
//         #region Brainwash
//         private bool wasBrainwashStartedInTutorial;
//         public bool WasBrainwashSuccessful(Character actor) {
//             WeightedDictionary<bool> brainwashWeightedDictionary = new WeightedDictionary<bool>();
//
//             int failWeight;
//             int successWeight;
//             
//             if (wasBrainwashStartedInTutorial) {
//                 //if create a cultist tutorial is currently active then make sure that the brainwashing always succeeds
//                 failWeight = 0;
//                 successWeight = 100;
//             } else {
//                 GetBrainwashSuccessAndFailWeights(actor, out successWeight, out failWeight);
//             }
//
//             // successWeight = 100;
//             // failWeight = 0;
//             
//             brainwashWeightedDictionary.AddElement(true, successWeight);
//             brainwashWeightedDictionary.AddElement(false, failWeight);
//
//             brainwashWeightedDictionary.LogDictionaryValues($"{GameManager.Instance.TodayLogString()}{actor.name} brainwash weights:");
//             
//             return brainwashWeightedDictionary.PickRandomElementGivenWeights();
//         }
//         private static void GetBrainwashSuccessAndFailWeights(Character actor, out int successWeight, out int failWeight) {
//             failWeight = 100;
//             successWeight = 20;
//
//             if (actor.moodComponent.moodState == MOOD_STATE.Normal) {
//                 if (actor.traitContainer.HasTrait("Evil")) {
//                     successWeight += 100;
//                 }
//                 if (actor.traitContainer.HasTrait("Treacherous")) {
//                     successWeight += 100;
//                 }
//             } else if (actor.moodComponent.moodState == MOOD_STATE.Bad || actor.moodComponent.moodState == MOOD_STATE.Critical) {
//                 if (actor.moodComponent.moodState == MOOD_STATE.Bad) {
//                     successWeight += 100;
//                 } else if (actor.moodComponent.moodState == MOOD_STATE.Critical) {
//                     successWeight += 200;
//                 }
//
//                 if (actor.traitContainer.HasTrait("Evil")) {
//                     successWeight += 150;
//                 }
//                 if (actor.traitContainer.HasTrait("Treacherous")) {
//                     successWeight += 300;
//                 }
//             }
//             
//             if (actor.traitContainer.HasTrait("Betrayed")) {
//                 successWeight += 100;
//             }
//             if (actor.isFactionLeader) {
//                 failWeight += 600;
//             }
//             if (actor.isSettlementRuler) {
//                 failWeight += 600;
//             }
//
//             if (actor.characterClass.className == "Hero" || actor.traitContainer.IsBlessed()) {
//                 successWeight = 0;
//                 failWeight = 100;
//             }
//         }
//         public static float GetBrainwashSuccessRate(Character character) {
//             GetBrainwashSuccessAndFailWeights(character, out int successWeight, out int failWeight);
//             return ((float)successWeight / (successWeight + failWeight)) * 100f;
//         }
//         public bool HasValidBrainwashTarget() {
//             List<Character> characters = charactersInRoom;
//             for (int i = 0; i < characters.Count; i++) {
//                 Character character = characters[i];
//                 //if there is a normal character that is not dead and is not a cultist
//                 if (IsValidBrainwashTarget(character)) {
//                     return true;
//                 }
//             }
//             return false;
//         }
//         public bool IsValidBrainwashTarget(Character character) {
//             return character.isNormalCharacter && !character.isDead && !character.traitContainer.HasTrait("Cultist") && !character.traitContainer.IsBlessed();
//         } 
//         public void StartBrainwash() {
//             wasBrainwashStartedInTutorial = TutorialManager.Instance.IsTutorialCurrentlyActive(TutorialManager.Tutorial.Create_A_Cultist);
//             DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
//             door?.Close();
//             Character chosenTarget = CollectionUtilities.GetRandomElement(charactersInRoom.Where(x => IsValidBrainwashTarget(x)));
//             currentBrainwashTarget = chosenTarget;
//             currentBrainwashTarget.interruptComponent.ForceEndNonSimultaneousInterrupt();
//             currentBrainwashTarget.interruptComponent.TriggerInterrupt(INTERRUPT.Being_Brainwashed, currentBrainwashTarget);
//             Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
//             Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
//         }
//         public void StartBrainwash(Character p_target) {
//             wasBrainwashStartedInTutorial = TutorialManager.Instance.IsTutorialCurrentlyActive(TutorialManager.Tutorial.Create_A_Cultist);
//             DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
//             door?.Close();
//             currentBrainwashTarget = p_target;
//             currentBrainwashTarget.interruptComponent.ForceEndNonSimultaneousInterrupt();
//             currentBrainwashTarget.interruptComponent.TriggerInterrupt(INTERRUPT.Being_Brainwashed, currentBrainwashTarget);
//             Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
//             Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
//         }
//         private void BrainwashDone() {
//             currentBrainwashTarget = null;
//             Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
//         }
//         private void CheckIfBrainwashFinished(INTERRUPT interrupt, Character chosenTarget) {
//             if (interrupt == INTERRUPT.Being_Brainwashed && chosenTarget == currentBrainwashTarget) {
//                 Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
//             
//                 DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
//                 door?.Open();
//
//                 if (chosenTarget.traitContainer.HasTrait("Cultist")) {
//                     //successfully converted
//                     GameDate dueDate = GameManager.Instance.Today();
//                     dueDate.AddTicks(1);
//                     SchedulingManager.Instance.AddEntry(dueDate, () => chosenTarget.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME), chosenTarget);
//                     chosenTarget.traitContainer.RemoveRestrainAndImprison(chosenTarget);
//                     chosenTarget.traitContainer.RemoveTrait(chosenTarget, "Unconscious");
//                     BrainwashDone();
//                 } else {
//                     chosenTarget.traitContainer.RestrainAndImprison(chosenTarget, null, PlayerManager.Instance.player.playerFaction);
//
//                     //chosenTarget.traitContainer.AddTrait(chosenTarget, "Restrained");
//                     //Prisoner prisonerTrait = chosenTarget.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner"); 
//                     //if(prisonerTrait != null) {
//                     //    prisonerTrait.SetPrisonerOfFaction(PlayerManager.Instance.player.playerFaction);
//                     //}
//                     //spawn skeleton to carry target
//                     skeleton = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, FactionManager.Instance.vagrantFaction, null, chosenTarget.currentRegion, className: "Archer");
//                     skeleton.SetIsVolatile(true);
//                     skeleton.SetShowNotificationOnDeath(false);
//                     skeleton.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
//                     skeleton.SetDestroyMarkerOnDeath(true);
//                     skeleton.ClearPlayerActions();
//                     skeleton.movementComponent.SetEnableDigging(true);
//
//                     List<LocationGridTile> dropChoices = ObjectPoolManager.Instance.CreateNewGridTileList();
//                     for (int i = 0; i < parentStructure.occupiedArea.gridTileComponent.gridTiles.Count; i++) {
//                         LocationGridTile tile = parentStructure.occupiedArea.gridTileComponent.gridTiles[i];
//                         if (tile.structure.structureType == STRUCTURE_TYPE.WILDERNESS) {
//                             dropChoices.Add(tile);
//                         }
//                     }
//                     LocationGridTile chosenDropTile = CollectionUtilities.GetRandomElement(dropChoices);
//                     ObjectPoolManager.Instance.ReturnGridTileListToPool(dropChoices);
//
//                     CharacterManager.Instance.PlaceSummonInitially(skeleton, CollectionUtilities.GetRandomElement(tilesInRoom));
//                     GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, chosenTarget, skeleton);
//                     job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {
//                         skeleton.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS),
//                         chosenDropTile
//                     });
//                     job.SetCannotBePushedBack(true);
//                     skeleton.jobQueue.AddJobInQueue(job);
//                     
//                     Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
//                 }                
//             }
//         }
//         private void OnJobRemovedFromCharacter(JobQueueItem job, Character character) {
//             if (character == skeleton && job.jobType == JOB_TYPE.MOVE_CHARACTER) {
//                 Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
//                 //close door
//                 DoorTileObject door = GetTileObjectInRoom<DoorTileObject>();
//                 door?.Close();
//                 
//                 //kill skeleton
//                 LocationStructure deathLocation = skeleton.currentStructure;
//                 skeleton.Death();
//                 deathLocation?.RemoveCharacterAtLocation(skeleton);
//                 currentBrainwashTarget.traitContainer.RemoveRestrainAndImprison(currentBrainwashTarget);
//                 currentBrainwashTarget.jobComponent.DisableReportStructure();
//                 if (!currentBrainwashTarget.traitContainer.HasTrait("Paralyzed")) {
//                     //No need to daze paralyzed characters, because we expect that characters than cannot perform should not be dazed.
//                     currentBrainwashTarget.traitContainer.AddTrait(currentBrainwashTarget, "Dazed");    
//                 }
//                 
//                 skeleton = null;
//                 BrainwashDone();
//             }
//         }
//         #endregion
//         
//         #region Destruction
//         public override void OnParentStructureDestroyed() {
//             base.OnParentStructureDestroyed();
//             Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
//             if (currentBrainwashTarget != null && currentBrainwashTarget.interruptComponent.isInterrupted && 
//                 currentBrainwashTarget.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed) {
//                 currentBrainwashTarget.interruptComponent.ForceEndNonSimultaneousInterrupt();
//             }
//         }
//         #endregion
//     }
// }
//
// #region Save Data
// public class SaveDataDefilerRoom : SaveDataStructureRoom {
//     public string brainwashTargetID;
//     public string skeletonID;
//     public override void Save(StructureRoom data) {
//         base.Save(data);
//         DefilerRoom defilerRoom = data as DefilerRoom;
//         brainwashTargetID = defilerRoom.currentBrainwashTarget == null ? string.Empty : defilerRoom.currentBrainwashTarget.persistentID;
//         skeletonID = defilerRoom.skeleton == null ? string.Empty : defilerRoom.skeleton.persistentID;
//     }
// }
// #endregion