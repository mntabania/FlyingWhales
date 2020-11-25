using UtilityScripts;
namespace Plague.Transmission {
    public class AirborneTransmission : Transmission<AirborneTransmission> {
        public override PLAGUE_TRANSMISSION transmissionType => PLAGUE_TRANSMISSION.Airborne;
        protected override int GetTransmissionRate(int level) {
            switch (level) {
                case 0:
                    return 0;
                case 1:
                    return 20;
                case 2:
                    return 35;
                case 3:
                    return 50;
                default:
                    return 0;
            }
        }
        public override int GetTransmissionNextLevelCost(int p_currentLevel) {
            switch (p_currentLevel) {
                case 0:
                    return 10;
                case 1:
                    return 30;
                case 2:
                    return 60;
                default:
                    return -1; //Already at max level
            }
        }
        public override void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl) {
            TryTransmitToInRange(p_infector, p_transmissionLvl);
        }
    }
}