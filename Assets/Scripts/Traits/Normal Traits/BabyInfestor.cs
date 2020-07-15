using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class BabyInfestor : Trait {
        public override bool isSingleton => true;

        public BabyInfestor() {
            name = "Baby Infestor";
            description = "This is a baby infestor.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.ticksPerHour;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                character.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Baby_Infestor_Behaviour);
            }
        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            base.OnRemoveTrait(sourcePOI, removedBy);
            if (sourcePOI is Summon summon) {
                if(summon.adultSummonType != SUMMON_TYPE.None) {
                    Character adult = CharacterManager.Instance.CreateNewSummon(summon.adultSummonType, summon.faction, summon.homeSettlement, summon.homeRegion, summon.homeStructure);
                    adult.SetName(summon.name);
                    adult.CreateMarker();
                    adult.InitialCharacterPlacement(summon.gridTileLocation, true);
                    adult.ClearTerritory();
                    for (int i = 0; i < summon.territorries.Count; i++) {
                        adult.AddTerritory(summon.territorries[i]);

                    }
                }
                summon.SetDestroyMarkerOnDeath(true);
                summon.Death();
                if(UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == summon) {
                    UIManager.Instance.monsterInfoUI.CloseMenu();
                }
            }
        }
        #endregion
    }
}

