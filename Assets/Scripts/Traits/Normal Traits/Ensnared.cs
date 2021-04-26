using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using UnityEngine.Assertions;

namespace Traits {
    public class Ensnared : Status, IElementalTrait {
        //public override bool isSingleton => true;

        public Character owner { get; private set; }
        public bool isPlayerSource { get; private set; }


        #region getters
        public override Type serializedData => typeof(SaveDataEnsnared);
        #endregion

        public Ensnared() {
            name = "Ensnared";
            description = "Trapped and unable to move.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(3);
            hindersMovement = true;
            hindersPerform = true;
            moodEffect = -5;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.REMOVE_ENSNARED };
        }

        #region Overrides
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataEnsnared data = saveDataTrait as SaveDataEnsnared;
            isPlayerSource = data.isPlayerSource;
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            owner = addTo as Character;
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            owner = addedTo as Character;
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            DisablePlayerSourceChaosOrb(owner);
        }
        protected override string GetDescriptionInUI() {
            string desc = base.GetDescriptionInUI();
            desc += "\nIs Player Source: " + isPlayerSource;
            return desc;
        }
        #endregion

        #region IElementalTrait
        public void SetIsPlayerSource(bool p_state) {
            if (isPlayerSource != p_state) {
                isPlayerSource = p_state;
                if (isPlayerSource) {
                    EnablePlayerSourceChaosOrb(owner);
                } else {
                    DisablePlayerSourceChaosOrb(owner);
                }
            }
        }
        #endregion
    }
}
public class SaveDataEnsnared : SaveDataTrait {
    public bool isPlayerSource;

    public override void Save(Trait trait) {
        base.Save(trait);
        Ensnared data = trait as Ensnared;
        Assert.IsNotNull(data);
        isPlayerSource = data.isPlayerSource;
    }
}