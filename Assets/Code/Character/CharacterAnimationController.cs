using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class CharacterAnimationController : MonoBehaviour
    {
        Agent agent;
        [SerializeField]
        Animator animator;
        [SerializeField]
        SpriteRenderer _renderer;
        private void Awake()
        {
            //animator = tryGetComponent<Animator>();
            agent = GetComponent<Agent>();
        }
        private void LateUpdate()
        {
            if (agent.Character.CharacteController.Velocity.x < -0.1f)
                _renderer.flipX = true;
            else if (agent.Character.CharacteController.Velocity.x > 0.1f)
                _renderer.flipX = false;

            animator.SetFloat("vel",agent.Character.CharacteController.Velocity.magnitude);
        }
    }
}
