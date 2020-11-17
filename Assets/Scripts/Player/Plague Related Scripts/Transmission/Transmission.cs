using System;
using UtilityScripts;
namespace Plague.Transmission {
    public abstract class Transmission<T> where T : Transmission<T>, new() {
        private static readonly Lazy<T> Lazy = new Lazy<T>(() => Activator.CreateInstance(typeof(T), true) as T);
        public static T Instance => Lazy.Value;

        public abstract PLAGUE_TRANSMISSION transmissionType { get; }
        
        protected abstract int GetTransmissionRate(int level);
        public abstract int GetTransmissionNextLevelCost(int p_currentLevel); 
        public abstract void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl);
        protected void TryTransmitToSingleTarget(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl) {
            int chance = GetTransmissionRate(p_transmissionLvl);
            if (GameUtilities.RollChance(chance)) {
                Infect(p_infector, p_target);
            }
        }
        protected void TryTransmitToInRange(IPointOfInterest p_infector, int p_transmissionLvl) {
            int chance = GetTransmissionRate(p_transmissionLvl);
            if (p_infector is Character infector && infector.marker != null) {
                for (int i = 0; i < infector.marker.inVisionCharacters.Count; i++) {
                    Character inVisionCharacter = infector.marker.inVisionCharacters[i];
                    if (GameUtilities.RollChance(chance)) {
                        Infect(p_infector, inVisionCharacter);
                    }
                }    
            }
        }
        private void Infect(IPointOfInterest p_infector, IPointOfInterest p_target) {
            if (p_target is Character character) {
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Plagued, character);
            } else {
                p_target.traitContainer.AddTrait(p_target, "Plagued", p_infector as Character, overrideDuration: PlagueDisease.Instance.lifespan.GetLifespanInTicksOfPlagueOn(p_target));
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Plagued", "contract");
                log.AddToFillers(p_target, p_target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase();
            }
            
        }
    }
}