using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Traits {
    public class Protection : Status {

        private StatusIcon _statusIcon;
        
        public Protection() {
            name = "Protection";
            description = "Surrounded by a magical barrier that reduces damage.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            //ticksDuration = GameManager.Instance.GetTicksBasedOnHour(8); //if this trait is only temporary, then it should not advertise GET_WATER
            ticksDuration = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.PROTECTION);
        }

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            UpdateVisualsOnAdd(addTo);
        }
        #endregion

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            UpdateVisualsOnAdd(addedTo);
        }
        
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            
            UpdateVisualsOnRemove(removedFrom);
        }
        #endregion

        #region Visuals
        private void UpdateVisualsOnAdd(ITraitable addedTo) {
            if (addedTo is Character character && _statusIcon == null && character.hasMarker) {
                _statusIcon = character.marker.AddStatusIcon("Protected");
            }
        }
        private void UpdateVisualsOnRemove(ITraitable removedFrom) {
            if (removedFrom is Character character && character.hasMarker) {
                ObjectPoolManager.Instance.DestroyObject(_statusIcon.gameObject);
            }
        }
        #endregion
    }
}
