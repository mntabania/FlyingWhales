using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Status : Trait {
        public bool hindersWitness; //if a character has this trait, and this is true, then he/she cannot witness events
        public bool hindersMovement; //if a character has this trait, and this is true, then he/she cannot move
        public bool hindersAttackTarget; //if a character has this trait, and this is true, then he/she cannot be attacked
        public bool hindersPerform; //if a character has this trait, and this is true, then he/she cannot be attacked
        public bool isStacking;
        public int stackLimit;
        public float stackModifier;

        #region Virtuals
        public virtual void OnStackStatus(ITraitable addedTo) {
            if (addedTo is Character) {
                Character character = addedTo as Character;
                character.moodComponent.AddMoodEffect(Mathf.RoundToInt(moodEffect * stackModifier), this);
            }
            SetDateEstablished(GameManager.Instance.Today());
        }
        /// <summary>
        /// Called when a stacking trait is added but the max stacks have been reached.
        /// </summary>
        public virtual void OnStackStatusAddedButStackIsAtLimit(ITraitable traitable) { }
        public virtual void OnUnstackStatus(ITraitable addedTo) {
            if (addedTo is Character) {
                Character character = addedTo as Character;
                character.moodComponent.RemoveMoodEffect(-Mathf.RoundToInt(moodEffect * stackModifier), this);
            }
        }
        public virtual bool IsTangible() { return false; } //is this trait tangible? Only used for traits on tiles, so that the tile's tile object will be activated when it has a tangible trait
        #endregion

        #region Overrides
        public override string GetTestingData(ITraitable traitable = null) {
            if (traitable != null && traitable.traitContainer.stacks.ContainsKey(this.name)) {
                return $"Stacks: {traitable.traitContainer.stacks[this.name].ToString()}/{stackLimit.ToString()}";
            }
            return string.Empty;
        }
        public override string GetNameInUI(ITraitable traitable) {
            Dictionary<string, int> stacks = traitable.traitContainer.stacks;
            if (isStacking && stacks.ContainsKey(name) && stacks[name] > 1) {
                int num = stacks[name];
                if (num > stackLimit) { num = stackLimit; }
                return $"{name} (x{num})";
            }
            return name;
        }
        #endregion
    }
}