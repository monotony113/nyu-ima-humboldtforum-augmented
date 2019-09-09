using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MuseumObjectController : MonoBehaviour
{
    public static GameObject museumExhibitPrefab;

    private GameObject museumObjDisplay;

    private GameObject indicator;
    private GameObject indicatorAnchor;
    private GameObject indicatorPin;
    private GameObject indicatorArrow;
    private GameObject indicatorSelection;
    private int indicatorNeighborCount;

    private GameObject sign;
    private GameObject signPlate;

    internal Color accentColor;
    internal Color textColor;

    internal MuseumObjectRep metadata;

    // Start is called before the first frame update
    void Start()
    {
        museumExhibitPrefab = Resources.Load<GameObject>("Prefabs/MuseumExhibit");
        museumObjDisplay = Instantiate(museumExhibitPrefab);
        museumObjDisplay.name = "MuseumObject";
        museumObjDisplay.transform.SetParent(transform);
        museumObjDisplay.transform.localPosition = Vector3.zero;

        indicator = museumObjDisplay.transform.Find("Indicator").gameObject;
        indicatorAnchor = indicator.transform.Find("ScreenAnchor").gameObject;
        indicatorPin = indicator.transform.Find("Pin").gameObject;
        indicatorArrow = indicator.transform.Find("Pin/Arrow").gameObject;

        sign = indicator.transform.Find("Sign").gameObject;
        signPlate = sign.transform.Find("Plate").gameObject;
        TextOf(sign).text = metadata.name;

        sign.transform.localPosition = Vector3.zero;

        accentColor = new Color32((byte)metadata.curatorial["colors"][0][0].AsInt, (byte)metadata.curatorial["colors"][0][1].AsInt, (byte)metadata.curatorial["colors"][0][2].AsInt, 255);
        textColor = accentColor.r * .2126 + accentColor.g * .7152 + accentColor.b * .0722 > 0.75 ? Color.black : Color.white;
        ChangeColor(indicatorPin, accentColor);
        ChangeColor(signPlate, accentColor);
        foreach (Transform t in indicatorArrow.transform) ChangeColor(t.gameObject, accentColor);
        TextOf(sign).color = textColor;

        MakeVisible(false);
    }

    private void FixedUpdate()
    {
        if (ARController.cameraMoving || (transform.position - Camera.main.transform.position).sqrMagnitude < 144)
        {
            indicatorPin.GetComponent<Rigidbody>().position = indicator.transform.position;
            signPlate.GetComponent<Rigidbody>().position = indicator.transform.position;

            indicatorPin.GetComponent<Rigidbody>().isKinematic = true;
            signPlate.GetComponent<Rigidbody>().isKinematic = true;
        }
        else if (Physics.Raycast(Camera.main.transform.position, (transform.position - Camera.main.transform.position).normalized, out RaycastHit hitInfo, Mathf.Infinity, 1 << 12))
        {
            indicatorPin.GetComponent<Rigidbody>().isKinematic = false;
            signPlate.GetComponent<Rigidbody>().isKinematic = false;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (ARController.cameraMoving) return;

        if (!metadata.included)
        {
            MakeVisible(false);
            return;
        }

        if ((transform.position - Camera.main.transform.position).sqrMagnitude < 144)
        {
            indicator.transform.localPosition = Vector3.zero;
            indicatorArrow.SetActive(metadata.sameRoomAsCamera);

            indicatorPin.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f) * Vector3.Distance(indicatorPin.transform.position, Camera.main.transform.position) / 10;
            sign.transform.localScale = new Vector3(6, 6, 1) * Vector3.Distance(indicatorPin.transform.position, Camera.main.transform.position) / 10;
        }
        else if (Physics.Raycast(Camera.main.transform.position, (transform.position - Camera.main.transform.position).normalized, out RaycastHit hitInfo, Mathf.Infinity, 1 << 12))
        {
            indicator.transform.position = hitInfo.point;
            indicatorArrow.SetActive(false);

            indicatorPin.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            sign.transform.localScale = new Vector3(6, 6, 1);

            Debug.DrawLine(Camera.main.transform.position, hitInfo.point, new Color(1, 0, 0, 1));
            Debug.DrawLine(indicatorAnchor.transform.position, indicatorPin.transform.position, new Color(1, 1, 0, 1));
            Debug.DrawLine(Camera.main.transform.position, transform.position, new Color(1, 0, 0, 0.5f));
        }
        else
        {
            MakeVisible(false);
            return;
        }

        indicatorPin.transform.rotation = LeveledCameraRotation();
        sign.transform.rotation = LeveledCameraRotation();
        signPlate.transform.localScale = new Vector3(TextOf(sign).GetRenderedValues().x / 85, TextOf(sign).GetRenderedValues().y / 95, 0.02f);

        //indicatorNeighborCount = signPlate.GetComponent<OverlappingDetection>().overlappingCount;
        //if (aimedAt)
        //{
        //    ToggleSign();
        //} else if (indicatorNeighborCount == 0)
        //{
        //    StartCoroutine(ShowSignOnStabilize());
        //} else
        //{
        //    ToggleSign(false);
        //}

        if (metadata.sameRoomAsCamera)
        {
            ChangeTransparency(signPlate, 1f);
        } else
        {
            ChangeTransparency(signPlate, 0.6f);
        }
        MakeVisible(true);

    }

    private void ChangeColor(GameObject GO, Color col)
    {
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        Renderer r = GO.GetComponent<Renderer>();
        r.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", col);
        r.SetPropertyBlock(propBlock);
    }
    private void ChangeTransparency(GameObject GO, float t)
    {
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        Renderer r = GO.GetComponent<Renderer>();
        r.GetPropertyBlock(propBlock);
        Color col = propBlock.GetColor("_Color");
        col.a = t;
        propBlock.SetColor("_Color", col);
        r.SetPropertyBlock(propBlock);
    }
    private TextMeshPro TextOf(GameObject GO)
    {
        return GO.transform.Find("Text").gameObject.GetComponent<TextMeshPro>();
    }
    private void MakeVisible(bool visible)
    {
        sign.SetActive(visible);
        indicator.SetActive(visible);
    }
    private Quaternion LeveledCameraRotation()
    {
        return Quaternion.Euler(Camera.main.transform.rotation.eulerAngles.x, Camera.main.transform.rotation.eulerAngles.y, 0);
    }
    private IEnumerator ShowSignOnStabilize()
    {
        for (int i = 0; i < 60; i++) 
        {
            if (metadata.aimedAt)
            {
                ToggleSign();
                yield break;
            }
            if (indicatorNeighborCount > 0)
            {
                ToggleSign(false);
                yield break;
            }
            yield return null;
        }
        ToggleSign();
    }
    internal void ToggleSign(bool visible = true)
    {
        foreach (Renderer r in sign.GetComponentsInChildren<Renderer>()) { r.enabled = visible; }
        indicatorPin.GetComponent<Renderer>().enabled = !visible;
    }

}
