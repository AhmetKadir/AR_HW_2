using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class showPositions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //show positions of child objects
        foreach (Transform child in transform)
        {
            Debug.Log(child.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
