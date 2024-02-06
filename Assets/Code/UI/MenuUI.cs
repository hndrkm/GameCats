using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CatGame.UI
{
    public class MenuUI : BaseUI
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (Context.PlayerData.Nickname.HasValue() == false) 
            {
                var changeNickname = OpenView<UIChangeNickname>();
                changeNickname.SetData("Ingresar Nickname", true);
            }
        }
        protected override void OnActivate()
        {
            base.OnActivate();
            if (Global.Networking.ErrorStatus.HasValue() == true)
            {
                
                Global.Networking.ClearErrorStatus();
            }
        }
    }
}
