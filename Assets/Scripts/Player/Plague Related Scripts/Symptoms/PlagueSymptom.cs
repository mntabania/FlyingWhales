using System;
using Traits;
namespace Plague.Symptom {
    public abstract class PlagueSymptom : Plagued.IPlaguedListener {

        public abstract PLAGUE_SYMPTOM symptomType { get; }
        
        protected abstract void ActivateSymptom(Character p_character);
        
        #region Plagued.IPlaguedListener
        public virtual void PerTickMovement(Character p_character) { }
        public virtual void CharacterGainedTrait(Character p_character, Trait p_gainedTrait) { }
        public virtual void CharacterStartedPerformingAction(Character p_character) { }
        public virtual void HourStarted(Character p_character, int p_numOfHoursPassed) { }
        #endregion
    }

    public static class PlagueSymptomExtensions{
        public static int GetFatalityCost(this PLAGUE_SYMPTOM p_symptom) {
            switch (p_symptom) {
                case PLAGUE_SYMPTOM.Paralysis:
                    return 30;
                case PLAGUE_SYMPTOM.Vomiting:
                    return 10;
                case PLAGUE_SYMPTOM.Lethargy:
                    return 15;
                case PLAGUE_SYMPTOM.Seizure:
                    return 25;
                case PLAGUE_SYMPTOM.Insomnia:
                    return 20;
                case PLAGUE_SYMPTOM.Poison_Cloud:
                    return 25;
                case PLAGUE_SYMPTOM.Monster_Scent:
                    return 30;
                case PLAGUE_SYMPTOM.Sneezing:
                    return 10;
                case PLAGUE_SYMPTOM.Depression:
                    return 30;
                case PLAGUE_SYMPTOM.Hunger_Pangs:
                    return 20;
                default:
                    throw new ArgumentOutOfRangeException(nameof(p_symptom), p_symptom, null);
            }
        }
    }
}