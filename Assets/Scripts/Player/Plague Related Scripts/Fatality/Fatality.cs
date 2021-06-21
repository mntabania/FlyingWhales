using System;
using Traits;
using UtilityScripts;
namespace Plague.Fatality {
    public abstract class Fatality : Plagued.IPlaguedListener {

        public abstract PLAGUE_FATALITY fatalityType { get; }
        
        protected abstract void ActivateFatality(Character p_character);

        protected void ActivateFatalityOn(Character p_character) {
            if (CanActivateFatalityOn(p_character)) {
                p_character.causeOfDeath = INTERACTION_TYPE.PLAGUE_FATALITY;
                ActivateFatality(p_character);
            }
        }

        #region Plagued.IPlaguedListener Implementation
        public virtual void PerTickWhileStationaryOrUnoccupied(Character p_character) { }
        public virtual void CharacterGainedTrait(Character p_character, Trait p_gainedTrait) { }
        public virtual void CharacterStartedPerformingAction(Character p_character, ActualGoapNode p_action) { }
        public virtual void CharacterDonePerformingAction(Character p_character, INTERACTION_TYPE p_actionPerformed) { }
        public virtual void HourStarted(Character p_character, int p_numOfHoursPassed) { }
        #endregion

        #region Virtuals
        protected virtual bool CanActivateFatalityOn(Character p_character) {
            if (p_character.traitContainer.HasTrait("Plague Reservoir") || p_character.characterClass.IsZombie()) {
                return false;
            }
            return true;
        }
        #endregion
    }

    public static class FatalityExtensions{
        public static int GetFatalityCost(this PLAGUE_FATALITY fatality) {
            switch (fatality) {
                case PLAGUE_FATALITY.Septic_Shock:
                    return SpellUtilities.GetModifiedSpellCost(30, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification()); ;
                case PLAGUE_FATALITY.Heart_Attack:
                    return SpellUtilities.GetModifiedSpellCost(30, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification()); ;
                case PLAGUE_FATALITY.Stroke:
                    return SpellUtilities.GetModifiedSpellCost(20, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification()); ;
                case PLAGUE_FATALITY.Total_Organ_Failure:
                    return SpellUtilities.GetModifiedSpellCost(40, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification()); ;
                case PLAGUE_FATALITY.Pneumonia:
                    return SpellUtilities.GetModifiedSpellCost(40, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification()); ;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fatality), fatality, null);
            }
        }
        
        public static string GetFatalityTooltip(this PLAGUE_FATALITY fatality) {
            switch (fatality) {
                case PLAGUE_FATALITY.Septic_Shock:
                    return "Plagued Villagers have a risk of succumbing to Septic Shock each time they become Hungry or Starving.";
                case PLAGUE_FATALITY.Heart_Attack:
                    return "Plagued Villagers have a risk of having a Heart Attack each time they become Spent or Drained.";
                case PLAGUE_FATALITY.Stroke:
                    return "Plagued Villagers have a risk of having a Stroke each time they become Tired or Exhausted.";
                case PLAGUE_FATALITY.Total_Organ_Failure:
                    return "Plagued Villagers have a very low risk of Total Organ Failure each time they perform an action.";
                case PLAGUE_FATALITY.Pneumonia:
                    return "Plagued Villagers have a very low risk of succumbing to Pneumonia while moving.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(fatality), fatality, null);
            }
        }
    }
}