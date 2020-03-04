using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
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
			region.innerMap.PlaceBuildSpotTileObjects();
		}
		ApplyPreGeneratedRelationships(data);
		yield return null;
	}
	
	private IEnumerator CreateSettlement(Region region, MapGenerationData data) {
		List<HexTile> settlementTiles = region.GetTilesWithFeature(TileFeatureDB.Inhabited_Feature);
		Settlement settlement = LandmarkManager.Instance.CreateNewSettlement
			(region, LOCATION_TYPE.HUMAN_SETTLEMENT, 1, settlementTiles.ToArray());
		int totalRemainingBuildSpots = settlementTiles.Count * 4; //*4 because each hex tile = 4 build spots
		totalRemainingBuildSpots -= 1; //to accomodate for city center

		int dwellingCount = totalRemainingBuildSpots;
		Faction faction = FactionManager.Instance.CreateNewFaction(CollectionUtilities.GetRandomElement(raceChoices));
		settlement.GenerateStructures(dwellingCount);
		settlement.AddStructure(region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
		LandmarkManager.Instance.OwnSettlement(faction, settlement);
		
		yield return MapGenerator.Instance.StartCoroutine(PlaceInitialStructures(settlement, region.innerMap));
		yield return MapGenerator.Instance.StartCoroutine(settlement.PlaceObjects());

		GenerateSettlementResidents(dwellingCount, settlement, faction, data);

		CharacterManager.Instance.PlaceInitialCharacters(faction.characters, settlement);
		settlement.OnAreaSetAsActive();
	}

	#region Settlement Structures
	private IEnumerator PlaceInitialStructures(Settlement settlement, InnerTileMap innerTileMap) {
		//order the structures based on their priorities
		Dictionary<STRUCTURE_TYPE, List<LocationStructure>> ordered = settlement.structures
			.OrderBy(x => x.Key.StructureGenerationPriority())
			.ToDictionary(x => x.Key, x => x.Value);

		foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in ordered) {
			if (keyValuePair.Key.ShouldBeGeneratedFromTemplate()) {
				for (int i = 0; i < keyValuePair.Value.Count; i++) {
					LocationStructure structure = keyValuePair.Value[i];
					List<GameObject> choices =
						InnerMapManager.Instance.GetStructurePrefabsForStructure(keyValuePair.Key);
					GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
					LocationStructureObject lso = chosenStructurePrefab.GetComponent<LocationStructureObject>();
					BuildingSpot chosenBuildingSpot;
					if (TryGetBuildSpotForStructureInSettlement(lso, settlement, out chosenBuildingSpot)) {
						innerTileMap.PlaceStructureObjectAt(chosenBuildingSpot, chosenStructurePrefab, structure);
					} else {
						throw new System.Exception(
							$"Could not find valid building spot for {structure.ToString()} using prefab {chosenStructurePrefab.name}");
					}
					yield return null;
				}
			}
		}
	}
	private bool TryGetBuildSpotForStructureInSettlement(LocationStructureObject structureObject, Settlement settlement, out BuildingSpot spot) {
		for (int i = 0; i < settlement.tiles.Count; i++) {
			HexTile currTile = settlement.tiles[i];
			for (int j = 0; j < currTile.ownedBuildSpots.Length; j++) {
				BuildingSpot currSpot = currTile.ownedBuildSpots[j];
				if (currSpot.isOccupied == false && currSpot.CanFitStructureOnSpot(structureObject, settlement.innerMap, "Player")) {
					spot = currSpot;
					return true;
				}
			}
		}
		spot = null;
		return false;
	}
	private int GetNumberOfDwellingsToHouseCharacters(List<Character> characters) {
		//To get number of dwellings needed,
		//loop through all the characters, then check if each one is single
		//if a character is single, assign it 1 dwelling, then remove that character from the list
		//if the character is not single then remove it and its lover from the list of characters, and assign them 1 dwelling.
		//continue the loop until the list of characters becomes empty

		List<Character> listOfCharacters = new List<Character>(characters);
		int neededDwellingCount = 0;

		while (listOfCharacters.Count != 0) {
			Character currCharacter = listOfCharacters[0];
			Character lover = CharacterManager.Instance.GetCharacterByID(currCharacter.relationshipContainer
				.GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER));
			if (lover != null) {
				listOfCharacters.Remove(lover);
			}
			listOfCharacters.Remove(currCharacter);
			neededDwellingCount++;
		}
		return neededDwellingCount;
	}
	#endregion

	#region Residents
	private void GenerateSettlementResidents(int dwellingCount, Settlement settlement, Faction faction, MapGenerationData data) {
		int citizenCount = 0;
		for (int i = 0; i < dwellingCount; i++) {
			int roll = Random.Range(0, 100);
			List<Dwelling> availableDwellings = GetAvailableDwellingsAtSettlement(settlement);
			if (availableDwellings.Count == 0) {
				break; //no more dwellings
			}
			Dwelling dwelling = CollectionUtilities.GetRandomElement(availableDwellings);
			if (roll < 40) {
				//couple
				List<Couple> couples = GetAvailableCouplesToBeSpawned(faction.race, data);
				if (couples.Count > 0) {
					Couple couple = CollectionUtilities.GetRandomElement(couples);
					SpawnCouple(couple, dwelling, faction, settlement);
					citizenCount += 2;
				} else {
					//no more couples left	
					List<Couple> siblingCouples = GetAvailableSiblingCouplesToBeSpawned(faction.race, data);
					if (siblingCouples.Count > 0) {
						Couple couple = CollectionUtilities.GetRandomElement(siblingCouples);
						SpawnCouple(couple, dwelling, faction, settlement);
						citizenCount += 2;
					} else {
						//no more sibling Couples	
						PreCharacterData singleCharacter =
							GetAvailableSingleCharacterForSettlement(faction.race, data, settlement);
						if (singleCharacter != null) {
							SpawnCharacter(singleCharacter, settlement.classManager.GetCurrentClassToCreate(), 
								dwelling, faction, settlement);
							citizenCount += 1;
						} else {
							//no more characters to spawn
							Debug.LogWarning("Could not find any more characters to spawn. Generating a new family tree.");
							FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
							data.familyTreeDatabase.AddFamilyTree(newFamily);
							singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, settlement);
							Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
							SpawnCharacter(singleCharacter, settlement.classManager.GetCurrentClassToCreate(), 
								dwelling, faction, settlement);
							citizenCount += 1;
						}
					}
				}
			} else {
				//single
				PreCharacterData singleCharacter =
					GetAvailableSingleCharacterForSettlement(faction.race, data, settlement);
				if (singleCharacter != null) {
					SpawnCharacter(singleCharacter, settlement.classManager.GetCurrentClassToCreate(), 
						dwelling, faction, settlement);
					citizenCount += 1;
				} else {
					//no more characters to spawn
					Debug.LogWarning("Could not find any more characters to spawn");
					FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
					data.familyTreeDatabase.AddFamilyTree(newFamily);
					singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, settlement);
					Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
					SpawnCharacter(singleCharacter, settlement.classManager.GetCurrentClassToCreate(), 
						dwelling, faction, settlement);
					citizenCount += 1;
				}
			}
		}
		settlement.SetInitialResidentCount(citizenCount);
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
	private PreCharacterData GetAvailableSingleCharacterForSettlement(RACE race, MapGenerationData data, Settlement settlement) {
		List<PreCharacterData> availableCharacters = new List<PreCharacterData>();
		List<FamilyTree> familyTrees = data.familyTreesDictionary[race];
		for (int i = 0; i < familyTrees.Count; i++) {
			FamilyTree familyTree = familyTrees[i];
			for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
				PreCharacterData familyMember = familyTree.allFamilyMembers[j];
				if (familyMember.hasBeenSpawned == false) {
					PreCharacterData lover = familyMember.GetCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER, data.familyTreeDatabase);
					//check if the character has a lover, if it does, check if its lover has been spawned, if it has, check that the lover was spawned in a different settlement
					if (lover == null || lover.hasBeenSpawned == false || 
					    CharacterManager.Instance.GetCharacterByID(lover.id).homeSettlement != settlement) {
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
	private List<Dwelling> GetAvailableDwellingsAtSettlement(Settlement settlement) {
		List<Dwelling> dwellings = new List<Dwelling>();
		if (settlement.structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
			List<LocationStructure> locationStructures = settlement.structures[STRUCTURE_TYPE.DWELLING];
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
	private void SpawnCouple(Couple couple, Dwelling dwelling, Faction faction, Settlement settlement) {
		SpawnCharacter(couple.character1, settlement.classManager.GetCurrentClassToCreate(), dwelling, faction, settlement);
		SpawnCharacter(couple.character2, settlement.classManager.GetCurrentClassToCreate(), dwelling, faction, settlement);
	}
	private void SpawnCharacter(PreCharacterData data, string className, Dwelling dwelling, Faction faction, Settlement settlement) {
		CharacterManager.Instance.CreateNewCharacter(data, className, faction, settlement, dwelling);
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