using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using UnityEngine;
using UnityEngine.UI;

public class MeterMark : PooledObject {

	[SerializeField] private Image mark;

	public void SetColor(Color color) {
		mark.color = color;
	}
}
