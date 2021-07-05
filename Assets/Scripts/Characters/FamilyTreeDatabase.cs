using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UtilityScripts;

[System.Serializable]
public class FamilyTreeDatabase {
    public Dictionary<RACE, List<FamilyTree>> allFamilyTreesDictionary;

    public FamilyTreeDatabase() {
        allFamilyTreesDictionary = new Dictionary<RACE, List<FamilyTree>>();
    }

    public void AddFamilyTree(FamilyTree familyTree) {
        if (allFamilyTreesDictionary.ContainsKey(familyTree.race) == false) {
            allFamilyTreesDictionary.Add(familyTree.race, new List<FamilyTree>());
        }
        allFamilyTreesDictionary[familyTree.race].Add(familyTree);
    }

    public PreCharacterData GetCharacterWithID(int id) {
        foreach (var kvp in allFamilyTreesDictionary) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                FamilyTree familyTree = kvp.Value[i];
                for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
                    PreCharacterData characterData = familyTree.allFamilyMembers[j];
                    if (characterData.id == id) {
                        return characterData;
                    }
                }
            }
        }
        return null;
    }
    /// <summary>
    /// Get a list of unspawned characters. If there are no unspawned characters left, this will
    /// generate a new family tree and return all newly generated characters
    /// </summary>
    /// <param name="race">Race to check.</param>
    /// <param name="faction">Faction to check</param>
    /// <returns>List of character data.</returns>
    public void ForcePopulateAllUnspawnedCharactersThatFitFaction(List<PreCharacterData> availableCharacters, RACE race, Faction faction) {
        List<FamilyTree> familyTrees = allFamilyTreesDictionary[race];
        for (int i = 0; i < familyTrees.Count; i++) {
            FamilyTree familyTree = familyTrees[i];
            for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
                PreCharacterData familyMember = familyTree.allFamilyMembers[j];
                if (familyMember.hasBeenSpawned == false && faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(familyMember)) {
                    availableCharacters.Add(familyMember);
                }
            }
        }
        if (availableCharacters.Count <= 0) {
            FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(race);
            AddFamilyTree(newFamily);
            for (int i = 0; i < newFamily.allFamilyMembers.Count; i++) {
                PreCharacterData familyMember = newFamily.allFamilyMembers[i];
                if (faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(familyMember)) {
                    availableCharacters.Add(familyMember);
                }
            }
        }
    }
    public void ForcePopulateAllUnspawnedCharactersThatFitRace(List<PreCharacterData> availableCharacters, RACE race) {
        List<FamilyTree> familyTrees = allFamilyTreesDictionary[race];
        for (int i = 0; i < familyTrees.Count; i++) {
            FamilyTree familyTree = familyTrees[i];
            for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
                PreCharacterData familyMember = familyTree.allFamilyMembers[j];
                if (familyMember.hasBeenSpawned == false) {
                    availableCharacters.Add(familyMember);
                }
            }
        }
        if (availableCharacters.Count <= 0) {
            FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(race);
            AddFamilyTree(newFamily);
            for (int i = 0; i < newFamily.allFamilyMembers.Count; i++) {
                PreCharacterData familyMember = newFamily.allFamilyMembers[i];
                availableCharacters.Add(familyMember);
            }
        }
    }

    public void Load(SaveDataCurrentProgress saveDataCurrentProgress) {
        allFamilyTreesDictionary = saveDataCurrentProgress.familyTreeDatabase.allFamilyTreesDictionary;
    }
    
    // public void Save() {
    //     var folder = Directory.CreateDirectory($"{Application.persistentDataPath}/Family Trees");
    //     XmlSerializer serializer = new XmlSerializer(typeof(FamilyTreeDatabase)); //Create serializer
    //     FileStream stream = new FileStream($"{Application.persistentDataPath}/Family Trees/FamilyTrees.xml", FileMode.Create); //Create file at this path
    //     serializer.Serialize(stream, this); //Write the data in the xml file
    //     stream.Close(); //Close the stream
    // }
    
    
}
