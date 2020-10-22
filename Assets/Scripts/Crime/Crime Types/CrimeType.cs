using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class CrimeType {
        public CRIME_TYPE type { get; private set; }
        public string name { get; private set; }

        #region getters
        public virtual string accuseText => name;
        #endregion

        public CrimeType(CRIME_TYPE type) {
            this.type = type;
            name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(type.ToString());
        }

        #region Virtuals
        public virtual CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target) {
            if(witness.traitContainer.HasTrait("Cultist") && actor.traitContainer.HasTrait("Cultist")) {
                //cultists should not consider each others actions as crimes, unless they are directed towards each other.
                //https://trello.com/c/5KPHa5gA/2587-prevent-cultists-from-reporting-each-other
                if (target is TileObject targetTileObject) {
                    if (!targetTileObject.IsOwnedBy(witness)) {
                        return CRIME_SEVERITY.None;
                    }
                } else if (target is Character targetCharacter) {
                    if (targetCharacter != witness) {
                        return CRIME_SEVERITY.None;    
                    }    
                } else {
                    return CRIME_SEVERITY.None;
                }
                
            }
            return CRIME_SEVERITY.Unapplicable;
        }
        public virtual string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) { return string.Empty; }
        #endregion
    }
}