using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class Evangelize : GoapAction {
    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;
    public Evangelize() : base(INTERACTION_TYPE.EVANGELIZE) {
        actionIconString = GoapActionStateDB.Cult_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Player, LOG_TAG.Crimes};
    }
    
    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Cultist Kit", false, GOAP_EFFECT_TARGET.ACTOR), HasCultistKit);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Evangelize Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +0(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 0;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateReactionsToActor(reactions, actor, target, witness, node, status);
        Character poiTarget = target as Character;
        if (witness.traitContainer.HasTrait("Cultist") == false) {
            reactions.Add(EMOTION.Shock);
            if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasOpinion(actor, RelationshipManager.Acquaintance)) {
                reactions.Add(EMOTION.Despair);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                reactions.Add(EMOTION.Fear);
            } else if (witness.traitContainer.HasTrait("Psychopath") == false) {
                reactions.Add(EMOTION.Threatened);
            }

            if (poiTarget != null && witness.relationshipContainer.IsEnemiesWith(poiTarget) == false) {
                reactions.Add(EMOTION.Disapproval);
            }
        } else {
            reactions.Add(EMOTION.Approval);
            if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                int chance = 10 * witness.relationshipContainer.GetCompatibility(actor);
                int roll = UnityEngine.Random.Range(0, 100);
                if (roll < chance) {
                    reactions.Add(EMOTION.Arousal);
                }
            }
        }
    }
    #endregion

    #region Preconditions
    private bool HasCultistKit(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.HasItem("Cultist Kit");
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job) {
        bool hasMetRequirements = base.AreRequirementsSatisfied(actor, target, otherData, job);
        if (hasMetRequirements) {
            return target != actor && !target.traitContainer.HasTrait("Cultist");
        }
        return false;
    }
    #endregion
    
    #region State Effects
    public void PreEvangelizeSuccess(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        Assert.IsNotNull(targetCharacter, $"Target of Evangelize is not a character! Actor: {goapNode.actor.name}. Target: {goapNode.poiTarget?.name ?? "Null"}");
        WeightedDictionary<bool> weights = new WeightedDictionary<bool>();

        int success = 50;
        int fail = 50;

        
        int opinion = targetCharacter.relationshipContainer.GetTotalOpinion(goapNode.actor);
        if (opinion > 0) {
            success += opinion;
        } else if (opinion < 0) {
            fail += Mathf.Abs(opinion);
        }

        if (goapNode.actor.traitContainer.HasTrait("Persuasive")) {
            success += 100;
        }
        
        //target success
        if (targetCharacter.traitContainer.HasTrait("Evil")) {
            success += 100;
        }
        if (targetCharacter.traitContainer.HasTrait("Treacherous")) {
            success += 100;
        }
        if (targetCharacter.traitContainer.HasTrait("Betrayed")) {
            success += 100;
        }
        if (targetCharacter.moodComponent.moodState == MOOD_STATE.Bad) {
            success += 100;
        } else if (targetCharacter.moodComponent.moodState == MOOD_STATE.Critical) {
            success += 200;
        }

        //target fail
        if (targetCharacter.traitContainer.HasTrait("Vigilant")) {
            fail += 50;
        }
        if (targetCharacter.traitContainer.IsBlessed()) {
            fail += 100;
        }
        if (targetCharacter.isSettlementRuler) {
            fail += 200;
        }
        if (targetCharacter.characterClass.className == "Hero") {
            fail += 500;
        }
        if (targetCharacter.isFactionLeader) {
            fail += 500;
        }
        
        weights.AddElement(true, success);
        weights.AddElement(false, fail);
        weights.LogDictionaryValues($"{goapNode.actor.name} evangelize of {targetCharacter.name} weights.");

        if (!weights.PickRandomElementGivenWeights()) {
            if ((targetCharacter.relationshipContainer.IsFamilyMember(goapNode.actor) || 
                 targetCharacter.relationshipContainer.HasRelationshipWith(goapNode.actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER) || 
                 targetCharacter.relationshipContainer.HasOpinionLabelWithCharacter(goapNode.actor, RelationshipManager.Close_Friend)) &&
                !targetCharacter.relationshipContainer.IsEnemiesWith(goapNode.actor)) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", "Evangelize", "nothing_happens", goapNode, LOG_TAG.Crimes, LOG_TAG.Work, LOG_TAG.Social);
                log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                goapNode.OverrideDescriptionLog(log);
            } else if (CrimeManager.Instance.GetCrimeSeverity(targetCharacter, goapNode.actor, targetCharacter, CRIME_TYPE.Demon_Worship) > CRIME_SEVERITY.Infraction) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", "Evangelize", "crime", goapNode, LOG_TAG.Crimes, LOG_TAG.Work, LOG_TAG.Social);
                log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                goapNode.OverrideDescriptionLog(log);
            }
        }
    }
    public void AfterEvangelizeSuccess(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        Assert.IsNotNull(targetCharacter, $"Target of Evangelize is not a character! Actor: {goapNode.actor.name}. Target: {goapNode.poiTarget?.name ?? "Null"}");
        //TODO: Use something else rather than the description log of the action, because this can easily be forgotten in case the log of this action changes.
        if (goapNode.descriptionLog.logText.Contains("effective")) {
            targetCharacter.traitContainer.AddTrait(targetCharacter, "Cultist");
        } else if (goapNode.descriptionLog.logText.Contains("accused")) {
            // CrimeManager.Instance.MakeCharacterACriminal(CRIME_TYPE.Demon_Worship, 
            //     CrimeManager.Instance.GetCrimeSeverity(targetCharacter, goapNode.actor, targetCharacter, CRIME_TYPE.Demon_Worship, goapNode), 
            //     goapNode, targetCharacter, goapNode.actor, targetCharacter, targetCharacter.faction, REACTION_STATUS.WITNESSED, goapNode.actor.traitContainer.GetNormalTrait<Criminal>("Criminal"));
            targetCharacter.assumptionComponent.CreateAndReactToNewAssumption(goapNode.actor, goapNode.actor, INTERACTION_TYPE.IS_CULTIST, REACTION_STATUS.WITNESSED);
        }
        goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.CULTIST_KIT);
    }
    #endregion

    #region Reactions
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Demon_Worship;
    }
    #endregion
}