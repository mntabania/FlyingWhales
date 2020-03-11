using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkedMeter : MonoBehaviour {

	[SerializeField] private Image meterBG;
	[SerializeField] private Image meterFill;
	[SerializeField] private GameObject meterMarkPrefab;
	[SerializeField] private Transform meterMarksParent;
	
	public void ResetMarks() {
		UtilityScripts.Utilities.DestroyChildren(meterMarksParent);
	}

	public void AddMark(float percent, Color color) {
		GameObject markGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(meterMarkPrefab.name, Vector3.zero,
			Quaternion.identity, meterMarksParent);

		Vector3 newPos = markGO.transform.localPosition;
		newPos.x = meterBG.rectTransform.sizeDelta.x * percent;
		markGO.transform.localPosition = newPos;

		MeterMark mark = markGO.GetComponent<MeterMark>();
		mark.SetColor(color);
	}
	public void SetFillAmount(float amount) {
		meterFill.fillAmount = amount;
	}
}
