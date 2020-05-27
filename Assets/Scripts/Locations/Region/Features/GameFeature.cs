using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEditor.VersionControl;
using UnityEngine.Assertions;
using UtilityScripts;
using Random = UnityEngine.Random;

namespace Locations.Features {
    public class GameFeature : TileFeature {

        private const int MaxAnimals = 6;

        private HexTile owner;
        private bool isGeneratingPerHour;
    
        private SUMMON_TYPE animalTypeBeingSpawned;
        public List<Animal> ownedAnimals { get; private set; } //list of animals that have been spawned by this feature

        private static readonly SUMMON_TYPE[] _spawnChoices = new[] {
            SUMMON_TYPE.Pig,
            SUMMON_TYPE.Sheep,
            SUMMON_TYPE.Chicken,
        }; 
    
        public GameFeature() {
            name = "Game";
            description = "Hunters can obtain food here.";
            ownedAnimals = new List<Animal>();
            SetSpawnType(CollectionUtilities.GetRandomElement(_spawnChoices));
        }
    
        #region Overrides
        public override void GameStartActions(HexTile tile) {
            owner = tile;
            //Spawn initial animals
            //since max animals are spawned at start, starting per hour generation is unnecessary
            //only when an animal is removed from the owned animals list, is the check for starting production performed
            for (int i = 0; i < MaxAnimals; i++) {
                SpawnNewAnimal();
            }
        }
        public override void OnRemoveFeature(HexTile tile) {
            base.OnRemoveFeature(tile);
            Messenger.RemoveListener(Signals.HOUR_STARTED, TryGeneratePerHour);
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
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
            Assert.IsTrue(_spawnChoices.Contains(summon), $"Setting spawn type of Game Feature to {summon.ToString()} but it is not part of the given spawn choices!");
            animalTypeBeingSpawned = summon;
        }

        private void RemoveOwnedAnimal(Animal animal) {
            if (ownedAnimals.Remove(animal)) {
                if (ownedAnimals.Count == 0) {
                    Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);    
                }
                if (ownedAnimals.Count < MaxAnimals && isGeneratingPerHour == false) {
                    //owned animals is less than max, and is not yet generating, start generation
                    isGeneratingPerHour = true;
                    Messenger.AddListener(Signals.HOUR_STARTED, TryGeneratePerHour);
                }
            }
        }
        private void AddOwnedAnimal(Animal animal) {
            ownedAnimals.Add(animal);
            if (ownedAnimals.Count == 1) {
                Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);   
            }
            if (ownedAnimals.Count >= MaxAnimals) { 
                //owned animals is at max, stop hourly generation
                isGeneratingPerHour = false;
                Messenger.RemoveListener(Signals.HOUR_STARTED, TryGeneratePerHour);
            }
        }

        private void TryGeneratePerHour() {
            if (Random.Range(0, 100) < 25) {
                SpawnNewAnimal();
            }
        }

        private void SpawnNewAnimal() {
            List<LocationGridTile> choices = owner.locationGridTiles.Where(x => x.isOccupied == false && x.structure.structureType.IsOpenSpace()).ToList();
            Assert.IsTrue(choices.Count > 0, $"{owner} is trying to spawn an {animalTypeBeingSpawned.ToString()} but no valid tiles were found!");
            LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(choices);
            Animal newAnimal = CharacterManager.Instance.CreateNewSummon(animalTypeBeingSpawned, FactionManager.Instance.neutralFaction, homeRegion: owner.region) as Animal;
            Assert.IsNotNull(newAnimal, $"No new animal was spawned at {owner} when spawn animal was called!");
            CharacterManager.Instance.PlaceSummon(newAnimal, chosenTile);
            newAnimal.AddTerritory(owner);
            AddOwnedAnimal(newAnimal);
        }
    }
}
