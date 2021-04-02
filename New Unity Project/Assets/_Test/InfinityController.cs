using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameWish.Game
{
	public class InfinityController : MonoBehaviour
	{
		private ScrollRect m_ScrollRect;
		public ScrollRect ScrollRect
        {
			get {
				if (m_ScrollRect == null) {
					m_ScrollRect = GetComponent<ScrollRect>();
				}
				return m_ScrollRect;
			}
		}
		private Rect m_ScrollViewWorldRect;
		[SerializeField]
		private RectTransform m_Content;
		[SerializeField]
		private RectTransform m_ViewPortRT;
		/// <summary>
		/// 移动方向
		/// </summary>
		public MoveDir MoveDir = MoveDir.Horizontal;
		/// <summary>
		/// 间隔，水平方向：每一列间隔，竖直方向：每一行间隔
		/// </summary>
		public float HorizontalIntervalDis = 0;
		public float VerticalIntervalDis = 0;
		public RectOffset Padding;
		/// <summary>
		/// 水平竖直行列个数
		/// </summary>
		public int HorizontalCount = 1;
		public int VerticalCount = 1;

		public RectTransform m_Prefab;

		public List<InfinityData> m_Datas = new List<InfinityData>();
		public List<InfinityItem> m_AllItems = new List<InfinityItem>();
		public Queue<InfinityItem> m_FreeItems = new Queue<InfinityItem>();

		private Rect m_ItemRect;

        private int m_SpawnCount;

		private RectTransform m_TempRect;

        private void Awake()
        {
			Init();
		}

        public void Init() 
		{
			Transform go = new GameObject("TempObj").transform;
			go.SetParent(m_Content);
			m_TempRect = go.gameObject.AddComponent<RectTransform>();
			m_TempRect.sizeDelta = new Vector2(m_Prefab.rect.width, m_Prefab.rect.height);
			m_TempRect.anchorMin = new Vector2(0, 1);
			m_TempRect.anchorMax = new Vector2(0, 1);

			m_ItemRect = m_Prefab.GetComponent<RectTransform>().rect;

			m_ScrollViewWorldRect = ScrollRect.GetComponent<RectTransform>().WorldRect();

			if (MoveDir == MoveDir.Vertical)
			{
				m_SpawnCount = (HorizontalCount + 1) * VerticalCount;

				ScrollRect.vertical = true;
				ScrollRect.horizontal = false;
			}
			else {
				m_SpawnCount = HorizontalCount * (VerticalCount + 1);

				ScrollRect.vertical = false;
				ScrollRect.horizontal = true;
			}			

			InitData();

			ScrollRect.onValueChanged.AddListener(OnScrollRectValueChangedCallBack);
		}

        private void InitData()
        {
            for (int i = 0; i < 500; i++)
            {
				InfinityData data = new InfinityData();

				m_TempRect.anchoredPosition = GetLocalPosByIndex(i);

				data.index = i;

				data.localPos = m_TempRect.anchoredPosition;

                m_Datas.Add(data);
			}

			for (int i = 0; i < m_SpawnCount; i++)
			{
				InfinityItem item = GetFreeItem(m_Datas[i]);

				item.SelfRT.anchoredPosition = GetLocalPosByIndex(i);

				m_Datas[i].Item = item;
			}

			UpdateContentSize();
        }
		private Vector2 GetLocalPosByIndex(int index) 
		{
			int row = 0;
			int col = 0;
			float x = 0.0f;
			float y = 0.0f;
			if (MoveDir == MoveDir.Vertical)
			{
				 row = index / VerticalCount;
				 col = index % VerticalCount;
			}
			else 
			{
				 col = index / HorizontalCount;
				 row = index % HorizontalCount;
			}

			y = m_ItemRect.height * (row + 0.5f) + row * VerticalIntervalDis + Padding.top;
			x = m_ItemRect.width * (col + 0.5f) + col * HorizontalIntervalDis + Padding.left;

			return new Vector2(x, -y);
		}
		private void UpdateContentSize()
		{
			int totalCount = m_Datas.Count;

			if (MoveDir == MoveDir.Vertical)
			{
				int totalRow = totalCount / VerticalCount;

				if (totalCount % VerticalCount != 0)
				{
					totalRow++;
				}

				float height = totalRow * m_ItemRect.height;
				height += (totalRow - 1) * VerticalIntervalDis;
				height += Padding.top;
				height += Padding.bottom;

				m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x, height);
			}
			else 
			{
				int totalCol = totalCount / HorizontalCount;
				if (totalCount % HorizontalCount != 0) {
					totalCol++;
				}
				float width = totalCol * m_ItemRect.width;
				width += (totalCol - 1) * HorizontalIntervalDis;
				width += Padding.left;
				width += Padding.right;

				m_Content.sizeDelta = new Vector2(width, m_Content.sizeDelta.y);
			}			
		}
		private InfinityItem GetFreeItem(InfinityData data) 
		{
			if (m_FreeItems.Count > 0) 
			{
				InfinityItem _item = m_FreeItems.Dequeue();
				_item.gameObject.SetActive(true);
				_item.OnShow(data);
				return _item;
			}

			InfinityItem item = Instantiate(m_Prefab,m_Content).GetComponent<InfinityItem>();
			item.OnShow(data);
			m_AllItems.Add(item);
			return item;
		}
		private void RecycleItem(InfinityItem item) 
		{
			item.OnHide();
			item.gameObject.SetActive(false);
			m_FreeItems.Enqueue(item);
		}
        private void OnScrollRectValueChangedCallBack(Vector2 arg0)
        {
            for (int i = 0; i < m_Datas.Count; i++)
            {
                InfinityData data = m_Datas[i];

				if (data.Item != null) 
				{
					m_TempRect.anchoredPosition = data.localPos;
					if (m_TempRect.WorldRect().Overlaps(m_ScrollViewWorldRect) == false)
					{
						RecycleItem(data.Item);
						data.Item = null;						
					}
				}				
            }

            for (int i = 0; i < m_Datas.Count; i++)
            {
				InfinityData data = m_Datas[i];
				if (data.Item == null) 
				{
					m_TempRect.anchoredPosition = data.localPos;
					if (m_TempRect.WorldRect().Overlaps(m_ScrollViewWorldRect))
					{					
						InfinityItem item = GetFreeItem(data);
						item.SelfRT.anchoredPosition = GetLocalPosByIndex(i);
						data.Item = item;						
					}
				}				
			}
        }
	}
	[System.Serializable]
	public class InfinityData 
	{
		public int index;
		public Vector3 localPos;
		public InfinityItem Item;
	}

	public enum MoveDir 
	{ 
		Horizontal,
		Vertical,
	}

	public static class RectTransformExtensions
	{
		public static bool Overlaps(this RectTransform a, RectTransform b)
		{
			return a.WorldRect().Overlaps(b.WorldRect());
		}
		public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse)
		{
			return a.WorldRect().Overlaps(b.WorldRect(), allowInverse);
		}

		public static Rect WorldRect(this RectTransform rectTransform)
		{
			Vector2 sizeDelta = rectTransform.sizeDelta;
			float rectTransformWidth = sizeDelta.x * rectTransform.lossyScale.x;
			float rectTransformHeight = sizeDelta.y * rectTransform.lossyScale.y;

			Vector3 position = rectTransform.position;

			return new Rect(
				position.x - rectTransformWidth * rectTransform.pivot.x,
				position.y - rectTransformHeight * rectTransform.pivot.y,
				rectTransformWidth,
				rectTransformHeight);
		}
	}
}