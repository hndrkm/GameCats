using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace CatGame
{
    public class CharacterMoveController : NetworkCharacterControllerPrototype
    {
        [SerializeField] private LayerMask _wallLayer;
  
        public float _Macceleration = 10f;
        public float _Mbraking = 10f;
        public float _MmaxSpeed= 2f;
        public float _MrotationSpeed = 15f;
        [Networked]
        public Vector2 VelocityT { get; set; }
        public override void FixedUpdateNetwork()
        {

        }

        public void MoveCharacter(Vector2 direction) 
        {
            var deltaTime = Runner.DeltaTime;
            var previusPos = transform.position;
            var moveVelocity = VelocityT;

            if (direction == default)
            {
                moveVelocity = Vector2.Lerp(moveVelocity, default, _Mbraking * deltaTime);
            }
            else 
            {
                moveVelocity = Vector2.ClampMagnitude(moveVelocity + direction*_Macceleration*deltaTime, _MmaxSpeed);
            }
            Controller.Move(moveVelocity*deltaTime);
            VelocityT = (transform.position - previusPos)*Runner.Config.Simulation.TickRate;
        }

    }
}
