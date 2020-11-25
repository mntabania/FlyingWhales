namespace Plague.Transmission {
    public class CombatRateTransmission : Transmission<CombatRateTransmission> {
        public override PLAGUE_TRANSMISSION transmissionType => PLAGUE_TRANSMISSION.Combat;
        protected override int GetTransmissionRate(int level) {
            switch (level) {
                case 0:
                    return 0;
                case 1:
                    return 10;
                case 2:
                    return 20;
                case 3:
                    return 30;
                default:
                    return 0;
            }
        }
        public override int GetTransmissionNextLevelCost(int p_currentLevel) {
            switch (p_currentLevel) {
                case 0:
                    return 10;
                case 1:
                    return 20;
                case 2:
                    return 30;
                default:
                    return -1; //Already at max level
            }
        }
        public override void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl) {
            TryTransmitToSingleTarget(p_infector, p_target, p_transmissionLvl);
        }
    }
}