using Traits;
namespace Plague.Fatality {
    public abstract class Fatality : Plagued.IPlaguedListener {

        public abstract FATALITY fatalityType { get; }
        
        public abstract void StartListeningForTrigger();
        protected abstract void ActivateFatality(Character p_character);
        
        #region Plagued.IPlaguedListener Implementation
        public void PerTickMovement(Character character) {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}