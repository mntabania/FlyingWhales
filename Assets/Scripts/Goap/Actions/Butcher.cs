using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Logs;
using UnityEngine;  
using Traits;

public class Butcher : GoapAction {

    public int m_amountProducedPerTick = 2;
    private const float _coinGainMultiplier = 0.344f;
    public Butcher() : base(INTERACTION_TYPE.BUTCHER) {
        actionIconString = GoapActionStateDB.Butcher_Icon;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.KOBOLD, RACE.TROLL, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work, LOG_TAG.Needs};
        shouldAddLogs = false;
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetDead);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_FOOD, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Transform Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = string.Empty;
#endif
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            if (!actor.partyComponent.hasParty) {
                //target is at structure that character is avoiding
#if DEBUG_LOG
                costLog += $" +2000(Location of target is in avoid structure)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return 2000;
            }
        }
        if (job.jobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP) {
            if (target.gridTileLocation != null && actor.gridTileLocation != null) {
                LocationGridTile centerGridTileOfTarget = target.gridTileLocation.area.gridTileComponent.centerGridTile;
                LocationGridTile centerGridTileOfActor = actor.gridTileLocation.area.gridTileComponent.centerGridTile;
                float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
                int distanceToCheck = InnerMapManager.AreaLocationGridTileSize.x * 3;

                if (distance > distanceToCheck) {
                    //target is at structure that character is avoiding
#if DEBUG_LOG
                    costLog += $" +2000(Location of target too far from actor)";
                    actor.logComponent.AppendCostLog(costLog);
#endif
                    return 2000;
                }
            }
            if (target is Character characterTarget) {
               if(actor.partyComponent.hasParty && actor.partyComponent.currentParty.IsMember(characterTarget)) {
                    //Should not butcher party members when party is camping
#if DEBUG_LOG
                    costLog += $" +2000(Cannot butcher party member when camping)";
                    actor.logComponent.AppendCostLog(costLog);
#endif
                    return 2000;
               }
            }
        }
#if DEBUG_LOG
        costLog = $"\n{name} {target.nameWithID}:";
#endif
        Character targetCharacter = GetDeadCharacter(target);
        int cost = 0;
        //int cost = GetFoodAmountTakenFromDead(deadCharacter);
        //costLog += " +" + cost + "(Initial)";
        if(targetCharacter != null) {
            if (actor == targetCharacter) {
                cost += 2000;
#if DEBUG_LOG
                costLog += " +2000(Actor/Target Same)";
#endif
            } else {
                if (actor.traitContainer.HasTrait("Enslaved") && job.jobType == JOB_TYPE.PRODUCE_FOOD && job.originalOwner.ownerType == JOB_OWNER.SETTLEMENT && targetCharacter.faction == actor.faction) {
                    cost += 2000;
#if DEBUG_LOG
                    costLog += " +2000(Actor is Slave, job is Produce Food Settlement, Actor/Target same faction)";
                    actor.logComponent.AppendCostLog(costLog);
#endif
                    return cost;
                }
                if (!actor.isNormalCharacter) {
                    cost += 10;
#if DEBUG_LOG
                    costLog += " +10(Actor is not a normal character)";
#endif
                } else {
                    bool isCannibal = actor.traitContainer.HasTrait("Cannibal");
                    if (job.jobType == JOB_TYPE.TRIGGER_FLAW && isCannibal && !actor.traitContainer.HasTrait("Vampire")) {
                        cost = UtilityScripts.Utilities.Rng.Next(450, 551);
#if DEBUG_LOG
                        costLog += $" {cost}(Actor is cannibal and job is trigger flaw)";
                        actor.logComponent.AppendCostLog(costLog);
#endif
                        return cost;
                    }
                    if (isCannibal && !actor.traitContainer.HasTrait("Vampire")) {
                        if (actor.traitContainer.HasTrait("Malnourished")) {
                            if (actor.relationshipContainer.IsFriendsWith(targetCharacter)) {
                                int currCost = UtilityScripts.Utilities.Rng.Next(100, 151);
                                cost += currCost;
#if DEBUG_LOG
                                costLog += $" +{currCost}(Cannibal, Malnourished, Friend/Close)";
#endif
                            }
                            if (targetCharacter.race.IsSapient() || targetCharacter.IsRatmanThatIsPartOfMajorFaction()) {
                                cost += 100;
#if DEBUG_LOG
                                costLog += " +300(Cannibal, Malnourished, Human/Elf/Sapient Ratman)";
#endif
                            }
                        } else {
                            if (actor.relationshipContainer.IsFriendsWith(targetCharacter)) {
                                cost += 2000;
#if DEBUG_LOG
                                costLog += " +2000(Cannibal, Friend/Close)";
#endif
                            }
                            if ((targetCharacter.race.IsSapient() || targetCharacter.IsRatmanThatIsPartOfMajorFaction()) && !actor.needsComponent.isStarving) {
                                cost += 200;
#if DEBUG_LOG
                                costLog += " +2000(Cannibal, Human/Elf/Sapient Ratman, not Starving)";
#endif
                            }
                        }
                    } else {
                        //not cannibal
                        if (actor.traitContainer.HasTrait("Malnourished")) {
                            if (actor.relationshipContainer.IsFriendsWith(targetCharacter)) {
                                int currCost = UtilityScripts.Utilities.Rng.Next(100, 151);
                                cost += currCost;
#if DEBUG_LOG
                                costLog += $" +{currCost}(not Cannibal, Malnourished, Friend/Close)";
#endif
                            }
                            if (targetCharacter.race.IsSapient() || targetCharacter.IsRatmanThatIsPartOfMajorFaction()) {
                                cost += 200;
#if DEBUG_LOG
                                costLog += " +200(not Cannibal, Malnourished, Human/Elf/Sapient Ratman)";
#endif
                            }
                        } else {
                            if (targetCharacter.race.IsSapient() || targetCharacter.IsRatmanThatIsPartOfMajorFaction()) {
                                cost += 2000;
#if DEBUG_LOG
                                costLog += " +2000(not Cannibal, not malnourished or starving, Human/Elf/Sapient Ratman)";
#endif
                            }
                            //if (actor.needsComponent.isStarving) {
                            //    if (actor.relationshipContainer.IsFriendsWith(targetCharacter)) {
                            //        int currCost = 300;
                            //        cost += currCost;
                            //        costLog += $" +300(not Cannibal, not Malnourished but starving, Friend/Close)";
                            //    }
                            //} else {
                            //    if (targetCharacter.race.IsSapient() || targetCharacter.IsRatmanThatIsPartOfMajorFaction()) {
                            //        cost += 2000;
                            //        costLog += " +2000(not Cannibal, not malnourished or starving, Human/Elf/Sapient Ratman)";
                            //    }    
                            //}
                        }
                    }
                }
            }
            if(targetCharacter.race == RACE.HUMANS) {
                int currCost = UtilityScripts.Utilities.Rng.Next(80, 91);
                cost += currCost;
#if DEBUG_LOG
                costLog += $" +{currCost}(Human)";
#endif
            } else if (targetCharacter.race == RACE.ELVES) {
                int currCost = UtilityScripts.Utilities.Rng.Next(80, 91);
                cost += currCost;
#if DEBUG_LOG
                costLog += $" +{currCost}(Elf)";
#endif
            } else if (targetCharacter.race == RACE.RATMAN) {
                int currCost = UtilityScripts.Utilities.Rng.Next(80, 91);
                cost += currCost;
#if DEBUG_LOG
                costLog += $" +{currCost}(Ratman)";
#endif
            }
            //else if (deadCharacter.race == RACE.WOLF) {
            //    int currCost = UtilityScripts.Utilities.Rng.Next(50, 61);
            //    cost += currCost;
            //    costLog += $" +{currCost}(Wolf)";
            //} 
            else if (targetCharacter.race == RACE.DEMON || targetCharacter.race == RACE.LESSER_DEMON) {
                int currCost = UtilityScripts.Utilities.Rng.Next(90, 111);
                cost += currCost;
#if DEBUG_LOG
                costLog += $" +{currCost}(Demon)";
#endif
            }

            //Not everyone loves eating Rat/Ratman
            //Humans are into butchering rat/ratman but Elves are not
            if (actor.race == RACE.ELVES) {
                if(targetCharacter.race == RACE.RATMAN || targetCharacter.race == RACE.RAT) {
                    cost += 150;
#if DEBUG_LOG
                    costLog += $" +150(Actor is Elf, Target is Rat/Ratman)";
#endif
                }
            }

            if (!targetCharacter.isDead) {
                cost *= 2;
#if DEBUG_LOG
                costLog += $" {cost}(Still Alive)";
#endif
            }
            if (targetCharacter is Animal || targetCharacter.race == RACE.WOLF || targetCharacter.race == RACE.SPIDER) {
                if (!actor.characterClass.IsCombatant() && !targetCharacter.isDead && (targetCharacter.race == RACE.WOLF || targetCharacter.race == RACE.SPIDER)) {
                    cost += 2000;
#if DEBUG_LOG
                    costLog += $" +{cost}(Non-combatant actor, Alive Wolf/Spider target)";
#endif
                }
                CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(actor, actor, targetCharacter, CRIME_TYPE.Animal_Killing);
                int currCost = 0;
                if (severity == CRIME_SEVERITY.Infraction) {
                    currCost += UtilityScripts.Utilities.Rng.Next(80, 91);
#if DEBUG_LOG
                    costLog += $" +{currCost}(Animal/Infraction)";
#endif
                } else if (severity == CRIME_SEVERITY.Misdemeanor || severity == CRIME_SEVERITY.Serious || severity == CRIME_SEVERITY.Heinous) {
                    if (actor.traitContainer.HasTrait("Malnourished")) {
                        if (actor.relationshipContainer.IsFriendsWith(targetCharacter)) {
                            currCost += 200;
#if DEBUG_LOG
                            costLog += $" +{currCost}(Animal/Misdemeanor/Serious/Heinous/Malnourished/Friend/Close Friend)";
#endif
                        }
                        currCost += UtilityScripts.Utilities.Rng.Next(100, 111);
#if DEBUG_LOG
                        costLog += $" +{currCost}(Animal/Misdemeanor/Serious/Heinous/Malnourished)";
#endif
                    } else {
                        currCost += 2000;
#if DEBUG_LOG
                        costLog += $" +{currCost}(Animal/Misdemeanor/Serious/Heinous/not Malnourished)";
#endif
                    }
                } else {
                    currCost += UtilityScripts.Utilities.Rng.Next(40, 51);
#if DEBUG_LOG
                    costLog += $" +{currCost}(Animal/No Severity)";
#endif
                }
                cost += currCost;
                if (!targetCharacter.isDead) {
                    cost *= 2;
#if DEBUG_LOG
                    costLog += $" {cost}(Still Alive)";
#endif
                }
            }
        }
#if DEBUG_LOG
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        IPointOfInterest poiTarget = node.poiTarget;
        if(node.poiTarget is Tombstone) {
            poiTarget = (node.poiTarget as Tombstone).character;
        }
        log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool defaultTargetMissing = this.IsTargetMissing(actor, poiTarget);
        return new GoapActionInvalidity(defaultTargetMissing, stateName, "target_unavailable");
    }
    private bool IsTargetMissing(Character actor, IPointOfInterest poiTarget) {
        if (poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion
              || (poiTarget is Character character && !character.isDead) || poiTarget.numOfActionsBeingPerformedOnThis > 0 || poiTarget.isBeingCarriedBy != null || (poiTarget is Character c && c.grave?.isBeingCarriedBy != null)) {
            return true;
        } else if (actor.gridTileLocation != poiTarget.gridTileLocation && !actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation, true)) {
            if (actor.hasMarker && actor.marker.IsCharacterInLineOfSightWith(poiTarget)) {
                return false;
            }
            return true;
        }
        return false;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        Character targetCharacter = GetDeadCharacter(target);
        if (targetCharacter != null) {
            if (!witness.traitContainer.HasTrait("Cannibal") && targetCharacter.race.IsSapient()) {
                reactions.Add(EMOTION.Shock);
                reactions.Add(EMOTION.Disgust);

                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
                if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    reactions.Add(EMOTION.Disappointment);
                }
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Fear);
                }
            }
            string witnessOpinionToTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (witnessOpinionToTarget == RelationshipManager.Friend || witnessOpinionToTarget == RelationshipManager.Close_Friend) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Rage);
                }
            } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER))
                && witnessOpinionToTarget != RelationshipManager.Rival) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Rage);
                }
            } else if (witnessOpinionToTarget == RelationshipManager.Acquaintance
                || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Anger);
                }
            }
        }
    }
    public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
        Character targetCharacter = GetDeadCharacter(target);
        if (targetCharacter != null) {
            string witnessOpinionToTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (witnessOpinionToTarget == RelationshipManager.Friend || witnessOpinionToTarget == RelationshipManager.Close_Friend) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Despair);
                    reactions.Add(EMOTION.Sadness);
                }
            } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                && witnessOpinionToTarget != RelationshipManager.Rival) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Despair);
                    reactions.Add(EMOTION.Sadness);
                }
            }
        }
    }
    //public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
    //    string reaction = base.ReactionToActor(actor, target, witness, node, status);
    //    Character targetCharacter = GetDeadCharacter(target); 
    //    if (!actor.isNormalCharacter && witness.homeSettlement != null && witness.faction != null && actor.homeStructure != null) {
    //        Prisoner prisoner = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
    //        if (node.targetStructure == actor.homeStructure || (prisoner != null && prisoner.IsConsideredPrisonerOf(actor))) {
    //            string relationshipName = witness.relationshipContainer.GetRelationshipNameWith(targetCharacter);
    //            if (relationshipName == RelationshipManager.Acquaintance || witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
    //                witness.faction.partyQuestBoard.CreateExterminatePartyQuest(witness, witness.homeSettlement, actor.homeStructure, witness.homeSettlement);    
    //            }    
    //        }
    //    }
    //    return reaction;
    //}
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.poiTarget is Character character) {
            if (character.isNormalCharacter) {
                return REACTABLE_EFFECT.Negative;
            }
        }
        return REACTABLE_EFFECT.Positive;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        if (target is Character targetCharacter) {
            if (actor.race.IsSapient() && targetCharacter.race.IsSapient()) {
                return CRIME_TYPE.Cannibalism;
            } else if(actor.race.IsSapient() && (targetCharacter is Animal || targetCharacter.race == RACE.WOLF || targetCharacter.race == RACE.SPIDER || targetCharacter.race == RACE.KOBOLD)) {
                return CRIME_TYPE.Animal_Killing;
            }
        }
        return base.GetCrimeType(actor, target, crime);
    }
    #endregion

    #region Requirements
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        if (node.currentStateDuration > 0) {
            ProduceMats(node);
        }
    }
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            if (poiTarget.isBeingCarriedBy != null) {
                return false;
            }
            if (poiTarget is Character character && character.grave != null && character.grave.isBeingCarriedBy != null) {
                return false;
            }
            return true;
            //if(poiTarget is Animal) {
            //    return true;
            //} else if (actor.isNormalCharacter == false) {
            //    return true;
            //} else {
            //    Character deadCharacter = GetDeadCharacter(poiTarget);
            //    if (deadCharacter != null && (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES)
            //        && actor.faction != deadCharacter.faction && actor.homeSettlement != deadCharacter.homeSettlement) {
            //        if (actor.traitContainer.HasTrait("Cannibal")) {
            //            return true;
            //        }
            //        return false;
            //    }
            //}
        }
        return false;
    }
#endregion

#region Preconditions
    private bool IsTargetDead(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        if (poiTarget is Character character) {
            return character.isDead;
        }
        return true;
    }
#endregion

    private Character GetDeadCharacter(IPointOfInterest poiTarget) {
        if (poiTarget is Character target) {
            return target;
        } else if (poiTarget is Tombstone tombstone) {
            return tombstone.character;
        }
        return null;
    }

#region State Effects
    public void PreTransformSuccess(ActualGoapNode goapNode) {
        Character deadCharacter = GetDeadCharacter(goapNode.poiTarget);
        int transformedFood = CharacterManager.Instance.GetFoodAmountTakenFromPOI(deadCharacter);

        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        goapNode.descriptionLog.AddToFillers(null, transformedFood.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterTransformSuccess(ActualGoapNode goapNode) {
        ProduceMats(goapNode);
    }

    public void ProduceMats(ActualGoapNode p_node) {
        IPointOfInterest poiTarget = p_node.poiTarget;
        LocationGridTile tileLocation = poiTarget.gridTileLocation;

        tileLocation.structure.RemoveCharacterAtLocation(poiTarget as Character);
        if (poiTarget is Character character) {
            if (character.grave != null && character.grave.gridTileLocation != null) {
                //if character is at a tombstone, destroy tombstone and character marker.
                character.grave.SetRespawnCorpseOnDestroy(false);
                character.grave.gridTileLocation.structure.RemovePOI(character.grave);
            } else {
                character.DestroyMarker();
                if (character.currentRegion != null) {
                    character.currentRegion.RemoveCharacterFromLocation(character);
                }
            }
        }

        FoodPile foodPile = CharacterManager.Instance.CreateFoodPileForPOI(poiTarget, tileLocation, false);
        int amount = p_node.currentStateDuration * m_amountProducedPerTick;
        p_node.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt(amount * _coinGainMultiplier));
        foodPile.SetResourceInPile(amount);
        if (p_node.associatedJobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP) {
            if (p_node.actor.partyComponent.hasParty && p_node.actor.partyComponent.currentParty.targetCamp != null) {
                p_node.actor.partyComponent.currentParty.jobComponent.CreateHaulForCampJob(foodPile, p_node.actor.partyComponent.currentParty.targetCamp);
                p_node.actor.marker.AddPOIAsInVisionRange(foodPile); //automatically add pile to character's vision so he/she can take haul job immediately after
            }
        } else {
            if (foodPile != null && p_node.actor.homeSettlement != null) { //&& !(foodPile is HumanMeat) && !(foodPile is ElfMeat)
                bool cannotCreateHaulJob = (foodPile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT || foodPile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT) && p_node.actor.faction != null && p_node.actor.faction.isMajorNonPlayer;
                if (!cannotCreateHaulJob) {
                    p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(foodPile);
                    p_node.actor.marker.AddPOIAsInVisionRange(foodPile); //automatically add pile to character's vision so he/she can take haul job immediately after
                }
            }
        }
        if (foodPile != null) {
            p_node.descriptionLog.AddInvolvedObjectManual(foodPile.persistentID);
            //if produced human/elf meat and the actor is not a cannibal, make him/her traumatized
            if ((foodPile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT || foodPile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT)
               && !p_node.actor.traitContainer.HasTrait("Cannibal") && p_node.actor.isNormalCharacter && poiTarget is Character targetCharacter) {
                p_node.actor.traitContainer.AddTrait(p_node.actor, "Traumatized", targetCharacter);
            }
        }
        p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Food).AdjustExperience(8, p_node.actor);
        ProduceLogs(p_node, foodPile);
    }

    public void ProduceLogs(ActualGoapNode p_node, FoodPile foodPile) {
        string addOnText = (p_node.currentStateDuration * m_amountProducedPerTick).ToString() + " " + UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(foodPile.tileObjectType.ToString());
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "produced_resources", p_node, LOG_TAG.Work);
        log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, addOnText, LOG_IDENTIFIER.STRING_1);
        p_node.LogAction(log, true);
    }
    #endregion
}