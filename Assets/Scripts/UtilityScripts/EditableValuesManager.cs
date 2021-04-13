﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public int GetChaosOrbHoverAmount() {
		return UnityEngine.Random.Range(currencyHoverData.minAmountHover, currencyHoverData.maxAmountHover);
	}

	public int GetInitialMaxChaoticEnergy() { 
		return currencyHoverData.maxChaoticPerValues[0];
	}

	public int GetMaxChaoticEnergyPerPortalLevel(int p_portalLevel) {
		int index = p_portalLevel - 1;
		if (currencyHoverData.maxChaoticPerValues.IsIndexInList(index)) {
			return currencyHoverData.maxChaoticPerValues[p_portalLevel - 1];	
		}
		return -1; //no data was provided for current level
	}
}
