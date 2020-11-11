using System;
using UtilityScripts;
namespace Plague.Transmission {
    public class PhysicalContactTransmission : Transmission<PhysicalContactTransmission> {
        protected override int GetTransmissionRate(int level) {
            switch (level) {
                case 1:
                    return 0;
                case 2:
                    return 5;
                case 3:
                    return 10;
                case 4:
                    return 20;
                default:
                    return 0;
            }
        }
        public override void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl) {
            TryTransmitToSingleTarget(p_infector, p_target, p_transmissionLvl);
        }
    }
}