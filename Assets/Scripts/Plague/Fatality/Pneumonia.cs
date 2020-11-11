using Traits;
namespace Plague.Fatality {
    public class Pneumonia : Fatality {
        
        public override FATALITY fatalityType => FATALITY.Pneumonia;
        
        public override void StartListeningForTrigger() {
            throw new System.NotImplementedException();
        }
        protected override void ActivateFatality(Character p_character) {
            throw new System.NotImplementedException();
        }
    }
}