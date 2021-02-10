﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scenario_Maps;
using Traits;
using UnityEngine;
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
            
            //apply faction related effects to each member
            for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                Faction faction = FactionManager.Instance.allFactions[i];
                if (faction.isMajorNonPlayer) {
                    if (faction.leader is Character factionLeader) {
                        ApplyFactionTypeRelatedEffectToMember(faction, factionLeader);
                    }
                    List<Character> settlementRulers = faction.characters.Where(c => c.isSettlementRuler).ToList();
                    for (int j = 0; j < settlementRulers.Count; j++) {
                        Character settlementRuler = settlementRulers[j];
                        ApplyFactionTypeRelatedEffectToMember(faction, settlementRuler);
                    }
                    
                    List<Character> normalMembers = faction.characters.Where(c => !c.isFactionLeader && !c.isSettlementRuler).ToList();
                    int halfMembers = Mathf.FloorToInt(normalMembers.Count() / 2f);
                    for (int j = 0; j < normalMembers.Count; j++) {
                        Character factionMember = normalMembers[j];
                        if (j < halfMembers) {
                            ApplyFactionTypeRelatedEffectToMember(faction, factionMember);
                        }
                    }
                }
            }
            yield return null;
        }
        private void ApplyFactionTypeRelatedEffectToMember(Faction p_faction, Character p_member) {
            switch (p_faction.factionType.type) {
                case FACTION_TYPE.Demon_Cult:
                    //No need to process demon cult members, since adding of cultist trait is handled by DemonCult.ProcessNewMember
                    break;
                case FACTION_TYPE.Vampire_Clan:
                    p_member.traitContainer.AddTrait(p_member, "Vampire");
                    break;
                case FACTION_TYPE.Lycan_Clan:
                    LycanthropeData lycanthropeData = new LycanthropeData(p_member);
                    p_member.traitContainer.AddTrait(p_member, "Lycanthrope");
                    break;
            }
        }

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
            if (character.homeSettlement.cityCenter.occupiedArea.biomeType == BIOMES.SNOW) {
                //Snow villagers starts with Cold Blooded
                character.traitContainer.AddTrait(character, "Cold Blooded");
            } else if (character.homeSettlement.cityCenter.occupiedArea.biomeType == BIOMES.DESERT) {
                //Desert villagers starts with Fire Proof
                character.traitContainer.AddTrait(character, "Fireproof");
            } else if (character.homeSettlement.cityCenter.occupiedArea.biomeType == BIOMES.GRASSLAND) {
                //Grassland villagers starts with Electric
                character.traitContainer.AddTrait(character, "Electric");
            } else if (character.homeSettlement.cityCenter.occupiedArea.biomeType == BIOMES.FOREST) {
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
                Trait buffOrNeutralTrait = character.traitContainer.GetTraitOrStatus<Trait>(chosenBuffOrNeutralTraitName);
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
            // if (index < totalCharacters/2) {
            //     //half of villagers are robust
            //     character.traitContainer.AddTrait(character, "Robust");
            // }
            if (index + 1 == totalCharacters) {
                //last villager is Evil.
                character.traitContainer.AddTrait(character, "Evil");
            } else {
                //all non evil characters are blessed
                character.traitContainer.AddTrait(character, "Blessed");
                character.traitContainer.AddTrait(character, "Robust");
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