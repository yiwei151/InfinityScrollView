using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameWish.Game
{
	public abstract class InfinityItem : MonoBehaviour
	{
		private RectTransform m_SelfRT = null;
		public RectTransform SelfRT
        {
			get {
				if (m_SelfRT == null) {
					m_SelfRT = GetComponent<RectTransform>();
				}
				return m_SelfRT;
			}
		}
		public abstract void OnShow(InfinityData data);
		public abstract void OnHide();
    }	
}