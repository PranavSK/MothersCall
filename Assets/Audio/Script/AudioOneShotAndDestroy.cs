using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioOneShotAndDestroy : MonoBehaviour
{
    public AudioSource AudioSource;

    public AudioClip[] AudioClip;

    AudioClip previousClip;

    private float ClipLength;

    [Range(0, 1)]
    public float rndVolMin;
    [Range(0, 1)]
    public float rndVolMax;


    [Space]

    [Range(0, 2)]
    public float rndPitchMin;
    [Range(0, 2)]
    public float rndPitchMax;

    // Start is called before the first frame update
    void Start()
    {
        AudioSource.clip = GetClip(AudioClip);

        ClipLength = AudioSource.clip.length;
        AudioSource.pitch = rndPitch();
        AudioSource.volume = rndVol();
        AudioSource.Play();
        StartCoroutine(WaitForClipLength());

    }


    IEnumerator WaitForClipLength()
    {
        yield return new WaitForSeconds(ClipLength);
        Destroy(this.gameObject);
    }

    float rndPitch()
    {
        return Random.Range(rndPitchMin, rndPitchMax);
    }

    float rndVol()
    {
        return Random.Range(rndVolMin, rndVolMax);
    }

    AudioClip GetClip(AudioClip[] clipArray)
    {
        int attempts = 3;
        AudioClip selectedClip = clipArray[Random.Range(0, clipArray.Length - 1)];

        while (selectedClip == previousClip && attempts > 0)
        {
            selectedClip = clipArray[Random.Range(0, clipArray.Length - 1)];

            attempts--;
        }

        previousClip = selectedClip;
        if (selectedClip == null)
        {
            return null;
        }

        return selectedClip;


    }
}
