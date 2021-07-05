using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Traits;

namespace Locations.Area_Features {
    public class HeatWaveFeature : AreaFeature {

        private List<Character> _charactersOutside;
        private string _currentRainCheckSchedule;
        private GameObject _effect;

        public int expiryInTicks { get; private set; }
        public GameDate expiryDate { get; private set; }
        public bool isPlayerSource { get; private set; }
        public override Type serializedData => typeof(SaveDataHeatWaveFeature);

        public HeatWaveFeature() {
            name = "Heat Wave";
            description = "There is a heat wave in this location.";
            _charactersOutside = new List<Character>();
            expiryInTicks = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.HEAT_WAVE);
        }

        #region Override
        public override void OnAddFeature(Area p_area) {
            base.OnAddFeature(p_area);
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, (character, structure) => OnCharacterArrivedAtStructure(character, structure, p_area));
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, (character, structure) => OnCharacterLeftStructure(character, structure, p_area));
            Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_EXITED_AREA, (character, area) => OnCharacterLeftArea(character, area, p_area));
            Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, (character, area) => OnCharacterEnteredArea(character, area, p_area));

            PopulateInitialCharactersOutside(p_area);
            RescheduleHeatWaveCheck(p_area);

            //schedule removal of this feature after x amount of ticks.
            expiryDate = GameManager.Instance.Today().AddTicks(expiryInTicks);
            SchedulingManager.Instance.AddEntry(expiryDate, () => p_area.featureComponent.RemoveFeature(this, p_area), this);
            if (GameManager.Instance.gameHasStarted) {
                //only create effect if game has started when this is added.
                //if this was added before game was started then CreateEffect will be
                //handled by GameStartActions()
                CreateEffect(p_area);    
            }
        }
        public override void OnRemoveFeature(Area p_area) {
            base.OnRemoveFeature(p_area);
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, (character, structure) => OnCharacterArrivedAtStructure(character, structure, p_area));
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, (character, structure) => OnCharacterLeftStructure(character, structure, p_area));
            Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_EXITED_AREA, (character, area) => OnCharacterLeftArea(character, area, p_area));
            Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, (character, area) => OnCharacterEnteredArea(character, area, p_area));
            //Messenger.RemoveListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED,
            //    (character, gridTile) => OnTileObjectPlaced(character, gridTile, tile));
            if (string.IsNullOrEmpty(_currentRainCheckSchedule) == false) {
                SchedulingManager.Instance.RemoveSpecificEntry(_currentRainCheckSchedule); //this will stop the freezing check loop 
            }
            ObjectPoolManager.Instance.DestroyObject(_effect);
        }
        public override void GameStartActions(Area p_area) {
            base.GameStartActions(p_area);
            CreateEffect(p_area);
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
        private void OnCharacterLeftArea(Character character, Area exitedTile, Area featureOwner) {
            if (exitedTile == featureOwner) {
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
            GameObject go =
                GameManager.Instance.CreateParticleEffectAt(p_area.gridTileComponent.centerGridTile, PARTICLE_EFFECT.Heat_Wave);
            _effect = go;
        }
        private void PopulateInitialCharactersOutside(Area p_area) {
            List<Character> allCharactersInArea = RuinarchListPool<Character>.Claim();
            p_area.locationCharacterTracker.PopulateCharacterListInsideHex(allCharactersInArea);
            if (allCharactersInArea != null) {
                for (int i = 0; i < allCharactersInArea.Count; i++) {
                    Character character = allCharactersInArea[i];
                    if (!character.currentStructure.isInterior) {
                        AddCharacterOutside(character);
                    }
                }
            }
            RuinarchListPool<Character>.Release(allCharactersInArea);
        }
        private void CheckForOverheating(Area p_area) {
            RESISTANCE resistanceType = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.HEAT_WAVE).resistanceType;
            float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.HEAT_WAVE);
            int baseChance = Mathf.RoundToInt(PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(PLAYER_SKILL_TYPE.HEAT_WAVE));
            for (int i = 0; i < _charactersOutside.Count; i++) {
                Character character = _charactersOutside[i];
                if (character.isDead) {
                    continue;
                }
                float resistanceValue = character.piercingAndResistancesComponent.GetResistanceValue(resistanceType);
                CombatManager.ModifyValueByPiercingAndResistance(ref baseChance, piercing, resistanceValue);
                if(GameUtilities.RollChance(baseChance)) {
                    character.traitContainer.AddTrait(character, "Overheating");
                    Overheating overheating = character.traitContainer.GetTraitOrStatus<Overheating>("Overheating");
                    overheating?.SetIsPlayerSource(isPlayerSource);
                } else {
                    character.reactionComponent.PlayResistVFX();
                    //character.reactionComponent.ResistRuinarchPower();
                }
            }
            RescheduleHeatWaveCheck(p_area);
        }
        private void RescheduleHeatWaveCheck(Area p_area) {
            if (p_area.featureComponent.HasFeature(name) == false) { return; }
            GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(6));
            _currentRainCheckSchedule = SchedulingManager.Instance.AddEntry(dueDate, () => CheckForOverheating(p_area), this);
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
    public class SaveDataHeatWaveFeature : SaveDataAreaFeature {

        public int expiryInTicks;
        public bool isPlayerSource;

        public override void Save(AreaFeature tileFeature) {
            base.Save(tileFeature);
            HeatWaveFeature heatWaveFeature = tileFeature as HeatWaveFeature;
            Assert.IsNotNull(heatWaveFeature, $"Passed feature is not Heat Wave! {tileFeature?.ToString() ?? "Null"}");
            expiryInTicks = GameManager.Instance.Today().GetTickDifference(heatWaveFeature.expiryDate);
            isPlayerSource = heatWaveFeature.isPlayerSource;
        }
        public override AreaFeature Load() {
            HeatWaveFeature heatWaveFeature = base.Load() as HeatWaveFeature;
            Assert.IsNotNull(heatWaveFeature, $"Passed feature is not Heat Wave! {heatWaveFeature?.ToString() ?? "Null"}");
            heatWaveFeature.SetExpiryInTicks(expiryInTicks);
            heatWaveFeature.SetIsPlayerSource(isPlayerSource);
            return heatWaveFeature;
        }
    } 
    
}