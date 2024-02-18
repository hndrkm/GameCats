
using TMPro;
using UnityEngine;

namespace CatGame.UI
{
    public class UIScoreboardItem : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _position;
        [SerializeField]
        private TextMeshProUGUI _nickname;
        [SerializeField]
        private TextMeshProUGUI _kills;
        [SerializeField]
        private TextMeshProUGUI _deaths;
        [SerializeField]
        private TextMeshProUGUI _score;

        public void SetData(PlayerStatistics statistics, string nickname)
        {
            _position.text = $"#{statistics.Position}";
            _nickname.text = nickname;
            _kills.text = statistics.Kills.ToString("N0");
            _deaths.text = statistics.Deaths.ToString("N0");
            _score.text = statistics.Score.ToString("N0");
        }
    }
}
