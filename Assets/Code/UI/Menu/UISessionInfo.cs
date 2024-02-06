using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CatGame.UI
{
    public class UISessionInfo : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _content;
        public void SetData(string info) 
        {
            _content.text = info;
        }
    }
}
