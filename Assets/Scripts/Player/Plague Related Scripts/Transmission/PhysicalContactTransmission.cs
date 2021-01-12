using System;
using UtilityScripts;
namespace Plague.Transmission {
    public class PhysicalContactTransmission : Transmission<PhysicalContactTransmission> {
        public override PLAGUE_TRANSMISSION transmissionType => PLAGUE_TRANSMISSION.Physical_Contact;
        protected override int GetTransmissionRate(int level) {
            switch (level) {
                case 0:
                    return 0;
                case 1:
                    return 12;
                case 2:
                    return 24;
                case 3:
                    return 36;
                default:
                    return 0;
            }
        }
        protected override int GetTransmissionNextLevelCost(int p_currentLevel) {
            switch (p_currentLevel) {
                case 0:
                    return 20;
                case 1:
                    return 40;
                case 2:
                    return 60;
                default:
                    return -1; //Already at max level
            }
        }
        public override void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl) {
            TryTransmitToSingleTarget(p_infector, p_target, p_transmissionLvl);
        }
    }
}