using System.Collections;
using System.Collections.Generic;
using Scenario_Maps;
using UtilityScripts;
namespace Generator.Map_Generation.Components {
    public class FactionFinalization : MapGenerationComponent  {
        
        #region Random Generation
        public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
            GenerateFactionLeadersAndFinalizeIdeologies();
            GenerateSettlementRulers();
            yield return null;
        }
        #endregion

        #region Scenario Maps
        public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
            if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
                List<FACTION_IDEOLOGY> ideologies = new List<FACTION_IDEOLOGY>() { FACTION_IDEOLOGY.Peaceful, FACTION_IDEOLOGY.Peaceful, FACTION_IDEOLOGY.Warmonger, FACTION_IDEOLOGY.Warmonger };
                //make villager factions neutral with each other
                for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                    Faction faction = FactionManager.Instance.allFactions[i];
                    if (faction.factionType.type == FACTION_TYPE.Elven_Kingdom || faction.factionType.type == FACTION_TYPE.Human_Empire) {
                        foreach (var factionRelationship in faction.relationships) {
                            if (factionRelationship.Key.factionType.type == FACTION_TYPE.Elven_Kingdom || factionRelationship.Key.factionType.type == FACTION_TYPE.Human_Empire) {
                                factionRelationship.Value.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Neutral);
                            }
                        }
                        if (ideologies.Count > 0) {
                            FACTION_IDEOLOGY ideology = CollectionUtilities.GetRandomElement(ideologies);
                            if (ideology == FACTION_IDEOLOGY.Peaceful) {
                                if (!faction.factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful)) {
                                    faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Warmonger);
                                    Peaceful peaceful = FactionManager.Instance.CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
                                    faction.factionType.AddIdeology(peaceful);
                                }
                            } else if (ideology == FACTION_IDEOLOGY.Warmonger) {
                                if (!faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger)) {
                                    faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Peaceful);
                                    Warmonger warmonger = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
                                    faction.factionType.AddIdeology(warmonger);
                                }
                            }
                            ideologies.Remove(ideology);    
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
            } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Aneem) {
                for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                    Faction faction = FactionManager.Instance.allFactions[i];
                    if (faction.factionType.type == FACTION_TYPE.Elven_Kingdom || faction.factionType.type == FACTION_TYPE.Human_Empire) {
                        foreach (var factionRelationship in faction.relationships) {
                            if (factionRelationship.Key.factionType.type == FACTION_TYPE.Elven_Kingdom || factionRelationship.Key.factionType.type == FACTION_TYPE.Human_Empire) {
                                factionRelationship.Value.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
                            }
                        }    
                    }
                }
            }
            
            GenerateFactionLeadersAndFinalizeIdeologies();
            GenerateSettlementRulers();
            yield return null;
        }
        #endregion
        
        
        private void GenerateSettlementRulers() {
            for (int j = 0; j < DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count; j++) {
                NPCSettlement settlement = DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[j];
                if (settlement.locationType == LOCATION_TYPE.VILLAGE) {
                    if (settlement.ruler == null) {
                        settlement.DesignateNewRuler(false);
                    }
                    settlement.GenerateInitialOpinionBetweenResidents();
                }
            }
        }
        private void GenerateFactionLeadersAndFinalizeIdeologies() {
            for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                Faction faction = FactionManager.Instance.allFactions[i];
                if (faction.isMajorNonPlayer) {
                    faction.DesignateNewLeader(false);
                    if (faction.leader is Character leader) {
                        FactionManager.Instance.RerollFactionLeaderTraitIdeology(faction, leader);
                        if (!faction.factionType.HasPeaceTypeIdeology()) {
                            FactionManager.Instance.RerollPeaceTypeIdeology(faction, leader);    
                        }
                    }
                    faction.GenerateInitialOpinionBetweenMembers();
                }
            }
        }
    }
}