using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameWish.Game
{
    public class GridItem : InfinityItem
    {
        public Text m_Text;

        public override void OnHide()
        {
            m_Text.gameObject.SetActive(false);
        }

        public override void OnShow(InfinityData data)
        {
            m_Text.text = data.index.ToString();
            m_Text.gameObject.SetActive(true);
        }
    }

}