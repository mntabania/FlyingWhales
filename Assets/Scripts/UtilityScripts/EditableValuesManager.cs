using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class EditableValuesManager : MonoBehaviour {

	public static EditableValuesManager Instance;

	[Header("Character Values")]
	[Header("Mood")]
	[SerializeField] private int _normalMoodMinThreshold;
	[SerializeField] private int _normalMoodHighThreshold;
	[SerializeField] private int _lowMoodMinThreshold;
	[SerializeField] private int _lowMoodHighThreshold;
	[SerializeField] private int _criticalMoodMinThreshold;
	[SerializeField] private int _criticalMoodHighThreshold;
	[Tooltip("Number hours a character needs to be in a critical mood to have a 100% chance to trigger a major mental break.")]
	[SerializeField] private int _majorMentalBreakHourThreshold;
	[Tooltip("Number days a character needs to be in a low mood to have a 100% chance to trigger a minor mental break.")]
	[SerializeField] private int _minorMentalBreakDayThreshold;

	[Header("Needs")]
	[SerializeField] private float _baseFullnessDecreaseRate;
	[SerializeField] private float _baseTirednessDecreaseRate;
	[SerializeField] private float _baseHappinessDecreaseRate;
	//[SerializeField] private float _baseStaminaDecreaseRate;

	[Header("Mana")]
	[SerializeField] private int _startingMana;
	[SerializeField] private int _maximumMana;
	[SerializeField] private int _summonMinionManaCost;
	[SerializeField] private int _corruptTileManaCost;
	[SerializeField] private int _triggerFlawManaCost;
	[SerializeField] private int _buildStructureManaCost;

	[Header("Chaos Orb from raid")]
	[SerializeField] private int _chaosOrbExpulsionThresholdFromRaid;

	[Header("Chaos Orb")]
	[SerializeField] private int _chaosOrbExpulsionThreshold;

	[Header("Visuals")]
	[SerializeField] private int _sortingOrdersInBetweenHexTileRows = 20; //this is the number of sorting orders in between rows of the world map.

	[Header("Currency Hover Values")]
	[SerializeField] private CurrencyHoverData currencyHoverData;

	[Header("Chaotic Energy")]
	[SerializeField] private int _initialChaoticEnergyCustom;
	[SerializeField] private int _initialChaoticEnergyScenario;

	[Header("Reveal Info Character")]
	[SerializeField] private int _revealInfoCharacterCost;

	[Header("Reveal Info Faction")]
	[SerializeField] private int _revealInfoFactionCost;

	[Header("Mana regen per hour")]
	[SerializeField] private int _manaRegenPerHr;

	[Header("Mana regen gain per manapit")]
	[SerializeField] private int _manaRegenPerManapit;

	[Header("Additional Max Mana per pit")]
	[SerializeField] private int _additionalMaxManaPerPit;
	[Space]

	[Header("Win target portal level")]
	[SerializeField] private int _targetPortalLevel;
	[Space]

	[Header("Beholder Costs")]
	[SerializeField] private List<Cost> m_eyeUpgradeCostPerLevel;
	[SerializeField] private List<Cost> m_radiusUpgradeCostPerLevel;
	[Space]

	[Header("Elven Power Crystal Bonus")]
	[SerializeField] private CharacterProgressionBonusData m_elvenBonusForPowerCrystal;
	[Space]

	public int vaporStacks;
	public int poisonCloudStacks;
	public int frostyFogStacks;

	//getters
	//mood
	public int normalMoodMinThreshold => _normalMoodMinThreshold;
	public int lowMoodMinThreshold => _lowMoodMinThreshold;
	public int lowMoodHighThreshold => _lowMoodHighThreshold;
	public int criticalMoodHighThreshold => _criticalMoodHighThreshold;
	public int majorMentalBreakHourThreshold => _majorMentalBreakHourThreshold;
	public int minorMentalBreakDayThreshold => _minorMentalBreakDayThreshold;
	public float baseFullnessDecreaseRate => _baseFullnessDecreaseRate;
	public float baseTirednessDecreaseRate => _baseTirednessDecreaseRate;
	public float baseHappinessDecreaseRate => _baseHappinessDecreaseRate;
	//public float baseStaminaDecreaseRate => _baseStaminaDecreaseRate;

	//mana
	public int summonMinionManaCost => _summonMinionManaCost;
	public int maximumMana {
		get { return _maximumMana; }
		set { _maximumMana = value; }
	}
	public int startingMana => _startingMana;
	public int corruptTileManaCost => _corruptTileManaCost;
	public int triggerFlawManaCost => _triggerFlawManaCost;
	public int buildStructureManaCost => _buildStructureManaCost;
	//visuals
	public int sortingOrdersInBetweenHexTileRows => _sortingOrdersInBetweenHexTileRows;
	//chaos orb
	public int chaosOrbExpulsionThreshold => _chaosOrbExpulsionThreshold;

	//chaos orb from raid
	public int chaosOrbExpulsionThresholdFromRaid => _chaosOrbExpulsionThresholdFromRaid;

	private void Awake() {
		Instance = this;
	}

	public int GetRevealCharacterInfoCost() {
		return SpellUtilities.GetModifiedSpellCost(_revealInfoCharacterCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
	}

	public int GetRevealFactionInfoCost() {
		return SpellUtilities.GetModifiedSpellCost(_revealInfoFactionCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
	}

	public int GetChaosOrbHoverAmount() {
		return GameUtilities.RandomBetweenTwoNumbers(currencyHoverData.minAmountHover, currencyHoverData.maxAmountHover);
	}

	public int GetInitialMaxChaoticEnergy() {
		return currencyHoverData.maxChaoticPerValues[0];
	}

	public int GetInitialChaoticEnergyCustom() {
		return _initialChaoticEnergyCustom;
	}

	public int GetInitialChaoticEnergyScenario() {
		return _initialChaoticEnergyScenario;
	}

	public int GetInitialChaoticEnergyBaseOnGameMode() {
		if (WorldSettings.Instance.worldSettingsData.IsScenarioMap()) {
			return GetInitialChaoticEnergyScenario();
		}
		return GetInitialChaoticEnergyCustom();
	}

	public int GetManaRegenPerHour() {
		return _manaRegenPerHr;
	}

	public int GetManaRegenPerManaPit() {
		return _manaRegenPerManapit;
	}

	public int GetAdditionalMaxManaPerManaPit() {
		return _additionalMaxManaPerPit;
	}

	public int GetTargetPortalLevel() {
		return _targetPortalLevel;
	}

	public int GetMaxChaoticEnergyPerPortalLevel(int p_portalLevel) {
		int index = p_portalLevel - 1;
		if (currencyHoverData.maxChaoticPerValues.IsIndexInList(index)) {
			return currencyHoverData.maxChaoticPerValues[p_portalLevel - 1];
		}
		return -1; //no data was provided for current level
	}
	public Cost GetReleaseAbilitiesRerollCost() {
		return currencyHoverData.releaseAbilitiesRerollCost;
	}
	public Cost GetCorruptTileCost() {
		return currencyHoverData.corruptFloorCost;
	}
	public Cost GetBuildWallCost() {
		return currencyHoverData.buildWallCost;
	}

	public Cost GetBeholderEyeUpgradeCostPerLevel(int level) {
		return m_eyeUpgradeCostPerLevel[level];
	}

	public Cost GetBeholderRadiusUpgradeCostPerLevel(int level) {
		return m_radiusUpgradeCostPerLevel[level];
	}

	public CharacterProgressionBonusData GetElvenProgressionBonusData() {
		return m_elvenBonusForPowerCrystal;
	}
}
