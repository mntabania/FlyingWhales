using System.Collections;
using Scenario_Maps;
namespace Generator.Map_Generation.Components {
    public class FactionFinalization : MapGenerationComponent  {
        public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
            if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
                //make villager factions neutral with each other
                for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                    Faction faction = FactionManager.Instance.allFactions[i];
                    if (faction.factionType.type == FACTION_TYPE.Elven_Kingdom || faction.factionType.type == FACTION_TYPE.Human_Empire) {
                        foreach (var factionRelationship in faction.relationships) {
                            if (factionRelationship.Key.factionType.type == FACTION_TYPE.Elven_Kingdom || factionRelationship.Key.factionType.type == FACTION_TYPE.Human_Empire) {
                                factionRelationship.Value.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Neutral);
                            }
                        }    
                    }
                }
            }
            yield return null;
        }
    }
}