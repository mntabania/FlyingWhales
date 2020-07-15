using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class CharacterClassManager : MonoBehaviour {

    public Dictionary<string, CharacterClass> classesDictionary { get; private set; }
    public Dictionary<string, List<CharacterClass>> identifierClasses { get; private set; }
    public List<CharacterClass> normalCombatantClasses { get; private set; }
    public List<CharacterClass> allClasses { get; private set; }


    //private Dictionary<string, System.Type[]> traitIdlePlans = new Dictionary<string, Type[]>() {
    //    { "Berserked",
    //        new Type[]{
    //            typeof(BerserkBehaviour),
    //        }
    //    },
    //    { "Glutton",
    //        new Type[]{
    //            typeof(GluttonBehaviour),
    //        }
    //    },
    //    { "SerialKiller",
    //        new Type[]{
    //            typeof(SerialKillerBehaviour),
    //        }
    //    },
    //    { "Suicidal",
    //        new Type[]{
    //            typeof(SuicidalBehaviour),
    //        }
    //    },
    //};

    public void Initialize() {
        ConstructAllClasses();
    }

    #region Classes
    private void ConstructAllClasses() {
        classesDictionary = new Dictionary<string, CharacterClass>();
        allClasses = new List<CharacterClass>();
        //normalClasses = new Dictionary<string, CharacterClass>();
        //uniqueClasses = new Dictionary<string, CharacterClass>();
        //beastClasses = new Dictionary<string, CharacterClass>();
        //demonClasses = new Dictionary<string, CharacterClass>();
        normalCombatantClasses = new List<CharacterClass>();
        identifierClasses = new Dictionary<string, List<CharacterClass>>();
        identifierClasses.Add("All", new List<CharacterClass>());
        string path = $"{UtilityScripts.Utilities.dataPath}CharacterClasses/";
        string[] classes = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < classes.Length; i++) {
            CharacterClass currentClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText(classes[i]));
            //currentClass.ConstructData();
            classesDictionary.Add(currentClass.className, currentClass);
            allClasses.Add(currentClass);
            //if(currentClass.identifier == "Normal") {
            //    normalClasses.Add(currentClass.className, currentClass);
            //    //if (!identifierClasses.ContainsKey(currentClass.identifier)) {
            //    //    identifierClasses.Add(currentClass.identifier, normalClasses);
            //    //}
            //}else if (currentClass.identifier == "Unique") {
            //    uniqueClasses.Add(currentClass.className, currentClass);
            //    //if (!identifierClasses.ContainsKey(currentClass.identifier)) {
            //    //    identifierClasses.Add(currentClass.identifier, uniqueClasses);
            //    //}
            //} else if (currentClass.identifier == "Beast") {
            //    beastClasses.Add(currentClass.className, currentClass);
            //    //if (!identifierClasses.ContainsKey(currentClass.identifier)) {
            //    //    identifierClasses.Add(currentClass.identifier, beastClasses);
            //    //}
            //} else if (currentClass.identifier == "Demon") {
            //    demonClasses.Add(currentClass.className, currentClass);
            //    //if (!identifierClasses.ContainsKey(currentClass.identifier)) {
            //    //    identifierClasses.Add(currentClass.identifier, demonClasses);
            //    //}
            //}
            //if (!identifierClasses.ContainsKey(currentClass.identifier)) {
            //    identifierClasses.Add(currentClass.identifier, new List<CharacterClass>() { currentClass });
            //} else {
            //    identifierClasses[currentClass.identifier].Add(currentClass);
            //}
            //identifierClasses["All"].Add(currentClass);

            if (currentClass.IsCombatant() && currentClass.identifier == "Normal" && currentClass.className != "Hero") { //TODO: Change checking of Hero, cannot change identifier since hero might be expected to join parties.
                normalCombatantClasses.Add(currentClass);
            }
        }

    }
    public CharacterClass CreateNewCharacterClass(string className) {
        if (classesDictionary.ContainsKey(className)) {
            return classesDictionary[className].CreateNewCopy();
        }
        return null;
    }
    public CharacterClass GetRandomCombatant() {
        return normalCombatantClasses[UnityEngine.Random.Range(0, normalCombatantClasses.Count)];
    }
    //public string GetRandomClassByIdentifier(string identifier) {
    //    if (identifierClasses.ContainsKey(identifier)) {
    //        List<CharacterClass> list = identifierClasses[identifier];
    //        return list[UnityEngine.Random.Range(0, list.Count)].className;
    //    }
    //    return string.Empty;
    //}
    #endregion

    #region Class Idle Plans
    //public System.Type[] GetTraitBehaviourComponents(string traitName) {
    //    if (traitIdlePlans.ContainsKey(traitName)) {
    //        return traitIdlePlans[traitName];
    //    }
    //    return null;
    //}
    //private string DefaultClassIdlePlan(Character character) {
    //    string log = "Default Class Idle Plan for " + character.name;
    //    if (character.faction.id != FactionManager.Instance.neutralFaction.id) {
    //        log += "\n-" + character.name + " has a faction";
    //        if (character.previousCurrentActionNode != null && character.previousCurrentActionNode.action.goapType == INTERACTION_TYPE.RETURN_HOME && character.currentStructure == character.homeStructure) {
    //            log += "\n-" + character.name + " is in home structure and just returned home";
    //            TileObject deskOrTable = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
    //            log += "\n-Sit if there is still an unoccupied Table or Desk in the current location";
    //            if (deskOrTable != null) {
    //                log += "\n  -" + character.name + " will do action Sit on " + deskOrTable.ToString();
    //                character.PlanIdle(INTERACTION_TYPE.SIT, deskOrTable);
    //            } else {
    //                log += "\n-Otherwise, stand idle";
    //                log += "\n  -" + character.name + " will do action Stand";
    //                character.PlanIdle(INTERACTION_TYPE.STAND, character);
    //            }
    //            return log;
    //        } else if (character.currentStructure == character.homeStructure) {
    //            log += "\n-" + character.name + " is in home structure and previous action is not returned home";
    //            TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);

    //            log += "\n-If it is Early Night, 35% chance to go to the current Inn and then set it as the Base Structure for 2.5 hours";
    //            if (currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //                int chance = UnityEngine.Random.Range(0, 100);
    //                log += "\n  -RNG roll: " + chance;
    //                if (chance < 35) {
    //                    //StartGOAP(INTERACTION_TYPE.DRINK, null, GOAP_CATEGORY.IDLE);
    //                    LocationStructure structure = character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.INN);
    //                    if (structure != null) {
    //                        log += "\n  -Early Night: " + character.name + " will go to Inn and set Base Structure for 2.5 hours";
    //                        character.PlanIdle(INTERACTION_TYPE.VISIT, character, new object[] { structure });
    //                        //LocationGridTile gridTile = structure.GetRandomTile();
    //                        //marker.GoTo(gridTile, () => trapStructure.SetStructureAndDuration(structure, GameManager.Instance.GetTicksBasedOnHour(2) + GameManager.Instance.GetTicksBasedOnMinutes(30)));
    //                        return log;
    //                    } else {
    //                        log += "\n  -No Inn Structure in the npcSettlement";
    //                    }
    //                }
    //            } else {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //            }
    //            log += "\n-Otherwise, if it is Lunch Time or Afternoon, 25% chance to nap if there is still an unoccupied Bed in the house";
    //            if (currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //                int chance = UnityEngine.Random.Range(0, 100);
    //                log += "\n  -RNG roll: " + chance;
    //                if (chance < 25) {
    //                    TileObject bed = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
    //                    if (bed != null) {
    //                        if (character.traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
    //                            log += "\n  -Character is vampiric, cannot do nap action";
    //                        } else {
    //                            log += "\n  -Afternoon: " + character.name + " will do action Nap on " + bed.ToString();
    //                            character.PlanIdle(INTERACTION_TYPE.NAP, bed);
    //                            return log;
    //                        }
    //                    } else {
    //                        log += "\n  -No unoccupied bed in the current structure";
    //                    }
    //                }
    //            } else {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //            }
    //            log += "\n-Otherwise, if it is Morning or Afternoon or Early Night, and the character has a positive relationship with someone currently Paralyzed or Catatonic, 30% chance to Check Out one at random";
    //            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //                int chance = UnityEngine.Random.Range(0, 100);
    //                log += "\n  -RNG roll: " + chance;
    //                if (chance < 30) {
    //                    Character chosenCharacter = character.GetParalyzedOrCatatonicCharacterToCheckOut();
    //                    if (chosenCharacter != null) {
    //                        log += "\n  -Will Check Out character " + chosenCharacter.name;
    //                        character.PlanIdle(new GoapEffect(GOAP_EFFECT_CONDITION.IN_VISION, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), chosenCharacter);
    //                        return log;
    //                    } else {
    //                        log += "\n  -No available character to check out ";
    //                    }
    //                }
    //            } else {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //            }
    //            log += "\n-Otherwise, if it is Morning or Lunch Time or Afternoon or Early Night, 25% chance to enter Stroll Outside Mode for 1 hour";
    //            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //                int chance = UnityEngine.Random.Range(0, 100);
    //                log += "\n  -RNG roll: " + chance;
    //                if (chance < 25) {
    //                    log += "\n  -Morning, Afternoon, or Early Night: " + character.name + " will enter Stroll Outside Mode";
    //                    character.PlanIdleStrollOutside(character.currentStructure);
    //                    return log;
    //                }
    //            } else {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //            }
    //            log += "\n-Otherwise, if it is Morning or Afternoon, 25% chance to someone with a positive relationship in current location and then set it as the Base Structure for 2.5 hours";
    //            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //                int chance = UnityEngine.Random.Range(0, 100);
    //                log += "\n  -RNG roll: " + chance;
    //                if (chance < 25) {
    //                    List<Relatable> positiveRelatables = character.relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_EFFECT.POSITIVE);
    //                    if (positiveRelatables.Count > 0) {
    //                        LocationStructure targetStructure = null;
    //                        while (positiveRelatables.Count > 0 && targetStructure == null) {
    //                            int index = UnityEngine.Random.Range(0, positiveRelatables.Count);
    //                            Relatable chosenRelatable = positiveRelatables[index];
    //                            if (chosenRelatable is AlterEgoData) {
    //                                targetStructure = (chosenRelatable as AlterEgoData).owner.homeStructure;
    //                            }
    //                            if (targetStructure == null) {
    //                                positiveRelatables.RemoveAt(index);
    //                            } else if (targetStructure == character.homeStructure) {
    //                                targetStructure = null;
    //                                positiveRelatables.RemoveAt(index);
    //                            }
    //                        }
    //                        if (targetStructure != null) {
    //                            log += "\n  -Morning or Afternoon: " + character.name + " will go to dwelling of character with positive relationship and set Base Structure for 2.5 hours";
    //                            character.PlanIdle(INTERACTION_TYPE.VISIT, character, new object[] { targetStructure });
    //                            //LocationGridTile gridTile = chosenCharacter.homeStructure.GetRandomTile();
    //                            //marker.GoTo(gridTile, () => trapStructure.SetStructureAndDuration(chosenCharacter.homeStructure, GameManager.Instance.GetTicksBasedOnHour(2) + GameManager.Instance.GetTicksBasedOnMinutes(30)));
    //                            return log;
    //                        } else {
    //                            log += "\n  -No positive relationship with home structure";
    //                        }
    //                    } else {
    //                        log += "\n  -No character with positive relationship";
    //                    }
    //                }
    //            } else {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //            }
    //            //if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
    //            //    int chance = UnityEngine.Random.Range(0, 100);
    //            //    if (chance < 15) {
    //            //        log += "\n-Morning or Afternoon: " + name + " will do action Daydream";
    //            //        PlanIdle(INTERACTION_TYPE.DAYDREAM, this);
    //            //        return log;
    //            //    }
    //            //}
    //            //int guitarChance = UnityEngine.Random.Range(0, 100);
    //            //if (guitarChance < 15) {
    //            //    TileObject guitar = GetUnoccupiedHomeTileObject(TILE_OBJECT_TYPE.GUITAR);
    //            //    if (guitar != null) {
    //            //        log += "\n-" + name + " will do action Play Guitar on " + guitar.ToString();
    //            //        PlanIdle(INTERACTION_TYPE.PLAY_GUITAR, guitar);
    //            //        return log;
    //            //    }
    //            //}

    //            log += "\n-Otherwise, sit if there is still an unoccupied Table or Desk";
    //            TileObject deskOrTable = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
    //            if (deskOrTable != null) {
    //                log += "\n  -" + character.name + " will do action Sit on " + deskOrTable.ToString();
    //                character.PlanIdle(INTERACTION_TYPE.SIT, deskOrTable);
    //                return log;
    //            } else {
    //                log += "\n  -No unoccupied Table or Desk";
    //            }

    //            log += "\n-Otherwise, stand idle";
    //            log += "\n  -" + character.name + " will do action Stand";
    //            character.PlanIdle(INTERACTION_TYPE.STAND, character);
    //            //PlanIdleStroll(currentStructure);
    //            return log;
    //        } else if ((character.currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA || character.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || character.currentStructure.structureType == STRUCTURE_TYPE.CEMETERY || character.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER) && character.specificLocation == character.homeNpcSettlement) {
    //            log += "\n-" + character.name + " is in the Work NPCSettlement/Wilderness/Cemetery/City Center of home location";

    //            log += "\n-If it is Morning or Afternoon, 25% chance to enter Stroll Outside Mode";
    //            TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);
    //            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //                int chance = UnityEngine.Random.Range(0, 100);
    //                log += "\n  -RNG roll: " + chance;
    //                if (chance < 25) {
    //                    log += "\n  -Morning or Afternoon: " + character.name + " will enter Stroll Outside State";
    //                    character.PlanIdleStrollOutside(character.currentStructure);
    //                    return log;
    //                }
    //            } else {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //            }
    //            log += "\n-Otherwise, if it is Morning or Afternoon or Early Night, and the character has a positive relationship with someone currently Paralyzed or Catatonic, 30% chance to Check Out one at random";
    //            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //                int chance = UnityEngine.Random.Range(0, 100);
    //                log += "\n  -RNG roll: " + chance;
    //                if (chance < 30) {
    //                    Character chosenCharacter = character.GetParalyzedOrCatatonicCharacterToCheckOut();
    //                    if (chosenCharacter != null) {
    //                        log += "\n  -Will Check Out character " + chosenCharacter.name;
    //                        character.PlanIdle(new GoapEffect(GOAP_EFFECT_CONDITION.IN_VISION, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), chosenCharacter);
    //                        return log;
    //                    } else {
    //                        log += "\n  -No available character to check out ";
    //                    }
    //                }
    //            } else {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //            }
    //            log += "\n-Otherwise, return home";
    //            log += "\n  -" + character.name + " will do action Return Home";
    //            character.PlanIdleReturnHome();
    //            return log;
    //        } else if (character.trapStructure.structure != null && character.currentStructure == character.trapStructure.structure) {
    //            log += "\n-" + character.name + "'s Base Structure is not empty and current structure is the Base Structure";
    //            log += "\n-15% chance to trigger a Chat conversation if there is anyone chat-compatible in range";
    //            int chance = UnityEngine.Random.Range(0, 100);
    //            log += "\n  -RNG roll: " + chance;
    //            if (chance < 15) {
    //                if (character.marker.inVisionCharacters.Count > 0) {
    //                    bool hasForcedChat = false;
    //                    for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
    //                        Character targetCharacter = character.marker.inVisionCharacters[i];
    //                        if (character.ChatCharacter(targetCharacter, 100)) {
    //                            log += "\n  -Chat with: " + targetCharacter.name;
    //                            hasForcedChat = true;
    //                            break;
    //                        }
    //                    }
    //                    if (hasForcedChat) {
    //                        return log;
    //                    } else {
    //                        log += "\n  -Could not chat with anyone in vision";
    //                    }
    //                } else {
    //                    log += "\n  -No characters in vision";
    //                }
    //            }
    //            log += "\n-Sit if there is still an unoccupied Table or Desk";
    //            TileObject deskOrTable = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
    //            if (deskOrTable != null) {
    //                log += "\n  -" + character.name + " will do action Sit on " + deskOrTable.ToString();
    //                character.PlanIdle(INTERACTION_TYPE.SIT, deskOrTable);
    //                return log;
    //            } else {
    //                log += "\n  -No unoccupied Table or Desk";
    //            }
    //            log += "\n-Otherwise, stand idle";
    //            log += "\n  -" + character.name + " will do action Stand";
    //            character.PlanIdle(INTERACTION_TYPE.STAND, character);
    //            return log;
    //        } else if (((character.currentStructure.structureType == STRUCTURE_TYPE.DWELLING && character.currentStructure != character.homeStructure)
    //            || character.currentStructure.structureType == STRUCTURE_TYPE.INN
    //            || character.currentStructure.structureType == STRUCTURE_TYPE.WAREHOUSE
    //            || character.currentStructure.structureType == STRUCTURE_TYPE.PRISON
    //            || character.currentStructure.structureType == STRUCTURE_TYPE.CEMETERY
    //            || character.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER)
    //            && character.trapStructure.structure == null) {
    //            log += "\n-" + character.name + " is in another Dwelling/Inn/Warehouse/Prison/Cemetery/City Center and Base Structure is empty";
    //            log += "\n-100% chance to return home";
    //            character.PlanIdleReturnHome();
    //            return log;
    //        } else if (character.specificLocation != character.homeNpcSettlement && character.trapStructure.structure == null) {
    //            log += "\n-" + character.name + " is in another npcSettlement and Base Structure is empty";
    //            log += "\n-100% chance to return home";
    //            character.PlanIdleReturnHome();
    //            return log;
    //        }
    //    } else {
    //        //Unaligned NPC Idle
    //        log += "\n-" + character.name + " has no faction";
    //        if (!character.isAtHomeRegion) {
    //            log += "\n-" + character.name + " is in another npcSettlement";
    //            log += "\n-100% chance to return home";
    //            character.PlanIdleReturnHome();
    //            return log;
    //        } else {
    //            log += "\n-" + character.name + " is in home npcSettlement";
    //            log += "\n-If it is Morning or Afternoon, 25% chance to play";
    //            TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);
    //            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //                int chance = UnityEngine.Random.Range(0, 100);
    //                log += "\n  -RNG roll: " + chance;
    //                if (chance < 25) {
    //                    log += "\n  -Morning or Afternoon: " + character.name + " will do action Play";
    //                    character.PlanIdle(INTERACTION_TYPE.PLAY, character);
    //                    return log;
    //                }
    //            } else {
    //                log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
    //            }

    //            log += "\n-Otherwise, enter stroll mode";
    //            log += "\n  -" + character.name + " will enter Stroll Mode";
    //            character.PlanIdleStroll(character.currentStructure);
    //            return log;
    //        }
    //    }
    //    return log;
    //}
    #endregion
}
