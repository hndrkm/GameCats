using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CatGame
{
    public class MenuUI : BaseService
    {
        [SerializeField]
        private TextMeshProUGUI _errorTxt;
        [SerializeField]
        private GameObject _panelError;
        protected override void OnInitialize()
        {
            base.OnInitialize();

            Debug.Log(Context.PlayerData.ToString());
            Context.PlayerData.AgentID = Context.Settings.Agent.Agents[0].ID;
            Debug.Log(Context.Settings.Agent.Agents[0].ID);
            if (Context.PlayerData.Nickname.HasValue() == false) 
            {
                //Open modal de nickname
                _panelError.SetActive(true);
                _errorTxt.text = "Nickname vacio";
            }
        }
        protected override void OnActivate()
        {
            base.OnActivate();
            if (Global.Networking.ErrorStatus.HasValue() == true)
            {
                // Show error de conexion
                _panelError.SetActive(true);
                _errorTxt.text = $"Error {Global.Networking.ErrorStatus}";

                Global.Networking.ClearErrorStatus();
            }
        }
    }
}
