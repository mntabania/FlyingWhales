using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Traits {
    public class Necromancer : Trait {
        public Character owner { get; private set; }
        public LocationStructure lairStructure { get; private set; }
        public string prevClassName { get; private set; }

        public Necromancer() {
            name = "Necromancer";
            description = "This is a necromancer.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            Character owner = addedTo as Character;
            prevClassName = owner.characterClass.className;
            owner.AssignClass("Necromancer");
            owner.SetNecromancerTrait(this);
            owner.ChangeFactionTo(FactionManager.Instance.undeadFaction);
            FactionManager.Instance.undeadFaction.OnlySetLeader(owner);
            CharacterManager.Instance.SetNecromancerInTheWorld(owner);
            owner.MigrateHomeStructureTo(null);
            owner.ClearTerritory();
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            owner.AssignClass(prevClassName);
            owner.SetNecromancerTrait(null);
            owner.ChangeFactionTo(FactionManager.Instance.friendlyNeutralFaction);
            FactionManager.Instance.undeadFaction.OnlySetLeader(null);
            CharacterManager.Instance.SetNecromancerInTheWorld(null);
        }
        #endregion

        #region Utilities
        public void SetLairStructure(LocationStructure structure) {
            lairStructure = structure;
        }
        #endregion
    }
}
