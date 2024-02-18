using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CatGame.UI
{
    public class UIGameplayPanel : UIWidget
    {
        [SerializeField]
        private TextMeshProUGUI _mode;
        [SerializeField]
        private TextMeshProUGUI _time;
        [SerializeField]
        private TextMeshProUGUI _extraTime;
        [SerializeField]
        private TextMeshProUGUI _lblExtraTime;

        private int _lastSeconds;
        private int _extraLastSeconds;
        private int _waitLastSeconds;
        private bool _isVersus;
        protected override void OnVisible()
        {
            base.OnVisible();
            _mode.text = Context.GameplayMode.GameplayName;
            _isVersus = Context.GameplayMode is VersusGameplayMode;
        }
        protected override void OnTick()
        {
            base.OnTick();
            if (Context.Runner == null || Context.Runner.Exists(Context.GameplayMode.Object) == false)
                return;

            if (_isVersus)
            {
                int waitSeconnds = Mathf.CeilToInt(((VersusGameplayMode)Context.GameplayMode).WaitTime);
                if  (waitSeconnds > 0) 
                {
                    _lblExtraTime.text = "Esperando jugadores";
                    if (_waitLastSeconds != waitSeconnds)
                    {
                        _extraTime.text = $"{waitSeconnds / 60}:{waitSeconnds % 60:00}";
                        _waitLastSeconds = waitSeconnds;
                    }
                }
                int startSeconnds = Mathf.CeilToInt(((VersusGameplayMode)Context.GameplayMode).DelayTime);
                if (startSeconnds > 0)
                {
                    _lblExtraTime.text = "Empezando";
                    if (_extraLastSeconds != startSeconnds)
                    {
                        _extraTime.text = $"{startSeconnds / 60}:{startSeconnds % 60:00}";
                        _extraLastSeconds = startSeconnds;
                    }
                }
                
            }
            int remainSeconnds = Mathf.CeilToInt(Context.GameplayMode.RemainingTime);
            if (remainSeconnds > 0)
            {
                if (_lastSeconds != remainSeconnds)
                {
                    _time.text = $"{remainSeconnds / 60}:{remainSeconnds % 60:00}";
                    _lastSeconds = remainSeconnds;
                }
            }
            else
                _time.text = "...";
        }


        private void Refresh() 
        {
            var localPlayer = Context.NetworkGame.GetPlayer(Context.LocalPlayerRef);
            var statistics = localPlayer != null? localPlayer.Statistics: default ;

        }


    }
}
