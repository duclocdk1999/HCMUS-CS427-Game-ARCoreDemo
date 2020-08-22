using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GoogleARCore;

#if UNITY_EDITOR
    using input = GoogleARCore.InstantPreviewInput;
#endif

public class ARController : MonoBehaviour
{
    // We will fill the list with the planes that ARCore detected in the current frame
    private readonly List<DetectedPlane> m_newTrackedPlanes = new List<DetectedPlane>();
    public GameObject gridPrefab;
    public GameObject portal;
    public GameObject arCamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Check ARCore session status
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        // The following function will fill m_newTrackedPlanes with the planes that ARCore detected
        // in the current frame
        Session.GetTrackables<DetectedPlane>(m_newTrackedPlanes, TrackableQueryFilter.New);

        // Instantiate a Grid for each Tracked Plane in m_newTrackedPlanes
        for (int i = 0; i < m_newTrackedPlanes.Count; i++)
        {
            GameObject grid = Instantiate(gridPrefab, Vector3.zero, Quaternion.identity, transform);

            // This function will set the position of grid and modify the vertices of the attached mesh
            grid.GetComponent<GridVisualizer>().Initialize(m_newTrackedPlanes[i]);
        }

        // Check if the user touch the screen
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began) {
            return;
        }

        // Check if the user touched any of the tracked planes
        DetectedPlane hit;
        if (Frame.Raycast(touch.position.x, touch.position.y, TrackableHitFlags.PlaneWithinPolygon, out hit))
        {
            // Let now place the portal on top of the tracked plane that we touched

            // Enable the portal
            portal.SetActive(true);

            // Create a new Anchor
            Anchor anchor = hit.CreateAnchor(hit.CenterPose);

            // Set the position of the portal to be the same as the hit position
            portal.transform.position = hit.CenterPose.position;
            portal.transform.rotation = hit.CenterPose.rotation;

            // We want the portal to face the camera
            Vector3 cameraPosition = arCamera.transform.position;

            // The portal should only rotate around the y-axis
            cameraPosition.y = hit.CenterPose.position.y;

            // Rotate the portal to face the camera
            portal.transform.LookAt(cameraPosition, portal.transform.up);

            // ARCore will keep understanding the world and update the anchors accordingly
            // hence we need to attach our portal to the anchor
            portal.transform.parent = anchor.transform;
        }
    }
}
