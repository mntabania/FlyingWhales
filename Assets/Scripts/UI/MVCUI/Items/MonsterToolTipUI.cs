using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterToolTipUI : MonoBehaviour {
	public GameObject toolTipParent;
	public GameObject tooltipWithChargeParent;
	public GameObject tooltipWithoutChargeParent;
	public RuinarchText txtToolTipDisplayName;
	public RuinarchText txtToolTipDisplayDescription;
	public RuinarchText txtToolTipChargingTime;

	public void DisplayToolTipWithCharge(string p_name, string p_description, string p_chargeDisplay) {
		toolTipParent.SetActive(true);
		tooltipWithoutChargeParent.SetActive(false);
		tooltipWithChargeParent.SetActive(true);
		txtToolTipDisplayName.text = p_name;
		txtToolTipDisplayDescription.text = p_description;
		txtToolTipChargingTime.text = p_chargeDisplay;
	}

	public void DisplayToolTipWithoutCharge(string p_name, string p_description) {
		toolTipParent.SetActive(true);
		tooltipWithoutChargeParent.SetActive(true);
		tooltipWithChargeParent.SetActive(false);
		txtToolTipDisplayName.text = p_name;
		txtToolTipDisplayDescription.text = p_description;
	}

	public void HideToolTip() {
		toolTipParent.SetActive(false);
		tooltipWithoutChargeParent.SetActive(false);
		tooltipWithChargeParent.SetActive(false);
	}
}