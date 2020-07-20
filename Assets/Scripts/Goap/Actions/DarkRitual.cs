using UnityEngine;

public class DarkRitual : GoapAction {
    public DarkRitual() : base(INTERACTION_TYPE.DARK_RITUAL) {
        actionIconString = GoapActionStateDB.Magic_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Cultist Kit", false, GOAP_EFFECT_TARGET.ACTOR), HasCultistKit);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Ritual Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        if (witness.traitContainer.HasTrait("Cultist") == false) {
            CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.SERIOUS);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);

            if (witness.relationshipContainer.IsFriendsWith(actor) || 
                witness.relationshipContainer.HasOpinionLabelWithCharacter(actor, BaseRelationshipContainer.Acquaintance)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status, node);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
            } else if (witness.traitContainer.HasTrait("Psychopath") == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
            }
        } else {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
            if (RelationshipManager.IsSexuallyCompatible(witness.sexuality, actor.sexuality, witness.gender,
                actor.gender)) {
                int chance = 10 * witness.relationshipContainer.GetCompatibility(actor);
                int roll = Random.Range(0, 100);
                if (roll < chance) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, actor, status, node);                    
                }
            }
        }
        
        
        return response;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, target, otherData);
        if (satisfied) {
            return target.gridTileLocation != null;
        }
        return false;
    }
    #endregion
    
    #region Preconditions
    private bool HasCultistKit(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.HasItem("Cultist Kit");
    }
    #endregion
    
    #region State Effects
    public void AfterRitualSuccess(ActualGoapNode goapNode) {
        Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, goapNode.poiTarget.worldPosition, Random.Range(2, 6), 
            goapNode.poiTarget.gridTileLocation.parentMap);
        goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.CULTIST_KIT);
    }
    #endregion
}