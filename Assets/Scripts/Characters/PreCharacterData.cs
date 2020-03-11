using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PreCharacterData {
    
    public int id { get; set; }
    public RACE race { get; set; }
    public GENDER gender { get; set; }
    public string firstName { get; set; }
    public string surName { get; set; }
    public SEXUALITY sexuality { get; set; }
    public UtilityScripts.SerializableDictionary<int, PreCharacterRelationship> relationships { get; set; }
    public bool hasBeenSpawned { get; set; }

    public string name => $"{firstName} {surName}";
    
    public PreCharacterData() {
        relationships = new UtilityScripts.SerializableDictionary<int, PreCharacterRelationship>();
    }
    
    public PreCharacterData(RACE _race, GENDER _gender, WeightedDictionary<SEXUALITY> _sexualityWeights) : this() {
        id = UtilityScripts.Utilities.SetID(this);
        race = _race;
        gender = _gender;
        SetName(RandomNameGenerator.GenerateRandomName(_race, _gender));
        sexuality = _sexualityWeights.PickRandomElementGivenWeights();
    }
    private void SetName(string name) {
        firstName = name.Split(' ')[0];
        surName = name.Split(' ')[1];
        RandomNameGenerator.RemoveNameAsAvailable(gender, race, firstName);
    }
    public void SetSurName(string _surName) {
        surName = _surName;
    }
    /// <summary>
    /// Randomize Compatibility of this character with another character.
    /// NOTE: This creates new relationship data if none exists yet for the target character.
    /// </summary>
    /// <param name="compatibility"></param>
    /// <param name="characterData">the target character.</param>
    public void SetCompatibility(int compatibility, PreCharacterData characterData) {
        PreCharacterRelationship relationship = GetOrInitializeRelationshipWith(characterData);
        relationship.SetCompatibility(compatibility);
    }
    
    /// <summary>
    /// Randomize Opinion of this character with another character.
    /// NOTE: This creates new relationship data if none exists yet for the target character.
    /// </summary>
    /// <param name="lowerBound">lower bound of random compatibility [inclusive].</param>
    /// <param name="upperBound">upper bound of random compatibility [inclusive].</param>
    /// <param name="characterData">the target character.</param>
    public void RandomizeOpinion(int lowerBound, int upperBound, PreCharacterData characterData) {
        PreCharacterRelationship relationship = GetOrInitializeRelationshipWith(characterData);
        relationship.SetOpinion(Random.Range(lowerBound, upperBound + 1));
    }

    public void AddRelationship(RELATIONSHIP_TYPE relationshipType, PreCharacterData characterData) {
        PreCharacterRelationship relationship = GetOrInitializeRelationshipWith(characterData);
        relationship.AddRelationship(relationshipType);
    }
    
    public PreCharacterRelationship GetOrInitializeRelationshipWith(PreCharacterData characterData) {
        if (relationships.ContainsKey(characterData.id) == false) {
            relationships.Add(characterData.id, new PreCharacterRelationship());
        }
        return relationships[characterData.id];
    }
    public PreCharacterData GetCharacterWithRelationship(RELATIONSHIP_TYPE relationshipType, FamilyTreeDatabase database) {
        foreach (var relationship in relationships) {
            if (relationship.Value.relationships.Contains(relationshipType)) {
                int id = relationship.Key;
                return database.GetCharacterWithID(id);
            }
        }
        return null;
    }

    public int GetCompatibilityWith(PreCharacterData character) {
        return relationships[character.id].compatibility;
    }

    public void SetHasBeenSpawned() {
        hasBeenSpawned = true;
    }
}
