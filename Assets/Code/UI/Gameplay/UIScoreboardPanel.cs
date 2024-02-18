using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CatGame.UI
{
    public class UIScoreboardPanel : UIWidget
    {

        [SerializeField]
        private UIScoreboardItem _item;
        [SerializeField]
        private TextMeshProUGUI _livesHeader;


        [SerializeField]
        private int _maxShownRecords = 10;
        [SerializeField]
        private int _fixedFirstPlaces = 3;


        private ElementCache<UIScoreboardItem> _items;
        private float _refreshTimer;
        private static readonly PlayerStatisticsComparer _playerStatisticsComparer = new PlayerStatisticsComparer();

        // UIView INTERFAFCE

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _items = new ElementCache<UIScoreboardItem>(_item,16);
        }

        protected override void OnVisible()
        {
            base.OnVisible();

            Refresh();
        }

        protected override void OnTick()
        {
            base.OnTick();

            _refreshTimer -= Time.deltaTime;

            if (_refreshTimer > 0f)
                return;

            Refresh();
        }

        // PRIVATE METHODS

        private void Refresh()
        {
            if (Context.Runner == null || Context.Runner.Exists(Context.GameplayMode.Object) == false)
                return;

            var allStatistics = ListPool.Get<PlayerStatistics>(200);
            var localPlayerPosition = 0;

            int playersCount = 0;

            foreach (var player in Context.NetworkGame.Players)
            {
                if (player == null)
                    continue;

                var statistics = player.Statistics;

                allStatistics.Add(statistics);

                if (statistics.PlayerRef == Context.LocalPlayerRef)
                {
                    localPlayerPosition = statistics.Position;
                }

                playersCount++;
            }

            allStatistics.Sort(_playerStatisticsComparer);


            if (localPlayerPosition <= _maxShownRecords)
            {
                var i = 0;
                var count = Mathf.Min(_maxShownRecords, allStatistics.Count);
                for (; i < count; i++)
                {
                    var statistics = allStatistics[i];
                    var player = Context.NetworkGame.GetPlayer(statistics.PlayerRef);

                    if (player != null)
                    {
                        _items[i].SetData(statistics, player.Nickname);
                    }
                }
            }
            else
            {
                var i = 0;
                for (int count = _fixedFirstPlaces; i < count; i++)
                {
                    var statistics = allStatistics[i];
                    var player = Context.NetworkGame.GetPlayer(statistics.PlayerRef);

                    _items[i].SetData(statistics, player.Nickname);
                }

                
                var aroundPlayer = _maxShownRecords - _fixedFirstPlaces;
                var secondsBlockStart = localPlayerPosition - aroundPlayer / 2 - 1;

                if (secondsBlockStart + aroundPlayer > allStatistics.Count)
                {
                    secondsBlockStart -= secondsBlockStart + aroundPlayer - allStatistics.Count;
                }

                for (int y = secondsBlockStart; y < secondsBlockStart + aroundPlayer; i++, y++)
                {
                    var statistics = allStatistics[y];
                    var player = Context.NetworkGame.GetPlayer(statistics.PlayerRef);

                    _items[i].SetData(statistics, player.Nickname);
                }
            }


            ListPool.Return(allStatistics);

            _refreshTimer = 1f;
        }

        // HELPERS

        private class PlayerStatisticsComparer : IComparer<PlayerStatistics>
        {
            int IComparer<PlayerStatistics>.Compare(PlayerStatistics x, PlayerStatistics y)
            {
                return x.Position.CompareTo(y.Position);
            }
        }
    }
}
