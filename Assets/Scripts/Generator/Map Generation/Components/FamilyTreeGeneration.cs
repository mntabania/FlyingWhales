using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scenario_Maps;
using UnityEngine;
using UtilityScripts;

public class FamilyTreeGeneration : MapGenerationComponent {

    #region Random Generation
    public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Generating Families...");
        //human family trees
        for (int i = 0; i < 15; i++) {
            FamilyTree familyTree = FamilyTreeGenerator.GenerateFamilyTree(RACE.HUMANS);
            DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(familyTree);    
        }
        //elven family trees
        for (int i = 0; i < 15; i++) {
            FamilyTree familyTree = FamilyTreeGenerator.GenerateFamilyTree(RACE.ELVES);
            DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(familyTree);    
        }

        GenerateAdditionalCouples(RACE.HUMANS, data);
        GenerateAdditionalCouples(RACE.ELVES, data);
        yield return null;
    }
    private void GenerateAdditionalCouples(RACE race, MapGenerationData data) {
        List<FamilyTree> families = DatabaseManager.Instance.familyTreeDatabase.allFamilyTreesDictionary[race];
        int pairCount = families.Count / 2;

        for (int i = 0; i < pairCount; i++) {
            int index = i * 2;
            FamilyTree firstFamily = families[index];
            FamilyTree secondFamily = families.ElementAt(index + 1);
            
            if (firstFamily.children.Count == 0 || secondFamily.children.Count == 0) {
                continue;
            }
            PreCharacterData randomChildFromFirst = CollectionUtilities.GetRandomElement(firstFamily.children);
            PreCharacterData compatibleChildFromSecond =
                GetCompatibleChildFromFamily(randomChildFromFirst, secondFamily, DatabaseManager.Instance.familyTreeDatabase);

            if (compatibleChildFromSecond != null) {
                randomChildFromFirst.AddRelationship(RELATIONSHIP_TYPE.LOVER, compatibleChildFromSecond);
                compatibleChildFromSecond.AddRelationship(RELATIONSHIP_TYPE.LOVER, randomChildFromFirst);
                
                randomChildFromFirst.SetCompatibility(5, compatibleChildFromSecond);
                compatibleChildFromSecond.SetCompatibility(5, randomChildFromFirst);
                
                randomChildFromFirst.RandomizeOpinion(30, 100, compatibleChildFromSecond);
                compatibleChildFromSecond.RandomizeOpinion(30, 100, randomChildFromFirst);
            }
        }
    }

    private PreCharacterData GetCompatibleChildFromFamily(PreCharacterData target, FamilyTree familyTree, FamilyTreeDatabase database) {
        for (int i = 0; i < familyTree.children.Count; i++) {
            PreCharacterData child = familyTree.children[i];
            if (RelationshipManager.IsSexuallyCompatible(target.sexuality, child.sexuality, target.gender, child.gender) 
                && child.GetCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER, database) == null) {
                return child;
            }
        }
        return null;
    }
    #endregion

    #region Scenario Maps
    public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
        yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
    }
    #endregion
    
    #region Saved World
    public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
        DatabaseManager.Instance.familyTreeDatabase.Load(saveData);
        yield return null;
    }
    #endregion
}
