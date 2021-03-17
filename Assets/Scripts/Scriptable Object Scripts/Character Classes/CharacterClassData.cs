using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Class Data", menuName = "Scriptable Objects/Character Class Data")]
public class CharacterClassData : ScriptableObject {
    [Header("Combat")]
    public CHARACTER_COMBAT_BEHAVIOUR combatBehaviourType;
    public COMBAT_SPECIAL_SKILL combatSpecialSkillType;
    [Header("Misc")] 
    public int summonCost;
    [Header("Visuals")]
    public CharacterClassAsset defaultSprites;
    public RaceSpriteListDictionary raceSprites;

    public CharacterClassAsset GetAssets(RACE p_race) {
        if (raceSprites.ContainsKey(p_race)) {
            return raceSprites[p_race];
        }
        return defaultSprites;
    }
}

#if UNITY_EDITOR
public class CreateCharacterClassScriptableObjects {
    [MenuItem("Assets/Create Character Class Data Assets")]
    public static void CreateCharacterClassDataAssets() {
        string characterClassAssetPath = "Assets/Textures/Character Markers/Class Assets";
        string[] classes = Directory.GetDirectories(characterClassAssetPath);
        
        //loop through races found in directory
        for (int i = 0; i < classes.Length; i++) {
            string currClassPath = classes[i];
            string className = new DirectoryInfo(currClassPath).Name;
            CharacterClassData asset = ScriptableObject.CreateInstance<CharacterClassData>();
            string[] races = Directory.GetDirectories(currClassPath);
            if (races.Length > 0) {
                asset.raceSprites = new RaceSpriteListDictionary();
                for (int j = 0; j < races.Length; j++) {
                    string racePath = races[j];
                    string raceStr = new DirectoryInfo(racePath).Name.ToUpper();
                    if (Enum.TryParse(raceStr, out RACE race)) {
                        CharacterClassAsset characterClassAsset = new CharacterClassAsset();
                        asset.raceSprites.Add(race, characterClassAsset);
                        string[] raceFiles = Directory.GetFiles(racePath);
                        for (int k = 0; k < raceFiles.Length; k++) {
                            string classAssetPath = raceFiles[k];
                            Sprite loadedSprite = (Sprite)UnityEditor.AssetDatabase.LoadAssetAtPath(classAssetPath, typeof(Sprite));
                            if (loadedSprite != null) {
                                if (loadedSprite.name.Contains("idle_1")) {
                                    characterClassAsset.stillSprite = loadedSprite;
                                }
                                //assume that sprite is for animation
                                characterClassAsset.animationSprites.Add(loadedSprite);
                            }
                        }    
                    }
                }
            } else {
                string[] classFiles = Directory.GetFiles(currClassPath);
                asset.defaultSprites = new CharacterClassAsset();
                for (int j = 0; j < classFiles.Length; j++) {
                    string classAssetPath = classFiles[j];
                    Sprite loadedSprite = (Sprite)UnityEditor.AssetDatabase.LoadAssetAtPath(classAssetPath, typeof(Sprite));
                    if (loadedSprite != null) {
                        if (loadedSprite.name.Contains("idle_1")) {
                            asset.defaultSprites.stillSprite = loadedSprite;
                        }
                        //assume that sprite is for animation
                        asset.defaultSprites.animationSprites.Add(loadedSprite);
                    }
                }
            }
            
            AssetDatabase.CreateAsset(asset, $"Assets/Scriptable Object Assets/Character Class Data/{className} Data.asset");
            AssetDatabase.SaveAssets();
        }
        
    }
}
#endif