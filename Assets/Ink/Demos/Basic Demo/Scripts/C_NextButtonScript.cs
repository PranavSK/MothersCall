using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C_NextButtonScript : MonoBehaviour
{
    private C_InkManager _inkManager;

    // Start is called before the first frame update
    void Start()
    {
        _inkManager = FindObjectOfType<C_InkManager>();

        if (_inkManager == null)
        {
            Debug.LogError("Ink Manager was not found");
        }
    }

    public void OnClick()
    {
        Debug.Log("Am I working?");
        _inkManager.DisplayNextLine();
        Debug.Log("Im working");
    }
}
