using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class NatureWorship : CrimeType {

        #region getters
        public override string accuseText => "being a Nature Worshiper";
        #endregion

        public NatureWorship() : base(CRIME_TYPE.Nature_Worship) { }

        #region Overrides
        public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target) {
            if (witness == target && actor.currentActionNode != null && actor.currentActionNode.action.goapType == INTERACTION_TYPE.EVANGELIZE && actor.currentActionNode.target == target) {
                //Target should not consider Demon Worship, Divine Worship or Nature Worship as a crime while being the target of a Preach Action
                //https://trello.com/c/mFCzllwZ/2934-live-03346-successful-preach-gets-reported
                return CRIME_SEVERITY.None;
            }
            return base.GetCrimeSeverity(witness, actor, target);
        }
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " worships Nature";
        }
        #endregion
    }
}