using UnityEngine;

public class BirdController : MonoBehaviour
{
    private AudioSource _birdSource;
    public float minWaitTime = 5f;
    public float maxWaitTime = 15f;

    void Start()
    {
        _birdSource = GetComponent<AudioSource>();
        _birdSource.loop = false; // We will handle the looping manually
        StartCoroutine(BirdLoop());
    }

    System.Collections.IEnumerator BirdLoop()
    {
        while (true)
        {
            // Wait for a random amount of time before the next chirp
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // Play the bird sound once
            _birdSource.Play();

            // Wait for the clip to finish before starting the loop again
            yield return new WaitForSeconds(_birdSource.clip.length);
        }
    }
}