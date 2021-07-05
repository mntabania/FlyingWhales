using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UtilityScripts;

[CreateAssetMenu(fileName = "New Character Class Data", menuName = "Scriptable Objects/Character Class Data")]
public class CharacterClassData : ScriptableObject {

    [System.Serializable]
    public class DroppableItemWithWeights {
        public TILE_OBJECT_TYPE item;
        public int weight;

        public DroppableItemWithWeights(TILE_OBJECT_TYPE p_type, int p_weight) {
            item = p_type;
            weight = p_weight;
        }
    }
    [Header("Combat")]
    public CHARACTER_COMBAT_BEHAVIOUR combatBehaviourType;
    public COMBAT_SPECIAL_SKILL combatSpecialSkillType;
    [Header("Structure")]
    public STRUCTURE_TYPE workStructureType;
    [Header("Misc")] 
    public int summonCost;
    [Header("Visuals")]
    public Sprite portraitSprite;
    public CharacterClassAsset defaultSprites;
    public RaceSpriteListDictionary raceSprites;
    
    [Header("Game Start Values")] 
    public float initialVillagerPiercing;
    public float[] initialVillagerPhysicalResistances;
    public float[] initialVillagerMentalResistances;
    public float[] initialVillagerElementalResistances;
    public float[] initialVillagerSecondaryResistances;
    
    [Header("Upgrade bonus per skill level up")]
    public CharacterProgressionBonusData characterSkillUpdateData;

    [Header("Craftable Equipments")]
    public List<TILE_OBJECT_TYPE> craftableWeapons = new List<TILE_OBJECT_TYPE>();
    public List<TILE_OBJECT_TYPE> craftableArmors = new List<TILE_OBJECT_TYPE>();
    public List<TILE_OBJECT_TYPE> craftableAccessories = new List<TILE_OBJECT_TYPE>();

    [Header("Droppable Items Equipments")]
    public List<DroppableItemWithWeights> droppableItems = new List<DroppableItemWithWeights>();
    public int GetSummonCost() {
        return SpellUtilities.GetModifiedSpellCost(summonCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
    }

    public CharacterClassAsset GetAssets(RACE p_race) {
        if (raceSprites.ContainsKey(p_race)) {
            return raceSprites[p_race];
        }
        return defaultSprites;
    }

    [ContextMenu("Set Monster Drop Items")]
    public void SetMonsterDrops() {
        droppableItems.Clear();
        droppableItems.Add(new DroppableItemWithWeights(TILE_OBJECT_TYPE.NONE, 100));
        droppableItems.Add(new DroppableItemWithWeights(TILE_OBJECT_TYPE.POWER_CRYSTAL, 10));
    }
    
    [ContextMenu("Set Knight Type Items")]
    public void SetKnightTypeItems() {
        craftableWeapons.Clear();
        craftableArmors.Clear();
        craftableAccessories.Clear();

        craftableWeapons.Add(TILE_OBJECT_TYPE.BASIC_SWORD);
        craftableWeapons.Add(TILE_OBJECT_TYPE.COPPER_SWORD);
        craftableWeapons.Add(TILE_OBJECT_TYPE.IRON_SWORD);
        craftableWeapons.Add(TILE_OBJECT_TYPE.MITHRIL_SWORD);
        craftableWeapons.Add(TILE_OBJECT_TYPE.ORICHALCUM_SWORD);

        craftableArmors.Add(TILE_OBJECT_TYPE.BASIC_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.RABBIT_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.MINK_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.WOOL_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.MOONWALKER_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.COPPER_ARMOR);
        craftableArmors.Add(TILE_OBJECT_TYPE.IRON_ARMOR);
        craftableArmors.Add(TILE_OBJECT_TYPE.MITHRIL_ARMOR);
        craftableArmors.Add(TILE_OBJECT_TYPE.ORICHALCUM_ARMOR);

        craftableAccessories.Add(TILE_OBJECT_TYPE.BRACER);
    }

    [ContextMenu("Set Archer Type Items")]
    public void SetArcherTypeItems() {
        craftableWeapons.Clear();
        craftableArmors.Clear();
        craftableAccessories.Clear();

        craftableWeapons.Add(TILE_OBJECT_TYPE.BASIC_BOW);
        craftableWeapons.Add(TILE_OBJECT_TYPE.COPPER_BOW);
        craftableWeapons.Add(TILE_OBJECT_TYPE.IRON_BOW);
        craftableWeapons.Add(TILE_OBJECT_TYPE.MITHRIL_BOW);
        craftableWeapons.Add(TILE_OBJECT_TYPE.ORICHALCUM_BOW);

        SetCultLeaderArmorSets();
    }

    [ContextMenu("Set Non Combatant Items")]
    public void SetNonCombatantTypeItems() {
        craftableWeapons.Clear();
        craftableArmors.Clear();
        craftableAccessories.Clear();

        craftableWeapons.Add(TILE_OBJECT_TYPE.BASIC_DAGGER);
        craftableWeapons.Add(TILE_OBJECT_TYPE.COPPER_DAGGER);
        craftableWeapons.Add(TILE_OBJECT_TYPE.IRON_DAGGER);
        craftableWeapons.Add(TILE_OBJECT_TYPE.MITHRIL_DAGGER);
        craftableWeapons.Add(TILE_OBJECT_TYPE.ORICHALCUM_DAGGER);

        craftableArmors.Add(TILE_OBJECT_TYPE.BASIC_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.RABBIT_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.MINK_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.WOOL_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.MOONWALKER_SHIRT);

        craftableAccessories.Add(TILE_OBJECT_TYPE.NECKLACE);
    }

    [ContextMenu("Set Mage Type Items")]
    public void SetMageTypeItems() {
        craftableWeapons.Clear();
        craftableArmors.Clear();
        craftableAccessories.Clear();

        craftableWeapons.Add(TILE_OBJECT_TYPE.BASIC_STAFF);
        craftableWeapons.Add(TILE_OBJECT_TYPE.COPPER_STAFF);
        craftableWeapons.Add(TILE_OBJECT_TYPE.IRON_STAFF);
        craftableWeapons.Add(TILE_OBJECT_TYPE.MITHRIL_STAFF);
        craftableWeapons.Add(TILE_OBJECT_TYPE.ORICHALCUM_STAFF);

        craftableArmors.Add(TILE_OBJECT_TYPE.BASIC_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.RABBIT_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.MINK_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.WOOL_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.MOONWALKER_SHIRT);

        craftableAccessories.Add(TILE_OBJECT_TYPE.RING);
        craftableAccessories.Add(TILE_OBJECT_TYPE.SCROLL);
    }

    [ContextMenu("Set Barbarian Type Items")]
    public void SetBarbarianTypeItems() {
        craftableWeapons.Clear();
        craftableArmors.Clear();
        craftableAccessories.Clear();

        craftableWeapons.Add(TILE_OBJECT_TYPE.BASIC_AXE);
        craftableWeapons.Add(TILE_OBJECT_TYPE.COPPER_AXE);
        craftableWeapons.Add(TILE_OBJECT_TYPE.IRON_AXE);
        craftableWeapons.Add(TILE_OBJECT_TYPE.MITHRIL_AXE);
        craftableWeapons.Add(TILE_OBJECT_TYPE.ORICHALCUM_AXE);

        craftableArmors.Add(TILE_OBJECT_TYPE.BASIC_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.RABBIT_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.MINK_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.WOOL_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.MOONWALKER_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.COPPER_ARMOR);
        craftableArmors.Add(TILE_OBJECT_TYPE.IRON_ARMOR);
        craftableArmors.Add(TILE_OBJECT_TYPE.MITHRIL_ARMOR);
        craftableArmors.Add(TILE_OBJECT_TYPE.ORICHALCUM_ARMOR);

        craftableAccessories.Add(TILE_OBJECT_TYPE.BRACER);
    }

    [ContextMenu("Set Cultleader Armors Set")]
    public void SetCultLeaderArmorSets() {
        
        craftableArmors.Clear();

        craftableArmors.Add(TILE_OBJECT_TYPE.BASIC_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.RABBIT_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.MINK_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.WOOL_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.MOONWALKER_SHIRT);
        craftableArmors.Add(TILE_OBJECT_TYPE.BOAR_HIDE_ARMOR);
        craftableArmors.Add(TILE_OBJECT_TYPE.BEAR_HIDE_ARMOR);
        craftableArmors.Add(TILE_OBJECT_TYPE.WOLF_HIDE_ARMOR);
        craftableArmors.Add(TILE_OBJECT_TYPE.SCALE_ARMOR);
        craftableArmors.Add(TILE_OBJECT_TYPE.DRAGON_ARMOR);

        craftableAccessories.Add(TILE_OBJECT_TYPE.BELT);
    }

    [ContextMenu("Clear Armors Craftable")]
    public void ClearArmorCraftables() {
        craftableArmors.Clear();
    }

    [ContextMenu("Clear Accessories Craftable")]
    public void ClearAccessoriesCraftables() {
        craftableAccessories.Clear();
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