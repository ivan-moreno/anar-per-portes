using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/Entities/Toymaker Pedro")]
    public class ToymakerPedro : MonoBehaviour
    {
        [SerializeField] private SoundResource[] spawnDialogs;
        private Animator animator;
        private AudioSource audioSource;

        void Start()
        {
            animator = GetComponentInChildren<Animator>();
            audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
        
        }
    }
}
