using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scenario_Maps;
using Traits;
using UtilityScripts;
namespace Generator.Map_Generation.Components {
    public class CharacterFinalization : MapGenerationComponent {
        public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
            for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                Character character = CharacterManager.Instance.allCharacters[i];
                //generate random traits for all villagers
                //NOTE: added checking for dead because it is possible to have initial dead characters at random map
                //because of Ancient Graveyard.
                if (character.isNormalCharacter && !character.isDead) {
                    character.CreateRandomInitialTraits();
                }
            }
            yield return null;
        }

        #region Saved World
        public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
            yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
        }
        #endregion

        #region Scenario Maps
        public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
            if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
                for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                    Character character = CharacterManager.Instance.allCharacters[i];
                    //generate random traits for all villagers
                    //NOTE: added checking for dead because it is possible to have initial dead characters at random map
                    //because of Ancient Graveyard.
                    if (character.isNormalCharacter && !character.isDead) {
                        ZenkoCharacterRandomInitialTraits(character);
                    }
                }
            } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
                List<Character> validCharacters = CharacterManager.Instance.allCharacters.Where(x => x.isDead == false && x.isNormalCharacter).ToList();
                validCharacters = CollectionUtilities.Shuffle(validCharacters);
                for (int i = 0; i < validCharacters.Count; i++) {
                    Character character = validCharacters[i];
                    IcalawaCharacterRandomInitialTraits(i, character, validCharacters.Count);
                }
            } else {
                yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
            }
        }
        #endregion

        #region Zenko
        private void ZenkoCharacterRandomInitialTraits(Character character) {
            List<string> buffTraits = new List<string>(TraitManager.Instance.buffTraitPool);
            List<string> neutralTraits = new List<string>(TraitManager.Instance.neutralTraitPool);
            List<string> flawTraits = new List<string>(TraitManager.Instance.flawTraitPool);
            
            //Up to three traits
            
            if (character.homeRegion.coreTile.biomeType == BIOMES.SNOW) {
                //Snow villagers starts with Cold Blooded
                character.traitContainer.AddTrait(character, "Cold Blooded");
            } else if (character.homeRegion.coreTile.biomeType == BIOMES.DESERT) {
                //Desert villagers starts with Fire Proof
                character.traitContainer.AddTrait(character, "Fireproof");
            } else if (character.homeRegion.coreTile.biomeType == BIOMES.GRASSLAND) {
                //Grassland villagers starts with Electric
                character.traitContainer.AddTrait(character, "Electric");
            } else if (character.homeRegion.coreTile.biomeType == BIOMES.FOREST) {
                //Forest villagers starts with Venomous
                character.traitContainer.AddTrait(character, "Venomous");
            }

            List<string> choices = new List<string>();
            //80% Trait 2: Buff + Neutral List
            if (GameUtilities.RollChance(80)) {
                choices.AddRange(buffTraits);
                choices.AddRange(neutralTraits);
                string chosenBuffOrNeutralTraitName;
                
                if (choices.Count > 0) {
                    chosenBuffOrNeutralTraitName = CollectionUtilities.GetRandomElement(choices); 
                    buffTraits.Remove(chosenBuffOrNeutralTraitName);
                    neutralTraits.Remove(chosenBuffOrNeutralTraitName);
                } else {
                    throw new Exception("No more buff or neutral traits!");
                }
                
                character.traitContainer.AddTrait(character, chosenBuffOrNeutralTraitName);
                Trait buffOrNeutralTrait = character.traitContainer.GetNormalTrait<Trait>(chosenBuffOrNeutralTraitName);
                if (buffOrNeutralTrait.mutuallyExclusive != null) {
                    buffTraits = CollectionUtilities.RemoveElements(ref buffTraits, buffOrNeutralTrait.mutuallyExclusive); //update buff traits pool to accomodate new trait
                    neutralTraits = CollectionUtilities.RemoveElements(ref neutralTraits, buffOrNeutralTrait.mutuallyExclusive); //update neutral traits pool to accomodate new trait
                    flawTraits = CollectionUtilities.RemoveElements(ref flawTraits, buffOrNeutralTrait.mutuallyExclusive); //update flaw traits pool to accomodate new trait
                }
            }
            
            
            //40% Trait 3: Buff + Neutral + Flaw List
            if (GameUtilities.RollChance(40)) {
                choices.Clear();
                choices.AddRange(buffTraits);
                choices.AddRange(neutralTraits);
                choices.AddRange(flawTraits);
                string chosenTrait;
                if (choices.Count > 0) {
                    chosenTrait = CollectionUtilities.GetRandomElement(choices);
                } else {
                    throw new Exception("No more buff, neutral or flaw traits!");
                }
                character.traitContainer.AddTrait(character, chosenTrait);
            }

        }
        #endregion

        #region Icalawa
        private void IcalawaCharacterRandomInitialTraits(int index, Character character, int totalCharacters) {
            if (index < totalCharacters/2) {
                //half of villagers are robust
                character.traitContainer.AddTrait(character, "Robust");
            }
            if (index + 1 == totalCharacters) {
                //last villager is Evil.
                character.traitContainer.AddTrait(character, "Evil");
            } else {
                //all non evil characters are blessed
                character.traitContainer.AddTrait(character, "Blessed");
            }
            List<string> buffTraits = new List<string>(TraitManager.Instance.buffTraitPool);
            buffTraits.Remove("Blessed");
            buffTraits.Remove("Robust");
            List<string> neutralTraits = new List<string>(TraitManager.Instance.neutralTraitPool);
            List<string> flawTraits = new List<string>(TraitManager.Instance.flawTraitPool);
            flawTraits.Remove("Evil");
            
            character.CreateRandomInitialTraits(buffTraits, neutralTraits, flawTraits);
        }
        #endregion
    }
}