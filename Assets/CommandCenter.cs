/*
Copyright 2019 Tony Wu

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleJSON;

public class CommandCenter : MonoBehaviour
{

    private JSONObject museumCollectionJSON;

    private GameObject museumWorld;
    public Camera placeholderCam;

    public static Dictionary<string, MuseumObjectRep> museumObjects = new Dictionary<string, MuseumObjectRep>();
    public static Dictionary<string, List<MuseumObjectRep>> museumThreads = new Dictionary<string, List<MuseumObjectRep>>();

    public static List<string> currentZones = new List<string>();
    public static string currentLocation = "";

    public static Vector3 museumDimension = new Vector3(183.811f, 40f, 121.7801f);

    // Start is called before the first frame update
    void Start()
    {
        LoadMuseumCollection();
        //UnloadAllScenesExcept("Controller");
        ARMapPreview();

        Physics.IgnoreLayerCollision(10, 12);
    }
    private void LoadMuseumCollection()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "metadata.json");
        string jsonStr = File.ReadAllText(filePath);
        museumCollectionJSON = (JSONObject)JSON.Parse(jsonStr);

        foreach (JSONNode item in museumCollectionJSON["objects"])
        {
            museumObjects.Add(item["id"], new MuseumObjectRep(item));
        }

        foreach (MuseumObjectRep m in museumObjects.Values.Where(m => m.type == "place" || m.type == "space"))
        {
            foreach (string c in m.relationship["children"].AsArray.Values)
            {
                museumObjects[c].room = m.id;
            }
        }

        museumThreads["Museum Navigation"] = new List<MuseumObjectRep>();
        foreach (MuseumObjectRep m in museumObjects.Values.Where(m => m.type == "place"))
        {
            museumThreads["Museum Navigation"].Add(m);
        }

        foreach (MuseumObjectRep m in museumObjects.Values.Where(m => m.type == "exhibit"))
        {
            if (m.curatorial != null)
            {
                foreach (string theme in m.curatorial["themes"].AsArray.Values)
                {
                    if (!museumThreads.ContainsKey(theme)) museumThreads[theme] = new List<MuseumObjectRep>();
                    museumThreads[theme].Add(m);
                }
                foreach (string theme in m.curatorial["exhibitions"].AsArray.Values)
                {
                    if (!museumThreads.ContainsKey(theme)) museumThreads[theme] = new List<MuseumObjectRep>();
                    museumThreads[theme].Add(m);
                }
                foreach (string theme in m.curatorial["keywords"].AsArray.Values)
                {
                    if (!museumThreads.ContainsKey(theme)) museumThreads[theme] = new List<MuseumObjectRep>();
                    museumThreads[theme].Add(m);
                }
                foreach (string theme in m.curatorial["classification"].AsArray.Values)
                {
                    if (!museumThreads.ContainsKey(theme)) museumThreads[theme] = new List<MuseumObjectRep>();
                    museumThreads[theme].Add(m);
                }
            }
        }

        museumThreads["My Bookmarks"] = new List<MuseumObjectRep>();
    }

    private IEnumerator AddSceneAndSetActive(string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Additive);
        yield return 0;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene));
    }
    private void UnloadAllScenesExcept(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != sceneName && scene.name != "UnityARKitRemote")
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }
    }

    public void ARMapPreview()
    {
        UnloadAllScenesExcept("Controller");
        StartCoroutine("AddSceneAndSetActive", "ARScene");
        placeholderCam.enabled = false;
    }
    public void ResetScenes()
    {
        UnloadAllScenesExcept("Controller");
        placeholderCam.enabled = true;
        ARMapPreview();
    }

    // Update is called once per frame
    void Update()
    {
        currentZones = new List<string>();
        foreach (MuseumObjectRep m in museumObjects.Values.Where(m => m.type == "space" || m.type == "place"))
        {
            if (m.GO)
            {
                if (m.GO.GetComponent<Positioning>().cameraIn)
                {
                    currentZones.Add(m.id);
                } else
                {
                    currentZones.Remove(m.id);
                }
            }
        }
        if (!currentZones.Contains(currentLocation)) currentLocation = "";
        foreach (string zoneId in currentZones)
        {
            if (string.Compare(zoneId, currentLocation) > 0)
            {
                currentLocation = zoneId;
            }
        }
    }

    // Helpers
    internal static Vector3 DenormalizedMuseumVectors(Vector3 pos, bool moveAnchor = false)
    {
        return moveAnchor ? Vector3.Scale(pos - new Vector3(.5f, .5f, -.5f), museumDimension) : Vector3.Scale(pos, museumDimension);
    }
}
