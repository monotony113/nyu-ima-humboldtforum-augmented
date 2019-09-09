using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Positioning : MonoBehaviour
{

    public bool cameraIn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerStay(Collider other)
    {
        cameraIn |= other.gameObject.layer == 14;
    }

    private void OnTriggerExit(Collider other)
    {
        cameraIn = false;
    }
}
