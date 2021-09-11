using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAmbianceManager : MonoBehaviour
{
    [Header("Wind")]
    [Space]

    public AudioSource AS_AmbMainLoop;
    public AudioClip AC_MainLoop;
    

    [Space]

    public bool AudioDebug = false;

    [Header("Gust Of Wind")]

    [Space]

    public GameObject RFX01_Prefab;
    [Space]
    public BoxCollider RFX01_Zone;
    [Space]


    [Range(0, 60)]
    public float RFX01_rndWaitTimeMin;
    [Range(0, 60)]
    public float RFX01_rndWaitTimeMax;

    [Space]

    [Header("Cars Drive By")]

    [Space]

    public GameObject RFX02_Prefab;
    [Space]
    public BoxCollider RFX02_Zone;
    [Space]


    [Range(0, 60)]
    public float RFX02_rndWaitTimeMin;
    [Range(0, 60)]
    public float RFX02_rndWaitTimeMax;

    [Space]

    [Header("Sirens")]

    [Space]

    public GameObject RFX03_Prefab;
    [Space]
    public BoxCollider RFX03_Zone;
    [Space]


    [Range(0, 60)]
    public float RFX03_rndWaitTimeMin;
    [Range(0, 60)]
    public float RFX03_rndWaitTimeMax;

    [Space]

    [Header("Dogs and man cough")]

    [Space]

    public GameObject RFX04_Prefab;
    [Space]
    public BoxCollider RFX04_Zone;
    [Space]


    [Range(0, 60)]
    public float RFX04_rndWaitTimeMin;
    [Range(0, 60)]
    public float RFX04_rndWaitTimeMax;

    // Start is called before the first frame update
    void Start()
    {
        AS_AmbMainLoop.clip = AC_MainLoop;
        int randomStartTime = Random.Range(0, AC_MainLoop.samples - 1); //clip.samples is the lengh of the clip in samples
        AS_AmbMainLoop.timeSamples = randomStartTime;
        AS_AmbMainLoop.Play();

        if (RFX01_Prefab != null && RFX01_Zone != null)
            StartCoroutine(LaunchRFX01());
        if (RFX02_Prefab != null && RFX02_Zone != null)
            StartCoroutine(LaunchRFX02());
        if (RFX03_Prefab != null && RFX03_Zone != null)
            StartCoroutine(LaunchRFX03());
        if (RFX04_Prefab != null && RFX04_Zone != null)
            StartCoroutine(LaunchRFX04());
    }

   
    // RFX 01 

    float rndWaitTimeRFX01()
    {
        return Random.Range(RFX01_rndWaitTimeMin, RFX01_rndWaitTimeMax);
    }

    IEnumerator LaunchRFX01()
    {
        
        float WaitTimeRFX01 = rndWaitTimeRFX01();
        Vector3 rndPosRFX01 = RandomPointInBounds(RFX01_Zone.bounds);
        yield return new WaitForSeconds(WaitTimeRFX01);
        Instantiate(RFX01_Prefab, rndPosRFX01 , Quaternion.identity);
        if (AudioDebug)
            Debug.Log("RFX 01 launch at " + rndPosRFX01);
        StartCoroutine(LaunchRFX01());
    }

    // RFX 02 

    float rndWaitTimeRFX02()
    {
        return Random.Range(RFX02_rndWaitTimeMin, RFX02_rndWaitTimeMax);
    }

    IEnumerator LaunchRFX02()
    {

        float WaitTimeRFX02 = rndWaitTimeRFX02();
        Vector3 rndPosRFX02 = RandomPointInBounds(RFX02_Zone.bounds);
        yield return new WaitForSeconds(WaitTimeRFX02);
        Instantiate(RFX02_Prefab, rndPosRFX02, Quaternion.identity);
        if (AudioDebug)
            Debug.Log("RFX 02 launch at " + rndPosRFX02);
        StartCoroutine(LaunchRFX02());
    }
    // RFX 03 

    float rndWaitTimeRFX03()
    {
        return Random.Range(RFX03_rndWaitTimeMin, RFX03_rndWaitTimeMax);
    }

    IEnumerator LaunchRFX03()
    {

        float WaitTimeRFX03= rndWaitTimeRFX03();
        Vector3 rndPosRFX03 = RandomPointInBounds(RFX03_Zone.bounds);
        yield return new WaitForSeconds(WaitTimeRFX03);
        Instantiate(RFX03_Prefab, rndPosRFX03, Quaternion.identity);
        if (AudioDebug)
            Debug.Log("RFX 02 launch at " + rndPosRFX03);
        StartCoroutine(LaunchRFX03());
    }

    // RFX 04
    float rndWaitTimeRFX04()
    {
        return Random.Range(RFX04_rndWaitTimeMin, RFX04_rndWaitTimeMax);
    }

    IEnumerator LaunchRFX04()
    {

        float WaitTimeRFX04 = rndWaitTimeRFX04();
        Vector3 rndPosRFX04 = RandomPointInBounds(RFX04_Zone.bounds);
        yield return new WaitForSeconds(WaitTimeRFX04);
        Instantiate(RFX04_Prefab, rndPosRFX04, Quaternion.identity);
        if (AudioDebug)
            Debug.Log("RFX 02 launch at " + rndPosRFX04);
        StartCoroutine(LaunchRFX04());
    }


    public static Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z));
    }
}
