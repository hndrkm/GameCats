using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace CatGame.UI
{
    public class UISessionInfo : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _content;
        [SerializeField]
        private Button _selectBtn;
        public event Action<int> OnBtnSelect;
        public int Index;
        public void Awake()
        {
            _selectBtn.onClick.AddListener(OnSelectBtn);
        }
        public void OnSelectBtn() 
        {
            OnBtnSelect(Index);
        }
        public void SetData(string info, int index) 
        {
            _content.text = info;
            Index = index;
        }
    }
}
