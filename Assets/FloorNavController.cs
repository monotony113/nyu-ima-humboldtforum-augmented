using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloorNavController : MonoBehaviour
{

    internal MuseumObjectRep m;
    internal MuseumObjectRep r;

    public GameObject label;
    public GameObject room;
    public GameObject arrow;
    public GameObject center;

    public GameObject turnLeftIndicator;
    public GameObject turnRightIndicator;

    public GameObject followObject;
    public GameObject followCamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m != null && r != null)
        {
            label.GetComponent<TextMeshPro>().text = m.name;
            room.GetComponent<TextMeshPro>().text = r.name;

            Vector3 oldHeading = arrow.transform.position - center.transform.position;
            Vector3 oldDirection = Vector3.Scale(new Vector3(1, 0, 1), oldHeading / oldHeading.magnitude);

            Vector3 newHeading = m.GO.transform.position - center.transform.position;
            Vector3 newDirection = Vector3.Scale(new Vector3(1, 0, 1), newHeading / newHeading.magnitude);

            Vector3 oldCameraDirection = Vector3.Scale(new Vector3(1, 0, 1), followCamera.transform.forward);
            Vector3 newCameraDirection = Vector3.Scale(new Vector3(1, 0, 1), Camera.main.transform.forward);

            float directionDiff = Vector3.SignedAngle(newCameraDirection, newDirection, Vector3.up);
            if (directionDiff > 20)
            {
                turnRightIndicator.SetActive(true);
                turnLeftIndicator.SetActive(false);
            } else if (directionDiff < -20)
            {
                turnRightIndicator.SetActive(false);
                turnLeftIndicator.SetActive(true);
            } else
            {
                turnRightIndicator.SetActive(false);
                turnLeftIndicator.SetActive(false);
            }

            followCamera.transform.RotateAround(center.transform.position, Vector3.up, Vector3.SignedAngle(oldCameraDirection, newCameraDirection, Vector3.up));
            followObject.transform.RotateAround(center.transform.position, Vector3.up, Vector3.SignedAngle(oldDirection, newDirection, Vector3.up));
        } else
        {
            label.GetComponent<TextMeshPro>().text = "Please select a destination";
            room.GetComponent<TextMeshPro>().text = "";
        }
    }
}
