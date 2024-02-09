using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CatGame
{
    
    public class AgentInput : ContextBehaviour, IBeforeUpdate, IBeforeTick
    {
        public GameplayInput FixedInput { get { CheckFixedAccess(false); return _fixedInput; } }
        public GameplayInput RenderInput { get { CheckRenderAccess(false); return _renderInput; } }
        public GameplayInput CachedInput { get { CheckFixedAccess(false); return _cachedInput; } }

        public bool IsReloadPower => Time.time < _reloadStartTime+_reloadDruration;
        [SerializeField]
        private float _reloadDruration = 1f;
        [SerializeField]
        private bool _logMissingInputs;

        [Networked(nameof(AgentInput))]
        private GameplayInput _lastKnownInput { get; set; }
        private Agent _agent;

        private GameplayInput _fixedInput;
        private GameplayInput _renderInput;
        private GameplayInput _cachedInput;
        private GameplayInput _baseFixedInput;
        private GameplayInput _baseRenderInput;
        private Vector2 _cachedMoveDirection;
        private Vector2 _cachedAimDirection;
        private float _cachedMoveDirectionSize;
        private bool _resetCachedInput;
        private int _missingInputsTotal;
        private int _missingInputsInRow;
        private int _logMissingInputFromTick;
        private float _reloadStartTime;


        public bool HasActive(EGameplayInputAction action)
        {
            if (Runner.Stage != default)
            {
                CheckFixedAccess(false);
                return action.IsActive(_fixedInput);
            }
            else
            {
                CheckRenderAccess(false);
                return action.IsActive(_renderInput);
            }
        }
        public bool WasActivated(EGameplayInputAction action)
        {
            if (Runner.Stage != default)
            {
                CheckFixedAccess(false);
                return action.WasActivated(_fixedInput, _baseFixedInput);
            }
            else
            {
                CheckRenderAccess(false);
                return action.WasActivated(_renderInput, _baseRenderInput);
            }
        }
        public bool WasActivated(EGameplayInputAction action, GameplayInput customInput)
        {
            if (Runner.Stage != default)
            {
                CheckFixedAccess(false);
                return action.WasActivated(customInput, _baseFixedInput);
            }
            else
            {
                CheckRenderAccess(false);
                return action.WasActivated(customInput, _baseRenderInput);
            }
        }
        public bool WasDeactivated(EGameplayInputAction action)
        {
            if (Runner.Stage != default)
            {
                CheckFixedAccess(false);
                return action.WasDeactivated(_fixedInput, _baseFixedInput);
            }
            else
            {
                CheckRenderAccess(false);
                return action.WasDeactivated(_renderInput, _baseRenderInput);
            }
        }
        public bool WasDeactivated(EGameplayInputAction action, GameplayInput customInput)
        {
            if (Runner.Stage != default)
            {
                CheckFixedAccess(false);
                return action.WasDeactivated(customInput, _baseFixedInput);
            }
            else
            {
                CheckRenderAccess(false);
                return action.WasDeactivated(customInput, _baseRenderInput);
            }
        }
        public void SetFixedInput(GameplayInput fixedInput, bool updateBaseInputs)
        {
            CheckFixedAccess(true);

            _fixedInput = fixedInput;

            if (updateBaseInputs == true)
            {
                _baseFixedInput = fixedInput;
                _baseRenderInput = fixedInput;
            }
        }
        public void SetRenderInput(GameplayInput renderInput, bool updateBaseInput)
        {
            CheckRenderAccess(false);

            _renderInput = renderInput;

            if (updateBaseInput == true)
            {
                _baseRenderInput = renderInput;
            }
        }
        public void SetLastKnownInput(GameplayInput fixedInput, bool updateBaseInputs)
        {
            CheckFixedAccess(true);

            _lastKnownInput = fixedInput;

            if (updateBaseInputs == true)
            {
                _baseFixedInput = fixedInput;
                _baseRenderInput = fixedInput;
            }
        }

        public override void Spawned()
        {
            _fixedInput = default;
            _renderInput = default;
            _cachedInput = default;
            _lastKnownInput = default;
            _baseFixedInput = default;
            _baseRenderInput = default;
            _missingInputsTotal = default;
            _missingInputsInRow = default;


            _logMissingInputFromTick = Runner.Simulation.Tick + Runner.Config.Simulation.TickRate * 4;

            if (_agent.IsLocal == false)
                return;

            NetworkEvents networkEvents = Runner.GetComponent<NetworkEvents>();
            networkEvents.OnInput.RemoveListener(OnInput);
            networkEvents.OnInput.AddListener(OnInput);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (runner != null)
            {
                NetworkEvents networkEvents = runner.GetComponent<NetworkEvents>();
                networkEvents.OnInput.RemoveListener(OnInput);
            }
        }
        void IBeforeUpdate.BeforeUpdate()
        {
            if (Object.HasInputAuthority == false)
                return;

            // Store last render input as a base to current render input.
            _baseRenderInput = _renderInput;

            // Reset input for current frame to default.
            _renderInput = default;

            // Cached input was polled and explicit reset requested.
            if (_resetCachedInput == true)
            {
                _resetCachedInput = false;

                _cachedInput = default;
                _cachedMoveDirection = default;
                _cachedMoveDirectionSize = default;
            }

            if (_agent.IsLocal == false)
                return;
            

            Vector2 moveDirection;
            Vector2 aimLocation = Vector2.zero;


            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;
            Vector2 mousePosition = mouse.delta.ReadValue()*0.05f;

            moveDirection = Vector2.zero;
            if (mouse.leftButton.isPressed)
            {
                aimLocation = mousePosition*Global.RuntimeSettings.AimSensitivity;
                _cachedAimDirection += aimLocation;
            }


            if (keyboard.wKey.isPressed == true) { moveDirection += Vector2.up; }
                if (keyboard.sKey.isPressed == true) { moveDirection += Vector2.down; }
                if (keyboard.aKey.isPressed == true) { moveDirection += Vector2.left; }
                if (keyboard.dKey.isPressed == true) { moveDirection += Vector2.right; }
            
            if (moveDirection.x ==0 && moveDirection.y ==0)
                {
                    moveDirection.Normalize();
                }
           
            
            _renderInput.MoveDirection = moveDirection;
            _renderInput.AimLocation = _cachedAimDirection;
            _renderInput.Aim = mouse.leftButton.isPressed;
            _renderInput.Attack = mouse.leftButton.wasReleasedThisFrame;
            _renderInput.Power = keyboard.eKey.isPressed;
            _renderInput.Reload = keyboard.rKey.isPressed;
            _renderInput.Interact = keyboard.fKey.isPressed;
#if UNITY_EDITOR
            _renderInput.ToggleSpeed = keyboard.backquoteKey.isPressed;
#else
			_renderInput.ToggleSpeed       = keyboard.leftCtrlKey.isPressed & keyboard.leftAltKey.isPressed & keyboard.backquoteKey.isPressed;
#endif
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                _cachedAimDirection = default;
            }
            
            float deltaTime = Time.deltaTime;
            _cachedMoveDirection += moveDirection * deltaTime;
            _cachedMoveDirectionSize += deltaTime;

            _cachedInput.Actions = new NetworkButtons(_cachedInput.Actions.Bits | _renderInput.Actions.Bits);
            _cachedInput.MoveDirection = _cachedMoveDirection / _cachedMoveDirectionSize;
            _cachedInput.AimLocation = _renderInput.AimLocation;

        }
        void IBeforeTick.BeforeTick()
        {
            

            
            _baseFixedInput = _lastKnownInput;

            
            _fixedInput = _lastKnownInput;

            if (Object.InputAuthority != PlayerRef.None)
            {
                
                if (Runner.TryGetInputForPlayer(Object.InputAuthority, out GameplayInput input) == true)
                {
                   
                    _fixedInput = input;

                    
                    _lastKnownInput = input;

                    if (Runner.Stage == SimulationStages.Forward)
                    {
                        _missingInputsInRow = 0;
                    }
                }
                else
                {
                    if (Runner.Stage == SimulationStages.Forward)
                    {
                        ++_missingInputsInRow;
                        ++_missingInputsTotal;

                        if (_missingInputsInRow > 5)
                        {
                            _fixedInput.AimLocation = default;
                        }
                        else if (_missingInputsInRow > 2)
                        {
                            _fixedInput.AimLocation *= 0.5f;
                        }

                        if (_logMissingInputs == true && Runner.Tick >= _logMissingInputFromTick)
                        {
                            Debug.LogWarning($"Missing input for {Object.InputAuthority} {Runner.Tick}. In Row: {_missingInputsInRow} Total: {_missingInputsTotal}", gameObject);
                        }
                    }
                }
            }

            
            _baseRenderInput = _fixedInput;
        }

        // PRIVATE METHODS

        private void Awake()
        {
            _agent = GetComponent<Agent>();
        }

        /// <summary>
        /// 2. Push cached input and reset properties, can be executed multiple times within single Unity frame if the rendering speed is slower than Fusion simulation (or there is a performance spike).
        /// </summary>
        private void OnInput(NetworkRunner runner, NetworkInput networkInput)
        {
            if (_agent.IsLocal == false )
            {
                _cachedInput = default;
                _renderInput = default;
                return;
            }

            GameplayInput gameplayInput = _cachedInput;

            // Input is polled for single fixed update, but at this time we don't know how many times in a row OnInput() will be executed.
            // This is the reason for having a reset flag instead of resetting input immediately, otherwise we could lose input for next fixed updates (for example move direction).

            _resetCachedInput = true;

            // Now we reset all properties which should not propagate into next OnInput() call (for example LookRotationDelta - this must be applied only once and reset immediately).
            // If there's a spike, OnInput() and FixedUpdateNetwork() will be called multiple times in a row without BeforeUpdate() in between, so we don't reset move direction to preserve movement.
            // Instead, move direction and other sensitive properties are reset in next BeforeUpdate() - driven by _resetCachedInput.

            //_cachedInput.AimLocation = default;

            // Input consumed by OnInput() call will be read in FixedUpdateNetwork() and immediately propagated to KCC.
            // Here we should reset render properties so they are not applied twice (fixed + render update).

            //_renderInput.AimLocation = default;

            networkInput.Set(gameplayInput);
        }

        

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void CheckFixedAccess(bool checkStage)
        {
            if (checkStage == true && Runner.Stage == default)
            {
                throw new InvalidOperationException("This call should be executed from FixedUpdateNetwork!");
            }

            if (Runner.Stage != default && Object.IsProxy == true)
            {
                throw new InvalidOperationException("Fixed input is available only on State & Input authority!");
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void CheckRenderAccess(bool checkStage)
        {
            if (checkStage == true && Runner.Stage != default)
            {
                throw new InvalidOperationException("This call should be executed outside of FixedUpdateNetwork!");
            }

            if (Runner.Stage == default && Object.HasInputAuthority == false)
            {
                throw new InvalidOperationException("Render and cached inputs are available only on Input authority!");
            }
        }
    }
}
