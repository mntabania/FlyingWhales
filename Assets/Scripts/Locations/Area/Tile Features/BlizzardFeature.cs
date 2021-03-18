using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

namespace Locations.Area_Features {
    public class BlizzardFeature : AreaFeature {

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
            RescheduleBlizzardDamageAndFreezingProcess(p_area); //this will start the freezing check loop
        
            //schedule removal of this feature after x amount of ticks.
            expiryDate = GameManager.Instance.Today().AddTicks(expiryInTicks);
            SchedulingManager.Instance.AddEntry(expiryDate, () => p_area.featureComponent.RemoveFeature(this, p_area), this);
            
            if (GameManager.Instance.gameHasStarted) {
                //only create blizzard effect if game has started when this is added.
                //if this was added before game was started then CreateBlizzardEffect will be
                //handled by GameStartActions()
                CreateBlizzardEffect(p_area);    
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
            if (string.IsNullOrEmpty(_currentFreezingCheckSchedule) == false) {
                SchedulingManager.Instance.RemoveSpecificEntry(_currentFreezingCheckSchedule); //this will stop the freezing check loop 
            }
            ObjectPoolManager.Instance.DestroyObject(_effect);
        }
        public override void GameStartActions(Area p_area) {
            base.GameStartActions(p_area);
            CreateBlizzardEffect(p_area);
        }
        private void CreateBlizzardEffect(Area p_area) {
            GameObject go = GameManager.Instance.CreateParticleEffectAt(p_area.gridTileComponent.centerGridTile, PARTICLE_EFFECT.Blizzard);
            _effect = go;
        }
        #endregion

        #region Listeners
        private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure, Area featureOwner) {
            if (structure != null && structure.isInterior == false && character.gridTileLocation != null
                && character.areaLocation == featureOwner) {
                AddCharacterOutside(character);
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
        private void BlizzardDamageAndFreezingProcess(Area hex) {
            string summary = $"{GameManager.Instance.TodayLogString()}Starting freezing check...";
            int baseChance = 35;
            float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.BLIZZARD);
            int blizzardDamage = PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.BLIZZARD);
            for (int i = 0; i < _charactersOutside.Count; i++) {
                Character character = _charactersOutside[i];
                //int roll = UnityEngine.Random.Range(0, 100);
                //summary =
                //    $"{summary}\nRolling freezing check for {character.name}. Roll is {roll.ToString()}. Chance is {chance.ToString()}";
                if (GameUtilities.RollChance(baseChance)) {
                    summary =
                        $"{summary}\n\tChance met for {character.name}. Adding Freezing trait...";
                    character.traitContainer.AddTrait(character, "Freezing", bypassElementalChance: true);
                    character.AdjustHP(-PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.BLIZZARD), ELEMENTAL_TYPE.Ice,
                        piercingPower: PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.BLIZZARD), showHPBar: true);
                }
                character.AdjustHP(blizzardDamage, ELEMENTAL_TYPE.Ice, triggerDeath: true, showHPBar: true, piercingPower: piercing);
                Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, blizzardDamage);
                if (!character.HasHealth()) {
                    character.skillCauseOfDeath = PLAYER_SKILL_TYPE.BLIZZARD;
                    Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                }
            }
            //reschedule 15 minutes after.
            RescheduleBlizzardDamageAndFreezingProcess(hex);
            Debug.Log(summary);
        }
        private void RescheduleBlizzardDamageAndFreezingProcess(Area p_area) {
            if (p_area.featureComponent.HasFeature(name) == false) { return; }
            GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(10));
            _currentFreezingCheckSchedule = SchedulingManager.Instance.AddEntry(dueDate, () => BlizzardDamageAndFreezingProcess(p_area), this);
        }
        #endregion

        #region Expiry
        public void SetExpiryInTicks(int ticks) {
            expiryInTicks = ticks;
        }
        #endregion
    }

    [System.Serializable]
    public class SaveDataBlizzardFeature : SaveDataAreaFeature {

        public int expiryInTicks;
        public override void Save(AreaFeature tileFeature) {
            base.Save(tileFeature);
            BlizzardFeature blizzardFeature = tileFeature as BlizzardFeature;
            Assert.IsNotNull(blizzardFeature, $"Passed feature is not Blizzard! {tileFeature?.ToString() ?? "Null"}");
            expiryInTicks = GameManager.Instance.Today().GetTickDifference(blizzardFeature.expiryDate) + PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.BLIZZARD);
        }
        public override AreaFeature Load() {
            BlizzardFeature blizzardFeature = base.Load() as BlizzardFeature;
            Assert.IsNotNull(blizzardFeature, $"Passed feature is not Blizzard! {blizzardFeature?.ToString() ?? "Null"}");
            blizzardFeature.SetExpiryInTicks(expiryInTicks);
            return blizzardFeature;
        }
    } 
}