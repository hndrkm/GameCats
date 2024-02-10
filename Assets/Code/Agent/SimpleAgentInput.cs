using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CatGame
{
    public class SimpleAgentInput : NetworkBehaviour, INetworkRunnerCallbacks
    {
        Agent _agent;
        Vector2 _moveDirection;
        Vector2 _aimLocation;
        private GameplayInput _actionInput;

        private void Awake()
        {
            _agent = GetComponent<Agent>();
        }


        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (Object.HasInputAuthority == false)
                return;
            input.Set(_actionInput);
            _actionInput = default;
        }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            
        }

        public void Update()
        {

            if (_agent.IsLocal == false)
                return;


            Vector2 moveDirection;
            Vector2 aimLocation = Vector2.zero;


            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;
            Vector2 mousePosition = mouse.delta.ReadValue() * 0.05f;

            moveDirection = Vector2.zero;
            if (mouse.leftButton.isPressed)
            {
                aimLocation = mousePosition * Global.RuntimeSettings.AimSensitivity;
                _aimLocation += aimLocation;
            }


            if (keyboard.wKey.isPressed == true) { moveDirection += Vector2.up; }
            if (keyboard.sKey.isPressed == true) { moveDirection += Vector2.down; }
            if (keyboard.aKey.isPressed == true) { moveDirection += Vector2.left; }
            if (keyboard.dKey.isPressed == true) { moveDirection += Vector2.right; }

            if (moveDirection.x == 0 && moveDirection.y == 0)
            {
                moveDirection.Normalize();
            }


            _actionInput.MoveDirection = moveDirection;
            _actionInput.AimLocation = _aimLocation;
            _actionInput.Aim = mouse.leftButton.isPressed;
            _actionInput.Attack = mouse.leftButton.wasReleasedThisFrame;
            _actionInput.Power = keyboard.eKey.isPressed;
            _actionInput.Reload = keyboard.rKey.isPressed;
            _actionInput.Interact = keyboard.fKey.isPressed;
#if UNITY_EDITOR
            _actionInput.ToggleSpeed = keyboard.backquoteKey.isPressed;
#else
			_actionInput.ToggleSpeed       = keyboard.leftCtrlKey.isPressed & keyboard.leftAltKey.isPressed & keyboard.backquoteKey.isPressed;
#endif
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                _aimLocation = default;
            }   

            float deltaTime = Time.deltaTime;
            _moveDirection += moveDirection * deltaTime;

            //_actionInput.Actions = new NetworkButtons(_cachedInput.Actions.Bits | _renderInput.Actions.Bits);
            //_actionInput.MoveDirection = _cachedMoveDirection / _cachedMoveDirectionSize;
            //_actionInput.AimLocation = _renderInput.AimLocation;
        }




        public void OnConnectedToServer(NetworkRunner runner)
        {
            
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            
        }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
            
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            
        }

        

    }
}
