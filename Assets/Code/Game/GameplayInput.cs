using Fusion;

namespace CatGame
{
    using UnityEngine;
        public enum EGameplayInputAction
        {
            Aim = 1,
            Attack = 2,
            ToggleSpeed = 3,
            Reload = 6,
            Interact = 7,
            Power = 8,
    }

        public struct GameplayInput : INetworkInput
        {
            public Vector2 MoveDirection;
            public Vector2 AimLocation;
            public NetworkButtons Actions;
            public byte Spell;

            public bool Power { get { return Actions.IsSet(EGameplayInputAction.Power); } set { Actions.Set(EGameplayInputAction.Power, value); } }
            public bool Aim { get { return Actions.IsSet(EGameplayInputAction.Aim); } set { Actions.Set(EGameplayInputAction.Aim, value); } }
            public bool Attack { get { return Actions.IsSet(EGameplayInputAction.Attack); } set { Actions.Set(EGameplayInputAction.Attack, value); } }
            public bool ToggleSpeed { get { return Actions.IsSet(EGameplayInputAction.ToggleSpeed); } set { Actions.Set(EGameplayInputAction.ToggleSpeed, value); } }
            public bool Reload { get { return Actions.IsSet(EGameplayInputAction.Reload); } set { Actions.Set(EGameplayInputAction.Reload, value); } }
            public bool Interact { get { return Actions.IsSet(EGameplayInputAction.Interact); } set { Actions.Set(EGameplayInputAction.Interact, value); } }
           
        }

        public static class GameplayInputActionExtensions
        {

            public static bool IsActive(this EGameplayInputAction action, GameplayInput input)
            {
                return input.Actions.IsSet(action) == true;
            }

            public static bool WasActivated(this EGameplayInputAction action, GameplayInput currentInput, GameplayInput previousInput)
            {
                return currentInput.Actions.IsSet(action) == true && previousInput.Actions.IsSet(action) == false;
            }

            public static bool WasDeactivated(this EGameplayInputAction action, GameplayInput currentInput, GameplayInput previousInput)
            {
                return currentInput.Actions.IsSet(action) == false && previousInput.Actions.IsSet(action) == true;
            }
        }
    
}
