using UtilityScripts;
namespace Plague.Transmission {
    public class ConsumptionTransmission : Transmission<ConsumptionTransmission> {
        protected override int GetTransmissionRate(int level) {
            switch (level) {
                case 1:
                    return 20;
                case 2:
                    return 20;
                case 3:
                    return 35;
                case 4:
                    return 50;
                default:
                    return 0;
            }
        }
        public override void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl) {
            TryTransmitToSingleTarget(p_infector, p_target, p_transmissionLvl);
        }
    }
}