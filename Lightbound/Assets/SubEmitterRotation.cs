using UnityEngine;
using System.Collections.Generic;

public class SubEmitterRotation : MonoBehaviour
{
    public GameObject bloodPrefab; // flat quad with blood texture
    public float offset = 0.01f;   // push it slightly off the surface
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    void OnParticleCollision(GameObject other)
    {
        var ps = GetComponent<ParticleSystem>();
        int count = ps.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < count; i++)
        {
            var hit = collisionEvents[i];
            Vector3 pos = hit.intersection + hit.normal * offset;
            Quaternion rot = Quaternion.LookRotation(-hit.normal, Vector3.up);

            // Random rotation around normal (for variety)
            rot *= Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            Instantiate(bloodPrefab, pos, rot);
        }
    }
}
