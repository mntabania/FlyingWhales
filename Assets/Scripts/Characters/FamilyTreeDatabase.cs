using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class FamilyTreeDatabase {
    public UtilityScripts.SerializableDictionary<RACE, List<FamilyTree>> allFamilyTreesDictionary;

    public FamilyTreeDatabase() {
        allFamilyTreesDictionary = new UtilityScripts.SerializableDictionary<RACE, List<FamilyTree>>();
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
    
    public void Save() {
        var folder = Directory.CreateDirectory($"{Application.persistentDataPath}/Family Trees");
        XmlSerializer serializer = new XmlSerializer(typeof(FamilyTreeDatabase)); //Create serializer
        FileStream stream = new FileStream($"{Application.persistentDataPath}/Family Trees/FamilyTrees.xml", FileMode.Create); //Create file at this path
        serializer.Serialize(stream, this); //Write the data in the xml file
        stream.Close(); //Close the stream
    }
    
    
}
