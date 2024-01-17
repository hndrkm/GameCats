using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace CatGame
{
    public class NicknameMenu : MonoBehaviour
    {
        [SerializeField]
        MenuUI _menuService;
        [SerializeField]
        TextMeshProUGUI _txtMsj;
        [SerializeField]
        TMP_InputField _inputNick;
        [SerializeField]
        Button _btnAcept;
        [SerializeField]
        int _minCharacters=3;
        private string nick;
        // Start is called before the first frame update
        void Start()
        {
            _inputNick.onValueChanged.AddListener(EditNickname);
            _btnAcept.onClick.AddListener(ChangeNickname);
        }
        private void EditNickname(string text) 
        {
            if (string.IsNullOrEmpty(text)) 
            {
                _txtMsj.text = "nickname vacio";
                return;
            }
            if (text.Length <= _minCharacters) 
            {
                _txtMsj.text = "nececita mas carateres";
                return;
            }
            nick= text;
        }
        private void ChangeNickname() 
        {
            if (string.IsNullOrEmpty(nick))
            {
                _txtMsj.text = "nickname vacio no se puede guardar";
                return;
            }
            if (nick.Length <= _minCharacters)
            {
                _txtMsj.text = "nececita mas carateres no se puede guardar";
                return;
            }
            _menuService.Context.PlayerData.Nickname = nick;
        }
    }
}
