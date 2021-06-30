using System.Globalization;
using System.Linq;
using Boo.Lang;
using Locations.Settlements;
using Locations.Settlements.Settlement_Events;
namespace UtilityScripts {
    public static class TestingUtilities {
        
        public static void ShowLocationInfo(Region region, NPCSettlement p_settlement) {
            string summary = $"{region.name} Info:";
            summary += "\nDivisions:";
            for (int i = 0; i < region.biomeDivisionComponent.divisions.Count; i++) {
                BiomeDivision division = region.biomeDivisionComponent.divisions[i];
                summary += $"\n- {division.biome.ToString()} - {division.tiles.Count.ToString()}";
            }
            List<NPCSettlement> settlements = GetSettlementsInRegion(region);
            summary += $"\n-----------------------------";
            summary += "\nLocations Info:";
            NPCSettlement npcSettlement = p_settlement;
            summary += $"\nLinked Beast Dens: {npcSettlement.occupiedVillageSpot?.GetLinkedBeastDensSummary()}";
            summary += $"\nLinked Structures: {npcSettlement.structureComponent.GetLinkedStructuresSummary()}";
            bool isRatmanFaction = npcSettlement.owner?.factionType.type == FACTION_TYPE.Ratmen;
            if (npcSettlement.locationType != LOCATION_TYPE.VILLAGE && !isRatmanFaction) {
                return;
            }
            if (!isRatmanFaction) {
                summary += $"\n<b>{npcSettlement.name}</b> Settlement Type: {npcSettlement.settlementType?.settlementType.ToString() ?? "None"}";
                summary += $"\nMax Facilities: {npcSettlement.settlementType?.maxFacilities}";
                summary += $"\nMax Dwellings: {npcSettlement.settlementType?.maxDwellings}";
                summary += $"\nMorning Needed Class: {npcSettlement.classComponent.morningScheduleDateForProcessingOfNeededClasses.ToString()}";
                summary += $"\nAfternoon Needed Class: {npcSettlement.classComponent.afternoonScheduleDateForProcessingOfNeededClasses.ToString()}";
                summary += $"\nParty Quests Processing: {npcSettlement.partyComponent.scheduleDateForProcessingOfPartyQuests.ToString()}";
                summary += $"\nBuilt Resource Piles in Settlement: {npcSettlement.SettlementResources.GetBuiltResourcePileCount(npcSettlement)}";
                summary += $"\nStorage: {npcSettlement.mainStorage?.name ?? "None"}. Prison: {npcSettlement.prison?.name ?? "None"}";
                //summary += $"\nRocks Count: {npcSettlement.SettlementResources.rocks.Count}";
                summary += $"\nTrees Count: {npcSettlement.SettlementResources.GetTreesCount(npcSettlement)}";
                summary += $"\nFishing Spots Count: {npcSettlement.SettlementResources.GetFishingSpotCount(npcSettlement)}";
                summary += $"\nCharacters Inside: {npcSettlement.SettlementResources.GetCharacterCount(npcSettlement)}";
                summary += $"\nNeeded Items: ";
                for (int j = 0; j < npcSettlement.neededObjects.Count; j++) {
                    summary += $"|{npcSettlement.neededObjects[j].ToString()}|";
                }
                summary += $"\nActive Events: ";
                for (int j = 0; j < npcSettlement.eventManager.activeEvents.Count; j++) {
                    SettlementEvent settlementEvent = npcSettlement.eventManager.activeEvents[j];
                    summary += $"|{settlementEvent.GetTestingInfo()}|";
                }
            }
            if (npcSettlement.owner != null) {
                summary += $"\n{npcSettlement.name} Location Job Queue:";
                if (npcSettlement.availableJobs.Count > 0) {
                    for (int j = 0; j < npcSettlement.availableJobs.Count; j++) {
                        JobQueueItem jqi = npcSettlement.availableJobs[j];
                        if (jqi is GoapPlanJob) {
                            GoapPlanJob gpj = jqi as GoapPlanJob;
                            summary += $"\n<b>{gpj.name} Targeting {gpj.targetPOI?.ToString() ?? "None"}</b>";
                        } else {
                            summary += $"\n<b>{jqi.name}</b>";
                        }
                        summary += $"\n Assigned Character: {jqi.assignedCharacter?.name}";
                    }
                } else {
                    summary += "\nNone";
                }
                if (!isRatmanFaction) {
                    if (npcSettlement.owner != null) {
                        summary += $"\nAdditional Migration Gain: {npcSettlement.owner.factionType.GetAdditionalMigrationMeterGain(npcSettlement)}";
                        summary += $"\n-----------------------------";
                        summary += $"\n{npcSettlement.owner.name} Faction Job Queue:";
                        if (npcSettlement.owner.availableJobs.Count > 0) {
                            for (int j = 0; j < npcSettlement.owner.availableJobs.Count; j++) {
                                JobQueueItem jqi = npcSettlement.owner.availableJobs[j];
                                if (jqi is GoapPlanJob) {
                                    GoapPlanJob gpj = jqi as GoapPlanJob;
                                    summary += $"\n<b>{gpj.name} Targeting {gpj.targetPOI?.ToString() ?? "None"}</b>";
                                } else {
                                    summary += $"\n<b>{jqi.name}</b>";
                                }
                                summary += $"\n Assigned Character: {jqi.assignedCharacter?.name}";
                            }
                        } else {
                            summary += "\nNone";
                        }
                        summary += $"\n-----------------------------";
                        summary += $"\n{npcSettlement.owner.name} Party Quests:";
                        if (npcSettlement.owner.partyQuestBoard.availablePartyQuests.Count > 0) {
                            for (int j = 0; j < npcSettlement.owner.partyQuestBoard.availablePartyQuests.Count; j++) {
                                PartyQuest quest = npcSettlement.owner.partyQuestBoard.availablePartyQuests[j];
                                summary += $"\n<b>{quest.partyQuestType.ToString()}</b>";
                                summary += $"(Assigned Party: {quest.assignedParty?.partyName})";
                            }
                        } else {
                            summary += "\nNone";
                        }
                    }
                }
            }
            // summary += $"\n{npcSettlement.name} Items in location: ";
            // for (int i = 0; i < npcSettlement.areas.Count; i++) {
            //     Area area = npcSettlement.areas[i];
            //     summary += $"{area.tileObjectComponent.itemsInArea.ComafyList()}";
            // }
            summary += "\n";
            UIManager.Instance.ShowSmallInfo(summary);
        }
        public static void HideLocationInfo() {
            UIManager.Instance.HideSmallInfo();
        }
        private static List<NPCSettlement> GetSettlementsInRegion(Region region) {
            List<NPCSettlement> settlements = new List<NPCSettlement>();
            for (int i = 0; i < LandmarkManager.Instance.allNonPlayerSettlements.Count; i++) {
                NPCSettlement npcSettlement = LandmarkManager.Instance.allNonPlayerSettlements[i];
                //if (npcSettlement.HasAreaInRegion(region)) {
                    settlements.Add(npcSettlement);
                //}
            }
            return settlements;
        }

        #region Character
        public static void ShowCharacterTestingInfo(Character activeCharacter) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        string summary = $"Home structure: {activeCharacter.homeStructure?.ToString() ?? "None"}";
        summary = $"{summary} {$"Class: {activeCharacter.characterClass.className ?? "None"}"}";
        summary = $"{summary} {$"Territory: {activeCharacter.territory?.name ?? "None"}"}";
        summary = $"{summary} Home Settlement: {activeCharacter.homeSettlement?.name ?? "None"}";
        summary = $"{summary} Current structure: {activeCharacter.currentStructure}";
        summary = $"{summary} Previous Home Structure: {activeCharacter.previousCharacterDataComponent.previousHomeStructure?.name}";
        summary = $"{summary} Previous Home Settlement: {activeCharacter.previousCharacterDataComponent.previousHomeSettlement?.name}";
        summary = $"{summary} Previous Faction: {activeCharacter.previousCharacterDataComponent.previousFaction?.name}";
        summary = $"{summary} {"POI State: " + activeCharacter.state.ToString()}";
        summary = $"{summary} {"Do Not Get Hungry: " + activeCharacter.needsComponent.doNotGetHungry.ToString()}";
        summary = $"{summary} {"Do Not Get Tired: " + activeCharacter.needsComponent.doNotGetTired.ToString()}";
        summary = $"{summary} {"Do Not Get Bored: " + activeCharacter.needsComponent.doNotGetBored.ToString()}";
        summary = $"{summary} {"Do Not Recover HP: " + activeCharacter.doNotRecoverHP.ToString()}";
        summary = $"{summary} {"Can Move: " + activeCharacter.limiterComponent.canMove.ToString()}";
        summary = $"{summary} {"Can Witness: " + activeCharacter.limiterComponent.canWitness.ToString()}";
        summary = $"{summary} {"Can Be Attacked: " + activeCharacter.limiterComponent.canBeAttacked.ToString()}";
        summary = $"{summary} {"Can Perform: " + activeCharacter.limiterComponent.canPerform.ToString()}";
        summary = $"{summary} {"Is Sociable: " + activeCharacter.limiterComponent.isSociable.ToString()}";
        summary = $"{summary} {"Is Running: " + activeCharacter.movementComponent.isRunning.ToString()}";
        summary = $"{summary} {"POI State: " + activeCharacter.state.ToString()}";
        summary = $"{summary} {"Personal Religion: " + activeCharacter.religionComponent.religion.ToString()}";
        summary = $"{summary} {"\nIs Reserved: " + !activeCharacter.classComponent.shouldChangeClass}";
        summary = $"{summary} {"\nCoins: " + activeCharacter.moneyComponent.coins.ToString()}";
        summary = $"{summary} {"\nAble Classes: " + activeCharacter.classComponent.GetAbleClassesText()}";
        // summary = $"{summary}{"\nFullness Time: " + (activeCharacter.needsComponent.fullnessForcedTick == 0 ? "N/A" : GameManager.Instance.ConvertTickToTime(activeCharacter.needsComponent.fullnessForcedTick))}";
        // summary = $"{summary}{"\nTiredness Time: " + (activeCharacter.needsComponent.tirednessForcedTick == 0 ? "N/A" : GameManager.Instance.ConvertTickToTime(activeCharacter.needsComponent.tirednessForcedTick))}";
        // summary = $"{summary}{"\nHappiness Time: " + (activeCharacter.needsComponent.happinessSecondForcedTick == 0 ? "N/A" : GameManager.Instance.ConvertTickToTime(activeCharacter.needsComponent.happinessSecondForcedTick))} - Satisfied Schedule Today ({activeCharacter.needsComponent.hasForcedSecondHappiness.ToString()})";
        // summary = $"{summary}{"\nRemaining Sleep Ticks: " + activeCharacter.needsComponent.currentSleepTicks.ToString()}";
        summary = $"{summary}{"\nSexuality: " + activeCharacter.sexuality.ToString()}";
        summary = $"{summary}{"\nAttack Range: " + activeCharacter.characterClass.attackRange.ToString(CultureInfo.InvariantCulture)}";
        summary = $"{summary}{"\nAttack Speed: " + activeCharacter.combatComponent.attackSpeed.ToString()}";
        summary = $"{summary}{"\nCombat Mode: " + activeCharacter.combatComponent.combatMode.ToString()}";
        summary = $"{summary}{"\nCombat Behaviour: " + activeCharacter.combatComponent.combatBehaviourParent.currentCombatBehaviour?.name}";
        summary = $"{summary}{"\nCombat Special Skill: " + activeCharacter.combatComponent.specialSkillParent.specialSkill?.name + ", Cooldown: " + activeCharacter.combatComponent.specialSkillParent.currentCooldown + "/" + activeCharacter.combatComponent.specialSkillParent.specialSkill?.cooldownInTicks}";
        summary = $"{summary}{"\nElemental Type: " + activeCharacter.combatComponent.elementalDamage.name}";
        //summary = $"{summary}{"\nPrimary Job: " + activeCharacter.jobComponent.primaryJob.ToString()}";
        //summary = $"{summary}{"\nPriority Jobs: " + activeCharacter.jobComponent.GetPriorityJobs()}";
        //summary = $"{summary}{"\nSecondary Jobs: " + activeCharacter.jobComponent.GetSecondaryJobs()}";
        summary = $"{summary}{"\nAble Jobs: " + activeCharacter.jobComponent.GetAbleJobs()}";
        // summary = $"{summary}{"\nAdditional Priority Jobs: " + activeCharacter.jobComponent.GetAdditionalPriorityJobs()}";
        summary = $"{summary}{("\nParty: " + (activeCharacter.partyComponent.hasParty ? activeCharacter.partyComponent.currentParty.partyName : "None") + ", State: " + activeCharacter.partyComponent.currentParty?.partyState.ToString() + ", Members: " + activeCharacter.partyComponent.currentParty?.members.Count + ", Beacon: " + activeCharacter.partyComponent.currentParty?.beaconComponent.currentBeaconCharacter?.name + ", Is Following: " + activeCharacter.partyComponent.isFollowingBeacon)}";
        summary = $"{summary}{"\nPrimary Bed: " + (activeCharacter.tileObjectComponent.primaryBed != null ? activeCharacter.tileObjectComponent.primaryBed.name : "None")}";
        summary = $"{summary}{"\nEnable Digging: " + activeCharacter.movementComponent.enableDigging.ToString()}";
        // summary = $"{summary}{"\nAvoid Settlements: " + activeCharacter.movementComponent.avoidSettlements.ToString()}";
        summary = $"{summary}{"\nPlanner Status: " + activeCharacter.planner.status.ToString()}";
        summary = $"{summary}{"\nNum of action being performed: " + activeCharacter.numOfActionsBeingPerformedOnThis}";

        if (activeCharacter.stateComponent.currentState != null) {
            summary = $"{summary}\nCurrent State: {activeCharacter.stateComponent.currentState}";
            summary = $"{summary}\n\tDuration in state: {activeCharacter.stateComponent.currentState.currentDuration.ToString()}/{activeCharacter.stateComponent.currentState.duration.ToString()}";
        }
        
        summary += "\nBehaviour Components: ";
        for (int i = 0; i < activeCharacter.behaviourComponent.currentBehaviourComponents.Count; i++) {
            CharacterBehaviourComponent component = activeCharacter.behaviourComponent.currentBehaviourComponents[i];
            summary += $"{component}, ";
        }
        
        summary += "\nInterested Items: ";
        for (int i = 0; i < activeCharacter.interestedItemNames.Count; i++) {
            summary += $"{activeCharacter.interestedItemNames[i]}, ";
        }
        
        summary += "\nPersonal Job Queue: ";
        if (activeCharacter.jobQueue.jobsInQueue.Count > 0) {
            for (int i = 0; i < activeCharacter.jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem poi = activeCharacter.jobQueue.jobsInQueue[i];
                summary += $"{poi.jobType.ToString()}-{poi}-Owner: {poi.originalOwner?.name}";
                // if (poi is GoapPlanJob goapPlanJob) {
                //     summary += $"-Plan: {goapPlanJob.assignedPlan?.LogPlan()}, ";    
                // }
                // else {
                    summary += ", ";
                // }
            }
        } else {
            summary += "None";
        }
        summary += $"\nCurrent Schedule Type: {activeCharacter.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick).ToString()}";
        summary += $"\nIs in first hour of Current Schedule section: {activeCharacter.dailyScheduleComponent.schedule.IsInFirstHourOfCurrentScheduleType(GameManager.Instance.currentTick).ToString()}";
        summary += $"\nDaily Schedule: {activeCharacter.dailyScheduleComponent.schedule.GetScheduleSummary()}";
        summary += $"\nToggled Wants: {activeCharacter.villagerWantsComponent?.wantsToProcess.ComafyList()}";
            if (activeCharacter is ShearableAnimal shearableAnimal) {
                summary += $"\nShearable Resource Count: {shearableAnimal.count}";
                summary += $"\nIs Shearable: {shearableAnimal.isAvailableForShearing}";
            }
            if (activeCharacter is SkinnableAnimal skinnable) {
                summary += $"\nSkinnable Resource Count: {skinnable.count}";
            }

            UIManager.Instance.ShowSmallInfo(summary);
#endif
        }
    public static void HideCharacterTestingInfo() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        UIManager.Instance.HideSmallInfo();
#endif
    }
        #endregion
    }
}