﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Reflection;
using System.IO;
using EZObjectPools;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Logs;
using UnityEngine.Assertions;
using UnityEngine.UI;
using FullSerializer;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

namespace UtilityScripts {
    public class Utilities : MonoBehaviour {
        public static readonly System.Random Rng = new System.Random();
        private static int _lastFactionColorIndex;
        private static int _lastLogID;
        private static int _lastFactionID;
        private static int _lastCharacterID;
        private static int _lastAreaID;
        //private static int _lastPartyID;
        private static int _lastTileObjectID;
        private static int _lastStructureID;
        private static int _lastRegionID;
        private static int _lastJobID;
        private static int _lastBurningSourceID;
        private static int _lastGoapPlanID;
        public static LANGUAGES defaultLanguage = LANGUAGES.ENGLISH;
        public static string dataPath => $"{Application.streamingAssetsPath}/Data/";
        public static string gameSavePath => $"{Application.persistentDataPath}/Ruinarch Game Saves/";
        public static string tempPath => $"{gameSavePath}/Temp/";
        public static string tempZipPath => $"{gameSavePath}/Temp/Temp/";

        private static readonly Dictionary<string, string> pluralExceptions = new Dictionary<string, string>() {
            { "man", "men" },
            { "woman", "women" },
            { "child", "children" },
            { "tooth", "teeth" },
            { "foot", "feet" },
            { "mouse", "mice" },
            { "belief", "beliefs" } };
        
        public static int SetID<T>(T obj) {
            if (obj is Log) {
                _lastLogID += 1;
                return _lastLogID;
            }
            if (obj is Faction) {
                _lastFactionID += 1;
                return _lastFactionID;
            }
            if (obj is Character || obj is PreCharacterData) {
                _lastCharacterID += 1;
                return _lastCharacterID;
            }
            if (obj is BaseSettlement) {
                _lastAreaID += 1;
                return _lastAreaID;
            }
            //if (obj is Party) {
            //    _lastPartyID += 1;
            //    return _lastPartyID;
            //}
            if (obj is TileObject) {
                _lastTileObjectID += 1;
                return _lastTileObjectID;
            }
            if (obj is LocationStructure) {
                _lastStructureID += 1;
                return _lastStructureID;
            }
            if (obj is Region) {
                _lastRegionID += 1;
                return _lastRegionID;
            }
            if (obj is JobQueueItem) {
                _lastJobID += 1;
                return _lastJobID;
            }
            if (obj is BurningSource) {
                _lastBurningSourceID += 1;
                return _lastBurningSourceID;
            }
            if (obj is GoapPlan) {
                _lastGoapPlanID += 1;
                return _lastGoapPlanID;
            }
            return 0;
        }

        //A checker is added before setting the last id of any object so that we can be sure that the last id value is always the highest number
        //This is mostly needed when loading data from save file because we don't know the order it will be loaded so we must be sure that last id is the highest number not the id of the last object to load
        public static int SetID<T>(T obj, int idToUse) {
            if (obj is Log) {
                if (_lastLogID <= idToUse) { _lastLogID = idToUse; }
            } else if (obj is Faction) {
                if (_lastFactionID <= idToUse) { _lastFactionID = idToUse; }
            } else if (obj is Character) {
                if (_lastCharacterID <= idToUse) { _lastCharacterID = idToUse; }
            } else if (obj is BaseSettlement) {
                if (_lastAreaID <= idToUse) { _lastAreaID = idToUse; }
            } 
            //else if (obj is Party) {
            //    if (_lastPartyID <= idToUse) { _lastPartyID = idToUse; }
            //} 
            else if (obj is LocationStructure) {
                if (_lastStructureID <= idToUse) { _lastStructureID = idToUse; }
            } else if (obj is TileObject) {
                if (_lastTileObjectID <= idToUse) { _lastTileObjectID = idToUse; }
            } else if (obj is Region) {
                if (_lastRegionID <= idToUse) { _lastRegionID = idToUse; }
            } else if (obj is JobQueueItem) {
                if (_lastJobID <= idToUse) { _lastJobID = idToUse; }
            } else if (obj is BurningSource) {
                if (_lastBurningSourceID <= idToUse) { _lastBurningSourceID = idToUse; }
            }
            return idToUse;
        }

        public static string GetNewUniqueID() {
            return Guid.NewGuid().ToString();
        }

        #region Pathfinding
        public static List<Point> EvenNeighbours {
            get {
                return new List<Point> {
                    new Point(-1, 1),
                    new Point(0, 1),
                    new Point(1, 0),
                    new Point(0, -1),
                    new Point(-1, -1),
                    new Point(-1, 0),

                };
            }
        }
        public static List<Point> OddNeighbours {
            get {
                return new List<Point> {
                    new Point(0, 1),
                    new Point(1, 1),
                    new Point(1, 0),
                    new Point(1, -1),
                    new Point(0, -1),
                    new Point(-1, 0),
                };
            }
        }
        public static Dictionary<GridNeighbourDirection, Point> gridPossibleNeighbours = new Dictionary<GridNeighbourDirection, Point>() {
            {GridNeighbourDirection.North, new Point(0,1) },
            {GridNeighbourDirection.South, new Point(0,-1) },
            {GridNeighbourDirection.West, new Point(-1,0) },
            {GridNeighbourDirection.East, new Point(1,0) },
            {GridNeighbourDirection.North_West, new Point(-1,1) },
            {GridNeighbourDirection.North_East, new Point(1,1) },
            {GridNeighbourDirection.South_West, new Point(-1,-1) },
            {GridNeighbourDirection.South_East, new Point(1,-1) },
        };
        #endregion

        #region Color Utilities
        public static Color darkGreen = new Color(0f / 255f, 100f / 255f, 0f / 255f);
        public static Color lightGreen = new Color(124f / 255f, 252f / 255f, 0f / 255f);
        public static Color darkRed = new Color(139f / 255f, 0f / 255f, 0f / 255f);
        public static Color lightRed = new Color(255f / 255f, 0f / 255f, 0f / 255f);
        public static Color[] factionColorCycle = new Color[] {
            new Color32(0xDB, 0x00, 0x00, 0x91), // Red DB000091
            new Color32(0x00, 0x51, 0xF3, 0x91), // Blue 0051F391
            new Color32(0xFF, 0xFF, 0x00, 0x91), // Yellow FFFF00A0
            new Color32(0xFF, 0xFF, 0xFF, 0x91), // White FFFFFF91
            new Color32(0x78, 0xFF, 0x2B, 0x91), // Mint Green 78FF2B91
            new Color32(0xF9, 0x5B, 0xCD, 0x91), // Pink F95BCD91
            new Color32(0x1D, 0x1D, 0x1D, 0x91), // Black 1D1D1D91
            new Color32(0x0F, 0xDD, 0xF0, 0xA0), // Cyan 0FDDF0A0
            new Color32(0xFF, 0x8E, 0x00, 0xA0), // Orange FF8E00A3
            new Color32(0x8D, 0x12, 0xCE, 0x91), // Violet 8D12CE91
            new Color32(0x0E, 0x77, 0x1B, 0x91), // Dark Green 0E771B91
            new Color32(0x8A, 0x07, 0x07, 0x94), // Dark Red 8A070794
            new Color32(0x03, 0x21, 0x8E, 0xA0), // Dark Blue 03218EA0
            new Color32(0xA6, 0x56, 0x00, 0xB9), // Brown A65600B9 
            new Color32(0x8A, 0x80, 0xFD, 0x94), // Light Violet 8A80FD91
            new Color32(0xBC, 0xFF, 0x00, 0xA6) // Yellow Green BCFF00A6
        };
        public static Dictionary<BIOMES, Color> biomeColor = new Dictionary<BIOMES, Color>() {
            {BIOMES.GRASSLAND, new Color(34f/255f, 139f/255f, 34f/255f)},
            {BIOMES.BARE, new Color(106f/255f, 108f/255f, 59f/255f)},
            {BIOMES.DESERT, new Color(93f/255f, 79f/255f, 69f/255f)},
            {BIOMES.FOREST, new Color(34f/255f, 139f/255f, 34f/255f)},
            {BIOMES.SNOW, new Color(255f/255f, 255f/255f, 255f/255f)},
            {BIOMES.TUNDRA, new Color(106f/255f, 108f/255f, 59f/255f)},
            //{BIOMES.WOODLAND, new Color(34f/255f, 139f/255f, 34f/255f)}
        };
        public static Color GetColorForFaction() {
            Color chosenColor = factionColorCycle[_lastFactionColorIndex];
            _lastFactionColorIndex += 1;
            if (_lastFactionColorIndex >= factionColorCycle.Length) {
                _lastFactionColorIndex = 0;
            }
            return chosenColor;
        }
        #endregion

        #region New Logs
        public static string RemoveRichText(string text) {
            StringBuilder sb = new StringBuilder(text.Length);
            bool tag = false;
            for (int index = 0; index < text.Length; index++) {
                char c = text[index];
                if (tag) {
                    if (c == '>') {
                        tag = false;
                    }
                }
                else {
                    if (c == '<') {
                        tag = true;
                    }
                    else {
                        sb.Append(c);
                    }
                }
            }
            return sb.ToString();
        }
        public static string LogReplacer(string unReplacedLog, List<LogFillerStruct> fillers) {
            if (string.IsNullOrEmpty(unReplacedLog)) {
                return string.Empty;
            }
            string newText = unReplacedLog;

            if (!string.IsNullOrEmpty(newText)) {
                string[] words = SplitAndKeepDelimiters(newText, ' ', '.', ',', '\'', '!', '"', ':');
                for (int i = 0; i < words.Length; i++) {
                    var replacedWord = string.Empty;
                    string word = words[i];
                    if (word.StartsWith("%") && (word.EndsWith("%") || word.EndsWith("@"))) { //OBJECT
                        replacedWord = CustomStringReplacer(word, fillers);
                    } else if (word.StartsWith("%") && (word.EndsWith("a") || word.EndsWith("b"))) { //PRONOUN
                        replacedWord = CustomPronounReplacer(word, fillers);
                    } else if (IsWordAVerb(word)) {
                        replacedWord = ColorizeActionInLog(word);
                    }
                    if (!string.IsNullOrEmpty(replacedWord)) {
                        words[i] = replacedWord;
                    }
                }
                newText = string.Empty;
                for (int i = 0; i < words.Length; i++) {
                    newText = $"{newText}{words[i]}";
                }
                newText = newText.Trim(' ');
            }
            return newText;
        }
        private static string CustomStringReplacer(string wordToBeReplaced, List<LogFillerStruct> fillers) {
            string wordToReplace = string.Empty;
            string strLogIdentifier = wordToBeReplaced.Substring(1, wordToBeReplaced.Length - 2);
            LOG_IDENTIFIER identifier = logIdentifiers[strLogIdentifier];
            if (identifier == LOG_IDENTIFIER.CHARACTER_LIST_1 || identifier == LOG_IDENTIFIER.CHARACTER_LIST_2) {
                int listCount = 0;
                for (int i = 0; i < fillers.Count; i++) {
                    LogFillerStruct logFiller = fillers[i];
                    if (logFiller.identifier == identifier) {
                        if (wordToReplace != string.Empty) {
                            wordToReplace = $"{wordToReplace}, ";
                        }
                        object obj = logFiller.GetObjectForFiller();
                        if(obj != null) {
                            if (obj is Character character) {
                                wordToReplace = $"{wordToReplace}{character.visuals.GetCharacterStringIcon()}" +
                                                $"{ColorizeName($"<b><link={logFiller.GetLinkText()}>{logFiller.value}</link></b>", CharacterManager.Instance.GetCharacterNameColorHex(character))}";
                            } else if (obj is Faction faction) {
                                wordToReplace = $"{wordToReplace}{ColorizeName($"<b><link={i}>{logFiller.value}</link></b>", FactionManager.Instance.GetFactionNameColorHex())}";
                            } else {
                                wordToReplace = $"{wordToReplace}{ColorizeName($"<b><link={logFiller.GetLinkText()}>{logFiller.value}</link></b>")}";    
                            }
                        } else {
                            wordToReplace = $"{wordToReplace}{ColorizeName($"<b>{logFiller.value}</b>")}";
                        }
                        listCount++;
                    }
                }
                if(listCount > 1) {
                    //Add 'and' after last comma
                    int commaLastIndex = wordToReplace.LastIndexOf(',');
                    wordToReplace = wordToReplace.Insert(commaLastIndex + 1, " and");
                }
            } else if (identifier == LOG_IDENTIFIER.APPEND) {
                for (int i = 0; i < fillers.Count; i++) {
                    LogFillerStruct logFiller = fillers[i];
                    if (logFiller.identifier == identifier) {
                        wordToReplace = StringReplacer(logFiller.value, fillers);
                        break;
                    }
                }
            } else {
                for (int i = 0; i < fillers.Count; i++) {
                    LogFillerStruct logFiller = fillers[i];
                    if (logFiller.identifier == identifier) {
                        object obj = logFiller.GetObjectForFiller();
                        if (obj != null) {
                            if (obj is Wilderness) {
                                //do not add link to wilderness, since it should not be selectable
                                wordToReplace = $"<b>{logFiller.value}</b>";
                            } else {
                                wordToReplace = $"<b><link={logFiller.GetLinkText()}>{logFiller.value}</link></b>";
                                if (obj is Character c) {
                                    wordToReplace = ColorizeName(wordToReplace, CharacterManager.Instance.GetCharacterNameColorHex(c));
                                } else if (obj is Faction) {
                                    wordToReplace = ColorizeName(wordToReplace, FactionManager.Instance.GetFactionNameColorHex());
                                } else if (obj is TileObject) {
                                    wordToReplace = ColorizeName(wordToReplace);
                                }
                                if (obj is Character character) {
                                    wordToReplace = $"{character.visuals.GetCharacterStringIcon()}{wordToReplace}";
                                }    
                            }
                            
                        } else {
                            wordToReplace = $"<b>{logFiller.value}</b>";
                        }
                        
                        break;
                    }
                }
            }
            return wordToReplace;
         }
        private static string CustomPronounReplacer(string wordToBeReplaced, List<LogFillerStruct> fillers) {
            LOG_IDENTIFIER identifier = logIdentifiers[wordToBeReplaced.Substring(1, wordToBeReplaced.Length - 2)];
            string identifierAsString = identifier.ToString();
            string wordToReplace = string.Empty;
            char lastCharacter = identifierAsString.Last(); 
            
            LOG_IDENTIFIER logIdentifier = LOG_IDENTIFIER.NONE;
            string logIdentifierAsString = identifierAsString.Substring(0, identifierAsString.Length - 10);
            if (System.Enum.TryParse(logIdentifierAsString, out logIdentifier)) {
                string pronouns = GetPronoun(lastCharacter, wordToBeReplaced.Last());
                for (int i = 0; i < fillers.Count; i++) {
                    if (fillers[i].identifier == logIdentifier) {
                        wordToReplace = PronounReplacer(pronouns, fillers[i].GetObjectForFiller());
                        break;
                    }
                } 
            }
            if(wordToReplace != string.Empty) { 
                return wordToReplace;
            } 
            return wordToBeReplaced;
        }
        public static string StringReplacer(string text, List<LogFillerStruct> fillers) {
            if (string.IsNullOrEmpty(text)) {
                return string.Empty;
            }
            string newText = text;

            if (!string.IsNullOrEmpty(newText)) {
                string[] words = SplitAndKeepDelimiters(newText, delimiters);
                for (int i = 0; i < words.Length; i++) {
                    var replacedWord = string.Empty;
                    string word = words[i];
                    if (word.StartsWith("%") && (word.EndsWith("%") || word.EndsWith("@"))) { //OBJECT
                        replacedWord = CustomStringReplacer(word, fillers);
                    } else if (word.StartsWith("%") && (word.EndsWith("a") || word.EndsWith("b"))) { //PRONOUN
                        replacedWord = CustomPronounReplacer(word, fillers);
                    } else if (IsWordAVerb(word)) {
                        replacedWord = ColorizeActionInLog(word);
                    }
                    if (!string.IsNullOrEmpty(replacedWord)) {
                        words[i] = replacedWord;
                    }
                }
                newText = string.Empty;
                for (int i = 0; i < words.Length; i++) {
                    newText = $"{newText}{words[i]}";
                }
                newText = newText.Trim(' ');
            }
            return newText;
        }
        #endregion
        
        #region Log Utilities
        // public static string LogReplacer(Log log, bool ignoreLogText = false) {
        //     if (!log.hasValue) {
        //         return string.Empty;
        //     }
        //     if(log.logText != string.Empty && ignoreLogText == false) {
        //         return log.logText;
        //     }
        //     string newText;
        //     if (string.IsNullOrEmpty(log.message)) {
        //         newText = LocalizationManager.Instance.GetLocalizedValue(log.category, log.file, log.key);
        //     } else {
        //         newText = log.message;
        //     }
        //
        //     //bool hasPeriod = newText.EndsWith(".");
        //
        //     if (!string.IsNullOrEmpty(newText)) {
        //         string[] words = SplitAndKeepDelimiters(newText, ' ', '.', ',', '\'', '!', '"', ':');
        //         for (int i = 0; i < words.Length; i++) {
        //             var replacedWord = string.Empty;
        //             string word = words[i];
        //             if (word.StartsWith("%") && (word.EndsWith("%") || word.EndsWith("@"))) { //OBJECT
        //                 replacedWord = CustomStringReplacer(word, log.fillers);
        //             } else if (word.StartsWith("%") && (word.EndsWith("a") || word.EndsWith("b"))) { //PRONOUN
        //                 replacedWord = CustomPronounReplacer(word, log.fillers);
        //             } else if (IsWordAVerb(word)) {
        //                 replacedWord = ColorizeActionInLog(word);
        //             }
        //             if (!string.IsNullOrEmpty(replacedWord)) {
        //                 words[i] = replacedWord;
        //             }
        //         }
        //         newText = string.Empty;
        //         for (int i = 0; i < words.Length; i++) {
        //             newText = $"{newText}{words[i]}";
        //         }
        //         newText = newText.Trim(' ');
        //     }
        //     log.SetLogText(newText);
        //     return newText;
        // }
        private static bool IsWordAVerb(string word) {
            if (InteractionManager.Instance.ignoredActionNames.Contains(word)) {
                return false;
            }
            if (InteractionManager.Instance.forcedActionNames.Contains(word)) {
                return true;
            }
            // if (word.EndsWith("ing") || word.EndsWith("ed")) {
            //     return true;
            // } else 
            if (InteractionManager.Instance.actionNames.Contains(word)) {
                return true;
            }
            return false;
        }
        public static char[] delimiters = new char[] {' ', '.', ',', '\'', '!', '"', ':'};
        public static string StringReplacer(string text, List<LogFiller> fillers) {
            if (string.IsNullOrEmpty(text)) {
                return string.Empty;
            }
            string newText = text;

            if (!string.IsNullOrEmpty(newText)) {
                string[] words = SplitAndKeepDelimiters(newText, delimiters);
                for (int i = 0; i < words.Length; i++) {
                    var replacedWord = string.Empty;
                    string word = words[i];
                    if (word.StartsWith("%") && (word.EndsWith("%") || word.EndsWith("@"))) { //OBJECT
                        replacedWord = CustomStringReplacer(word, fillers);
                    } else if (word.StartsWith("%") && (word.EndsWith("a") || word.EndsWith("b"))) { //PRONOUN
                        replacedWord = CustomPronounReplacer(word, fillers);
                    } else if (IsWordAVerb(word)) {
                        replacedWord = ColorizeActionInLog(word);
                    }
                    if (!string.IsNullOrEmpty(replacedWord)) {
                        words[i] = replacedWord;
                    }
                }
                newText = string.Empty;
                for (int i = 0; i < words.Length; i++) {
                    newText = $"{newText}{words[i]}";
                }
                newText = newText.Trim(' ');
            }
            return newText;
        }
        // public static string LogDontReplace(Log log) {
        //     if (log == null) {
        //         return string.Empty;
        //     }
        //     string newText = LocalizationManager.Instance.GetLocalizedValue(log.category, log.file, log.key);
        //     return newText;
        // }
        public static string LogDontReplace(string category, string file, string key) {
            return LocalizationManager.Instance.GetLocalizedValue(category, file, key);
        }
        private static string CustomPronounReplacer(string wordToBeReplaced, List<LogFiller> objectLog) {
            LOG_IDENTIFIER identifier = logIdentifiers[wordToBeReplaced.Substring(1, wordToBeReplaced.Length - 2)];
            string identifierAsString = identifier.ToString();
            string wordToReplace = string.Empty;
            char lastCharacter = identifierAsString.Last(); 
            
            LOG_IDENTIFIER logIdentifier = LOG_IDENTIFIER.NONE;
            string logIdentifierAsString = identifierAsString.Substring(0, identifierAsString.Length - 10);
            if (System.Enum.TryParse(logIdentifierAsString, out logIdentifier)) {
                string pronouns = GetPronoun(lastCharacter, wordToBeReplaced.Last());
                for (int i = 0; i < objectLog.Count; i++) {
                    if (objectLog[i].identifier == logIdentifier) {
                        wordToReplace = PronounReplacer(pronouns, objectLog[i].obj);
                        break;
                    }
                }
            }
            if(wordToReplace != string.Empty) {
                return wordToReplace;
            }
            return wordToBeReplaced;
        }
        private static string CustomStringReplacer(string wordToBeReplaced, List<LogFiller> objectLog) {
            string wordToReplace = string.Empty;
            string strLogIdentifier = wordToBeReplaced.Substring(1, wordToBeReplaced.Length - 2);
            LOG_IDENTIFIER identifier = logIdentifiers[strLogIdentifier];
            if (identifier == LOG_IDENTIFIER.CHARACTER_LIST_1 || identifier == LOG_IDENTIFIER.CHARACTER_LIST_2) {
                int listCount = 0;
                for (int i = 0; i < objectLog.Count; i++) {
                    LogFiller logFiller = objectLog[i];
                    if (logFiller.identifier == identifier) {
                        if (wordToReplace != string.Empty) {
                            wordToReplace = $"{wordToReplace}, ";
                        }
                        if(logFiller.obj != null) {
                            if (logFiller.obj is Character character) {
                                wordToReplace = $"{wordToReplace}{character.visuals.GetCharacterStringIcon()}" +
                                                $"{ColorizeName($"<b><link={i}>{logFiller.value}</link></b>", CharacterManager.Instance.GetCharacterNameColorHex(character))}";
                            } else if (logFiller.obj is Faction faction) {
                                wordToReplace = $"{wordToReplace}{ColorizeName($"<b><link={i}>{logFiller.value}</link></b>", FactionManager.Instance.GetFactionNameColorHex())}";
                            } else {
                                wordToReplace = $"{wordToReplace}{ColorizeName($"<b><link={i}>{logFiller.value}</link></b>")}";    
                            }
                        } else {
                            wordToReplace = $"{wordToReplace}{ColorizeName($"<b>{logFiller.value}</b>")}";
                        }
                        listCount++;
                    }
                }
                if(listCount > 1) {
                    //Add 'and' after last comma
                    int commaLastIndex = wordToReplace.LastIndexOf(',');
                    wordToReplace = wordToReplace.Insert(commaLastIndex + 1, " and");
                }
            } else if (identifier == LOG_IDENTIFIER.APPEND) {
                for (int i = 0; i < objectLog.Count; i++) {
                    LogFiller logFiller = objectLog[i];
                    if (logFiller.identifier == identifier) {
                        wordToReplace = StringReplacer(logFiller.value, objectLog);
                        break;
                    }
                }
            } else {
                for (int i = 0; i < objectLog.Count; i++) {
                    LogFiller logFiller = objectLog[i];
                    if (logFiller.identifier == identifier) {
                        if (logFiller.obj != null) {
                            if (logFiller.obj is Wilderness) {
                                //do not add link to wilderness, since it should not be selectable
                                wordToReplace = $"<b>{logFiller.value}</b>";
                            } else {
                                wordToReplace = $"<b><link={i}>{logFiller.value}</link></b>";
                                if (logFiller.obj is Character c) {
                                    wordToReplace = ColorizeName(wordToReplace, CharacterManager.Instance.GetCharacterNameColorHex(c));
                                } else if (logFiller.obj is Faction) {
                                    wordToReplace = ColorizeName(wordToReplace, FactionManager.Instance.GetFactionNameColorHex());
                                } else if (logFiller.obj is TileObject) {
                                    wordToReplace = ColorizeName(wordToReplace);
                                }
                                if (logFiller.obj is Character character) {
                                    wordToReplace = $"{character.visuals.GetCharacterStringIcon()}{wordToReplace}";
                                }
                            }
                        } else {
                            wordToReplace = $"<b>{logFiller.value}</b>";
                        }
                        
                        break;
                    }
                }
            }
            //if (wordToBeReplaced.EndsWith("@")) {

            //}
            //else if (wordToBeReplaced.EndsWith("%")) {
            //    if (identifier.ToString().Contains("LIST")) {
            //        int listCount = 0;
            //        for (int i = 0; i < objectLog.Count; i++) {
            //            if (objectLog[i].identifier == identifier) {
            //                if (wordToReplace != string.Empty) {
            //                    wordToReplace += ", ";
            //                }
            //                wordToReplace += "<b>" + objectLog[i].value + "</b>";
            //                listCount++;
            //            }
            //        }
            //        if (listCount > 1) {
            //            //Add 'and' after last comma
            //            int commaLastIndex = wordToReplace.LastIndexOf(',');
            //            wordToReplace = wordToReplace.Insert(commaLastIndex + 1, " and");
            //        }
            //    } else {
            //        for (int i = 0; i < objectLog.Count; i++) {
            //            if (objectLog[i].identifier == identifier) {
            //                wordToReplace = "<b>" + objectLog[i].value + "</b>";
            //                break;
            //            }
            //        }
            //    }
            //}

            return wordToReplace;

        }
        //	public static string CustomStringReplacer(string wordToBeReplaced, LogFiller objectLog, int index){
        //		string wordToReplace = string.Empty;
        //		string value = string.Empty;
        //
        //		if(wordToBeReplaced.Contains("@")){
        //			wordToReplace = "[url=" + index.ToString() + "][b]" + objectLog.value + "[/b][/url]";
        //		}else{
        //			wordToReplace = objectLog.value;
        //		}
        //
        //		return wordToReplace;
        //
        //	}
        public static Dictionary<string, LOG_IDENTIFIER> logIdentifiers = new Dictionary<string, LOG_IDENTIFIER>() {
            {"00", LOG_IDENTIFIER.ACTIVE_CHARACTER},
            {"01", LOG_IDENTIFIER.FACTION_1},
            {"02", LOG_IDENTIFIER.FACTION_LEADER_1},
            {"04", LOG_IDENTIFIER.LANDMARK_1},
            {"05", LOG_IDENTIFIER.PARTY_1},
            {"06", LOG_IDENTIFIER.STRUCTURE_1},
            {"07", LOG_IDENTIFIER.STRUCTURE_2},
            {"08", LOG_IDENTIFIER.STRUCTURE_3},
            {"10", LOG_IDENTIFIER.TARGET_CHARACTER},
            {"11", LOG_IDENTIFIER.FACTION_2},
            {"12", LOG_IDENTIFIER.FACTION_LEADER_2},
            {"14", LOG_IDENTIFIER.LANDMARK_2},
            {"15", LOG_IDENTIFIER.PARTY_2},
            {"20", LOG_IDENTIFIER.CHARACTER_3},
            {"21", LOG_IDENTIFIER.FACTION_3},
            {"22", LOG_IDENTIFIER.FACTION_LEADER_3},
            {"24", LOG_IDENTIFIER.LANDMARK_3},
            {"25", LOG_IDENTIFIER.PARTY_3},
            {"81", LOG_IDENTIFIER.ACTION_DESCRIPTION},
            {"82", LOG_IDENTIFIER.QUEST_NAME},
            {"83", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_S},
            {"84", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_O},
            {"85", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_P},
            {"86", LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_R},
            {"87", LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_S},
            {"88", LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_O},
            {"89", LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_P},
            {"90", LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_R},
            {"91", LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_S},
            {"92", LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_O},
            {"93", LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_P},
            {"94", LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_R},
            {"95", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_S},
            {"96", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_O},
            {"97", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_P},
            {"98", LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_R},
            {"100", LOG_IDENTIFIER.TASK},
            {"101", LOG_IDENTIFIER.DATE},
            {"102", LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_S},
            {"103", LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_O},
            {"104", LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_P},
            {"105", LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_R},
            {"106", LOG_IDENTIFIER.OTHER},
            {"107", LOG_IDENTIFIER.ITEM_1},
            {"108", LOG_IDENTIFIER.ITEM_2},
            {"109", LOG_IDENTIFIER.ITEM_3},
            {"110", LOG_IDENTIFIER.COMBAT_FILLER},
            {"111", LOG_IDENTIFIER.STRING_1},
            {"112", LOG_IDENTIFIER.STRING_2},
            {"113", LOG_IDENTIFIER.MINION_1},
            {"114", LOG_IDENTIFIER.MINION_1_PRONOUN_S},
            {"115", LOG_IDENTIFIER.MINION_1_PRONOUN_O},
            {"116", LOG_IDENTIFIER.MINION_1_PRONOUN_P},
            {"117", LOG_IDENTIFIER.MINION_1_PRONOUN_R},
            {"118", LOG_IDENTIFIER.MINION_2},
            {"119", LOG_IDENTIFIER.MINION_2_PRONOUN_S},
            {"120", LOG_IDENTIFIER.MINION_2_PRONOUN_O},
            {"121", LOG_IDENTIFIER.MINION_2_PRONOUN_P},
            {"122", LOG_IDENTIFIER.MINION_2_PRONOUN_R},
            {"123", LOG_IDENTIFIER.CHARACTER_LIST_1},
            {"124", LOG_IDENTIFIER.CHARACTER_LIST_2},
            {"125", LOG_IDENTIFIER.APPEND},
            {"126", LOG_IDENTIFIER.OTHER_2},
        };
        public static Dictionary<LOG_IDENTIFIER, string> logIdentifierStrings = new Dictionary<LOG_IDENTIFIER, string>() {
            {LOG_IDENTIFIER.ACTIVE_CHARACTER, "00"},
            {LOG_IDENTIFIER.FACTION_1, "01"},
            {LOG_IDENTIFIER.FACTION_LEADER_1, "02"},
            {LOG_IDENTIFIER.LANDMARK_1, "04"},
            {LOG_IDENTIFIER.PARTY_1, "05"},
            {LOG_IDENTIFIER.STRUCTURE_1, "06"},
            {LOG_IDENTIFIER.STRUCTURE_2, "07"},
            {LOG_IDENTIFIER.STRUCTURE_3, "08"},
            {LOG_IDENTIFIER.TARGET_CHARACTER, "10"},
            {LOG_IDENTIFIER.FACTION_2, "11"},
            {LOG_IDENTIFIER.FACTION_LEADER_2, "12"},
            {LOG_IDENTIFIER.LANDMARK_2, "14"},
            {LOG_IDENTIFIER.PARTY_2, "15"},
            {LOG_IDENTIFIER.CHARACTER_3, "20"},
            {LOG_IDENTIFIER.FACTION_3, "21"},
            {LOG_IDENTIFIER.FACTION_LEADER_3, "22"},
            {LOG_IDENTIFIER.LANDMARK_3, "24"},
            {LOG_IDENTIFIER.PARTY_3, "25"},
            {LOG_IDENTIFIER.ACTION_DESCRIPTION, "81"},
            {LOG_IDENTIFIER.QUEST_NAME, "82"},
            {LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_S, "83"},
            {LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_O, "84"},
            {LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_P, "85"},
            {LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_R, "86"},
            {LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_S, "87"},
            {LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_O, "88"},
            {LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_P, "89"},
            {LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_R, "90"},
            {LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_S, "91"},
            {LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_O, "92"},
            {LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_P, "93"},
            {LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_R, "94"},
            {LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_S, "95"},
            {LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_O, "96"},
            {LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_P, "97"},
            {LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_R, "98"},
            {LOG_IDENTIFIER.TASK, "100"},
            {LOG_IDENTIFIER.DATE, "101"},
            {LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_S, "102"},
            {LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_O, "103"},
            {LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_P, "104"},
            {LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_R, "105"},
            {LOG_IDENTIFIER.OTHER, "106"},
            {LOG_IDENTIFIER.ITEM_1, "107"},
            {LOG_IDENTIFIER.ITEM_2, "108"},
            {LOG_IDENTIFIER.ITEM_3, "109"},
            {LOG_IDENTIFIER.COMBAT_FILLER, "110"},
            {LOG_IDENTIFIER.STRING_1, "111"},
            {LOG_IDENTIFIER.STRING_2, "112"},
            {LOG_IDENTIFIER.MINION_1, "113"},
            {LOG_IDENTIFIER.MINION_1_PRONOUN_S, "114"},
            {LOG_IDENTIFIER.MINION_1_PRONOUN_O, "115"},
            {LOG_IDENTIFIER.MINION_1_PRONOUN_P, "116"},
            {LOG_IDENTIFIER.MINION_1_PRONOUN_R, "117"},
            {LOG_IDENTIFIER.MINION_2, "118"},
            {LOG_IDENTIFIER.MINION_2_PRONOUN_S, "119"},
            {LOG_IDENTIFIER.MINION_2_PRONOUN_O, "120"},
            {LOG_IDENTIFIER.MINION_2_PRONOUN_P, "121"},
            {LOG_IDENTIFIER.MINION_2_PRONOUN_R, "122"},
            {LOG_IDENTIFIER.CHARACTER_LIST_1, "123"},
            {LOG_IDENTIFIER.CHARACTER_LIST_2, "124"},
            {LOG_IDENTIFIER.APPEND, "125"},
            {LOG_IDENTIFIER.OTHER_2, "126"},
        };
        public static string GetStringForIdentifier(LOG_IDENTIFIER identifier) {
            foreach (KeyValuePair<string, LOG_IDENTIFIER> item in logIdentifiers) {
                if (item.Value == identifier) {
                    string key = item.Key;
                    key = $"%{key}";
                    if (identifier.ToString().Contains("PRONOUN")) {
                        key += "b";
                        return key;
                    }
                    switch (identifier) {
                        case LOG_IDENTIFIER.ACTIVE_CHARACTER:
                        case LOG_IDENTIFIER.FACTION_1:
                        case LOG_IDENTIFIER.FACTION_LEADER_1:
                        case LOG_IDENTIFIER.LANDMARK_1:
                        case LOG_IDENTIFIER.PARTY_1:
                        case LOG_IDENTIFIER.TARGET_CHARACTER:
                        case LOG_IDENTIFIER.FACTION_2:
                        case LOG_IDENTIFIER.FACTION_LEADER_2:
                        case LOG_IDENTIFIER.LANDMARK_2:
                        case LOG_IDENTIFIER.PARTY_2:
                        case LOG_IDENTIFIER.CHARACTER_3:
                        case LOG_IDENTIFIER.FACTION_3:
                        case LOG_IDENTIFIER.FACTION_LEADER_3:
                        case LOG_IDENTIFIER.LANDMARK_3:
                        case LOG_IDENTIFIER.PARTY_3:
                        case LOG_IDENTIFIER.TASK:
                        case LOG_IDENTIFIER.COMBAT_FILLER:
                            key += "@";
                            break;
                        default:
                            key += "%";
                            break;
                    }
                    return key;
                }
            }
            return string.Empty;
        }
        public static string PronounReplacer(string word, object genderSubject) {
            //		string pronoun = Utilities.GetStringBetweenTwoChars (word, '_', '_');
            string[] pronouns = word.Split('/');
            GENDER gender = GENDER.MALE;
            if (genderSubject is Character) {
                gender = (genderSubject as Character).gender;
            }else if (genderSubject is Minion) {
                gender = (genderSubject as Minion).character.gender;
            }
            if (gender == GENDER.MALE) {
                if (pronouns.Length > 0) {
                    if (!string.IsNullOrEmpty(pronouns[0])) {
                        return pronouns[0];
                    }
                }
            } else {
                if (pronouns.Length > 1) {
                    if (!string.IsNullOrEmpty(pronouns[0])) {
                        return pronouns[1];
                    }
                }
            }
            return string.Empty;
        }
        public static string GetPronoun(string type, string caseIdentifier) {
            if (type == "S") {
                if (caseIdentifier == "a") {
                    return "He/She";
                }
                return "he/she";
            } else if (type == "O") {
                if (caseIdentifier == "a") {
                    return "Him/Her";
                }
                return "him/her";
            } else if (type == "P") {
                if (caseIdentifier == "a") {
                    return "His/Her";
                }
                return "his/her";
            } else if (type == "R") {
                if (caseIdentifier == "a") {
                    return "Himself/Herself";
                }
                return "himself/herself";
            }
            return string.Empty;
        }
        public static string GetPronoun(char type, char caseIdentifier) {
            if (type == 'S') {
                if (caseIdentifier == 'a') {
                    return "He/She";
                }
                return "he/she";
            } else if (type == 'O') {
                if (caseIdentifier == 'a') {
                    return "Him/Her";
                }
                return "him/her";
            } else if (type == 'P') {
                if (caseIdentifier == 'a') {
                    return "His/Her";
                }
                return "his/her";
            } else if (type == 'R') {
                if (caseIdentifier == 'a') {
                    return "Himself/Herself";
                }
                return "himself/herself";
            }
            return string.Empty;
        }
        public static string GetPronounString(GENDER gender, PRONOUN_TYPE type, bool isUppercaseFirstLetter) {
            string pronoun = string.Empty;
            if(gender == GENDER.MALE) {
                if (type == PRONOUN_TYPE.SUBJECTIVE) {
                    pronoun = "he";
                } else if (type == PRONOUN_TYPE.OBJECTIVE) {
                    pronoun = "him";
                } else if (type == PRONOUN_TYPE.POSSESSIVE) {
                    pronoun = "his";
                } else if (type == PRONOUN_TYPE.REFLEXIVE) {
                    pronoun = "himself";
                }
            } else {
                if (type == PRONOUN_TYPE.SUBJECTIVE) {
                    pronoun = "she";
                } else if (type == PRONOUN_TYPE.OBJECTIVE) {
                    pronoun = "her";
                } else if (type == PRONOUN_TYPE.POSSESSIVE) {
                    pronoun = "her";
                } else if (type == PRONOUN_TYPE.REFLEXIVE) {
                    pronoun = "herself";
                }
            }
            if (isUppercaseFirstLetter) {
                pronoun = FirstLetterToUpperCase(pronoun);
            }
            return pronoun;
        }
        //	public static string PronounReplacer(string word, object genderSubject){
        //		string pronoun = Utilities.GetStringBetweenTwoChars (word, '_', '_');
        //		string[] pronouns = pronoun.Split ('/');
        //
        //		if(genderSubject is Citizen){
        //			GENDER gender = ((Citizen)genderSubject).gender;
        //			if(gender == GENDER.MALE){
        //				if(pronouns.Length > 0){
        //					if(!string.IsNullOrEmpty(pronouns[0])){
        //						return pronouns [0];
        //					}
        //				}
        //			}else{
        //				if (pronouns.Length > 1) {
        //					if (!string.IsNullOrEmpty (pronouns [0])) {
        //						return pronouns [1];
        //					}
        //				}
        //			}
        //
        //
        //		}
        //		return string.Empty;
        //	}
        public static string GetStringBetweenTwoChars(string word, char first, char last) {
            int indexFirst = word.IndexOf(first);
            int indexLast = word.LastIndexOf(last);

            if (indexFirst == -1 || indexLast == -1) {
                return string.Empty;
            }
            indexFirst += 1;
            if (indexFirst >= word.Length) {
                return string.Empty;
            }

            return word.Substring(indexFirst, (indexLast - indexFirst));
        }
        public static List<string> GetAllWordsInAString(string wordToFind, string text) {
            List<string> words = new List<string>();
            string word = string.Empty;
            int index = 0;
            int wordCount = 0;
            int startingIndex = index;
            while (index != -1) {
                index = text.IndexOf(wordToFind, startingIndex);
                if (index != -1) {
                    startingIndex = index + 1;
                    if (startingIndex > text.Length - 1) {
                        startingIndex = text.Length - 1;
                    }

                    wordCount = 0;
                    for (int i = index; i < text.Length; i++) {
                        if (text[i] != ' ') {
                            wordCount += 1;
                        } else {
                            break;
                        }
                    }
                    word = text.Substring(index, wordCount);
                    words.Add(word);
                }

            }
            return words;
        }
        public static string[] SplitAndKeepDelimiters(string s, params char[] delimiters) {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(s)) {
                int iFirst = 0;
                do {
                    int iLast = s.IndexOfAny(delimiters, iFirst);
                    if (iLast >= 0) {
                        if (iLast > iFirst)
                            parts.Add(s.Substring(iFirst, iLast - iFirst)); //part before the delimiter
                        parts.Add(new string(s[iLast], 1));//the delimiter
                        iFirst = iLast + 1;
                        continue;
                    }

                    //No delimiters were found, but at least one character remains. Add the rest and stop.
                    parts.Add(s.Substring(iFirst, s.Length - iFirst));
                    break;

                } while (iFirst < s.Length);
            }

            return parts.ToArray();
        }
        public static string SplitStringIntoNewLines(string s, params char[] delimiters) {
            string newText = string.Empty;
            if (string.IsNullOrEmpty(s) == false) {
                string[] reasons = s.Split(delimiters);
                for (int i = 0; i < reasons.Length; i++) {
                    string reason = reasons[i];
                    newText += $"{reason}\n";
                }
            }
            return newText;
        }
        #endregion

        #region Sprite Utilities
        public static void SetSpriteSortingLayer(SpriteRenderer sprite, string layerName) {
            sprite.sortingLayerName = layerName;
        }
        public static void SetLayerRecursively(GameObject go, int layerNumber) {
            foreach (Transform trans in go.GetComponentsInChildren<Transform>(true)) {
                trans.gameObject.layer = layerNumber;
            }
        }
        public static List<BIOMES> biomeLayering = new List<BIOMES>() {
            BIOMES.GRASSLAND,
            //BIOMES.WOODLAND,
            BIOMES.TUNDRA,
            BIOMES.FOREST,
            BIOMES.DESERT,
            BIOMES.SNOW
        };
        #endregion

        #region String Utilities
        public static string InvalidColorize(string str) {
            return $"<color=#FE3E83>{str}</color>";
        }
        public static string MonsterIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Monster_Icon\">";
        }
        public static string VillagerIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Villager_Icon\">";
        }
        public static string ManaIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Mana_Icon\">";
        }
        public static string UpgradeArrowIcon() {
            return "<sprite=\"Text_Sprites\" name=\"upgrade_arrow_icon\">";
        }
        public static string YellowDotIcon() {
            return "<sprite=\"Text_Sprites\" name=\"exclamation\">";
        }
        public static string ChargesIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Charges_Icon\">";
        }
        public static string BonusChargesIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Bonus_Charges_Icon\">";
        }
        public static string ThreatIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Threat_Icon\">";
        }
        public static string CooldownIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Cooldown_Icon\">";
        }
        public static string CultistIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Cultist_Icon\">";
        }
        public static string LeaderIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Leader_Icon\"> ";
        }
        public static string DemonIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Demon_Icon\">";
        }
        public static string UndeadIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Undead_Icon\">";
        }
        public static string ChaoticEnergyIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Plague_Icon\">";
        }
        public static string SpiritEnergyIcon() {
            return "<sprite=\"Text_Sprites\" name=\"spirit_energy_icon\">";
        }
        public static string AttackIcon() {
            return "<sprite=\"Text_Sprites\" name=\"stats_attack_icon\">";
        }
        public static string EarthIcon() {
            return "<sprite=\"Text_Sprites\" name=\"element_earth_icon\">";
        }
        public static string NormalIcon() {
            return "<sprite=\"Text_Sprites\" name=\"element_normal_icon\">";
        }
        public static string HealthIcon() {
            return "<sprite=\"Text_Sprites\" name=\"stats_health_icon\">";
        }
        public static string SpeedIcon() {
            return "<sprite=\"Text_Sprites\" name=\"stats_speed_icon\">";
        }
        public static string WaterIcon() {
            return "<sprite=\"Text_Sprites\" name=\"element_water_icon\">";
        }
        public static string PoisonIcon() {
            return "<sprite=\"Text_Sprites\" name=\"element_poison_icon\">";
        }
        public static string IceIcon() {
            return "<sprite=\"Text_Sprites\" name=\"element_ice_icon\">";
        }
        public static string ResistanceIcon() {
            return "<sprite=\"Text_Sprites\" name=\"resistance_icon\">";
        }
        public static string PiercingIcon() {
            return "<sprite=\"Text_Sprites\" name=\"piercing_icon\">";
        }
        public static string WindIcon() {
            return "<sprite=\"Text_Sprites\" name=\"element_wind_icon\">";
        }
        public static string FireIcon() {
            return "<sprite=\"Text_Sprites\" name=\"element_fire_icon\">";
        }
        public static string ElectricIcon() {
            return "<sprite=\"Text_Sprites\" name=\"element_electric_icon\">";
        }
        public static string RatmanIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Ratman_Icon\">";
        }
        public static string VillageIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Village_Icon\">";
        }
        public static string TileObjectIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Tile_Object_Icon\">";
        }
        public static string StructureIcon() {
            return "<sprite=\"Text_Sprites\" name=\"Structure_Icon\">";
        }
        //public static string BonusChargesIcon() {
        //    return "<sprite=\"Text_Sprites\" name=\"bonus_charges_icon\">";
        //}
        //public static string ObjectIcon() {
        //    return "<sprite=\"Text_Sprites\" name=\"icon_object\">";
        //}
        //public static string VillageIcon() {
        //    return "<sprite=\"Text_Sprites\" name=\"icon_village\">";
        //}
        //public static string StructureIcon() {
        //    return "<sprite=\"Text_Sprites\" name=\"icon_structure\">";
        //}
        public static string ColorizeAction(string actionString) {
            return $"<color=#f87f43>{actionString}</color>";
        }
        public static string ColorizeActionInLog(string actionString) {
            return $"<color=#FFC700>{actionString}</color>";
        }
        public static string ColorizeInvalidText(string p_text) {
            return $"<color=#FE3E83>{p_text}</color>";
        }
        public static string ColorizeUpgradeText(string p_text) {
            return $"<color=#81FF00>{p_text}</color>";
        }
        public static string ColorizeName(string name) {
            return $"<color=#f8ed43>{name}</color>";
        }
        public static string ColorizeName(string name, string color) {
            return $"<color=#{color}>{name}</color>";
        }
        public static string ColorizeAndBoldName(string name) {
            return $"<b>{ColorizeName(name)}</b>";
        }
        public static string ColorizeAndBoldName(string name, string color) {
            return $"<b>{ColorizeName(name, color)}</b>";
        }
        public static string ColorizeSpellTitle(string name) {
            return $"<color=#D7AE50>{name}</color>";
        }
        public static string GetRichTextIconForElement(ELEMENTAL_TYPE p_type) {
            string formatted = string.Empty;
            switch (p_type) {
                case ELEMENTAL_TYPE.Normal:
                formatted = $"{ColorizeSpellTitle($"{NormalIcon()}")}";
                break;
                case ELEMENTAL_TYPE.Poison:
                formatted = $"{ColorizeSpellTitle($"{PoisonIcon()}")}";
                break;
                case ELEMENTAL_TYPE.Fire:
                formatted = $"{ColorizeSpellTitle($"{FireIcon()}")}";
                break;
                case ELEMENTAL_TYPE.Water:
                formatted = $"{ColorizeSpellTitle($"{WaterIcon()}")}";
                break;
                case ELEMENTAL_TYPE.Wind:
                formatted = $"{ColorizeSpellTitle($"{WindIcon()}")}";
                break;
                case ELEMENTAL_TYPE.Ice:
                formatted = $"{ColorizeSpellTitle($"{IceIcon()}")}";
                break;
                case ELEMENTAL_TYPE.Electric:
                formatted = $"{ColorizeSpellTitle($"{ElectricIcon()}")}";
                break;
                case ELEMENTAL_TYPE.Earth:
                formatted = $"{ColorizeSpellTitle($"{EarthIcon()}")}";
                break;
            }
            return formatted;
        }
        public static string GetFirstFewEmotionsAndComafy(string emotionsStr, int emotionCount) {
            string[] emotions = emotionsStr.Split(' ');
            string trimmedEmotions = string.Empty;
            int obtainedEmotions = 0;
            for (int i = 0; i < emotions.Length; i++) {
                string emotion = emotions[i];
                if (string.IsNullOrEmpty(emotion) || string.IsNullOrWhiteSpace(emotion)) { continue; }
                trimmedEmotions += $"{emotion} ";
                obtainedEmotions++;
                if (obtainedEmotions == emotionCount) {
                    break; //only get up to 2 emotions
                }
            }
            return Comafy(trimmedEmotions);
        }
        public static string NormalizeNoSpaceString(string s) {
            string[] words = System.Text.RegularExpressions.Regex.Split(s, @"(?<!^)(?=[A-Z])");
            string normalizedString = Utilities.FirstLetterToUpperCase(words.First());
            for (int i = 1; i < words.Length; i++) {
                normalizedString += $" {words[i]}";
            }
            return normalizedString;
        }
        public static string NormalizeStringUpperCaseFirstLetterOnly(string s) {
            //s = s.ToLowerInvariant();
            //string[] words = s.Split('_');
            //string normalizedString = Utilities.FirstLetterToUpperCase(words[0]);
            //for (int i = 1; i < words.Length; i++) {
            //    normalizedString += " " + words[i];
            //}
            //return normalizedString;
            string normalizedString = s.Replace('_', ' ').ToLowerInvariant();
            normalizedString = FirstLetterToUpperCase(normalizedString);
            return normalizedString;
        }
        public static string NormalizeStringUpperCaseFirstLetters(string s) {
            //s = s.ToLowerInvariant();
            //string[] words = s.Split('_');
            //string normalizedString = Utilities.FirstLetterToUpperCase(words[0]);
            //for (int i = 1; i < words.Length; i++) {
            //    normalizedString += " " + Utilities.FirstLetterToUpperCase(words[i]);
            //}
            //return normalizedString;
            string normalizedString = s.Replace('_', ' ').ToLowerInvariant();
            normalizedString = UpperCaseFirstLetters(normalizedString);
            return normalizedString;
        }
        /// <summary>
        /// Convert a string with just words separated by spaces into a proper sentence.
        /// </summary>
        /// <param name="originalText">The original text.</param>
        /// <returns>A for</returns>
        public static string Comafy(string originalText) {
            string finalReaction = string.Empty;
            string[] words = originalText.Split(' ').Where(x => string.IsNullOrEmpty(x) == false).ToArray();
            for (int i = 0; i < words.Length; i++) {
                string word = words[i];
                if (i + 1 == words.Length) {
                    finalReaction += $"{word}";
                } else if (i + 2 == words.Length) {
                    finalReaction += $"{word} and ";
                } else {
                    finalReaction += $"{word}, ";
                }
            }
            finalReaction = finalReaction.Replace('_', ' ');
            return finalReaction;
        }
        public static string NormalizeStringUpperCaseFirstLettersNoSpace(string s) {
            s = s.ToLowerInvariant();
            string[] words = s.Split('_');
            string normalizedString = Utilities.FirstLetterToUpperCase(words[0]);
            for (int i = 1; i < words.Length; i++) {
                normalizedString += Utilities.FirstLetterToUpperCase(words[i]);
            }
            return normalizedString;
        }
        public static string NotNormalizedConversionEnumToString(string s) {
            return s.Replace('_', ' ');
        }
        public static string NotNormalizedConversionEnumToStringNoSpaces(string s) {
            s = s.Replace('_', ' ');
            return RemoveAllWhiteSpace(s);
        }
        public static string NotNormalizedConversionStringToEnum(string s) {
            return s.Replace(' ', '_');
        }
        public static string FirstLetterToUpperCase(string s) {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("There is no first letter");

            char[] a = s.ToCharArray();
            a[0] = char.ToUpperInvariant(a[0]);
            return new string(a);
        }
        public static string UpperCaseFirstLetters(string s) { //Upper case first letters following spaces
            //s = s.ToLowerInvariant();
            //string[] words = s.Split('_');
            //string normalizedString = Utilities.FirstLetterToUpperCase(words[0]);
            //for (int i = 1; i < words.Length; i++) {
            //    normalizedString += " " + Utilities.FirstLetterToUpperCase(words[i]);
            //}
            //return normalizedString;
            if (s.Length == 1) {
                return s.ToUpperInvariant();
            } else if (s.Length > 1) {
                char[] strCharacters = s.ToCharArray();
                if (char.IsLower(strCharacters[0])) { strCharacters[0] = char.ToUpperInvariant(strCharacters[0]); }
                for (int i = 1; i < strCharacters.Length; i++) {
                    if (strCharacters[i - 1] == ' ' && char.IsLower(strCharacters[i])) {
                        strCharacters[i] = char.ToUpperInvariant(strCharacters[i]);
                    }
                }
                return new string(strCharacters);
            }
            return s;
        }
        public static List<string> GetEnumChoices<T>(bool includeNone = false, params T[] exclude) {
            List<string> options = new List<string>();
            T[] values = (T[]) Enum.GetValues(typeof(T));
            for (int i = 0; i < values.Length; i++) {
                T currOption = values[i];
                string currString = currOption.ToString();
                if (!includeNone) { //do not include none
                    if (currString.Equals("NONE", StringComparison.InvariantCultureIgnoreCase)) {
                        continue;
                    }
                }
                if (exclude != null && exclude.Contains(currOption)) {
                    continue; //skip excluded items
                }
                options.Add(currString);
            }
            return options;
        }
        public static List<string> GetEnumChoices<T>(params T[] choices) {
            List<string> options = new List<string>();
            if (choices.Length == 0) {
                choices = (T[]) Enum.GetValues(typeof(T));    
            }
            for (int i = 0; i < choices.Length; i++) {
                T option = choices[i];
                string normalizedStr = NormalizeStringUpperCaseFirstLetters(option.ToString());
                options.Add(normalizedStr);
            }
            return options;
        }
        public static string GetPossessivePronounForCharacter(Character character, bool capitalized = true) {
            if (character.gender == GENDER.MALE) {
                if (capitalized) {
                    return "His";
                } else {
                    return "his";
                }
            } else {
                if (capitalized) {
                    return "Her";
                } else {
                    return "her";
                }
            }
        }
        public static List<string> ExtractFromString(string text, string startString, string endString) {
            List<string> matched = new List<string>();
            int indexStart = 0, indexEnd = 0;
            bool exit = false;
            while (!exit) {
                indexStart = text.IndexOf(startString);
                indexEnd = text.IndexOf(endString);
                if (indexStart != -1 && indexEnd != -1) {
                    matched.Add(text.Substring(indexStart + startString.Length,
                        indexEnd - indexStart - startString.Length));
                    text = text.Substring(indexEnd + endString.Length);
                } else
                    exit = true;
            }
            return matched;
        }
        public static string GetArticleForWord(string word, bool capitalized = false) {
            char firstCharacter = word.ToLowerInvariant().First();
            if (firstCharacter == 'a' || firstCharacter == 'e' || firstCharacter == 'i' || firstCharacter == 'o' || firstCharacter == 'u') {
                if (capitalized) {
                    return "An";
                }
                return "an";
            } else {
                if (capitalized) {
                    return "A";
                }
                return "a";
            }
        }
        public static string GetRelationshipPlural(RELATIONSHIP_TYPE rel) {
            switch (rel) {
                case RELATIONSHIP_TYPE.RELATIVE:
                    return "Relatives";
                case RELATIONSHIP_TYPE.LOVER:
                    return "Lovers";
                case RELATIONSHIP_TYPE.AFFAIR:
                    return "Affairs";
                default:
                    return NormalizeStringUpperCaseFirstLetters(rel.ToString());
            }
        }
        public static string RemoveAllWhiteSpace(string str) {
            var len = str.Length;
            var src = str.ToCharArray();
            int dstIdx = 0;

            for (int i = 0; i < len; i++) {
                var ch = src[i];
                switch (ch) {
                    case '\u0020':
                    case '\u00A0':
                    case '\u1680':
                    case '\u2000':
                    case '\u2001':
                    case '\u2002':
                    case '\u2003':
                    case '\u2004':
                    case '\u2005':
                    case '\u2006':
                    case '\u2007':
                    case '\u2008':
                    case '\u2009':
                    case '\u200A':
                    case '\u202F':
                    case '\u205F':
                    case '\u3000':
                    case '\u2028':
                    case '\u2029':
                    case '\u0009':
                    case '\u000A':
                    case '\u000B':
                    case '\u000C':
                    case '\u000D':
                    case '\u0085':
                        continue;

                    default:
                        src[dstIdx++] = ch;
                        break;
                }
            }
            return new string(src, 0, dstIdx);
        }
        public static string[] ConvertStringToArray(string str, char separator) {
            if (!string.IsNullOrEmpty(str)) {
                string[] arr = str.Split(separator);
                return arr;
            }
            return null;
        }
        public static string ConvertArrayToString(string[] str, char separator) {
            string joinedStr = string.Empty;
            if (str != null) {
                for (int i = 0; i < str.Length; i++) {
                    if (i > 0) {
                        joinedStr += separator;
                    }
                    joinedStr += str[i];
                }
            }
            return joinedStr;
        }
        public static string PluralizeString(string s) {
            if(s.Length <= 1) {
                return s;
            } else {
                if (pluralExceptions.ContainsKey(s.ToLowerInvariant())) {
                    return pluralExceptions[s.ToLowerInvariant()];
                } else if (s.EndsWith("y", StringComparison.OrdinalIgnoreCase) &&
                           !s.EndsWith("ay", StringComparison.OrdinalIgnoreCase) &&
                           !s.EndsWith("ey", StringComparison.OrdinalIgnoreCase) &&
                           !s.EndsWith("iy", StringComparison.OrdinalIgnoreCase) &&
                           !s.EndsWith("oy", StringComparison.OrdinalIgnoreCase) &&
                           !s.EndsWith("uy", StringComparison.OrdinalIgnoreCase)) {
                    return $"{s.Substring(0, s.Length - 1)}ies";
                } else if (s.EndsWith("us", StringComparison.InvariantCultureIgnoreCase)) {
                    //http://en.wikipedia.org/wiki/Plural_form_of_words_ending_in_-us
                    return $"{s}es";
                } else if (s.EndsWith("ss", StringComparison.InvariantCultureIgnoreCase)) {
                    return $"{s}es";
                } else if (s.EndsWith("s", StringComparison.InvariantCultureIgnoreCase)) {
                    return s;
                } else if (s.EndsWith("x", StringComparison.InvariantCultureIgnoreCase) ||
                           s.EndsWith("ch", StringComparison.InvariantCultureIgnoreCase) ||
                           s.EndsWith("sh", StringComparison.InvariantCultureIgnoreCase)) {
                    return $"{s}es";
                } else if (s.EndsWith("f", StringComparison.InvariantCultureIgnoreCase) && s.Length > 1) {
                    return $"{s.Substring(0, s.Length - 1)}ves";
                } else if (s.EndsWith("fe", StringComparison.InvariantCultureIgnoreCase) && s.Length > 2) {
                    return $"{s.Substring(0, s.Length - 2)}ves";
                } else if (!s.Equals("Shaman", StringComparison.InvariantCultureIgnoreCase)
                        && !s.Equals("Human", StringComparison.InvariantCultureIgnoreCase) 
                        && s.EndsWith("man", StringComparison.InvariantCultureIgnoreCase)) {
                    return $"{s.Substring(0, s.Length - 3)}men";
                } else {
                    return $"{s}s";
                }
            }
        }
        public static string FormulateTextFromEmotions(string emotions, Character actor, IPointOfInterest target, Character reactor) {
            if (string.IsNullOrEmpty(emotions) || string.IsNullOrWhiteSpace(emotions)) {
                //character had no reaction
                return actor == reactor ? "I know what I did." : "A proper response to this information has not been implemented yet.";
            } else {
                if (emotions == "aware") {
                    return $"{ColorizeAndBoldName(reactor.name)} already knows this.";
                } else {
                    string[] emotionsToActorAndTarget = emotions.Split('/');

                    string emotionsTowardsActor = emotionsToActorAndTarget.ElementAtOrDefault(0);
                    string emotionsTowardsTarget = emotionsToActorAndTarget.ElementAtOrDefault(1);

                    bool hasReactionToActor = string.IsNullOrEmpty(emotionsTowardsActor) == false;
                    bool hasReactionToTarget = string.IsNullOrEmpty(emotionsTowardsTarget) == false;

                    if (hasReactionToActor == false && hasReactionToTarget == false) {
                        //has no reactions to actor and target
                        return $"{reactor.visuals.GetCharacterStringIcon()}{ColorizeAndBoldName(reactor.name)} seemed Disinterested about this.";
                    } else {
                        if (hasReactionToActor) {
                            return $"{reactor.visuals.GetCharacterStringIcon()}{ColorizeAndBoldName(reactor.name)} seemed {GetFirstFewEmotionsAndComafy(emotionsTowardsActor, 2)} at {actor.visuals.GetCharacterStringIcon()}{ColorizeAndBoldName(actor.name)} after receiving the new information.";
                        }
                        if (hasReactionToTarget) {
                            if (target is Character targetCharacter) {
                                return $"{reactor.visuals.GetCharacterStringIcon()}{ColorizeAndBoldName(reactor.name)} seemed {GetFirstFewEmotionsAndComafy(emotionsTowardsTarget, 2)} at {targetCharacter.visuals.GetCharacterStringIcon()}{ColorizeAndBoldName(targetCharacter.name)} after receiving the new information.";
                            } else {
                                return $"{reactor.visuals.GetCharacterStringIcon()}{ColorizeAndBoldName(reactor.name)} seemed {ColorizeAndBoldName(target.name)} after receiving the new information.";
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }
        public static T Deserialize<T>(string text) {
            Encoding encoding = Encoding.UTF8;
            Stream stream = new MemoryStream(encoding.GetBytes(text));
            T result = Deserialize<T>(stream, encoding);
            stream.Dispose();
            return result;
        }
        public static T Deserialize<T>(Stream stream, Encoding encoding) {
            T result = default(T);
#if !UNITY_WSA || !UNITY_WINRT
            try {
                StreamReader reader = new StreamReader(stream, encoding);
                fsSerializer serializer = new fsSerializer();
                fsData data = fsJsonParser.Parse(reader.ReadToEnd());
                serializer.TryDeserialize(data, ref result);
                if (result == null) {
                    result = default(T);
                }
                reader.Dispose();
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
#else
			StreamReader reader = new StreamReader ( stream, encoding );
			result = JsonUtility.FromJson<T> ( reader.ReadToEnd () );
			reader.Dispose ();
#endif
            return result;
        }
        #endregion

        #region Weighted Dictionary
        public static Dictionary<T, int> MergeWeightedActionDictionaries<T>(Dictionary<T, int> dict1, Dictionary<T, int> dict2) {
            Dictionary<T, int> mergedDict = new Dictionary<T, int>();
            foreach (KeyValuePair<T, int> kvp in dict1) {
                T currKey = kvp.Key;
                int currValue = kvp.Value;
                if (dict2.ContainsKey(currKey)) {
                    currValue += dict2[currKey];
                }
                mergedDict.Add(currKey, currValue);
            }
            foreach (KeyValuePair<T, int> kvp in dict2) {
                T currKey = kvp.Key;
                int currValue = kvp.Value;
                if (dict1.ContainsKey(currKey)) {
                    currValue += dict1[currKey];
                }
                if (!mergedDict.ContainsKey(currKey)) {
                    mergedDict.Add(currKey, currValue);
                }

            }
            return mergedDict;
        }
        public static Dictionary<T, Dictionary<T, int>> MergeWeightedActionDictionaries<T>(Dictionary<T, Dictionary<T, int>> dict1, Dictionary<T, Dictionary<T, int>> dict2) {
            Dictionary<T, Dictionary<T, int>> mergedDict = new Dictionary<T, Dictionary<T, int>>();
            foreach (KeyValuePair<T, Dictionary<T, int>> kvp in dict1) {
                T currKey = kvp.Key;
                Dictionary<T, int> currValue = kvp.Value;
                if (dict2.ContainsKey(currKey)) {
                    currValue = MergeWeightedActionDictionaries<T>(currValue, dict2[currKey]);
                }
                mergedDict.Add(currKey, currValue);
            }
            foreach (KeyValuePair<T, Dictionary<T, int>> kvp in dict2) {
                T currKey = kvp.Key;
                Dictionary<T, int> currValue = kvp.Value;
                if (!mergedDict.ContainsKey(currKey)) {
                    if (dict1.ContainsKey(currKey)) {
                        currValue = MergeWeightedActionDictionaries<T>(currValue, dict1[currKey]);
                    }
                    mergedDict.Add(currKey, currValue);
                }
            }
            return mergedDict;
        }
        public static T PickRandomElementWithWeights<T>(Dictionary<T, int> weights) {
            int totalOfAllWeights = GetTotalOfWeights(weights);
            if(totalOfAllWeights > 0) {
                int chance = Rng.Next(0, totalOfAllWeights);
                int upperBound = 0;
                int lowerBound = 0;
                foreach (KeyValuePair<T, int> kvp in weights) {
                    T currElementType = kvp.Key;
                    int weightOfCurrElement = kvp.Value;
                    if (weightOfCurrElement <= 0) {
                        continue;
                    }
                    upperBound += weightOfCurrElement;
                    if (chance >= lowerBound && chance < upperBound) {
                        return currElementType;
                    }
                    lowerBound = upperBound;
                }
            }
            return default;
            //throw new Exception("Could not pick element in weights");
        }
        public static T PickRandomElementWithWeights<T>(Dictionary<T, float> weights) {
            float totalOfAllWeights = GetTotalOfWeights(weights);
            int chance = Rng.Next(0, (int) totalOfAllWeights);
            float upperBound = 0;
            float lowerBound = 0;
            foreach (KeyValuePair<T, float> kvp in weights) {
                T currElementType = kvp.Key;
                float weightOfCurrElement = kvp.Value;
                if (weightOfCurrElement <= 0) {
                    continue;
                }
                upperBound += weightOfCurrElement;
                if (chance >= lowerBound && chance < upperBound) {
                    return currElementType;
                }
                lowerBound = upperBound;
            }
            throw new Exception("Could not pick element in weights");
        }
        /*
     * This will return an array that has 2 elements 
     * of the same type. The 1st element is the first key in the dictionary, and the
     * 2nd element is the second key.
     * */
        public static T[] PickRandomElementWithWeights<T>(Dictionary<T, Dictionary<T, int>> weights) {
            int totalOfAllWeights = GetTotalOfWeights(weights);
            int chance = UnityEngine.Random.Range(0, totalOfAllWeights);
            int upperBound = 0;
            int lowerBound = 0;
            foreach (KeyValuePair<T, Dictionary<T, int>> kvp in weights) {
                T currElementType = kvp.Key;
                foreach (KeyValuePair<T, int> pair in kvp.Value) {
                    T otherElement = pair.Key;
                    int weightOfOtherElement = pair.Value;
                    if(weightOfOtherElement < 0) {
                        //Skip negative values
                        continue;
                    }
                    upperBound += weightOfOtherElement;
                    if (chance >= lowerBound && chance < upperBound) {
                        return new T[] { currElementType, otherElement };
                    }
                    lowerBound = upperBound;
                }
            }
            throw new Exception("Could not pick element in weights");
        }
        public static int GetTotalOfWeights<T>(Dictionary<T, Dictionary<T, int>> weights) {
            int totalOfAllWeights = 0;
            foreach (KeyValuePair<T, Dictionary<T, int>> kvp in weights) {
                foreach (KeyValuePair<T, int> pair in kvp.Value) {
                    if(pair.Value > 0) {
                        totalOfAllWeights += pair.Value;
                    }
                }
            }
            return totalOfAllWeights;
        }
        public static int GetTotalOfWeights<T>(Dictionary<T, int> weights) {
            //Must ignore negative values for getting total weight
            //Because for example if: A -> -50, and B -> 20, if you get the total, the answer will be negative, this will break the weight system since the random generation is from 0 - total weight
            return weights.Where(x => x.Value > 0).Sum(x => x.Value);
        }
        public static float GetTotalOfWeights<T>(Dictionary<T, float> weights) {
            float totalOfAllWeights = 0f;
            foreach (float weight in weights.Values) {
                if(weight > 0) {
                    totalOfAllWeights += weight;
                }
            }
            return totalOfAllWeights;
        }
        public static string GetWeightsSummary<T>(Dictionary<T, int> weights, string title = "Weights Summary: ") {
            string actionWeightsSummary = title;
            foreach (KeyValuePair<T, int> kvp in weights) {
                T key = kvp.Key;
                int value = kvp.Value;
                if (key is Character) {
                    actionWeightsSummary += $"\n{(key as Character).name} - {kvp.Value}";
                } else {
                    actionWeightsSummary += $"\n{kvp.Key} - {kvp.Value}";
                }

            }
            return actionWeightsSummary;
        }
        public static string GetWeightsSummary<T>(Dictionary<T, float> weights, string title = "Weights Summary: ") {
            string actionWeightsSummary = title;
            foreach (KeyValuePair<T, float> kvp in weights) {
                T key = kvp.Key;
                float value = kvp.Value;
                if (key is Character) {
                    actionWeightsSummary += $"\n{(key as Character).name} - {kvp.Value}";
                } else {
                    actionWeightsSummary += $"\n{kvp.Key} - {kvp.Value}";
                }

            }
            return actionWeightsSummary;
        }
        public static string GetWeightsSummary<T>(Dictionary<T, Dictionary<T, int>> weights, string title = "Weights Summary: ") {
            string actionWeightsSummary = title;
            foreach (KeyValuePair<T, Dictionary<T, int>> kvp in weights) {
                actionWeightsSummary += $"\n{kvp.Key} : ";
                foreach (KeyValuePair<T, int> pair in kvp.Value) {
                    actionWeightsSummary += $"\n     {pair.Key} - {pair.Value}";
                }
            }
            return actionWeightsSummary;
        }
        //public static string GetWeightsSummary(Dictionary<Kingdom, int> weights, string title = "Weights Summary: ") {
        //    string actionWeightsSummary = title;
        //    foreach (KeyValuePair<Kingdom, int> kvp in weights) {
        //        actionWeightsSummary += "\n" + kvp.Key.name + " - " + kvp.Value.ToString();
        //    }
        //    return actionWeightsSummary;
        //}

        //public static string GetWeightsSummary(Dictionary<Kingdom, Dictionary<Kingdom, int>> weights, string title = "Weights Summary: ") {
        //    string actionWeightsSummary = title;
        //    foreach (KeyValuePair<Kingdom, Dictionary<Kingdom, int>> kvp in weights) {
        //        actionWeightsSummary += "\n" + kvp.Key.name + " : ";
        //        foreach (KeyValuePair<Kingdom, int> pair in kvp.Value) {
        //            actionWeightsSummary += "\n     " + pair.Key.name + " - " + pair.Value.ToString();
        //        }
        //    }
        //    return actionWeightsSummary;
        //}
        #endregion

        #region Landmarks
        // public static BASE_LANDMARK_TYPE GetBaseLandmarkType(LANDMARK_TYPE landmarkType) {
        //     switch (landmarkType) {
        //         case LANDMARK_TYPE.CLAY:
        //         case LANDMARK_TYPE.LIMESTONE:
        //         case LANDMARK_TYPE.GRANITE:
        //         case LANDMARK_TYPE.MARBLE:
        //         case LANDMARK_TYPE.SILK:
        //         case LANDMARK_TYPE.COTTON:
        //         case LANDMARK_TYPE.FLAX:
        //         case LANDMARK_TYPE.CORN:
        //         case LANDMARK_TYPE.RICE:
        //         case LANDMARK_TYPE.PIGMEAT:
        //         case LANDMARK_TYPE.COWMEAT:
        //         case LANDMARK_TYPE.GOATHIDE:
        //         case LANDMARK_TYPE.DEERHIDE:
        //         case LANDMARK_TYPE.BEHEMOTHHIDE:
        //         case LANDMARK_TYPE.OAK:
        //         case LANDMARK_TYPE.YEW:
        //         case LANDMARK_TYPE.EBONY:
        //         case LANDMARK_TYPE.IRON:
        //         case LANDMARK_TYPE.COBALT:
        //         case LANDMARK_TYPE.MITHRIL:
        //             return BASE_LANDMARK_TYPE.RESOURCE;
        //         case LANDMARK_TYPE.ANCIENT_RUIN:
        //         case LANDMARK_TYPE.VAMPIRE_TOMB:
        //         case LANDMARK_TYPE.ANCIENT_REACTOR:
        //         case LANDMARK_TYPE.CAVE:
        //         case LANDMARK_TYPE.WILDLANDS:
        //case LANDMARK_TYPE.RITUAL_STONES:
        //             return BASE_LANDMARK_TYPE.DUNGEON;
        //case LANDMARK_TYPE.CITY:
        //case LANDMARK_TYPE.GOBLIN_CAMP:
        //         case LANDMARK_TYPE.HUT:
        //case LANDMARK_TYPE.CRATER:
        //             return BASE_LANDMARK_TYPE.SETTLEMENT;
        //         default:
        //             return BASE_LANDMARK_TYPE.NONE;
        //     }
        // }
        #endregion

        #region File Utilities
        public static bool DoesFileExist(string path) {
            return System.IO.File.Exists(path);
        }
        public static List<string> GetFileChoices(string path, string searchPattern) {
            List<string> choices = new List<string>();
            string[] classes = System.IO.Directory.GetFiles(path, searchPattern);
            for (int i = 0; i < classes.Length; i++) {
                CharacterClass currentClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText(classes[i]));
                choices.Add(currentClass.className);
            }
            return choices;
        }
        #endregion

        #region Characters
        //public static bool IsRoleClassless(CHARACTER_ROLE role) {
        //    //if (role == CHARACTER_ROLE.WORKER) {
        //    //    return true;
        //    //}
        //    return false;
        //}
        //public static ITEM_TYPE GetItemTypeOfEquipment(EQUIPMENT_TYPE equipmentType) {
        //    switch (equipmentType) {
        //        case EQUIPMENT_TYPE.SWORD:
        //        case EQUIPMENT_TYPE.DAGGER:
        //        case EQUIPMENT_TYPE.SPEAR:
        //        case EQUIPMENT_TYPE.BOW:
        //        case EQUIPMENT_TYPE.STAFF:
        //        case EQUIPMENT_TYPE.AXE:
        //            return ITEM_TYPE.WEAPON;
        //        case EQUIPMENT_TYPE.SHIRT:
        //        case EQUIPMENT_TYPE.BRACER:
        //        case EQUIPMENT_TYPE.HELMET:
        //        case EQUIPMENT_TYPE.LEGGINGS:
        //        case EQUIPMENT_TYPE.BOOT:
        //            return ITEM_TYPE.ARMOR;
        //        default:
        //            return ITEM_TYPE.WEAPON;
        //    }
        //}
        public static List<string> specialClasses = new List<string>() {
            "Necromancer",
            "Archmage",
            "Witch",
            "Beastmaster",
            "Tempest",
        };
        public static SEXUALITY GetCompatibleSexuality(SEXUALITY sexuality) {
            int chance = Rng.Next(0, 100);
            if (sexuality == SEXUALITY.STRAIGHT) {
                if (chance < 80) {
                    return SEXUALITY.STRAIGHT;
                }
                return SEXUALITY.BISEXUAL;
            } else if (sexuality == SEXUALITY.BISEXUAL) {
                if (chance < 80) {
                    return SEXUALITY.STRAIGHT;
                } else if (chance >= 80 && chance < 90) {
                    return SEXUALITY.GAY;
                }
                return SEXUALITY.BISEXUAL;
            } else if (sexuality == SEXUALITY.GAY) {
                if (chance < 50) {
                    return SEXUALITY.GAY;
                }
                return SEXUALITY.BISEXUAL;
            }
            return sexuality;
        }
        public static GENDER GetRandomGender() {
            if (UnityEngine.Random.Range(0, 2) == 0) {
                return GENDER.MALE;
            }
            return GENDER.FEMALE;
        }
        public static GENDER GetOppositeGender(GENDER gender) {
            if (gender == GENDER.FEMALE) {
                return GENDER.MALE;
            }
            return GENDER.FEMALE;
        }
        #endregion

        #region Number Utilities
        public static bool IsEven(int num) {
            return num % 2 == 0;
        }
        public static bool IsPositive(float num) {
            if (num > 0) {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Is the given value within the given range
        /// lower bound is inclusive, upper bound is exclusive
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="lowerBound">Lower bound [inclusive]</param>
        /// <param name="upperBound">Upper Bound [exclusive]</param>
        /// <returns>True or false</returns>
        public static bool IsInRange(int value, int lowerBound, int upperBound) {
            if (value >= lowerBound && value < upperBound) {
                return true;
            }
            return false;
        }
        #endregion

        #region Character Relationship
        //   public static Dictionary<CHARACTER_RELATIONSHIP, CHARACTER_RELATIONSHIP_CATEGORY> charRelationshipCategory = new Dictionary<CHARACTER_RELATIONSHIP, CHARACTER_RELATIONSHIP_CATEGORY> () {
        //	{CHARACTER_RELATIONSHIP.RIVAL, CHARACTER_RELATIONSHIP_CATEGORY.NEGATIVE},
        //	{CHARACTER_RELATIONSHIP.FRIEND, CHARACTER_RELATIONSHIP_CATEGORY.POSITIVE},
        //	{CHARACTER_RELATIONSHIP.ENEMY, CHARACTER_RELATIONSHIP_CATEGORY.FAMILIAL},
        //	{CHARACTER_RELATIONSHIP.SIBLING, CHARACTER_RELATIONSHIP_CATEGORY.FAMILIAL},
        //	{CHARACTER_RELATIONSHIP.PARENT, CHARACTER_RELATIONSHIP_CATEGORY.FAMILIAL},
        //	{CHARACTER_RELATIONSHIP.CHILD, CHARACTER_RELATIONSHIP_CATEGORY.FAMILIAL},
        //	{CHARACTER_RELATIONSHIP.LOVER, CHARACTER_RELATIONSHIP_CATEGORY.POSITIVE},
        //	{CHARACTER_RELATIONSHIP.EX_LOVER, CHARACTER_RELATIONSHIP_CATEGORY.NEUTRAL},
        //	{CHARACTER_RELATIONSHIP.APPRENTICE, CHARACTER_RELATIONSHIP_CATEGORY.POSITIVE},
        //	{CHARACTER_RELATIONSHIP.MENTOR, CHARACTER_RELATIONSHIP_CATEGORY.POSITIVE},
        //	{CHARACTER_RELATIONSHIP.ACQUAINTANCE, CHARACTER_RELATIONSHIP_CATEGORY.NEUTRAL},
        //};
        #endregion

        #region UI Utilities
        public static void DestroyChildren(Transform parent) {
            Transform[] children = GameUtilities.GetComponentsInDirectChildren<Transform>(parent.gameObject);
            for (int i = 0; i < children.Length; i++) {
                Transform currTransform = children[i];
                PooledObject pooledObject = currTransform.gameObject.GetComponent<EZObjectPools.PooledObject>(); 
                if (ObjectPoolManager.Instance == null || ReferenceEquals(pooledObject, null)) {
                    if (Application.isEditor) {
                        GameObject.DestroyImmediate(currTransform.gameObject);
                    } else {
                        GameObject.Destroy(currTransform.gameObject);
                    }
                } else {
                    ObjectPoolManager.Instance.DestroyObject(pooledObject);
                }
            
            }
        }
        /// <summary>
        /// Destroy all the child game objects of a given parent.
        /// NOTE: This assumes that all children are part of the Object Pool System.
        /// Possible side effect of using this on a non Object Pool parent is that, it's non
        /// Object Pooled children will not be destroyed. 
        /// </summary>
        /// <param name="parent">The parent object to destroy the children of.</param>
        public static void DestroyChildrenObjectPool(Transform parent) {
            PooledObject[] children = GameUtilities.GetComponentsInDirectChildren<PooledObject>(parent.gameObject);
            for (int i = 0; i < children.Length; i++) {
                PooledObject pooledObject = children[i];
                ObjectPoolManager.Instance.DestroyObject(pooledObject);
            }
        }
        public static bool IsUIElementInsideScreen(RectTransform uiElement, Canvas canvas) {
            Vector3[] objectCorners = new Vector3[4];
            uiElement.GetWorldCorners(objectCorners);
            Rect screenRect = Camera.main.pixelRect;
            for (int i = 0; i < objectCorners.Length; i++) {
                Vector3 currCorner = objectCorners[i];
                if (!screenRect.Contains(currCorner)) {
                    return false;
                }
            }
            return true;
        }
        public static void ScrolRectSnapTo(ScrollRect scrollRect, RectTransform target) {
            Canvas.ForceUpdateCanvases();

            scrollRect.content.anchoredPosition =
                (Vector2)scrollRect.transform.InverseTransformPoint(scrollRect.content.position)
                - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
        }
        public static void GetAnchorMinMax(TextAnchor type, out Vector2 anchorMin, out Vector2 anchorMax) {
            switch (type) {
                case TextAnchor.UpperLeft:
                    anchorMin = new Vector2(0f, 1f);
                    anchorMax = new Vector2(0f, 1f);
                    break;
                case TextAnchor.UpperCenter:
                    anchorMin = new Vector2(0.5f, 1f);
                    anchorMax = new Vector2(0.5f, 1f);
                    break;
                case TextAnchor.UpperRight:
                    anchorMin = new Vector2(1f, 1f);
                    anchorMax = new Vector2(1f, 1f);
                    break;
                case TextAnchor.MiddleLeft:
                    anchorMin = new Vector2(0f, 0.5f);
                    anchorMax = new Vector2(0f, 0.5f);
                    break;
                case TextAnchor.MiddleCenter:
                    anchorMin = new Vector2(0.5f, 0.5f);
                    anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                case TextAnchor.MiddleRight:
                    anchorMin = new Vector2(1f, 0.5f);
                    anchorMax = new Vector2(1f, 0.5f);
                    break;
                case TextAnchor.LowerLeft:
                    anchorMin = new Vector2(0f, 0f);
                    anchorMax = new Vector2(0f, 0f);
                    break;
                case TextAnchor.LowerCenter:
                    anchorMin = new Vector2(0.5f, 0f);
                    anchorMax = new Vector2(0.5f, 0f);
                    break;
                case TextAnchor.LowerRight:
                    anchorMin = new Vector2(1f, 0f);
                    anchorMax = new Vector2(1f, 0f);
                    break;
                default:
                    anchorMin = new Vector2(1f, 1f);
                    anchorMax = new Vector2(1f, 1f);
                    break;
            }
        }
        #endregion

        #region Dictionary
        public static TValue GetRandomValueFromDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict) {
            List<TValue> values = Enumerable.ToList(dict.Values);
            return values[Rng.Next(dict.Count)];
        }
        public static TKey GetRandomKeyFromDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict) {
            List<TKey> keys = Enumerable.ToList(dict.Keys);
            return keys[Rng.Next(dict.Count)];
        }
        #endregion

        #region Interaction
        public static INTERACTION_TYPE[] interactionPriorityList = new INTERACTION_TYPE[] {
            //Faction Created Events
            //INTERACTION_TYPE.DEFENSE_MOBILIZATION,
            //INTERACTION_TYPE.DEFENSE_UPGRADE,
            //INTERACTION_TYPE.MOVE_TO_ATTACK,

            //character targeted actions
            //INTERACTION_TYPE.ABDUCT_ACTION,
            //INTERACTION_TYPE.CHANCE_ENCOUNTER,
            //INTERACTION_TYPE.CHARM_ACTION,
            //INTERACTION_TYPE.EAT_DEFENSELESS,
            //INTERACTION_TYPE.HUNT_ACTION,
            //INTERACTION_TYPE.PATROL_ACTION,
            //INTERACTION_TYPE.RECRUIT_ACTION,
            //INTERACTION_TYPE.REANIMATE_ACTION,
            //INTERACTION_TYPE.STEAL_ACTION,
            //INTERACTION_TYPE.TORTURE_ACTION,

            //Character Departure
            //INTERACTION_TYPE.RETURN_HOME,
            //INTERACTION_TYPE.MOVE_TO_SCAVENGE,
            //INTERACTION_TYPE.MOVE_TO_RAID,
            //INTERACTION_TYPE.MOVE_TO_PEACE_NEGOTIATION,
            //INTERACTION_TYPE.MOVE_TO_EXPLORE,
            //INTERACTION_TYPE.MOVE_TO_EXPAND,
            //INTERACTION_TYPE.MOVE_TO_RAID_EVENT,
            //INTERACTION_TYPE.MOVE_TO_ATTACK,
            //INTERACTION_TYPE.MOVE_TO_PEACE_NEGOTIATION,
            //INTERACTION_TYPE.MOVE_TO_EXPLORE_EVENT,
            //INTERACTION_TYPE.MOVE_TO_EXPANSION_EVENT,
            //INTERACTION_TYPE.MOVE_TO_RETURN_HOME,
            //INTERACTION_TYPE.MOVE_TO_IMPROVE_RELATIONS_EVENT,
            //INTERACTION_TYPE.MOVE_TO_RECRUIT_ACTION,
            //INTERACTION_TYPE.MOVE_TO_CHARM_ACTION,
            //INTERACTION_TYPE.MOVE_TO_ABDUCT_ACTION,
            //INTERACTION_TYPE.MOVE_TO_STEAL_ACTION,
            //INTERACTION_TYPE.MOVE_TO_HUNT_ACTION,
            //INTERACTION_TYPE.MOVE_TO_REANIMATE_ACTION,

            //Combat
            //INTERACTION_TYPE.ATTACK,

            //Expansion

            //Peace
            //INTERACTION_TYPE.CHARACTER_PEACE_NEGOTIATION,
            //INTERACTION_TYPE.MINION_PEACE_NEGOTIATION,

            //Character Arrival
            //INTERACTION_TYPE.CHARACTER_PEACE_NEGOTIATION,
        };
        public static int GetInteractionPriorityIndex(INTERACTION_TYPE interactionType) {
            for (int i = 0; i < interactionPriorityList.Length; i++) {
                if(interactionType == interactionPriorityList[i]) {
                    return i;
                }
            }
            return -1;
        }
        #endregion


        /// <summary>
        /// Method to create lists containing possible combinations of an input list of items. This is 
        /// basically copied from code by user "jaolho" on this thread:
        /// http://stackoverflow.com/questions/7802822/all-possible-combinations-of-a-list-of-values
        /// </summary>
        /// <typeparam name="T">type of the items on the input list</typeparam>
        /// <param name="inputList">list of items</param>
        /// <param name="minimumItems">minimum number of items wanted in the generated combinations, 
        ///                            if zero the empty combination is included,
        ///                            default is one</param>
        /// <param name="maximumItems">maximum number of items wanted in the generated combinations,
        ///                            default is no maximum limit</param>
        /// <returns>list of lists for possible combinations of the input items</returns>
        public static List<List<T>> ItemCombinations<T>(List<T> inputList, int numOfResults = 0, int minimumItems = 1,
            int maximumItems = int.MaxValue) {
            int nonEmptyCombinations = (int) Math.Pow(2, inputList.Count) - 1;
            List<List<T>> listOfLists = new List<List<T>>(nonEmptyCombinations + 1);

            if(numOfResults <= 0) {
                // Optimize generation of empty combination, if empty combination is wanted
                if (minimumItems == 0)
                    listOfLists.Add(new List<T>());

                if (minimumItems <= 1 && maximumItems >= inputList.Count) {
                    // Simple case, generate all possible non-empty combinations
                    for (int bitPattern = 1; bitPattern <= nonEmptyCombinations; bitPattern++)
                        listOfLists.Add(GenerateCombination(inputList, bitPattern));
                } else {
                    // Not-so-simple case, avoid generating the unwanted combinations
                    for (int bitPattern = 1; bitPattern <= nonEmptyCombinations; bitPattern++) {
                        int bitCount = CountBits(bitPattern);
                        if (bitCount >= minimumItems && bitCount <= maximumItems)
                            listOfLists.Add(GenerateCombination(inputList, bitPattern));
                    }
                }
            } else {
                // Optimize generation of empty combination, if empty combination is wanted
                if (minimumItems == 0)
                    listOfLists.Add(new List<T>());
                if(listOfLists.Count >= numOfResults) { return listOfLists; }

                if (minimumItems <= 1 && maximumItems >= inputList.Count) {
                    // Simple case, generate all possible non-empty combinations
                    for (int bitPattern = 1; bitPattern <= nonEmptyCombinations; bitPattern++)
                        listOfLists.Add(GenerateCombination(inputList, bitPattern));
                    if (listOfLists.Count >= numOfResults) { return listOfLists; }

                } else {
                    // Not-so-simple case, avoid generating the unwanted combinations
                    for (int bitPattern = 1; bitPattern <= nonEmptyCombinations; bitPattern++) {
                        int bitCount = CountBits(bitPattern);
                        if (bitCount >= minimumItems && bitCount <= maximumItems)
                            listOfLists.Add(GenerateCombination(inputList, bitPattern));
                        if (listOfLists.Count >= numOfResults) { return listOfLists; }
                    }
                }
            }
            return listOfLists;
        }

        /// <summary>
        /// Sub-method of ItemCombinations() method to generate a combination based on a bit pattern.
        /// </summary>
        private static List<T> GenerateCombination<T>(List<T> inputList, int bitPattern) {
            List<T> thisCombination = new List<T>(inputList.Count);
            for (int j = 0; j < inputList.Count; j++) {
                if ((bitPattern >> j & 1) == 1)
                    thisCombination.Add(inputList[j]);
            }
            return thisCombination;
        }

        /// <summary>
        /// Sub-method of ItemCombinations() method to count the bits in a bit pattern. Based on this:
        /// https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetKernighan
        /// </summary>
        private static int CountBits(int bitPattern) {
            int numberBits = 0;
            while (bitPattern != 0) {
                numberBits++;
                bitPattern &= bitPattern - 1;
            }
            return numberBits;
        }

        private static RACE[,] opposingRaces = new RACE[,] { { RACE.HUMANS, RACE.ELVES }, { RACE.FAERY, RACE.GOBLIN } };
        
        public static bool AreTwoCharactersFromOpposingRaces(Character character1, Character character2) {
            if(character1.race != character2.race) {
                int outerLength = opposingRaces.GetLength(0);
                for (int i = 0; i < outerLength; i++) {
                    if((character1.race == opposingRaces[i, 0] || character1.race == opposingRaces[i, 1])
                       && (character2.race == opposingRaces[i, 0] || character2.race == opposingRaces[i, 1])) {
                        return true;
                    }
                }
            }
            return false;
        }
        public static T GetRandomEnumValue<T>() {
            var values = Enum.GetValues(typeof(T));
            int random = UnityEngine.Random.Range(0, values.Length);
            return (T) values.GetValue(random);
        }
        
    }
}