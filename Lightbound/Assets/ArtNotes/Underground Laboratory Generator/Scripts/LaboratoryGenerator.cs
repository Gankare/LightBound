using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArtNotes.UndergroundLaboratoryGenerator
{
    public class LaboratoryGenerator : MonoBehaviour
    {
        public bool GenerateOnStart = true;
        [Range(3, 100)] public int RoomCount = 9;
        public LayerMask CellLayer;

        public GameObject InsteadDoor;
        public GameObject[] DoorPrefabs;
        public Cell[] CellPrefabs;
        public GameObject player;


        private void Start()
        {
            if (GenerateOnStart)
                StartCoroutine(StartGeneration());
        }

        private IEnumerator StartGeneration()
        {
            List<Transform> CreatedExits = new List<Transform>();
            Cell StartRoom = Instantiate(CellPrefabs[0], Vector3.zero, Quaternion.identity, transform);
            for (int i = 0; i < StartRoom.Exits.Length; i++)
                CreatedExits.Add(StartRoom.Exits[i].transform);
            StartRoom.TriggerBox.enabled = true;
            int limit = 1000, roomsLeft = RoomCount - 1;
            while (limit > 0 && roomsLeft > 0)
            {
                limit--;
                Cell selectedPrefab;
                if (roomsLeft == 1) 
                    selectedPrefab = Instantiate(CellPrefabs[CellPrefabs.Length - 1], Vector3.zero, Quaternion.identity, transform);
                else
                    selectedPrefab = Instantiate(CellPrefabs[Random.Range(1, CellPrefabs.Length - 2)], Vector3.zero, Quaternion.identity, transform);

                int lim = 100;
                bool collided;
                Transform selectedExit;
                Transform createdExit; 

                selectedPrefab.TriggerBox.enabled = false;

                do
                {
                    lim--;

                    createdExit = CreatedExits[Random.Range(0, CreatedExits.Count)];
                    selectedExit = selectedPrefab.Exits[Random.Range(0, selectedPrefab.Exits.Length)].transform;

                    // rotation
                    float shiftAngle = createdExit.eulerAngles.y + 180 - selectedExit.eulerAngles.y;
                    selectedPrefab.transform.Rotate(new Vector3(0, shiftAngle, 0)); 

                    // position
                    Vector3 shiftPosition = createdExit.position - selectedExit.position;
                    selectedPrefab.transform.position += shiftPosition; 

                    // check
                    Vector3 center = selectedPrefab.transform.position + selectedPrefab.TriggerBox.center.z * selectedPrefab.transform.forward
                        + selectedPrefab.TriggerBox.center.y * selectedPrefab.transform.up
                        + selectedPrefab.TriggerBox.center.x * selectedPrefab.transform.right; // selectedPrefab.TriggerBox.center
                    Vector3 size = selectedPrefab.TriggerBox.size / 2f; // half size
                    Quaternion rot = selectedPrefab.transform.localRotation;
                    collided = Physics.CheckBox(center, size, rot, CellLayer, QueryTriggerInteraction.Collide);

                    yield return null;

                } while (collided && lim > 0);

                selectedPrefab.TriggerBox.enabled = false; // Peng
                StartRoom.TriggerBox.enabled = false;
                if (lim > 0)
                {
                    roomsLeft--;

                    for (int j = 0; j < selectedPrefab.Exits.Length; j++)
                        CreatedExits.Add(selectedPrefab.Exits[j].transform);

                    CreatedExits.Remove(createdExit);
                    CreatedExits.Remove(selectedExit);

                    Instantiate(DoorPrefabs[Random.Range(0, DoorPrefabs.Length)], createdExit.transform.position, createdExit.transform.rotation, transform);
                    DestroyImmediate(createdExit.gameObject);
                    DestroyImmediate(selectedExit.gameObject);
                }
                else
                    DestroyImmediate(selectedPrefab.gameObject);


                // --- player placement (same frame) ---
                Bounds roomBounds = new Bounds(StartRoom.transform.position, Vector3.zero);
                Collider[] roomCols = StartRoom.GetComponentsInChildren<Collider>();
                foreach (var c in roomCols) roomBounds.Encapsulate(c.bounds);
                Vector3 spawnPos = roomBounds.center;
                player.transform.position = spawnPos;   // instant move
                yield return null;
            }

            // instead doors
            for (int i = 0; i < CreatedExits.Count; i++)
            {
                Instantiate(InsteadDoor, CreatedExits[i].position, CreatedExits[i].rotation, transform);
                DestroyImmediate(CreatedExits[i].gameObject);
            }

            //Debug.Log("Finished " + Time.time);
        }
    }
}