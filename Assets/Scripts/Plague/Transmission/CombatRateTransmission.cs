namespace Plague.Transmission {
    public class CombatRateTransmission : Transmission<CombatRateTransmission> {
        protected override int GetTransmissionRate(int level) {
            switch (level) {
                case 1:
                    return 0;
                case 2:
                    return 1;
                case 3:
                    return 3;
                case 4:
                    return 5;
                default:
                    return 0;
            }
        }
        public override void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl) {
            TryTransmitToSingleTarget(p_infector, p_target, p_transmissionLvl);
        }
    }
}