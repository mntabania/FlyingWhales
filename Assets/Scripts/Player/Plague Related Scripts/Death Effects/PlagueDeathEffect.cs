using System;
using Traits;
namespace Plague.Death_Effect {
    public abstract class PlagueDeathEffect : Plagued.IPlagueDeathListener {
        public abstract PLAGUE_DEATH_EFFECT deathEffectType { get; }
        protected int _level;

        protected abstract void ActivateEffect(Character p_character);
        public abstract int GetNextLevelUpgradeCost();
        public abstract string GetCurrentEffectDescription();
        
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

    public static class PlagueDeathEffectExtensions {
        public static int GetUnlockCost(this PLAGUE_DEATH_EFFECT p_deathEffect) {
            switch (p_deathEffect) {
                case PLAGUE_DEATH_EFFECT.Explosion:
                    return 5;
                case PLAGUE_DEATH_EFFECT.Zombie:
                    return 10;
                case PLAGUE_DEATH_EFFECT.Mana_Generator:
                    return 5;
                case PLAGUE_DEATH_EFFECT.Haunted_Spirits:
                    return 10;
                default:
                    throw new ArgumentOutOfRangeException(nameof(p_deathEffect), p_deathEffect, null);
            }
        }
    }
}