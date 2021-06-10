using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Traits;

namespace Locations.Area_Features {
    public class BlizzardFeature : AreaFeature {

        private List<Character> _charactersOutside;
        private string _currentFreezingCheckSchedule;
        private GameObject _effect;
        
        public int expiryInTicks { get; private set; }
        public GameDate expiryDate { get; private set; }
        public bool isPlayerSource { get; private set; }
        public override Type serializedData => typeof(SaveDataBlizzardFeature);

        public BlizzardFeature() {
            name = "Blizzard";
            description = "There is a blizzard in this location.";
            _charactersOutside = new List<Character>();
            expiryInTicks = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.BLIZZARD);
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

            PopulateInitialCharactersOutside(p_area);
            
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
            ClearCharactersOutside();
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
        private void ClearCharactersOutside() {
            _charactersOutside.Clear();
        }
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
#if DEBUG_LOG
            string summary = $"{GameManager.Instance.TodayLogString()}Starting freezing check...";
#endif
            SkillData blizzardData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.BLIZZARD);
            int baseChance = (int)PlayerSkillManager.Instance.GetChanceBonusPerLevel(blizzardData);
            float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(blizzardData);
            int blizzardDamage = PlayerSkillManager.Instance.GetDamageBaseOnLevel(blizzardData);
            for (int i = 0; i < _charactersOutside.Count; i++) {
                Character character = _charactersOutside[i];
                if (character.isDead) {
                    continue;
                }
                //int roll = UnityEngine.Random.Range(0, 100);
                //summary =
                //    $"{summary}\nRolling freezing check for {character.name}. Roll is {roll.ToString()}. Chance is {chance.ToString()}";
                if (GameUtilities.RollChance(baseChance)) {
#if DEBUG_LOG
                    summary =
                        $"{summary}\n\tChance met for {character.name}. Adding Freezing trait...";
#endif
                    character.traitContainer.AddTrait(character, "Freezing", bypassElementalChance: true);
                    Freezing freezing = character.traitContainer.GetTraitOrStatus<Freezing>("Freezing");
                    freezing?.SetIsPlayerSource(isPlayerSource);
                    //character.AdjustHP(-blizzardDamage, ELEMENTAL_TYPE.Ice,
                    //    piercingPower: PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.BLIZZARD), showHPBar: true);
                }
                character.AdjustHP(-blizzardDamage, ELEMENTAL_TYPE.Ice, triggerDeath: true, showHPBar: true, piercingPower: piercing, isPlayerSource: isPlayerSource, source: isPlayerSource ? blizzardData : null);
                Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, blizzardDamage);
                if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE) {
                    character.skillCauseOfDeath = PLAYER_SKILL_TYPE.BLIZZARD;
                    //Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                    //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                }
            }
            //reschedule 15 minutes after.
            RescheduleBlizzardDamageAndFreezingProcess(hex);
#if DEBUG_LOG
            Debug.Log(summary);
#endif
        }
        private void RescheduleBlizzardDamageAndFreezingProcess(Area p_area) {
            if (p_area.featureComponent.HasFeature(name) == false) { return; }
            GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(6));
            _currentFreezingCheckSchedule = SchedulingManager.Instance.AddEntry(dueDate, () => BlizzardDamageAndFreezingProcess(p_area), this);
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
    public class SaveDataBlizzardFeature : SaveDataAreaFeature {

        public int expiryInTicks;
        public bool isPlayerSource;

        public override void Save(AreaFeature tileFeature) {
            base.Save(tileFeature);
            BlizzardFeature blizzardFeature = tileFeature as BlizzardFeature;
            Assert.IsNotNull(blizzardFeature, $"Passed feature is not Blizzard! {tileFeature?.ToString() ?? "Null"}");
            expiryInTicks = blizzardFeature.expiryInTicks;
            isPlayerSource = blizzardFeature.isPlayerSource;
        }
        public override AreaFeature Load() {
            BlizzardFeature blizzardFeature = base.Load() as BlizzardFeature;
            Assert.IsNotNull(blizzardFeature, $"Passed feature is not Blizzard! {blizzardFeature?.ToString() ?? "Null"}");
            blizzardFeature.SetExpiryInTicks(expiryInTicks);
            blizzardFeature.SetIsPlayerSource(isPlayerSource);
            return blizzardFeature;
        }
    } 
}