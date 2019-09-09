using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ARController : MonoBehaviour
{

    public GameObject cameraParent;

    public GameObject museumEnvironment;

    public GameObject defaultCamLocation;
    public GameObject museumLocationService;

    private GameObject augmentationPlane;

    public GameObject screens;
    public GameObject floorNav;

    private GameObject inquiryModule;
    private TMP_Dropdown topicDropdown;
    private TMP_Dropdown exhibitDropdown;
    private TMP_Dropdown.OptionData bookmarkOption = new TMP_Dropdown.OptionData("My Bookmarks");

    public GameObject debugOut;
    private static GameObject debugOutStatic;

    public static bool cameraMoving;

    private MuseumObjectRep inquiredMuseumObject;

    // Start is called before the first frame update
    void Start()
    {
        museumEnvironment.SetActive(false);

        InstantiateMO(CommandCenter.museumObjects.Values.ToList(), museumEnvironment.transform);
        OrganizeMuseumSpaces(CommandCenter.museumObjects.Values.Where(m => m.type == "space" || m.type == "place").ToList(), museumEnvironment.transform);

        var frustumHeight = 2.0f * 10 * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad) + 1f;
        var frustumWidth = frustumHeight * Camera.main.aspect;
        augmentationPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        augmentationPlane.name = "AugmentationPlane";
        augmentationPlane.transform.localScale = new Vector3(frustumWidth * 1.1f, frustumHeight * 1.1f, 1);
        augmentationPlane.transform.SetParent(museumEnvironment.transform);
        augmentationPlane.layer = 12;
        augmentationPlane.GetComponent<MeshCollider>().convex = true;
        augmentationPlane.GetComponent<Renderer>().enabled = false;
        //augmentationPlane.AddComponent<ReverseNormals>();

        topicDropdown = screens.transform.Find("Topic").GetComponent<TMP_Dropdown>();
        topicDropdown.ClearOptions();
        topicDropdown.AddOptions(CommandCenter.museumThreads.Keys.ToList());

        exhibitDropdown = screens.transform.Find("Exhibit").GetComponent<TMP_Dropdown>();
        exhibitDropdown.ClearOptions();

        inquiryModule = screens.transform.Find("Inquiry").gameObject;

        museumLocationService.GetComponent<MuseumLocationService>().UpdateLocationEvent += UpdateLocation;

        floorNav.transform.parent = museumEnvironment.transform;

        debugOutStatic = debugOut;

        Input.location.Start();

        StartCoroutine(DemonstrationInitialScreen());
    }

    private void InstantiateMO(List<MuseumObjectRep> mObjs, Transform parent)
    {
       foreach (MuseumObjectRep mObj in mObjs) if (!mObj.GO) { mObj.InstantiateGO(parent); }
    }
    private void OrganizeMuseumSpaces(List<MuseumObjectRep> mObjs, Transform parent)
    {
        foreach (MuseumObjectRep mObj in mObjs)
        {
            foreach (string child in mObj.relationship["children"].AsArray.Values)
            {
                var childTransform = parent.Find(child);
                if (childTransform)
                {
                    childTransform.parent = mObj.GO.transform;
                }
            }
        }
    }
    private void FilterMuseumObjects(string thread)
    {
        foreach (MuseumObjectRep m in CommandCenter.museumObjects.Values)
        {
            if (CommandCenter.museumThreads[thread].Contains(m))
            {
                m.included = true;
            }
            else
            {
                m.included = false;
            }
        }

        exhibitDropdown.ClearOptions();
        exhibitDropdown.AddOptions(CommandCenter.museumThreads[thread].Select(m => m.name).ToList());
        UpdateFloorNav(exhibitDropdown.value);
    }

    // Update is called once per frame
    void Update()
    {
        museumEnvironment.transform.rotation = Quaternion.Euler(0, -Input.compass.trueHeading, 0);
        //ScreenDebug(gameObject, museumEnvironment.transform.rotation.eulerAngles.ToString());
        //augmentationPlane.transform.LookAt(Camera.main.transform.position);
        augmentationPlane.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 10;
        augmentationPlane.transform.rotation = Camera.main.transform.rotation;

        floorNav.transform.position = Camera.main.transform.position - Vector3.up * 1f;

        if (CommandCenter.museumThreads["My Bookmarks"].Count == 0)
        {
            topicDropdown.options.RemoveAll(d => d.text == "My Bookmarks");
        } else if (!topicDropdown.options.Contains(bookmarkOption))
        {
            topicDropdown.options.Add(bookmarkOption);
        }

        if (cameraMoving)
        {
            UpdateScreenText("Status/Text", "Relocating...");
        }
        else
        {
            if (CommandCenter.currentLocation != "")
            {
                UpdateScreenText("Status/Text", CommandCenter.museumObjects[CommandCenter.currentLocation].desc);
            }
            else
            {
                UpdateScreenText("Status/Text", "Outside museum!");
            }
        }

        foreach (MuseumObjectRep m in CommandCenter.museumObjects.Values.Where(m => m.type == "exhibit"))
        {
            m.sameRoomAsCamera = m.room == CommandCenter.currentLocation;
        }

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, Mathf.Infinity, 1 << 10))
        {
            inquiredMuseumObject = hit.collider.gameObject.GetComponentInParent<MuseumObjectController>().metadata;
            if (CommandCenter.museumThreads["My Bookmarks"].Contains(inquiredMuseumObject))
            {
                inquiryModule.transform.Find("Button-AddBookmark").gameObject.SetActive(false);
                inquiryModule.transform.Find("Button-DeleteBookmark").gameObject.SetActive(true);
            } else
            {
                inquiryModule.transform.Find("Button-AddBookmark").gameObject.SetActive(true);
                inquiryModule.transform.Find("Button-DeleteBookmark").gameObject.SetActive(false);
            }
            inquiryModule.SetActive(true);
        } else
        {
            inquiredMuseumObject = null;
            inquiryModule.SetActive(false);
        }

        //ScreenDebug(gameObject, EnumerateTransform(museumLocationService.transform, museumLocationService.name + "/", 0, 3));
    }

    public void RelocateCamera(Transform t)
    {
        cameraParent.transform.position = t.position;
        Camera.main.transform.localPosition = Vector3.zero;
    }
    public void RelocateCamera(Vector3 v, bool normalized = false)
    {
        cameraParent.transform.position = normalized ? CommandCenter.DenormalizedMuseumVectors(v, true) : v;
        Camera.main.transform.localPosition = Vector3.zero;
    }
    public void MoveCamera(string direction)
    {
        Vector3 newPosition = cameraParent.transform.position;
        Vector3 oldPosition = cameraParent.transform.position;

        switch(direction)
        {
            case "W":
                newPosition += Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)) * 3;
                StartCoroutine(Alert("Moving 3m forward", 2));
                break;
            case "A":
                newPosition += Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1)) * -3;
                StartCoroutine(Alert("Moving 3m left", 2));
                break;
            case "S":
                newPosition += Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)) * -3;
                StartCoroutine(Alert("Moving 3m backward", 2));
                break;
            case "D":
                newPosition += Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1)) * 3;
                StartCoroutine(Alert("Moving 3m right", 2));
                break;
            case "E":
                newPosition += CommandCenter.DenormalizedMuseumVectors(new Vector3(0f, 0.25f, 0f), false);
                StartCoroutine(Alert("Moving up 1 floor", 2));
                break;
            case "C":
                newPosition += CommandCenter.DenormalizedMuseumVectors(new Vector3(0f, -0.25f, 0f), false);
                StartCoroutine(Alert("Moving down 1 floor", 2));
                break;
        }

        StartCoroutine(InterpolateCamera(oldPosition, newPosition, 60));
    }
    private IEnumerator InterpolateCamera(Vector3 start, Vector3 end, int frames)
    {
        cameraMoving = true;
        for (int i = 1; i <= frames; i++)
        {
            RelocateCamera(Vector3.Lerp(start, end, i / (float)frames));
            yield return null;
        }
        cameraMoving = false;
    }

    internal void UpdateLocation(object sender, EventMessage e)
    {
        string exhibitId = e.msg;
        if (!CommandCenter.museumObjects.TryGetValue(exhibitId, out MuseumObjectRep m)) return;

        Vector3 cameraHeading = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1));
        Vector3 newPosition = m.GO.transform.position - cameraHeading * 0.3f;

        StartCoroutine(InterpolateCamera(cameraParent.transform.position, newPosition, 60));
    }

    private void UpdateScreenText(string path, string text)
    {
        screens.transform.Find(path).gameObject.GetComponent<TextMeshProUGUI>().text = text;
    }
    private IEnumerator Alert(string msg, float t)
    {
        ToggleScreenElementVisible("Alert");
        UpdateScreenText("Alert/Text", msg);
        yield return new WaitForSeconds(t);
        ToggleScreenElementVisible("Alert", false);
    }
    private void ToggleScreenElementVisible(string path, bool visible = true)
    {
        screens.transform.Find(path).gameObject.SetActive(visible);
    }

    public void Bookmark(bool add)
    {
        if (add)
        {
            CommandCenter.museumThreads["My Bookmarks"].Add(inquiredMuseumObject);
        } else
        {
            CommandCenter.museumThreads["My Bookmarks"].Remove(inquiredMuseumObject);
            FilterMuseumObjects(topicDropdown.captionText.text);
        }
    }
    public void ShowDescription(bool visible)
    {
        if (visible) UpdateScreenText("Desc/Text", inquiredMuseumObject.desc);
        ToggleScreenElementVisible("Desc", visible);
    }
    public void ShowMovementControl(bool visible)
    {
        ToggleScreenElementVisible("Controls/Simulation", visible);
    }
    public void UpdateMuseumObjectFilter(int o)
    {
        string thread = topicDropdown.options[o].text;
        if (CommandCenter.museumThreads[thread] != null) FilterMuseumObjects(thread);
    }
    public void UpdateFloorNav(int o)
    {
        string thread = topicDropdown.captionText.text;
        if (CommandCenter.museumThreads.TryGetValue(thread, out List<MuseumObjectRep> t) && t.Count > 0)
        {
            MuseumObjectRep m = CommandCenter.museumThreads[thread][o];
            MuseumObjectRep r = CommandCenter.museumObjects[m.room];

            floorNav.GetComponent<FloorNavController>().m = m;
            floorNav.GetComponent<FloorNavController>().r = r;
        } else
        {
            floorNav.GetComponent<FloorNavController>().m = null;
            floorNav.GetComponent<FloorNavController>().r = null;
        }
    }

    private IEnumerator DemonstrationInitialScreen()
    {
        UpdateScreenText("Status/Text", "Acquiring location...");
        ToggleScreenElementVisible("Topic", false);
        ToggleScreenElementVisible("Exhibit", false);
        UpdateScreenText("Alert/Text", "Please slowly move your phone around...");
        ToggleScreenElementVisible("Controls/Options", false);
        RelocateCamera(defaultCamLocation.transform.position);

        yield return new WaitForSeconds(5);

        museumEnvironment.SetActive(true);
        ToggleScreenElementVisible("Topic");
        ToggleScreenElementVisible("Exhibit");
        ToggleScreenElementVisible("Controls/Options");
        UpdateScreenText("Alert/Text", "Location acquired");

        FilterMuseumObjects("Museum Navigation");
        topicDropdown.RefreshShownValue();
        yield return new WaitForSeconds(3);

        floorNav.SetActive(true);
        ToggleScreenElementVisible("Alert", false);
    }

    private string EnumerateTransform(Transform t, string basePath, int currentLevel = 0, int maxLevel = 1000)
    {
        if (currentLevel >= maxLevel) return "";
        string output = "";
        foreach (Transform c in t)
        {
            string newLn = c.gameObject.activeSelf + ", " + basePath + c.name + "/";
            output += newLn + "\n";
            output += EnumerateTransform(c, newLn, currentLevel + 1, maxLevel);
        }
        return output;
    }
    public static void ScreenDebug(GameObject o, string msg)
    {
        debugOutStatic.GetComponent<TextMeshProUGUI>().text = Mathf.Round(Time.time * 1000) / 1000 + (o != null ? " " + o.name + " " : "") + "\n";
        debugOutStatic.GetComponent<TextMeshProUGUI>().text += msg + "\n";
    }

}
