using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class MainController : MonoBehaviour {

    public Camera firstpersonCamera;
    public GameObject detectedPlane;
    public List<GameObject> modelsPrefabs = new List<GameObject>();
    public GameObject searchingForPlaneUI;

    // A list to hold all planes ARCore is tracking in the current frame. This object is used across
    // the application to avoid per-frame allocations.
    private List<DetectedPlane> allPlanes = new List<DetectedPlane>();

    // True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
    private bool isQuitting = false;

    private void Update() {
        UpdateApplicationLifecycle();

        Session.GetTrackables<DetectedPlane>(allPlanes);
        bool showSearchingUI = true;
        for (int i = 0; i < allPlanes.Count; i++) {
            if (allPlanes[i].TrackingState == TrackingState.Tracking) {
                showSearchingUI = false;
                break;
            }
        }

        searchingForPlaneUI.SetActive(showSearchingUI);

        // If the player has not touched the screen, we are done with this update.
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began) {
            return;
        }

        // Raycast against the location the player touched to search for planes.
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinBounds | TrackableHitFlags.FeaturePointWithSurfaceNormal;

        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit)) {
            // Use hit pose and camera pose to check if hittest is from the
            // back of the plane, if it is, no need to create the anchor.
            if ((hit.Trackable is DetectedPlane) && Vector3.Dot(firstpersonCamera.transform.position
                - hit.Pose.position, hit.Pose.rotation * Vector3.up) < 0) {

                Debug.Log("Hit at back of the current DetectedPlane");
            }
            else {
                int randomObj = Random.Range(0, modelsPrefabs.Count);
                // Instantiate Andy model at the hit pose.
                var arObject = Instantiate(modelsPrefabs[randomObj], hit.Pose.position, hit.Pose.rotation);

                // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                // world evolves.
                var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                // Make Andy model a child of the anchor.
                arObject.transform.parent = anchor.transform;
            }
        }
    }

    private void UpdateApplicationLifecycle() {
        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking) {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        }
        else {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (isQuitting) {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted) {
            ShowAndroidToastMessage("Camera permission is needed to run this application.");
            isQuitting = true;
            Invoke("DoQuit", 0.5f);
        }
        else if (Session.Status.IsError()) {
            ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            isQuitting = true;
            Invoke("Quit", 0.5f);
        }
    }

    private void Quit() {
        Application.Quit();
    }

    private void ShowAndroidToastMessage(string message) {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null) {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
