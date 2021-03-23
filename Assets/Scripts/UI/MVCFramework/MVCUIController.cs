using UnityEngine;
using System;

namespace Ruinarch.MVCFramework
{
	public abstract class MVCUIController : MonoBehaviour
	{
		public Action onUIINstantiated;

		protected MVCUIView m_mvcUIView;
		protected MVCUIModel m_mvcUIModel;

		[SerializeField]
		protected Canvas _canvas;

		[SerializeField] protected int siblingIndex = -1;

		public virtual void InstantiateUI()
		{

		}

		public void InitUI(MVCUIModel p_newModel, MVCUIView p_newView) {
			m_mvcUIView = p_newView;
			m_mvcUIModel = p_newModel;
			m_mvcUIView.transform.SetParent(GameObject.FindObjectOfType<Camera>().transform);
			onUIINstantiated?.Invoke();
		}

		public virtual void ShowUI() 
		{
			m_mvcUIView.ShowUI();
			if (siblingIndex == -1) {
				m_mvcUIModel.transform.SetAsLastSibling();	
			} else {
				m_mvcUIModel.transform.SetSiblingIndex(siblingIndex);
			}
		}

		public virtual void HideUI()
		{
			m_mvcUIView.HideUI();
		}

		public virtual void SetParent(Transform p_newParent)
		{
			m_mvcUIModel.parentDisplay.transform.parent.SetParent(p_newParent, false);
		}	
	}
}