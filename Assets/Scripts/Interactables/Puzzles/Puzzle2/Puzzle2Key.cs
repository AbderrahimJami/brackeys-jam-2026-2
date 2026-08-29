using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactables.Puzzles.Puzzle2
{
    
    public class Puzzle2Key : MonoBehaviour
    {



        MeshRenderer meshRenderer;
        BoxCollider collider;

        [SerializeField]
        List<Puzzle2Button> allButtons;


        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            collider = GetComponent<BoxCollider>();

            meshRenderer.enabled = false;
            collider.enabled = false;

        }


        public void puzzleSolved() {

            meshRenderer.enabled = true;
            collider.enabled = true;

            for (int i = 0; i < allButtons.Count; i++)
            {
                allButtons[i].gameObject.SetActive(false);
            }


        }

    }
}
