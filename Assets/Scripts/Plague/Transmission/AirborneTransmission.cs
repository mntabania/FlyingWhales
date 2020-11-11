using UtilityScripts;
namespace Plague.Transmission {
    public class AirborneTransmission : Transmission<AirborneTransmission> {
        protected override int GetTransmissionRate(int level) {
            switch (level) {
                case 1:
                    return 0;
                case 2:
                    return 2;
                case 3:
                    return 5;
                case 4:
                    return 10;
                default:
                    return 0;
            }
        }
        public override void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl) {
            TryTransmitToInRange(p_infector, p_transmissionLvl);
        }
    }
}