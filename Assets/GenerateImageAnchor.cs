using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.iOS;

public class GenerateImageAnchor : MonoBehaviour
{


    [SerializeField]
    private ARReferenceImagesSet referenceImagesSet;

    [SerializeField]
    private GameObject prefabToGenerate;

    [Tooltip("Tick this if you want your object to keep matching its position to the image position.")]
    public bool bindObjectToImage = true;

    // Use this for initialization
    void Start()
    {
        UnityARSessionNativeInterface.ARImageAnchorAddedEvent += AddImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent += UpdateImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorRemovedEvent += RemoveImageAnchor;

    }

    void AddImageAnchor(ARImageAnchor arImageAnchor)
    {
        Debug.LogFormat("image anchor added[{0}] : tracked => {1}", arImageAnchor.identifier, arImageAnchor.isTracked);
        foreach (ARReferenceImage referenceImage in referenceImagesSet.referenceImages)
        {
            if (arImageAnchor.referenceImageName == referenceImage.imageName)
            {
                if (MuseumLocationService.imageAnchors.TryGetValue(referenceImage.imageName, out GameObject GO)) continue;

                // ---------------------------------------------------------------
                // THIS PART IS WHERE CODE GETS EXECUTED WHEN AN IMAGE IS DETECTED
                // ---------------------------------------------------------------
                Vector3 position = UnityARMatrixOps.GetPosition(arImageAnchor.transform);
                Quaternion rotation = UnityARMatrixOps.GetRotation(arImageAnchor.transform);

                GO = Instantiate<GameObject>(prefabToGenerate, position, rotation);
                GO.name = prefabToGenerate.name + "-" + referenceImage.imageName;
                GO.transform.parent = transform;
                MuseumLocationService.imageAnchors[referenceImage.imageName] = GO;
            }
        }
    }

    void UpdateImageAnchor(ARImageAnchor arImageAnchor)
    {
        //Debug.LogFormat("image anchor updated[{0}] : tracked => {1}", arImageAnchor.identifier, arImageAnchor.isTracked);
        if (bindObjectToImage && MuseumLocationService.imageAnchors.TryGetValue(arImageAnchor.referenceImageName, out GameObject GO))
        {
            GO = MuseumLocationService.imageAnchors[arImageAnchor.referenceImageName];
            if (arImageAnchor.isTracked)
            {
                if (!GO.activeSelf)
                {
                    GO.SetActive(true);
                }

                if (bindObjectToImage)
                {
                    GO.transform.position = UnityARMatrixOps.GetPosition(arImageAnchor.transform);
                    GO.transform.rotation = UnityARMatrixOps.GetRotation(arImageAnchor.transform);
                }
            }
            else if (GO)
            {
                GO.SetActive(false);
            }
        }
    }

    void RemoveImageAnchor(ARImageAnchor arImageAnchor)
    {
        Debug.LogFormat("image anchor removed[{0}] : tracked => {1}", arImageAnchor.identifier, arImageAnchor.isTracked);
        if (MuseumLocationService.imageAnchors.TryGetValue(arImageAnchor.referenceImageName, out GameObject GO))
        {
            Destroy(GO);
        }

    }

    void OnDestroy()
    {
        UnityARSessionNativeInterface.ARImageAnchorAddedEvent -= AddImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent -= UpdateImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorRemovedEvent -= RemoveImageAnchor;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
