using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
namespace Locations.Tile_Features {
    public class BlizzardFeature : TileFeature {

        private List<Character> _charactersOutside;
        private string _currentFreezingCheckSchedule;
        private GameObject _effect;
        
        public int expiryInTicks { get; private set; }
        public GameDate expiryDate { get; private set; }

        public BlizzardFeature() {
            name = "Blizzard";
            description = "There is a blizzard in this location.";
            _charactersOutside = new List<Character>();
            expiryInTicks = GameManager.Instance.GetTicksBasedOnHour(6);
        }

        #region Override
        public override void OnAddFeature(HexTile tile) {
            base.OnAddFeature(tile);
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE,
                (character, structure) => OnCharacterArrivedAtStructure(character, structure, tile));
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, 
                (character, structure) => OnCharacterLeftStructure(character, structure, tile));
            Messenger.AddListener<Character, HexTile>(CharacterSignals.CHARACTER_EXITED_HEXTILE, 
                (character, hexTile) => OnCharacterLeftHexTile(character, hexTile, tile));
            Messenger.AddListener<Character, HexTile>(CharacterSignals.CHARACTER_ENTERED_HEXTILE,
                (character, hexTile) => OnCharacterEnteredHexTile(character, hexTile, tile));
            RescheduleFreezingCheck(tile); //this will start the freezing check loop
        
            //schedule removal of this feature after x amount of ticks.
            expiryDate = GameManager.Instance.Today().AddTicks(expiryInTicks);
            SchedulingManager.Instance.AddEntry(expiryDate, () => tile.featureComponent.RemoveFeature(this, tile), this);
            
            if (GameManager.Instance.gameHasStarted) {
                //only create blizzard effect if game has started when this is added.
                //if this was added before game was started then CreateBlizzardEffect will be
                //handled by GameStartActions()
                CreateBlizzardEffect(tile);    
            }
        }
        public override void OnRemoveFeature(HexTile tile) {
            base.OnRemoveFeature(tile);
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE,
                (character, structure) => OnCharacterArrivedAtStructure(character, structure, tile));
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, 
                (character, structure) => OnCharacterLeftStructure(character, structure, tile));
            Messenger.RemoveListener<Character, HexTile>(CharacterSignals.CHARACTER_EXITED_HEXTILE, 
                (character, hexTile) => OnCharacterLeftHexTile(character, hexTile, tile));
            Messenger.RemoveListener<Character, HexTile>(CharacterSignals.CHARACTER_ENTERED_HEXTILE,
                (character, hexTile) => OnCharacterEnteredHexTile(character, hexTile, tile));
            if (string.IsNullOrEmpty(_currentFreezingCheckSchedule) == false) {
                SchedulingManager.Instance.RemoveSpecificEntry(_currentFreezingCheckSchedule); //this will stop the freezing check loop 
            }
            ObjectPoolManager.Instance.DestroyObject(_effect);
        }
        public override void GameStartActions(HexTile tile) {
            base.GameStartActions(tile);
            CreateBlizzardEffect(tile);
        }
        private void CreateBlizzardEffect(HexTile tile) {
            GameObject go = GameManager.Instance.CreateParticleEffectAt(tile.GetCenterLocationGridTile(), PARTICLE_EFFECT.Blizzard);
            _effect = go;
        }
        #endregion

        #region Listeners
        private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure, HexTile featureOwner) {
            if (structure != null && structure.isInterior == false && character.gridTileLocation != null 
                && character.gridTileLocation.collectionOwner.isPartOfParentRegionMap 
                && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == featureOwner) {
                AddCharacterOutside(character);
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
            }
        }
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
        private void CheckForFreezing(HexTile hex) {
            string summary = $"{GameManager.Instance.TodayLogString()}Starting freezing check...";
            int chance = 35;
            for (int i = 0; i < _charactersOutside.Count; i++) {
                Character character = _charactersOutside[i];
                int roll = UnityEngine.Random.Range(0, 100);
                summary =
                    $"{summary}\nRolling freezing check for {character.name}. Roll is {roll.ToString()}. Chance is {chance.ToString()}";
                if (roll < chance) {
                    summary =
                        $"{summary}\n\tChance met for {character.name}. Adding Freezing trait...";
                    character.traitContainer.AddTrait(character, "Freezing", bypassElementalChance: true);
                    character.AdjustHP(PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.BLIZZARD) * -1, ELEMENTAL_TYPE.Ice,
                        piercingPower: PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.BLIZZARD), showHPBar: true);
                }
            }
            //reschedule 15 minutes after.
            RescheduleFreezingCheck(hex);
            Debug.Log(summary);
        }
        private void RescheduleFreezingCheck(HexTile hex) {
            if (hex.featureComponent.HasFeature(name) == false) { return; }
            GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(10));
            _currentFreezingCheckSchedule = SchedulingManager.Instance.AddEntry(dueDate, () => CheckForFreezing(hex), this);
        }
        #endregion

        #region Expiry
        public void SetExpiryInTicks(int ticks) {
            expiryInTicks = ticks;
        }
        #endregion
    }

    [System.Serializable]
    public class SaveDataBlizzardFeature : SaveDataTileFeature {

        public int expiryInTicks;
        public override void Save(TileFeature tileFeature) {
            base.Save(tileFeature);
            BlizzardFeature blizzardFeature = tileFeature as BlizzardFeature;
            Assert.IsNotNull(blizzardFeature, $"Passed feature is not Blizzard! {tileFeature?.ToString() ?? "Null"}");
            expiryInTicks = GameManager.Instance.Today().GetTickDifference(blizzardFeature.expiryDate) + PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.BLIZZARD);
        }
        public override TileFeature Load() {
            BlizzardFeature blizzardFeature = base.Load() as BlizzardFeature;
            Assert.IsNotNull(blizzardFeature, $"Passed feature is not Blizzard! {blizzardFeature?.ToString() ?? "Null"}");
            blizzardFeature.SetExpiryInTicks(expiryInTicks);
            return blizzardFeature;
        }
    } 
}