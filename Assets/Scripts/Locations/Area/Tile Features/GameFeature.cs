using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts; //using UnityEditor.VersionControl;
using Random = UnityEngine.Random;

namespace Locations.Area_Features {
    public class GameFeature : AreaFeature {

        private const int MaxAnimals = 6;

        private Area owner;
        public bool isGeneratingPerHour { get; private set; }
        public SUMMON_TYPE animalTypeBeingSpawned { get; private set; }
        public List<Animal> ownedAnimals { get; private set; } //list of animals that have been spawned by this feature
        public override Type serializedData => typeof(SaveDataGameFeature);
        
        public static readonly SUMMON_TYPE[] spawnChoices = new[] {
            SUMMON_TYPE.Pig,
            SUMMON_TYPE.Sheep,
            SUMMON_TYPE.Chicken,
        }; 
    
        public GameFeature() {
            name = "Game";
            description = "Hunters can obtain food here.";
            ownedAnimals = new List<Animal>();
            SetSpawnType(CollectionUtilities.GetRandomElement(spawnChoices));
        }

        #region Overrides
        public override void GameStartActions(Area p_area) {
            owner = p_area;
            List<Character> animals = RuinarchListPool<Character>.Claim();
            p_area.locationCharacterTracker.PopulateAnimalsListInsideHex(animals, includeBeingSeized: true);
            if (animals != null) {
                for (int i = 0; i < animals.Count; i++) {
                    Animal animal = animals[i] as Animal;
                    AddOwnedAnimal(animal);
                }
            }
            RuinarchListPool<Character>.Release(animals);

            if (ownedAnimals.Count < MaxAnimals) {
                int missingAnimals = MaxAnimals - ownedAnimals.Count;
                //Spawn initial animals
                //since max animals are spawned at start, starting per hour generation is unnecessary
                //only when an animal is removed from the owned animals list, is the check for starting production performed
                for (int i = 0; i < missingAnimals; i++) {
                    SpawnNewAnimal();
                }
            }
        }
        public override void LoadedGameStartActions(Area p_area) {
            //Do not do anything when loading a saved game. Since animals are saved and loaded elsewhere.
            owner = p_area;
        }
        public override void OnRemoveFeature(Area p_area) {
            base.OnRemoveFeature(p_area);
            Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
            if (ownedAnimals != null) {
                ownedAnimals.Clear();
                ownedAnimals = null;    
            }
        }
        #endregion

        #region Listeners
        private void OnCharacterDied(Character character) {
            if (character is Animal animal) {
                RemoveOwnedAnimal(animal);
            }
        }
        #endregion

        public void SetSpawnType(SUMMON_TYPE summon) {
            Assert.IsTrue(spawnChoices.Contains(summon), $"Setting spawn type of Game Feature to {summon.ToString()} but it is not part of the given spawn choices!");
            animalTypeBeingSpawned = summon;
#if DEBUG_LOG
            Debug.Log($"Set spawn type of game feature to {summon.ToString()}");
#endif
        }

        private void RemoveOwnedAnimal(Animal animal) {
            if (ownedAnimals.Remove(animal)) {
                if (ownedAnimals.Count == 0) {
                    Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);    
                }
                if (ownedAnimals.Count < MaxAnimals && isGeneratingPerHour == false) {
                    //owned animals is less than max, and is not yet generating, start generation
                    isGeneratingPerHour = true;
                    Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
                }
            }
        }
        private void AddOwnedAnimal(Animal animal) {
            ownedAnimals.Add(animal);
            if (ownedAnimals.Count == 1) {
                Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);   
            }
            if (ownedAnimals.Count >= MaxAnimals) { 
                //owned animals is at max, stop hourly generation
                isGeneratingPerHour = false;
                Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
            }
        }

        private void OnHourStarted() {
            if(GameManager.Instance.currentTick == 120) { //6 am
                int defaultNumOfLivestockToBeSpawned = 2;
                int currentNeededNumberOfLivestock = MaxAnimals - ownedAnimals.Count;
                if(currentNeededNumberOfLivestock < 0) {
                    currentNeededNumberOfLivestock = 0;
                }
                int numOfLivestockToBeSpawned = Mathf.Min(defaultNumOfLivestockToBeSpawned, currentNeededNumberOfLivestock);
                for (int i = 0; i < numOfLivestockToBeSpawned; i++) {
                    SpawnNewAnimal();
                }
            }
            //if (Random.Range(0, 100) < 10) {
            //    SpawnNewAnimal();
            //}
        }

        private void SpawnNewAnimal() {
            LocationGridTile chosenTile = owner.gridTileComponent.GetRandomTileThatIsPassableAndOpenSpace();
            //Assert.IsTrue(choices.Count > 0, $"{owner} is trying to spawn an {animalTypeBeingSpawned.ToString()} but no valid tiles were found!");
            if(chosenTile != null) {
                Animal newAnimal = CharacterManager.Instance.CreateNewSummon(animalTypeBeingSpawned, FactionManager.Instance.neutralFaction, homeRegion: owner.region) as Animal;
                Assert.IsNotNull(newAnimal, $"No new animal was spawned at {owner} when spawn animal was called!");
                CharacterManager.Instance.PlaceSummonInitially(newAnimal, chosenTile);
                newAnimal.SetTerritory(owner, GameManager.Instance.gameHasStarted); //only plan return home if game has started.
                AddOwnedAnimal(newAnimal);
            }

        }

        #region Loading
        public void LoadAnimals(List<string> p_ids) {
            for (int i = 0; i < p_ids.Count; i++) {
                string id = p_ids[i];
                Animal animal = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(id) as Animal;
                if (animal != null) {
                    AddOwnedAnimal(animal);
                }
            }
        }

        public void LoadGeneration(bool p_isGeneratingPerHour) {
            isGeneratingPerHour = p_isGeneratingPerHour;
            if (isGeneratingPerHour) {
                Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
            }
        }
        #endregion

        #region For Testing
        public override string GetTestingData() {
            return $"Game feature spawned animals: {ownedAnimals.ComafyList()}";
        }
        #endregion
    }
    
    [System.Serializable]
    public class SaveDataGameFeature : SaveDataAreaFeature {

        public SUMMON_TYPE summon;
        public List<string> ownedAnimals;
        public bool isGeneratingPerHour;
        public override void Save(AreaFeature tileFeature) {
            base.Save(tileFeature);
            GameFeature gameFeature = tileFeature as GameFeature;
            Assert.IsNotNull(gameFeature);
            summon = gameFeature.animalTypeBeingSpawned;
            ownedAnimals = SaveUtilities.ConvertSavableListToIDs(gameFeature.ownedAnimals);
            isGeneratingPerHour = gameFeature.isGeneratingPerHour;
        }
        public override AreaFeature Load() {
            GameFeature gameFeature = base.Load() as GameFeature;
            Assert.IsNotNull(gameFeature);
            gameFeature.SetSpawnType(summon);
            gameFeature.LoadAnimals(ownedAnimals);
            gameFeature.LoadGeneration(isGeneratingPerHour);
            return gameFeature;
        }
    } 
}
