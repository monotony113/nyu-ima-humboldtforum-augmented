using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class MuseumLocationService : MonoBehaviour
{
    public static Dictionary<string, GameObject> imageAnchors = new Dictionary<string, GameObject>();
    private static Dictionary<string, int> imageAnchorTimer = new Dictionary<string, int>();
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        foreach (KeyValuePair<string, GameObject> kvp in imageAnchors)
        {
            if (kvp.Value.activeSelf)
            {
                if (!imageAnchorTimer.ContainsKey(kvp.Key)) imageAnchorTimer[kvp.Key] = 0;
                imageAnchorTimer[kvp.Key] += 1;

                if (imageAnchorTimer[kvp.Key] == 120) OnUpdateLocation(new EventMessage(kvp.Key));
            } else
            {
                imageAnchorTimer[kvp.Key] = 0;
            }
        }
    }

    internal protected virtual void OnUpdateLocation(EventMessage e)
    {
        UpdateLocationEvent?.Invoke(this, e);
    }
    internal event EventHandler<EventMessage> UpdateLocationEvent;

    public void TestUpdateLocation()
    {
        OnUpdateLocation(new EventMessage("1891.12.1"));
    }

}

public class EventMessage : EventArgs
{
    internal readonly string msg;
    internal EventMessage(string m)
    {
        msg = m;
    }
}
