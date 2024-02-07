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
        private void Awake()
        {
            //animator = tryGetComponent<Animator>();
            agent = GetComponent<Agent>();
        }
        private void Update()
        {
            animator.SetFloat("vel",agent.Character.CMC.Velocity.magnitude);
        }
    }
}
