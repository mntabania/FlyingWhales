using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class MonsterGeneration : MapGenerationComponent {

	public override IEnumerator Execute(MapGenerationData data) {
		string[] monsterChoices = new[] {"Small Spiders"}; //"Golem", "Wolves", "Seducer", "Fire Elementals", "Kobolds", "Giant Spiders", "Ent"
        List<BaseLandmark> monsterLairs = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.MONSTER_LAIR);
		for (int i = 0; i < monsterLairs.Count; i++) {
			BaseLandmark monsterLair = monsterLairs[i];
			string randomSet = CollectionUtilities.GetRandomElement(monsterChoices);
			Settlement settlementOnTile = monsterLair.tileLocation.settlementOnTile;
			LocationStructure monsterLairStructure =
				settlementOnTile.GetRandomStructureOfType(STRUCTURE_TYPE.MONSTER_LAIR);
			Assert.IsTrue(monsterLairStructure.unoccupiedTiles.Count > 0, 
				$"Monster Lair at {monsterLair.tileLocation.region.name} does not have any unoccupied tiles, but is trying to spawn monsters!");
			int randomAmount = 0;
			if (randomSet == "Golem") {
				randomAmount = Random.Range(1, 3);
				for (int j = 0; j < randomAmount; j++) {
					CreateMonster(SUMMON_TYPE.Golem, settlementOnTile, monsterLair, monsterLairStructure);
				}
			} else if (randomSet == "Wolves") {
				randomAmount = Random.Range(3, 6);
				for (int j = 0; j < randomAmount; j++) {
					CreateMonster(SUMMON_TYPE.Wolf, settlementOnTile, monsterLair, monsterLairStructure);
				}
			} else if (randomSet == "Seducer") {
				int random = Random.Range(0, 3);
				if (random == 0) {
					//incubus, succubus
					CreateMonster(SUMMON_TYPE.Incubus, settlementOnTile, monsterLair, monsterLairStructure);
					CreateMonster(SUMMON_TYPE.Succubus, settlementOnTile, monsterLair, monsterLairStructure);
				} else if (random == 1) {
					//incubus
					CreateMonster(SUMMON_TYPE.Incubus, settlementOnTile, monsterLair, monsterLairStructure);
				} else if (random == 2) {
					//succubus
					CreateMonster(SUMMON_TYPE.Succubus, settlementOnTile, monsterLair, monsterLairStructure);
				}
			} else if (randomSet == "Fire Elementals") {
				randomAmount = Random.Range(1, 3);
				for (int j = 0; j < randomAmount; j++) {
					CreateMonster(SUMMON_TYPE.FireElemental, settlementOnTile, monsterLair, monsterLairStructure);
				}
			} else if (randomSet == "Kobolds") {
				randomAmount = 3;
				for (int j = 0; j < randomAmount; j++) {
					CreateMonster(SUMMON_TYPE.Kobold, settlementOnTile, monsterLair, monsterLairStructure);
				}
			} else if (randomSet == "Giant Spiders") {
				randomAmount = Random.Range(1, 4);
				for (int j = 0; j < randomAmount; j++) {
					CreateMonster(SUMMON_TYPE.GiantSpider, settlementOnTile, monsterLair, monsterLairStructure);
				}
            } else if (randomSet == "Ent") {
                randomAmount = Random.Range(1, 4);
                for (int j = 0; j < randomAmount; j++) {
                    Summon summon = CreateMonster(SUMMON_TYPE.Ent, settlementOnTile, monsterLair, monsterLairStructure);
                    if(monsterLair.tileLocation.biomeType == BIOMES.DESERT) {
                        summon.AssignClass("Desert Ent");
                    } else if (monsterLair.tileLocation.biomeType == BIOMES.FOREST) {
                        summon.AssignClass("Forest Ent");
                    } else if (monsterLair.tileLocation.biomeType == BIOMES.SNOW) {
                        summon.AssignClass("Snow Ent");
                    } else if (monsterLair.tileLocation.biomeType == BIOMES.GRASSLAND) {
                        summon.AssignClass("Grass Ent");
                    } else if (monsterLair.tileLocation.isCorrupted) {
                        summon.AssignClass("Corrupt Ent");
                    }
                }
            } else if (randomSet == "Small Spiders") {
                randomAmount = Random.Range(1, 4);
                for (int j = 0; j < randomAmount; j++) {
                    CreateMonster(SUMMON_TYPE.Small_Spider, settlementOnTile, monsterLair, monsterLairStructure);
                }
            }
        }
		yield return null;
	}

	private Summon CreateMonster(SUMMON_TYPE summonType, Settlement settlementOnTile, BaseLandmark monsterLair,
		LocationStructure monsterLairStructure) {
		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, FactionManager.Instance.neutralFaction, settlementOnTile);
		CharacterManager.Instance.PlaceSummon(summon, CollectionUtilities.GetRandomElement(monsterLairStructure.unoccupiedTiles));
		summon.AddTerritory(monsterLair.tileLocation);
        return summon;
	}
}
