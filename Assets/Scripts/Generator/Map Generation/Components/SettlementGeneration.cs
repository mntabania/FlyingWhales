using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Random = UnityEngine.Random;

public class SettlementGeneration : MapGenerationComponent {

	private List<RACE> raceChoices = new List<RACE>() {RACE.HUMANS, RACE.ELVES};
	public override IEnumerator Execute(MapGenerationData data) {
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			if (region.HasTileWithFeature(TileFeatureDB.Inhabited_Feature)) {
				yield return MapGenerator.Instance.StartCoroutine(CreateSettlement(region, data));
			}
			// region.innerMap.PlaceBuildSpotTileObjects();
		}
		ApplyPreGeneratedRelationships(data);
		yield return null;
	}
	
	private IEnumerator CreateSettlement(Region region, MapGenerationData data) {
		List<HexTile> settlementTiles = region.GetTilesWithFeature(TileFeatureDB.Inhabited_Feature);
		Faction faction = FactionManager.Instance.CreateNewFaction(CollectionUtilities.GetRandomElement(raceChoices));
		LOCATION_TYPE locationType = LOCATION_TYPE.HUMAN_SETTLEMENT;
		if (faction.race == RACE.ELVES) {
			locationType = LOCATION_TYPE.ELVEN_SETTLEMENT;
		}
		NPCSettlement npcSettlement = LandmarkManager.Instance.CreateNewSettlement
			(region, locationType, 1, settlementTiles.ToArray());
		npcSettlement.AddStructure(region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
		LandmarkManager.Instance.OwnSettlement(faction, npcSettlement);
		
		List<STRUCTURE_TYPE> structureTypes = GenerateStructures(npcSettlement);

		yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceBuiltStructuresForSettlement(npcSettlement, region.innerMap, structureTypes.ToArray()));
		yield return MapGenerator.Instance.StartCoroutine(npcSettlement.PlaceObjects());

		int dwellingCount = npcSettlement.structures[STRUCTURE_TYPE.DWELLING].Count;
		GenerateSettlementResidents(dwellingCount, npcSettlement, faction, data);

		CharacterManager.Instance.PlaceInitialCharacters(faction.characters, npcSettlement);
		npcSettlement.Initialize();
	}

	#region Settlement Structures
	private List<STRUCTURE_TYPE> GenerateStructures(NPCSettlement settlement) {
		List<STRUCTURE_TYPE> structureTypes = new List<STRUCTURE_TYPE> { STRUCTURE_TYPE.CITY_CENTER };
		for (int i = 1; i < settlement.tiles.Count; i++) {
			HexTile tile = settlement.tiles[i];
			WeightedDictionary<STRUCTURE_TYPE> structuresChoices = GetStructureWeights(tile, structureTypes);
			STRUCTURE_TYPE chosenStructureType = structuresChoices.PickRandomElementGivenWeights();
			structureTypes.Add(chosenStructureType);
		}
		return structureTypes;
	}
	private WeightedDictionary<STRUCTURE_TYPE> GetStructureWeights(HexTile tile, List<STRUCTURE_TYPE> structureTypes) {
		WeightedDictionary<STRUCTURE_TYPE> structureWeights = new WeightedDictionary<STRUCTURE_TYPE>();
		if (structureTypes.Contains(STRUCTURE_TYPE.MAGE_QUARTERS) == false) {
			//Mage Quarter: +6 (disable if already selected from previous hex tile)
			structureWeights.AddElement(STRUCTURE_TYPE.MAGE_QUARTERS, 6);
		}
		if (structureTypes.Contains(STRUCTURE_TYPE.PRISON) == false) {
			//Prison: +3 (disable if already selected from previous hex tile)
			structureWeights.AddElement(STRUCTURE_TYPE.PRISON, 3);
		}
		if (structureTypes.Contains(STRUCTURE_TYPE.APOTHECARY) == false) {
			//Apothecary: +6 (disable if already selected from previous hex tile)
			structureWeights.AddElement(STRUCTURE_TYPE.APOTHECARY, 6);
		}
		structureWeights.AddElement(STRUCTURE_TYPE.FARM, 1); //Farm: +1
		// if (structureTypes.Contains(STRUCTURE_TYPE.BARRACKS) == false) {
		// 	//Barracks: +6 (disable if already selected from previous hex tile)
		// 	structureWeights.AddElement(STRUCTURE_TYPE.BARRACKS, 6);
		// }
		if (tile.featureComponent.HasFeature(TileFeatureDB.Fertile_Feature)) {
			structureWeights.AddElement(STRUCTURE_TYPE.FARM, 15);
		}
		// if (tile.HasNeighbourWithFeature(TileFeatureDB.Metal_Source_Feature)) {
		// 	structureWeights.AddElement(STRUCTURE_TYPE.MINE, 15);
		// }
		if (tile.HasNeighbourWithFeature(TileFeatureDB.Wood_Source_Feature)) {
			structureWeights.AddElement(STRUCTURE_TYPE.LUMBERYARD, 15);
		}
		if (tile.HasNeighbourWithFeature(TileFeatureDB.Game_Feature) 
		     && structureTypes.Contains(STRUCTURE_TYPE.HUNTER_LODGE) == false) {
		 	//Hunter's Lodge: +0 (disable if already selected from previous hex tile)
		 	//if tile is adjacent to Game 
		 	structureWeights.AddElement(STRUCTURE_TYPE.HUNTER_LODGE, 15);
		}
		return structureWeights;
	}
	#endregion

	#region Residents
	private void GenerateSettlementResidents(int dwellingCount, NPCSettlement npcSettlement, Faction faction, MapGenerationData data) {
		int citizenCount = 0;
		for (int i = 0; i < dwellingCount; i++) {
			int roll = Random.Range(0, 100);
			List<Dwelling> availableDwellings = GetAvailableDwellingsAtSettlement(npcSettlement);
			if (availableDwellings.Count == 0) {
				break; //no more dwellings
			}
			Dwelling dwelling = CollectionUtilities.GetRandomElement(availableDwellings);
			if (roll < 40) {
				//couple
				List<Couple> couples = GetAvailableCouplesToBeSpawned(faction.race, data);
				if (couples.Count > 0) {
					Couple couple = CollectionUtilities.GetRandomElement(couples);
					SpawnCouple(couple, dwelling, faction, npcSettlement);
					citizenCount += 2;
				} else {
					//no more couples left	
					List<Couple> siblingCouples = GetAvailableSiblingCouplesToBeSpawned(faction.race, data);
					if (siblingCouples.Count > 0) {
						Couple couple = CollectionUtilities.GetRandomElement(siblingCouples);
						SpawnCouple(couple, dwelling, faction, npcSettlement);
						citizenCount += 2;
					} else {
						//no more sibling Couples	
						PreCharacterData singleCharacter =
							GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
						if (singleCharacter != null) {
							SpawnCharacter(singleCharacter, npcSettlement.classManager.GetCurrentClassToCreate(), 
								dwelling, faction, npcSettlement);
							citizenCount += 1;
						} else {
							//no more characters to spawn
							Debug.LogWarning("Could not find any more characters to spawn. Generating a new family tree.");
							FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
							data.familyTreeDatabase.AddFamilyTree(newFamily);
							singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
							Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
							SpawnCharacter(singleCharacter, npcSettlement.classManager.GetCurrentClassToCreate(), 
								dwelling, faction, npcSettlement);
							citizenCount += 1;
						}
					}
				}
			} else {
				//single
				PreCharacterData singleCharacter =
					GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
				if (singleCharacter != null) {
					SpawnCharacter(singleCharacter, npcSettlement.classManager.GetCurrentClassToCreate(), 
						dwelling, faction, npcSettlement);
					citizenCount += 1;
				} else {
					//no more characters to spawn
					Debug.LogWarning("Could not find any more characters to spawn");
					FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
					data.familyTreeDatabase.AddFamilyTree(newFamily);
					singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
					Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
					SpawnCharacter(singleCharacter, npcSettlement.classManager.GetCurrentClassToCreate(), 
						dwelling, faction, npcSettlement);
					citizenCount += 1;
				}
			}
		}
		npcSettlement.SetInitialResidentCount(citizenCount);
	}
	private List<Couple> GetAvailableCouplesToBeSpawned(RACE race, MapGenerationData data) {
		List<Couple> couples = new List<Couple>();
		List<FamilyTree> familyTrees = data.familyTreesDictionary[race];
		for (int i = 0; i < familyTrees.Count; i++) {
			FamilyTree familyTree = familyTrees[i];
			for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
				PreCharacterData familyMember = familyTree.allFamilyMembers[j];
				if (familyMember.hasBeenSpawned == false) {
					PreCharacterData lover = familyMember.GetCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER, data.familyTreeDatabase);
					if (lover != null && lover.hasBeenSpawned == false) {
						Couple couple = new Couple(familyMember, lover);
						if (couples.Contains(couple) == false) {
							couples.Add(couple);
						}
					}
				}
			}
		}
		return couples;
	}
	private List<Couple> GetAvailableSiblingCouplesToBeSpawned(RACE race, MapGenerationData data) {
		List<Couple> couples = new List<Couple>();
		List<FamilyTree> familyTrees = data.familyTreesDictionary[race];
		for (int i = 0; i < familyTrees.Count; i++) {
			FamilyTree familyTree = familyTrees[i];
			if (familyTree.children != null && familyTree.children.Count >= 2) {
				List<PreCharacterData> unspawnedChildren = familyTree.children.Where(x => x.hasBeenSpawned == false).ToList();
				if (unspawnedChildren.Count >= 2) {
					PreCharacterData random1 = CollectionUtilities.GetRandomElement(unspawnedChildren);
					unspawnedChildren.Remove(random1);
					PreCharacterData random2 = CollectionUtilities.GetRandomElement(unspawnedChildren);
					Couple couple = new Couple(random1, random2);
					if (couples.Contains(couple) == false) {
						couples.Add(couple);
					}
				}
			}
		}
		return couples;
	}
	private PreCharacterData GetAvailableSingleCharacterForSettlement(RACE race, MapGenerationData data, NPCSettlement npcSettlement) {
		List<PreCharacterData> availableCharacters = new List<PreCharacterData>();
		List<FamilyTree> familyTrees = data.familyTreesDictionary[race];
		for (int i = 0; i < familyTrees.Count; i++) {
			FamilyTree familyTree = familyTrees[i];
			for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
				PreCharacterData familyMember = familyTree.allFamilyMembers[j];
				if (familyMember.hasBeenSpawned == false) {
					PreCharacterData lover = familyMember.GetCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER, data.familyTreeDatabase);
					//check if the character has a lover, if it does, check if its lover has been spawned, if it has, check that the lover was spawned in a different npcSettlement
					if (lover == null || lover.hasBeenSpawned == false || 
					    CharacterManager.Instance.GetCharacterByID(lover.id).homeSettlement != npcSettlement) {
						availableCharacters.Add(familyMember);
					}
				}
			}
		}

		if (availableCharacters.Count > 0) {
			return CollectionUtilities.GetRandomElement(availableCharacters);
		}
		return null;
	}
	private List<Dwelling> GetAvailableDwellingsAtSettlement(NPCSettlement npcSettlement) {
		List<Dwelling> dwellings = new List<Dwelling>();
		if (npcSettlement.structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
			List<LocationStructure> locationStructures = npcSettlement.structures[STRUCTURE_TYPE.DWELLING];
			for (int i = 0; i < locationStructures.Count; i++) {
				LocationStructure currStructure = locationStructures[i];
				Dwelling dwelling = currStructure as Dwelling;
				if (dwelling.residents.Count == 0) {
					dwellings.Add(dwelling);	
				}
			}
		}
		return dwellings;
	}
	private void SpawnCouple(Couple couple, Dwelling dwelling, Faction faction, NPCSettlement npcSettlement) {
		SpawnCharacter(couple.character1, npcSettlement.classManager.GetCurrentClassToCreate(), dwelling, faction, npcSettlement);
		SpawnCharacter(couple.character2, npcSettlement.classManager.GetCurrentClassToCreate(), dwelling, faction, npcSettlement);
	}
	private void SpawnCharacter(PreCharacterData data, string className, Dwelling dwelling, Faction faction, NPCSettlement npcSettlement) {
		CharacterManager.Instance.CreateNewCharacter(data, className, faction, npcSettlement, dwelling);
	}
	#endregion

	#region Relationships
	private void ApplyPreGeneratedRelationships(MapGenerationData data) {
		foreach (var pair in data.familyTreesDictionary) {
			for (int i = 0; i < pair.Value.Count; i++) {
				FamilyTree familyTree = pair.Value[i];
				for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
					PreCharacterData characterData = familyTree.allFamilyMembers[j];
					if (characterData.hasBeenSpawned) {
						Character character = CharacterManager.Instance.GetCharacterByID(characterData.id);
						foreach (var kvp in characterData.relationships) {
							PreCharacterData targetCharacterData = data.familyTreeDatabase.GetCharacterWithID(kvp.Key);
							IRelationshipData relationshipData = character.relationshipContainer
								.GetOrCreateRelationshipDataWith(character, targetCharacterData.id,
									targetCharacterData.firstName, targetCharacterData.gender);
							
							character.relationshipContainer.SetOpinion(character, targetCharacterData.id, 
								targetCharacterData.firstName, targetCharacterData.gender,
								"Base", kvp.Value.baseOpinion);
							
							relationshipData.opinions.SetCompatibilityValue(kvp.Value.compatibility);
							
							for (int k = 0; k < kvp.Value.relationships.Count; k++) {
								RELATIONSHIP_TYPE relationshipType = kvp.Value.relationships[k];
								relationshipData.AddRelationship(relationshipType);
							}
						}
					}
				}
			}
		}
	}
	#endregion
}

public class Couple : IEquatable<Couple> {
	public PreCharacterData character1 { get; }
	public PreCharacterData character2 { get; }

	public Couple(PreCharacterData _character1, PreCharacterData _character2) {
		character1 = _character1;
		character2 = _character2;
	}
	public bool Equals(Couple other) {
		if (other == null) {
			return false;
		}
		return (character1.id == other.character1.id && character2.id == other.character2.id) ||
		       (character1.id == other.character2.id && character2.id == other.character1.id);
	}
	public override bool Equals(object obj) {
		return Equals(obj as  Couple);
	}
	public override int GetHashCode() {
		return character1.id + character2.id;
	}
}