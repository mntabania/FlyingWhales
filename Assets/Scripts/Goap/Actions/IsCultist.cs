public class IsCultist : GoapAction {
    public IsCultist() : base(INTERACTION_TYPE.IS_CULTIST) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Cultist Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(witness, node, status);
        Character actor = node.actor;
        Character target = node.poiTarget as Character;
        if (witness.traitContainer.HasTrait("Cultist") == false) {
            CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.SERIOUS);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
            if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasOpinion(actor, BaseRelationshipContainer.Acquaintance)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status, node);    
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
            } else if (witness.traitContainer.HasTrait("Psychopath") == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
            }

            if (target != null && witness.relationshipContainer.IsEnemiesWith(target) == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
            }
        }
        else {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
            if (RelationshipManager.IsSexuallyCompatible(witness.sexuality, actor.sexuality, witness.gender,
                actor.gender)) {
                int chance = 10 * witness.relationshipContainer.GetCompatibility(actor);
                int roll = UnityEngine.Random.Range(0, 100);
                if (roll < chance) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, actor, status, node);                    
                }
            }
        }
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    #endregion
    
    #region State Effects
    public void PreCultistSuccess(ActualGoapNode goapNode) { }
    public void PerTickCultistSuccess(ActualGoapNode goapNode) { }
    public void AfterCultistSuccess(ActualGoapNode goapNode) { }
    #endregion
}