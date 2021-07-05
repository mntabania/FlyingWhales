using System;
using Traits;
using UtilityScripts;
namespace Plague.Death_Effect {
    public abstract class PlagueDeathEffect : Plagued.IPlagueDeathListener {
        public abstract PLAGUE_DEATH_EFFECT deathEffectType { get; }
        protected int _level;

        protected abstract void ActivateEffect(Character p_character);
        protected abstract int GetNextLevelUpgradeCost();
        public int GetFinalNextLevelUpgradeCost() {
            int baseCost = GetNextLevelUpgradeCost();
            return SpellUtilities.GetModifiedSpellCost(baseCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
        }
        public abstract string GetCurrentEffectDescription();

        protected void ActivateEffectOn(Character p_character) {
            if (CanActivateEffectOn(p_character)) {
                ActivateEffect(p_character);
            }
        }


        #region getters
        public int level => _level;
        #endregion

        public PlagueDeathEffect() {
            _level = 1;
        }

        #region Plagued.IPlagueDeathListener
        public virtual void OnDeath(Character p_character) { }
        #endregion

        #region Virtuals
        protected virtual bool CanActivateEffectOn(Character p_character) {
            if (p_character.traitContainer.HasTrait("Plague Reservoir") || p_character.characterClass.IsZombie()) {
                return false;
            }
            return true;
        }
        #endregion

        public void AdjustLevel(int amount) {
            _level += amount;
        }
        public void SetLevel(int amount) {
            _level = amount;
        }
    }

    public static class PlagueDeathEffectExtensions {
        public static int GetUnlockCost(this PLAGUE_DEATH_EFFECT p_deathEffect) {
            switch (p_deathEffect) {
                case PLAGUE_DEATH_EFFECT.Explosion:
                    return SpellUtilities.GetModifiedSpellCost(25, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
                case PLAGUE_DEATH_EFFECT.Zombie:
                    return SpellUtilities.GetModifiedSpellCost(25, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
                case PLAGUE_DEATH_EFFECT.Chaos_Generator:
                    return SpellUtilities.GetModifiedSpellCost(25, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
                case PLAGUE_DEATH_EFFECT.Haunted_Spirits:
                    return SpellUtilities.GetModifiedSpellCost(25, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
                default:
                    throw new ArgumentOutOfRangeException(nameof(p_deathEffect), p_deathEffect, null);
            }
        }
        public static string GetEffectTooltip(this PLAGUE_DEATH_EFFECT p_deathEffect, int p_level) {
            switch (p_deathEffect) {
                case PLAGUE_DEATH_EFFECT.Explosion:
                    if (p_level == 1)
                        return "Trigger a Fire Blast around the corpse when it dies.";
                    else if (p_level == 2)
                        return "Trigger a Fire Blast and spawn a Fire Elemental around the corpse when it dies.";
                    else if (p_level == 3)
                        return "Cast a Meteor on the corpse when it dies.";
                    else
                        return "-";
                case PLAGUE_DEATH_EFFECT.Zombie:
                    if (p_level == 1)
                        return "Plagued corpses will eventually reanimate as a slow-moving zombie.";
                    else if (p_level == 2)
                        return "Plagued corpses will keep on reanimating as Night Zombies at dusk and then revert to lifelessness each dawn.";
                    else if (p_level == 3)
                        return "Plagued corpses will eventually reanimate into various different types of zombies.";
                    else
                        return "-";
                case PLAGUE_DEATH_EFFECT.Chaos_Generator:
                    if (p_level == 1 || p_level == 2 || p_level == 3)
                        return "Plagued victims produce Chaos Orbs when they die.";
                    else
                        return "-";
                case PLAGUE_DEATH_EFFECT.Haunted_Spirits:
                    if (p_level == 1 || p_level == 2 || p_level == 3)
                        return "Plagued victims spawn random Spirits when they die";
                    else
                        return "-";
                default:
                    throw new ArgumentOutOfRangeException(nameof(p_deathEffect), p_deathEffect, null);
            }
        }
        
    }
}