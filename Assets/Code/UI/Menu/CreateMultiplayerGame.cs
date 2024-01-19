using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatGame
{
    public class CreateMultiplayerGame : MonoBehaviour
    {
        [SerializeField]
        private MenuUI _menuUI;
        [SerializeField]
        private MapSettings _mapSettings;
        [SerializeField]
        private TMP_InputField _inputName;
        [SerializeField]
        private Button _btnCreate;
        void Start()
        {
            _btnCreate.onClick.AddListener(OnCreateBtn);
        }

        private void OnCreateBtn() 
        {
            var request = new SessionRequest
            {
                DisplayName = _inputName.text,
                GameMode = Fusion.GameMode.Host,
                ScenePath = _mapSettings.Maps[0].ScenePath,
                MaxPlayers = 10,
                GameplayType = EGameplayType.Versus,
            };
            _menuUI.Context.Matchmaking.CreateSession(request);
        }
    }
}
