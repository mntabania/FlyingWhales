﻿using System;
using Traits;
namespace Plague.Symptom {
    public abstract class PlagueSymptom : Plagued.IPlaguedListener {

        public abstract PLAGUE_SYMPTOM symptomType { get; }
        
        protected abstract void ActivateSymptom(Character p_character);
        
        #region Plagued.IPlaguedListener
        public virtual void PerTickWhileStationaryOrUnoccupied(Character p_character) { }
        public virtual void CharacterGainedTrait(Character p_character, Trait p_gainedTrait) { }
        /// <summary>
        /// Listener for when a character starts performing an action
        /// </summary>
        /// <param name="p_character">The actor.</param>
        /// <param name="p_action">The action to be performed.</param>
        /// <returns>Whether or not the action will proceed after this.</returns>
        public virtual void CharacterStartedPerformingAction(Character p_character, ActualGoapNode p_action) { }
        public virtual void CharacterDonePerformingAction(Character p_character, ActualGoapNode p_actionPerformed) { }
        public virtual void HourStarted(Character p_character, int p_numOfHoursPassed) { }
        #endregion
    }

    public static class PlagueSymptomExtensions{
        public static int GetSymptomCost(this PLAGUE_SYMPTOM p_symptom) {
            switch (p_symptom) {
                case PLAGUE_SYMPTOM.Paralysis:
                    return 30;
                case PLAGUE_SYMPTOM.Vomiting:
                    return 25;
                case PLAGUE_SYMPTOM.Lethargy:
                    return 20;
                case PLAGUE_SYMPTOM.Seizure:
                    return 25;
                case PLAGUE_SYMPTOM.Insomnia:
                    return 20;
                case PLAGUE_SYMPTOM.Poison_Cloud:
                    return 40;
                case PLAGUE_SYMPTOM.Monster_Scent:
                    return 20;
                case PLAGUE_SYMPTOM.Sneezing:
                    return 35;
                case PLAGUE_SYMPTOM.Depression:
                    return 30;
                case PLAGUE_SYMPTOM.Hunger_Pangs:
                    return 20;
                default:
                    throw new ArgumentOutOfRangeException(nameof(p_symptom), p_symptom, null);
            }
        }
        public static string GetSymptomTooltip(this PLAGUE_SYMPTOM p_symptom) {
            switch (p_symptom) {
                case PLAGUE_SYMPTOM.Paralysis:
                    return "The Plague may eventually render about a quarter of Plague victims paralyzed.";
                case PLAGUE_SYMPTOM.Vomiting:
                    return $"The Plague may sometimes trigger vomiting. This symptom produces {UtilityScripts.Utilities.PlagueIcon()}Plague Points.";
                case PLAGUE_SYMPTOM.Lethargy:
                    return "Plagued victims will always becomes Lethargic after waking up or sitting down.";
                case PLAGUE_SYMPTOM.Seizure:
                    return $"The Plague may sometimes trigger seizures. This symptom produces \n{UtilityScripts.Utilities.PlagueIcon()}Plague Points.";
                case PLAGUE_SYMPTOM.Insomnia:
                    return "Plagued victims have insomnia which sometimes prevent them from having restful sleep.";
                case PLAGUE_SYMPTOM.Poison_Cloud:
                    return $"The Plague may sometimes trigger its victim to produce small Poison Clouds while walking. Also applies to Objects. This symptom produces \n{UtilityScripts.Utilities.PlagueIcon()}Plague Points.";
                case PLAGUE_SYMPTOM.Monster_Scent:
                    return "The Plague gives Sapient victims an alluring scent that may attract \nmonster attacks.";
                case PLAGUE_SYMPTOM.Sneezing:
                    return $"The Plague may sometimes trigger sneezing. Sneezing may trigger airborne transmission. This symptom produces {UtilityScripts.Utilities.PlagueIcon()}Plague Points.";
                case PLAGUE_SYMPTOM.Depression:
                    return "Plagued victims have depression which sometimes prevent them from doing entertaining activities.";
                case PLAGUE_SYMPTOM.Hunger_Pangs:
                    return "Plagued victims may sometimes lose an amount of their Fullness Meter while they are moving.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(p_symptom), p_symptom, null);
            }
        }
    }
}