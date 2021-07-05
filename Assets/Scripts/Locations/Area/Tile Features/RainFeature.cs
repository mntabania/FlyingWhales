using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using Traits;
using UtilityScripts;

namespace Locations.Area_Features {
    public class RainFeature : AreaFeature {

        private readonly List<Character> _charactersOutside;
        private string _currentRainCheckSchedule;
        private GameObject _effect;
        private AudioObject _audioObject;

        public int expiryInTicks { get; private set; }
        public GameDate expiryDate { get; private set; }
        public bool isPlayerSource { get; private set; }
        public override Type serializedData => typeof(SaveDataRainFeature);

        public RainFeature() {
            name = "Rain";
            description = "Rain is pouring down in this location.";
            _charactersOutside = new List<Character>();
            expiryInTicks = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.RAIN);
        }

        #region Override
        public override void OnAddFeature(Area p_area) {
            base.OnAddFeature(p_area);
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE,
                (character, structure) => OnCharacterArrivedAtStructure(character, structure, p_area));
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE,
                (character, structure) => OnCharacterLeftStructure(character, structure, p_area));
            Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_EXITED_AREA,
                (character, area) => OnCharacterLeftArea(character, area, p_area));
            Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA,
                (character, area) => OnCharacterEnteredArea(character, area, p_area));
            //Messenger.AddListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED,
            //    (character, gridTile) => OnTileObjectPlaced(character, gridTile, tile));

            RescheduleRainCheck(p_area); //this will start the rain check loop
            //CheckForWet(tile);

            //schedule removal of this feature after x amount of ticks.
            expiryDate = GameManager.Instance.Today().AddTicks(expiryInTicks);
            SchedulingManager.Instance.AddEntry(expiryDate, () => p_area.featureComponent.RemoveFeature(this, p_area), this);
            
            if (GameManager.Instance.gameHasStarted) {
                //only create effect if game has started when this is added.
                //if this was added before game was started then CreateEffect will be
                //handled by GameStartActions()
                CreateEffect(p_area);    
                PopulateInitialCharactersOutside(p_area);
            }
        }
        public override void OnRemoveFeature(Area p_area) {
            base.OnRemoveFeature(p_area);
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE,
                (character, structure) => OnCharacterArrivedAtStructure(character, structure, p_area));
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE,
                (character, structure) => OnCharacterLeftStructure(character, structure, p_area));
            Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_EXITED_AREA,
                (character, area) => OnCharacterLeftArea(character, area, p_area));
            Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA,
                (character, area) => OnCharacterEnteredArea(character, area, p_area));
            //Messenger.RemoveListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED,
            //    (character, gridTile) => OnTileObjectPlaced(character, gridTile, tile));
            if (string.IsNullOrEmpty(_currentRainCheckSchedule) == false) {
                SchedulingManager.Instance.RemoveSpecificEntry(_currentRainCheckSchedule); //this will stop the freezing check loop 
            }
            ObjectPoolManager.Instance.DestroyObject(_effect);
            ObjectPoolManager.Instance.DestroyObject(_audioObject);
        }
        public override void GameStartActions(Area p_area) {
            base.GameStartActions(p_area);
            CreateEffect(p_area);
            PopulateInitialCharactersOutside(p_area);
        }
        #endregion

        #region Listeners
        private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure, Area featureOwner) {
            if (structure != null && structure.isInterior == false && character.gridTileLocation != null && character.areaLocation == featureOwner) {
                AddCharacterOutside(character);
                //if (!character.traitContainer.HasTrait("Wet")) {
                //    character.traitContainer.AddTrait(character, "Wet");
                //}
            }
        }
        private void OnCharacterLeftStructure(Character character, LocationStructure structure, Area featureOwner) {
            //character left a structure that was outside. If the character entered a structure that is outside. That 
            //is handled at OnCharacterArrivedAtStructure
            if (structure.isInterior == false && character.gridTileLocation != null && character.areaLocation == featureOwner) {
                RemoveCharacterOutside(character);
            }
        }
        private void OnCharacterLeftArea(Character character, Area exitedArea, Area featureOwner) {
            if (exitedArea == featureOwner) {
                //character left the hextile that owns this feature
                RemoveCharacterOutside(character);
            }
        }
        private void OnCharacterEnteredArea(Character character, Area enteredArea, Area featureOwner) {
            if (enteredArea == featureOwner && character.currentStructure.isInterior == false) {
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
        private void CreateEffect(Area p_area) {
            LocationGridTile centerTile = p_area.gridTileComponent.centerGridTile;
            GameObject go = GameManager.Instance.CreateParticleEffectAt(centerTile, PARTICLE_EFFECT.Rain);
            _audioObject = AudioManager.Instance.TryCreateAudioObject(
                PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<RainSkillData>(PLAYER_SKILL_TYPE.RAIN).rainSoundEffect, centerTile, 7,
                true);
            _effect = go;
        }
        private void PopulateInitialCharactersOutside(Area p_area) {
            List<Character> allCharactersInHex = RuinarchListPool<Character>.Claim();
            p_area.locationCharacterTracker.PopulateCharacterListInsideHexThatIsAlive(allCharactersInHex);
            if (allCharactersInHex != null) {
                for (int i = 0; i < allCharactersInHex.Count; i++) {
                    Character character = allCharactersInHex[i];
                    if (!character.currentStructure.isInterior) {
                        AddCharacterOutside(character);
                    }
                }
            }
            RuinarchListPool<Character>.Release(allCharactersInHex);
        }
        private void CheckForWet(Area p_area) {
            for (int i = 0; i < _charactersOutside.Count; i++) {
                Character character = _charactersOutside[i];
                character.traitContainer.AddTrait(character, "Wet");
                Wet wet = character.traitContainer.GetTraitOrStatus<Wet>("Wet");
                wet?.SetIsPlayerSource(isPlayerSource);
            }
            for (int i = 0; i < p_area.gridTileComponent.gridTiles.Count; i++) {
                LocationGridTile gridTile = p_area.gridTileComponent.gridTiles[i];
                if (!gridTile.structure.isInterior) {
                    gridTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(gridTile.tileObjectComponent.genericTileObject, "Wet");
                    Wet wet = gridTile.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Wet>("Wet");
                    wet?.SetIsPlayerSource(isPlayerSource);
                    if (gridTile.tileObjectComponent.objHere != null) {
                        gridTile.tileObjectComponent.objHere.traitContainer.AddTrait(gridTile.tileObjectComponent.objHere, "Wet");
                        Wet wetObjHere = gridTile.tileObjectComponent.objHere.traitContainer.GetTraitOrStatus<Wet>("Wet");
                        wetObjHere?.SetIsPlayerSource(isPlayerSource);
                    }
                }
            }
            RescheduleRainCheck(p_area);
        }
        private void RescheduleRainCheck(Area p_area) {
            if (p_area.featureComponent.HasFeature(name) == false) { return; }
            GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(15));
            _currentRainCheckSchedule = SchedulingManager.Instance.AddEntry(dueDate, () => CheckForWet(p_area), this);
        }
        #endregion
        
        #region Expiry
        public void SetExpiryInTicks(int ticks) {
            expiryInTicks = ticks;
        }
        #endregion

        public void SetIsPlayerSource(bool p_state) {
            isPlayerSource = p_state;
        }
    }
    
    [System.Serializable]
    public class SaveDataRainFeature : SaveDataAreaFeature {

        public int expiryInTicks;
        public bool isPlayerSource;
        public override void Save(AreaFeature tileFeature) {
            base.Save(tileFeature);
            RainFeature rainFeature = tileFeature as RainFeature;
            Assert.IsNotNull(rainFeature, $"Passed feature is not Rain! {tileFeature?.ToString() ?? "Null"}");
            expiryInTicks = GameManager.Instance.Today().GetTickDifference(rainFeature.expiryDate);
            isPlayerSource = rainFeature.isPlayerSource;
        }
        public override AreaFeature Load() {
            RainFeature rainFeature = base.Load() as RainFeature;
            Assert.IsNotNull(rainFeature, $"Passed feature is not Rain! {rainFeature?.ToString() ?? "Null"}");
            rainFeature.SetExpiryInTicks(expiryInTicks);
            rainFeature.SetIsPlayerSource(isPlayerSource);
            return rainFeature;
        }
    } 
}