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
            if (sourcePOI is Summon babySummon) {
                if(babySummon.adultSummonType != SUMMON_TYPE.None) {
                    Summon adult = CharacterManager.Instance.CreateNewSummon(babySummon.adultSummonType, babySummon.faction, babySummon.homeSettlement, babySummon.homeRegion, babySummon.homeStructure);
                    if (!babySummon.isUsingDefaultName) {
                        adult.SetFirstAndLastName(babySummon.firstName, babySummon.surName);    
                    }
                    CharacterManager.Instance.PlaceSummonInitially(adult, babySummon.gridTileLocation);
                    adult.ClearTerritory();
                    
                    Log growUpLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "become_giant_spider", null, LOG_TAG.Life_Changes);
                    growUpLog.AddToFillers(adult, adult.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    growUpLog.AddLogToDatabase(true);

                    if (babySummon.HasTerritory()) {
                        adult.SetTerritory(babySummon.territory);
                    }
                    if (UIManager.Instance.IsContextMenuShowingForTarget(babySummon)) {
                        UIManager.Instance.RefreshPlayerActionContextMenuWithNewTarget(adult);
                    }
                }
                babySummon.SetDestroyMarkerOnDeath(true);
                babySummon.SetShowNotificationOnDeath(false);
                babySummon.Death();
                if(UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == babySummon) {
                    UIManager.Instance.monsterInfoUI.CloseMenu();
                }
            }
        }
        #endregion
    }
}

