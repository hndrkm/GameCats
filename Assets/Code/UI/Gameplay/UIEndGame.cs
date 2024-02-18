using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CatGame.UI
{
    public class UIEndGame : UIView
    {
        [SerializeField]
        private TextMeshProUGUI _winner;
        [SerializeField]
        private Button _restartButton;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _restartButton.onClick.AddListener(OnRestartButton);
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            var winnerStatistics = GetWinner();
            Player winner = null;

            if (winnerStatistics.IsValid == true)
            {
                winner = Context.NetworkGame.GetPlayer(winnerStatistics.PlayerRef);
            }

            if (winner != null)
            {
                _winner.text = $"El ganador es {winner.Nickname}";
                
            }
            else
            {
                //_winner.text = $"El ganador es {winner.Nickname}";
            }


            Global.Networking.StopGameOnDisconnect();
        }

        protected override void OnDeinitialize()
        {
            _restartButton.onClick.RemoveListener(OnRestartButton);

            base.OnDeinitialize();
        }

        private PlayerStatistics GetWinner()
        {
            foreach (var player in Context.NetworkGame.Players)
            {
                if (player == null)
                    continue;

                var statistics = player.Statistics;
                if (statistics.Position == 1)
                {
                    return statistics;
                }
            }

            return default;
        }

        private void OnRestartButton()
        {
            Global.Networking.StopGame();
        }
    }
}
