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
            } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
                for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                    Faction faction = FactionManager.Instance.allFactions[i];
                    if (faction.factionType.type == FACTION_TYPE.Elven_Kingdom || faction.factionType.type == FACTION_TYPE.Human_Empire) {
                        foreach (var factionRelationship in faction.relationships) {
                            if (factionRelationship.Key.factionType.type == FACTION_TYPE.Elven_Kingdom || factionRelationship.Key.factionType.type == FACTION_TYPE.Human_Empire) {
                                factionRelationship.Value.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
                            }
                        }    
                    }
                    if (faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
                        //elven kingdom should be warmonger
                        if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful)) {
                            faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Peaceful);
                            Warmonger warmonger = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
                            faction.factionType.AddIdeology(warmonger);
                        }
                    } else if (faction.factionType.type == FACTION_TYPE.Human_Empire) {
                        //human kingdom should be peaceful
                        if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger)) {
                            faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Warmonger);
                            Peaceful peaceful = FactionManager.Instance.CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
                            faction.factionType.AddIdeology(peaceful);
                        }
                    }
                }
            }
            yield return null;
        }
    }
}