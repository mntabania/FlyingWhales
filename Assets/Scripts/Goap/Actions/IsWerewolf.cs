using Traits;

public class IsWerewolf : GoapAction {
    public IsWerewolf() : base(INTERACTION_TYPE.IS_WEREWOLF) {
        actionIconString = GoapActionStateDB.No_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        logTags = new[] {LOG_TAG.Crimes};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Werewolf Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
  public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);

        if(actor.isLycanthrope) {
            actor.lycanData.AddAwareCharacter(witness);
        }

        CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, node.crimeType, node, status);

        CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Werewolf);
        if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status, node);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
            } else if (!witness.traitContainer.HasTrait("Psychopath")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
            }
            if (target is Character targetCharacter) {
                string opinionToTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionToTarget == RelationshipManager.Friend || opinionToTarget == RelationshipManager.Close_Friend) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                } else if (opinionToTarget == RelationshipManager.Acquaintance || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
                    if (!witness.traitContainer.HasTrait("Psychopath")) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                    }
                }
            }
        } else {
            if (witness.traitContainer.HasTrait("Lycanphiliac")) {
                if(RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, actor, status, node);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
                }
            } else if (witness.traitContainer.HasTrait("Lycanphobic")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
            }
        }
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Werewolf;
    }
    #endregion

    #region State Effects
    public void PreVampireSuccess(ActualGoapNode goapNode) { }
    public void PerTickVampireSuccess(ActualGoapNode goapNode) { }
    public void AfterVampireSuccess(ActualGoapNode goapNode) { }
    #endregion
}