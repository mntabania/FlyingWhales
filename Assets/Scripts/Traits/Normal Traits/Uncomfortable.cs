namespace Traits {
    public class Uncomfortable : Status {
        
        public Uncomfortable() {
            name = "Uncomfortable";
            description = "Visited a dirty place.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            isStacking = true;
            stackLimit = 1;
            stackModifier = 0.5f;
            moodEffect = -8;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
        }
    }
}