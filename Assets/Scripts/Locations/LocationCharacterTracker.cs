using System;
using System.Collections.Generic;
using UnityEngine.Profiling;
using UtilityScripts;
namespace Locations {
    public class LocationCharacterTracker {
        public List<Character> charactersAtLocation { get; private set; }

        public LocationCharacterTracker() {
            charactersAtLocation = new List<Character>();
        }

        public void AddCharacterAtLocation(Character p_character) {
            charactersAtLocation.Add(p_character);
        }
        public void RemoveCharacterFromLocation(Character p_character) {
            charactersAtLocation.Remove(p_character);
            
            Profiler.BeginSample($"Check job applicability - Remove Status");
            Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.REMOVE_STATUS, p_character as IPointOfInterest);
            Profiler.EndSample();
                
            Profiler.BeginSample($"Check job applicability - Apprehend");
            Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.APPREHEND, p_character as IPointOfInterest);
            Profiler.EndSample();
                
            Profiler.BeginSample($"Check job applicability - Knockout");
            Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.KNOCKOUT, p_character as IPointOfInterest);
            Profiler.EndSample();
        }

        #region Utilities
        public void PopulateCharacterListInsideHex(List<Character> p_characterList) {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) { continue; }
                if (c.isBeingSeized) { continue; }
                p_characterList.Add(c);
            }
        }
        public void PopulateAnimalsListInsideHex(List<Character> p_characterList, bool includeBeingSeized = false) {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) { continue; }
                if (!includeBeingSeized && c.isBeingSeized) { continue; }
                if (c is Animal) {
                    p_characterList.Add(c);
                }
            }
        }
        public void PopulateAnimalsListInsideHexThatIsNotTheSameRaceAs(List<Character> p_characterList, RACE p_race) {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) { continue; }
                if (c.isBeingSeized) { continue; }
                if (c is Animal && c.race != p_race) {
                    p_characterList.Add(c);
                }
            }
        }
        public void PopulateCharacterListInsideHexThatHasTrait(List<Character> p_characterList, string p_traitName) {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) { continue; }
                if (c.isBeingSeized) { continue; }
                if (c.traitContainer.HasTrait(p_traitName)) {
                    p_characterList.Add(c);
                }
            }
        }
        public void PopulateCharacterListInsideHexThatHasTraitAndNotRace(List<Character> p_characterList, string p_traitName, RACE p_race) {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) { continue; }
                if (c.isBeingSeized) { continue; }
                if (c.traitContainer.HasTrait(p_traitName) && c.race != p_race) {
                    p_characterList.Add(c);
                }
            }
        }
        public void PopulateCharacterListInsideHexThatIsAlive(List<Character> p_characterList) {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) { continue; }
                if (c.isBeingSeized) { continue; }
                if (!c.isDead) {
                    p_characterList.Add(c);
                }
            }
        }
        public void PopulateCharacterListInsideHexForInvadeBehaviour(List<Character> p_characterList, Character p_exception = null) {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) { continue; }
                if (c.isBeingSeized) { continue; }
                if ((p_exception == null || p_exception != c) && c.isNormalCharacter && c.isDead == false && c.isAlliedWithPlayer == false && !c.traitContainer.HasTrait("Hibernating", "Indestructible")
                    && !c.isInLimbo && c.carryComponent.IsNotBeingCarried()) {
                    p_characterList.Add(c);
                }
            }
        }
        public void PopulateCharacterListInsideHexForKoboldBehaviour(List<Character> p_characterList, Character p_exception = null) {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) { continue; }
                if (c.isBeingSeized) { continue; }
                if (c.traitContainer.HasTrait("Frozen") && c.race != RACE.KOBOLD && c.HasJobTargetingThis(JOB_TYPE.CAPTURE_CHARACTER) == false) {
                    p_characterList.Add(c);
                }
            }
        }
        public void PopulateCharacterListInsideHexForPangatLooTargetForInvasion(List<Character> p_characterList) {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) { continue; }
                if (c.isBeingSeized) { continue; }
                if (c.isNormalCharacter && c.isDead == false && !c.isInLimbo
                    && c.carryComponent.IsNotBeingCarried() 
                    && !c.traitContainer.HasTrait("Hibernating", "Indestructible")) {
                    p_characterList.Add(c);
                }
            }
        }
        public void PopulateCharacterListInsideHexForVengefulGhostBehaviour(List<Character> p_characterList, Character p_invader) {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) { continue; }
                if (c.isBeingSeized) { continue; }
                if (c != p_invader && p_invader.IsHostileWith(c) 
                    && !c.isDead 
                    && !c.traitContainer.HasTrait("Hibernating", "Indestructible") 
                    && !c.isInLimbo 
                    && c.carryComponent.IsNotBeingCarried()) {
                    p_characterList.Add(c);
                }
            }
        }
        public Character GetFirstCharacterInsideHexForBoneGolemBehaviour(Character p_boneGolem) {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) {
                    continue; //skip this character
                }
                if (CharacterManager.Instance.IsCharacterConsideredTargetOfBoneGolem(p_boneGolem, c)) {
                    return c;
                }
            }
            return null;
        }
        public Character GetFirstCharacterInsideHexThatIsAliveHostileNotAlliedWithPlayerThatHasPathTo(Character p_character) {
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) {
                    continue; //skip this character
                }
                if (p_character != c && p_character.IsHostileWith(c) && !c.isDead && !c.isAlliedWithPlayer
                    && c.marker && c.marker.isMainVisualActive && p_character.movementComponent.HasPathTo(c.gridTileLocation)
                    && !c.isInLimbo && !c.isBeingSeized && c.carryComponent.IsNotBeingCarried()
                    && !c.traitContainer.HasTrait("Hibernating", "Indestructible")) {
                    return c;
                }
            }
            return null;
        }
        public Character GetRandomCharacterInsideHexThatIsAliveAndConsidersAreaAsTerritory(Area p_area) {
            Character chosenCharacter = null;
            List<Character> characters = RuinarchListPool<Character>.Claim();
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) {
                    continue; //skip this character
                }
                if (!c.isDead && c.IsTerritory(p_area)) {
                    characters.Add(c);
                }
            }
            if (characters.Count > 0) {
                chosenCharacter = CollectionUtilities.GetRandomElement(characters);
            }
            RuinarchListPool<Character>.Release(characters);
            return chosenCharacter;
        }
        public int GetNumOfCharactersInsideHexThatHasRaceAndClassOf(RACE p_race, string p_className, Type p_behaviourTypeException = null) {
            int count = 0;
            for (int i = 0; i < charactersAtLocation.Count; i++) {
                Character c = charactersAtLocation[i];
                if (c.gridTileLocation == null) {
                    continue; //skip this character
                }
                if (c.race == p_race && c.characterClass.className == p_className
                    && (p_behaviourTypeException == null || !c.behaviourComponent.HasBehaviour(p_behaviourTypeException))) {
                    count++;
                }
            }
            return count;
        }
        #endregion

        #region Testing
        public string GetCharactersSummary() {
            return charactersAtLocation.ComafyList();
        }
        #endregion
    }
}