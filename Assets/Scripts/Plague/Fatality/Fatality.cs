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
}