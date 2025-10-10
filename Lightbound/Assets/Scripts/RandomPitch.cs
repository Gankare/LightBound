using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RandomPitch : MonoBehaviour
{
    [Header("Pitch Settings")]
    [Tooltip("Minimum pitch range.")]
    public float minPitch = 0.9f;

    [Tooltip("Maximum pitch range.")]
    public float maxPitch = 1.1f;

    [Header("Timing")]
    [Tooltip("Play sound automatically when enabled.")]
    public bool playOnStart = true;

    [Tooltip("Minimum random delay before playing (in seconds).")]
    public float minDelay = 0f;

    [Tooltip("Maximum random delay before playing (in seconds).")]
    public float maxDelay = 0.2f;

    public AudioSource audioSource;

    private void Start()
    {
        ApplyRandomPitch();

        if (playOnStart)
            StartCoroutine(PlayWithDelay());
    }

    public void ApplyRandomPitch()
    {
        audioSource.pitch = Random.Range(minPitch, maxPitch);
    }

    private IEnumerator PlayWithDelay()
    {
        float delay = Random.Range(minDelay, maxDelay);
        yield return new WaitForSeconds(delay);
        audioSource.Play();
    }
}
