using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class BabyInfestor : Trait {
        public override bool isSingleton => true;

        public BabyInfestor() {
            name = "Baby Infestor";
            description = "Grows and transforms to older Infestor types";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.ticksPerDay;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character character) {
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
                    adult.InitialCharacterPlacement(summon.gridTileLocation);
                    adult.ClearTerritory();
                    
                    Log growUpLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "become_giant_spider", null, LOG_TAG.Life_Changes);
                    growUpLog.AddToFillers(adult, adult.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    growUpLog.AddLogToDatabase();
                    
                    for (int i = 0; i < summon.territories.Count; i++) {
                        adult.AddTerritory(summon.territories[i]);
                    }
                }
                summon.SetDestroyMarkerOnDeath(true);
                summon.SetShowNotificationOnDeath(false);
                summon.Death();
                if(UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == summon) {
                    UIManager.Instance.monsterInfoUI.CloseMenu();
                }
            }
        }
        #endregion
    }
}

