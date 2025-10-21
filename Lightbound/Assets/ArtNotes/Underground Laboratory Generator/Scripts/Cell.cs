using UnityEngine;

namespace ArtNotes.UndergroundLaboratoryGenerator
{
    //[RequireComponent(typeof(BoxCollider))]
    public class Cell : MonoBehaviour
    {
        public BoxCollider TriggerBox;
        [HideInInspector]
        public GameObject[] Exits;

        private void Awake()
        {
            if (TriggerBox == null) 
                TriggerBox = GetComponent<BoxCollider>();
            TriggerBox.isTrigger = true;
        }
    }
}