using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FamilyTreeGenerator {
    
    private static WeightedDictionary<SEXUALITY> parentsSexuality = new WeightedDictionary<SEXUALITY>(new Dictionary<SEXUALITY, int>() {
        {SEXUALITY.STRAIGHT, 90},
        {SEXUALITY.BISEXUAL, 10},
    });
    private static WeightedDictionary<int> childCountWeights = new WeightedDictionary<int>(new Dictionary<int, int>() {
        {0, 10},
        {1, 20},
        {2, 50},
        {3, 20},
    });
    private static WeightedDictionary<int> demoChildCountWeights = new WeightedDictionary<int>(new Dictionary<int, int>() {
        {2, 50},
        {3, 50},
    });
    private static WeightedDictionary<SEXUALITY> childSexuality = new WeightedDictionary<SEXUALITY>(new Dictionary<SEXUALITY, int>() {
        {SEXUALITY.STRAIGHT, 80},
        {SEXUALITY.BISEXUAL, 10},
        {SEXUALITY.GAY, 10},
    });
    
    public static FamilyTree GenerateFamilyTree(RACE race) {
        PreCharacterData father = new PreCharacterData(race, GENDER.MALE, parentsSexuality);
        PreCharacterData mother = new PreCharacterData(race, GENDER.FEMALE, parentsSexuality);
        mother.SetSurName(father.surName);

        int parentsCompatibility = Random.Range(3, 6);
        father.SetCompatibility(parentsCompatibility, mother);
        mother.SetCompatibility(parentsCompatibility, father);
        
        father.AddRelationship(RELATIONSHIP_TYPE.LOVER, mother);
        mother.AddRelationship(RELATIONSHIP_TYPE.LOVER, father);

        int randomChildren = WorldConfigManager.Instance.isDemoWorld ? demoChildCountWeights.PickRandomElementGivenWeights() : childCountWeights.PickRandomElementGivenWeights();
        List<PreCharacterData> children = new List<PreCharacterData>();
        for (int i = 0; i < randomChildren; i++) {
            PreCharacterData child = new PreCharacterData(race, UtilityScripts.Utilities.GetRandomGender(), childSexuality);
            child.SetSurName(father.surName);
            children.Add(child);
        }
        
        for (int i = 0; i < children.Count; i++) {
            PreCharacterData child = children[i];
            //randomize compatibility with parents
            int compatibilityWithFather = Random.Range(2, 5);
            int compatibilityWithMother = Random.Range(2, 5);
            child.SetCompatibility(compatibilityWithFather, father);
            child.SetCompatibility(compatibilityWithMother, mother);
            //parents randomize compatibility with child
            father.SetCompatibility(compatibilityWithFather, child);
            mother.SetCompatibility(compatibilityWithMother, child);
            
            child.AddRelationship(RELATIONSHIP_TYPE.PARENT, father);
            child.AddRelationship(RELATIONSHIP_TYPE.PARENT, mother);
            father.AddRelationship(RELATIONSHIP_TYPE.CHILD, child);
            mother.AddRelationship(RELATIONSHIP_TYPE.CHILD, child);
            
            for (int j = 0; j < children.Count; j++) {
                PreCharacterData otherChild = children[j];
                if (child != otherChild) {
                    child.GetOrInitializeRelationshipWith(otherChild);
                    otherChild.GetOrInitializeRelationshipWith(child);
                    int compatibilityWithSibling = otherChild.GetCompatibilityWith(child);
                    if (compatibilityWithSibling == -1) {
                        compatibilityWithSibling = Random.Range(2, 5);
                    }
                    //child randomize compatibility with sibling
                    child.SetCompatibility(compatibilityWithSibling, otherChild);

                    //add sibling relationship between created children                     
                    child.AddRelationship(RELATIONSHIP_TYPE.SIBLING, otherChild);
                }
            }    
        }
        
        FamilyTree familyTree = new FamilyTree(father, mother, children);
        
        //randomize opinions
        for (int i = 0; i < familyTree.allFamilyMembers.Count; i++) {
            PreCharacterData familyMember = familyTree.allFamilyMembers[i];
            for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
                PreCharacterData otherFamilyMember = familyTree.allFamilyMembers[j];
                if (familyMember != otherFamilyMember) {
                    if ((familyMember == father && otherFamilyMember == mother) 
                        || familyMember == mother && otherFamilyMember == father) {
                        //mother and father
                        familyMember.RandomizeOpinion(30, 100, otherFamilyMember);
                    } else {
                        int compatibility = familyMember.GetCompatibilityWith(otherFamilyMember);
                        if (compatibility == 2) {
                            familyMember.RandomizeOpinion(-40, 20, otherFamilyMember);
                        } else if (compatibility == 3) {
                            familyMember.RandomizeOpinion(10, 50, otherFamilyMember);
                        } else if (compatibility == 4) {
                            familyMember.RandomizeOpinion(30, 70, otherFamilyMember);
                        } else if (compatibility >= 5) {
                            familyMember.RandomizeOpinion(50, 100, otherFamilyMember);
                        }
                    }
                }
            
            }    
        }

        return familyTree;
    }
}
