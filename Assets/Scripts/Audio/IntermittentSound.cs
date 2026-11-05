using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class IntermittentSound : MonoBehaviour
{
    [SerializeField] private float minInterval = 20f;
    [SerializeField] private float maxInterval = 40f;

    private IEnumerator Start()
    {
        AudioSource source = GetComponent<AudioSource>();

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            source.Play();
        }
    }
}
