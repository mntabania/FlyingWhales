using System;
using Traits;
namespace Plague.Death_Effect {
    public abstract class PlagueDeathEffect : Plagued.IPlagueDeathListener {
        public abstract PLAGUE_DEATH_EFFECT deathEffectType { get; }
        protected int _level;

        protected abstract void ActivateEffect(Character p_character);

        public PlagueDeathEffect() {
            _level = 1;
        }

        #region Plagued.IPlagueDeathListener
        public virtual void OnDeath(Character p_character) { }
        #endregion

        public void AdjustLevel(int amount) {
            _level += amount;
        }
    }
}