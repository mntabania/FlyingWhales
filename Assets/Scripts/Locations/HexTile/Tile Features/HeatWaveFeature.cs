using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
namespace Locations.Tile_Features {
    public class HeatWaveFeature : TileFeature {

        private List<Character> _charactersOutside;
        private string _currentRainCheckSchedule;
        private GameObject _effect;

        public int expiryInTicks { get; private set; }
        public GameDate expiryDate { get; private set; }
        
        public HeatWaveFeature() {
            name = "Heat Wave";
            description = "There is a heat wave in this location.";
            _charactersOutside = new List<Character>();
            expiryInTicks = GameManager.Instance.GetTicksBasedOnHour(6);
        }

        #region Override
        public override void OnAddFeature(HexTile tile) {
            base.OnAddFeature(tile);
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, (character, structure) => OnCharacterArrivedAtStructure(character, structure, tile));
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, (character, structure) => OnCharacterLeftStructure(character, structure, tile));
            Messenger.AddListener<Character, HexTile>(CharacterSignals.CHARACTER_EXITED_HEXTILE, (character, hexTile) => OnCharacterLeftHexTile(character, hexTile, tile));
            Messenger.AddListener<Character, HexTile>(CharacterSignals.CHARACTER_ENTERED_HEXTILE, (character, hexTile) => OnCharacterEnteredHexTile(character, hexTile, tile));

            PopulateInitialCharactersOutside(tile);
            RescheduleHeatWaveCheck(tile);

            //schedule removal of this feature after x amount of ticks.
            expiryDate = GameManager.Instance.Today().AddTicks(expiryInTicks);
            SchedulingManager.Instance.AddEntry(expiryDate, () => tile.featureComponent.RemoveFeature(this, tile), this);
            if (GameManager.Instance.gameHasStarted) {
                //only create effect if game has started when this is added.
                //if this was added before game was started then CreateEffect will be
                //handled by GameStartActions()
                CreateEffect(tile);    
            }
        }
        public override void OnRemoveFeature(HexTile tile) {
            base.OnRemoveFeature(tile);
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, (character, structure) => OnCharacterArrivedAtStructure(character, structure, tile));
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, (character, structure) => OnCharacterLeftStructure(character, structure, tile));
            Messenger.RemoveListener<Character, HexTile>(CharacterSignals.CHARACTER_EXITED_HEXTILE, (character, hexTile) => OnCharacterLeftHexTile(character, hexTile, tile));
            Messenger.RemoveListener<Character, HexTile>(CharacterSignals.CHARACTER_ENTERED_HEXTILE, (character, hexTile) => OnCharacterEnteredHexTile(character, hexTile, tile));
            //Messenger.RemoveListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED,
            //    (character, gridTile) => OnTileObjectPlaced(character, gridTile, tile));
            if (string.IsNullOrEmpty(_currentRainCheckSchedule) == false) {
                SchedulingManager.Instance.RemoveSpecificEntry(_currentRainCheckSchedule); //this will stop the freezing check loop 
            }
            ObjectPoolManager.Instance.DestroyObject(_effect);
        }
        public override void GameStartActions(HexTile tile) {
            base.GameStartActions(tile);
            CreateEffect(tile);
        }
        #endregion

        #region Listeners
        private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure, HexTile featureOwner) {
            if (structure != null && structure.isInterior == false && character.gridTileLocation != null 
                && character.gridTileLocation.collectionOwner.isPartOfParentRegionMap 
                && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == featureOwner) {
                AddCharacterOutside(character);
                //if (!character.traitContainer.HasTrait("Wet")) {
                //    character.traitContainer.AddTrait(character, "Wet");
                //}
            }
        }
        private void OnCharacterLeftStructure(Character character, LocationStructure structure, HexTile featureOwner) {
            //character left a structure that was outside. If the character entered a structure that is outside. That 
            //is handled at OnCharacterArrivedAtStructure
            if (structure.isInterior == false && character.gridTileLocation != null && character.gridTileLocation.collectionOwner.isPartOfParentRegionMap && 
                character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == featureOwner) {
                RemoveCharacterOutside(character);
            }
        }
        private void OnCharacterLeftHexTile(Character character, HexTile exitedTile, HexTile featureOwner) {
            if (exitedTile == featureOwner) {
                //character left the hextile that owns this feature
                RemoveCharacterOutside(character);
            }
        }
        private void OnCharacterEnteredHexTile(Character character, HexTile enteredTile, HexTile featureOwner) {
            if (enteredTile == featureOwner && character.currentStructure.isInterior == false) {
                AddCharacterOutside(character);
                //if (!character.traitContainer.HasTrait("Wet")) {
                //    character.traitContainer.AddTrait(character, "Wet");
                //}
            }
        }
        //private void OnTileObjectPlaced(TileObject obj, LocationGridTile gridTile, HexTile featureOwner) {
        //    if(gridTile.buildSpotOwner.partOfHextile.hexTileOwner == featureOwner && gridTile != null && !gridTile.structure.isInterior) {
        //        obj.traitContainer.AddTrait(obj, "Wet");
        //    }
        //}
        #endregion

        #region Characters Outisde
        private void AddCharacterOutside(Character character) {
            Assert.IsTrue(character.currentStructure.isInterior == false,
                $"{character.name} is being added to characters outside, but isn't actually outside!");
            if (_charactersOutside.Contains(character) == false) {
                _charactersOutside.Add(character);
            }
        }
        private void RemoveCharacterOutside(Character character) {
            _charactersOutside.Remove(character);
        }
        #endregion

        #region Effects
        private void CreateEffect(HexTile tile) {
            GameObject go =
                GameManager.Instance.CreateParticleEffectAt(tile.GetCenterLocationGridTile(), PARTICLE_EFFECT.Heat_Wave);
            _effect = go;
        }
        private void PopulateInitialCharactersOutside(HexTile hex) {
            List<Character> allCharactersInHex = hex.GetAllCharactersInsideHexThatMeetCriteria<Character>(c => !c.isDead);
            if (allCharactersInHex != null) {
                for (int i = 0; i < allCharactersInHex.Count; i++) {
                    Character character = allCharactersInHex[i];
                    if (!character.currentStructure.isInterior) {
                        AddCharacterOutside(character);
                    }
                }
            }
        }
        private void CheckForOverheating(HexTile hex) {
            for (int i = 0; i < _charactersOutside.Count; i++) {
                Character character = _charactersOutside[i];
                if(UnityEngine.Random.Range(0, 100) < 15) {
                    character.traitContainer.AddTrait(character, "Overheating");
                }
            }
            RescheduleHeatWaveCheck(hex);
        }
        private void RescheduleHeatWaveCheck(HexTile hex) {
            if (hex.featureComponent.HasFeature(name) == false) { return; }
            GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(10));
            _currentRainCheckSchedule = SchedulingManager.Instance.AddEntry(dueDate, () => CheckForOverheating(hex), this);
        }
        #endregion
        
        #region Expiry
        public void SetExpiryInTicks(int ticks) {
            expiryInTicks = ticks;
        }
        #endregion
    }
    
    [System.Serializable]
    public class SaveDataHeatWaveFeature : SaveDataTileFeature {

        public int expiryInTicks;
        public override void Save(TileFeature tileFeature) {
            base.Save(tileFeature);
            HeatWaveFeature heatWaveFeature = tileFeature as HeatWaveFeature;
            Assert.IsNotNull(heatWaveFeature, $"Passed feature is not Heat Wave! {tileFeature?.ToString() ?? "Null"}");
            expiryInTicks = GameManager.Instance.Today().GetTickDifference(heatWaveFeature.expiryDate);
        }
        public override TileFeature Load() {
            HeatWaveFeature heatWaveFeature = base.Load() as HeatWaveFeature;
            Assert.IsNotNull(heatWaveFeature, $"Passed feature is not Heat Wave! {heatWaveFeature?.ToString() ?? "Null"}");
            heatWaveFeature.SetExpiryInTicks(expiryInTicks);
            return heatWaveFeature;
        }
    } 
    
}