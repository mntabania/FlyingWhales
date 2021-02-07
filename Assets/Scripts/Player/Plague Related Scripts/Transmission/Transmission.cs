using System;
using UnityEngine;
using UtilityScripts;
namespace Plague.Transmission {

    public interface IPlagueTransmissionListener {
        void OnPlagueTransmitted(IPointOfInterest p_target);
    }
    
    public abstract class Transmission<T> where T : Transmission<T>, new() {
        private static readonly Lazy<T> Lazy = new Lazy<T>(() => Activator.CreateInstance(typeof(T), true) as T);
        public static T Instance => Lazy.Value;

        private Action<IPointOfInterest> _plagueTransmitted;
        
        public abstract PLAGUE_TRANSMISSION transmissionType { get; }
        
        protected abstract int GetTransmissionRate(int level);
        protected abstract int GetTransmissionNextLevelCost(int p_currentLevel);
        public int GetFinalTransmissionNextLevelCost(int p_currentLevel) {
            int baseCost = GetTransmissionNextLevelCost(p_currentLevel);
            return SpellUtilities.GetModifiedSpellCost(baseCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
        }
        public abstract void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl);
        protected void TryTransmitToSingleTarget(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl) {
            int chance = GetTransmissionRate(p_transmissionLvl);
            chance = AdjustTransmissionChancesBasedOnInfector(p_infector, chance);
            if (GameUtilities.RollChance(chance)) {
                Infect(p_infector, p_target);
            }
        }
        protected void TryTransmitToInRange(IPointOfInterest p_infector, int p_transmissionLvl) {
            int chance = GetTransmissionRate(p_transmissionLvl);
            chance = AdjustTransmissionChancesBasedOnInfector(p_infector, chance);
            if (p_infector is Character infector && infector.hasMarker) {
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
                if (PlagueDisease.Instance.AddPlaguedStatusOnPOIWithLifespanDuration(p_target)) {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Plagued", "contract");
                    log.AddToFillers(p_target, p_target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddLogToDatabase(true);
                }
            }
            _plagueTransmitted?.Invoke(p_target);   
        }

        private int AdjustTransmissionChancesBasedOnInfector(IPointOfInterest p_infector, int p_baseChance) {
            int adjustedChance = p_baseChance;
            if (p_infector.traitContainer.HasTrait("Quarantined")) {
                adjustedChance -= Mathf.FloorToInt(adjustedChance * 0.75f);
            }
            return adjustedChance;
        }
        
        #region Listeners
        public void SubscribeToTransmission(IPlagueTransmissionListener p_PlagueTransmissionListener) {
            _plagueTransmitted += p_PlagueTransmissionListener.OnPlagueTransmitted;
        }
        public void UnsubscribeToTransmission(IPlagueTransmissionListener p_PlagueTransmissionListener) {
            _plagueTransmitted -= p_PlagueTransmissionListener.OnPlagueTransmitted;
        }
        #endregion
    }

    public static class TransmissionExtensions {
        public static string GetTransmissionTooltip(this PLAGUE_TRANSMISSION p_transmissionType) {
            switch (p_transmissionType) {
                case PLAGUE_TRANSMISSION.Airborne:
                    return "Transmission via aerosols typically produced when a Villager talks, sings or sneezes.";
                case PLAGUE_TRANSMISSION.Consumption:
                    return "Transmission from food sources consumed by a Villager.";
                case PLAGUE_TRANSMISSION.Physical_Contact:
                    return "Transmission when one Villager physically interacts with a Plagued Villager or object. Excludes combat.";
                case PLAGUE_TRANSMISSION.Combat:
                    return "Transmission when a Plagued Villager attacks another. Includes ranged and magical attacks.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(p_transmissionType), p_transmissionType, null);
            }
        }
    }
}