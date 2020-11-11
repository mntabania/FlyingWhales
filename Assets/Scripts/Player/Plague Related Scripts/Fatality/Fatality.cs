using System;
using Traits;
namespace Plague.Fatality {
    public abstract class Fatality : Plagued.IPlaguedListener {

        public abstract FATALITY fatalityType { get; }
        
        protected abstract void ActivateFatality(Character p_character);
        
        #region Plagued.IPlaguedListener Implementation
        public virtual void PerTickMovement(Character p_character) { }
        public virtual void CharacterGainedTrait(Character p_character, Trait p_gainedTrait) { }
        public virtual void CharacterStartedPerformingAction(Character p_character) { }
        #endregion
    }
    
    public static class FatalityExtensions{
        public static int GetFatalityCost(this FATALITY fatality) {
            switch (fatality) {
                case FATALITY.Septic_Shock:
                    return 30;
                case FATALITY.Heart_Attack:
                    return 30;
                case FATALITY.Stroke:
                    return 30;
                case FATALITY.Total_Organ_Failure:
                    return 30;
                case FATALITY.Pneumonia:
                    return 30;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fatality), fatality, null);
            }
        }
    }
}