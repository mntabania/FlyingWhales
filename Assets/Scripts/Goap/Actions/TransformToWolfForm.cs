using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class TransformToWolfForm : GoapAction {

    public TransformToWolfForm() : base(INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.NONE, targetPOI = actor });
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Transform Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 5;
    }
    #endregion

    #region State Effects
    public void PreTransformSuccess(ActualGoapNode goapNode) { }
    public void AfterTransformSuccess(ActualGoapNode goapNode) {
        //Lycanthrope lycanthropy = goapNode.actor.traitContainer.GetNormalTrait<Trait>("Lycanthrope") as Lycanthrope;
        //lycanthropy.TurnToWolf();
    }
    #endregion

    //#region Intel Reactions
    //private List<string> TransformSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    //Lycanthropy lycanthropy = actor.GetTrait("Lycanthrope") as Lycanthropy;
    //    //Faction actorOrigFaction = lycanthropy.data.faction;

    //    //Recipient and Actor is the same:
    //    if (recipient == actor) {
    //        //- **Recipient Response Text**: Please do not tell anyone else about this. I beg you!
    //        reactions.Add("Please do not tell anyone else about this. I beg you!");
    //        //-**Recipient Effect * *: no effect
    //    }
    //    //Recipient and Actor are from the same faction and are lovers or affairs
    //    else if (actorAlterEgo.faction == recipient.faction && (recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.LOVER) || recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.AFFAIR))) {
    //        //- **Recipient Response Text**: [Actor Name] may be a monster, but I love [him/her] still!
    //        reactions.Add(string.Format("{0} may be a monster, but I love {1} still!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //        //- **Recipient Effect**: no effect
    //    }
    //    //Recipient and Actor are from the same faction and are friends:
    //    else if (actorAlterEgo.faction == recipient.faction && recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
    //        //- **Recipient Response Text**: I cannot be friends with a lycanthrope but I will not report this to the others as my last act of friendship.
    //        reactions.Add("I cannot be friends with a lycanthrope but I will not report this to the others as my last act of friendship.");
    //        //- **Recipient Effect**: Recipient and actor will no longer be friends
    //        RelationshipManager.Instance.RemoveRelationshipBetween(recipient, actorAlterEgo, RELATIONSHIP_TRAIT.FRIEND);
    //    }
    //    //Recipient and Actor are from the same faction and have no relationship or are enemies:
    //    //Ask Marvin if actor and recipient must also have the same home location and they must be both at their home location
    //    else if (actorAlterEgo.faction == recipient.faction && (!recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo) || recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY))) {
    //        //- **Recipient Response Text**: Lycanthropes are not welcome here. [Actor Name] must be restrained!
    //        reactions.Add(string.Format("Lycanthropes are not welcome here. {0} must be restrained!", actor.name));
    //        //-**Recipient Effect**: If soldier, noble or faction leader, brand Actor with Aberration crime (add Apprehend job). Otherwise, add a personal Report Crime job to the Recipient.
    //        if (recipient.role.roleType == CHARACTER_ROLE.SOLDIER || recipient.role.roleType == CHARACTER_ROLE.NOBLE || recipient.role.roleType == CHARACTER_ROLE.LEADER) {
    //            actor.AddCriminalTrait(CRIME.ABERRATION, this);
    //            recipient.CreateApprehendJobFor(actor);
    //            //if (job != null) {
    //            //    recipient.homeSettlement.jobQueue.AssignCharacterToJob(job, this);
    //            //}
    //        } else {
    //            recipient.CreateReportCrimeJob(committedCrime, this, actorAlterEgo);
    //        }
    //    }
    //    //Recipient and Actor are from the same faction (catches all other situations):
    //    else if (actorAlterEgo.faction == recipient.faction) {
    //        //- **Recipient Response Text**: Lycanthropes are not welcome here. [Actor Name] must be restrained!
    //        reactions.Add(string.Format("Lycanthropes are not welcome here. {0} must be restrained!", actor.name));
    //        //-**Recipient Effect**: If soldier, noble or faction leader, brand Actor with Aberration crime (add Apprehend job). Otherwise, add a personal Report Crime job to the Recipient.
    //        if (recipient.role.roleType == CHARACTER_ROLE.SOLDIER || recipient.role.roleType == CHARACTER_ROLE.NOBLE || recipient.role.roleType == CHARACTER_ROLE.LEADER) {
    //            actor.AddCriminalTrait(CRIME.ABERRATION, this);
    //            recipient.CreateApprehendJobFor(actor);
    //            //if (job != null) {
    //            //    recipient.homeSettlement.jobQueue.AssignCharacterToJob(job, this);
    //            //}
    //        } else {
    //            recipient.CreateReportCrimeJob(committedCrime, this, actorAlterEgo);
    //        }
    //    }
    //    //Recipient and Actor are from a different faction and have a positive relationship:
    //    else if (recipient.faction != actorAlterEgo.faction && recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
    //        //- **Recipient Response Text**: I cannot be friends with a lycanthrope.
    //        reactions.Add("I cannot be friends with a lycanthrope.");
    //        //- **Recipient Effect**: Recipient and actor will no longer be friends
    //        RelationshipManager.Instance.RemoveRelationshipBetween(recipient, actorAlterEgo, RELATIONSHIP_TRAIT.FRIEND);
    //    }
    //    //Recipient and Actor are from a different faction and are enemies:
    //    else if (recipient.faction != actorAlterEgo.faction && recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
    //        //- **Recipient Response Text**: I knew there was something impure about [Actor Name]!
    //        reactions.Add(string.Format("I knew there was something impure about {0}!", actor.name));
    //        //- **Recipient Effect**: no effect
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class TransformToWolfFormData : GoapActionData {
    public TransformToWolfFormData() : base(INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }
}