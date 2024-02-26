using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame.UI
{
    public class GameplayUI : BaseUI
    {
        [SerializeField]
        private float _gameOverScreenDelay = 3f;
        private bool _gameOverShown;
        private Coroutine _gameOverCoroutine;

        protected override void OnInitializeInternal()
        {
            base.OnInitializeInternal();

            //_deathView = Get<UIDeathView>();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();

            if (_gameOverCoroutine != null)
            {
                StopCoroutine(_gameOverCoroutine);
                _gameOverCoroutine = null;
            }

            _gameOverShown = false;
        }

        protected override void OnTickInternal()
        {
            base.OnTickInternal();

            if (_gameOverShown == true)
                return;
            if (Context.Runner == null || Context.Runner.Exists(Context.GameplayMode.Object) == false)
                return;

            var player = Context.NetworkGame.GetPlayer(Context.LocalPlayerRef);
            if (player == null || player.Statistics.IsAlive == true)
            {
                //_deathView.Close();
            }
            else
            {
                //_deathView.Open();
            }

            if (Context.GameplayMode.State == GameplayMode.EState.Finished && _gameOverCoroutine == null)
            {
                _gameOverCoroutine = StartCoroutine(ShowGameOver_Coroutine(_gameOverScreenDelay));
            }
        }

        private IEnumerator ShowGameOver_Coroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            _gameOverShown = true;

            //_deathView.Close();
            CloseView<UIGameplay>();
            OpenView<UIEndGame>();

            _gameOverCoroutine = null;
        }

    }
}
