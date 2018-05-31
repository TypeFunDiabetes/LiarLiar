using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PolyToolkit;
using GoogleARCore;
namespace GoogleARCore.Examples.Common {
    public class PolyImageManager : MonoBehaviour {
        public Text _statusText;
        public Text _attributionsText;
        public GameObject FPCAMERA;

        public bool didFirstFrame;

        public static bool didCompleteRequest;
        public static PolyStatusOr<PolyListAssetsResult> results;

        void CheckTimeout() {
            if (didCompleteRequest)
                return;

            Debug.Log("PolyImageManager>10 second request TIMED OUT. Requesting again..");

            CancelInvoke("CheckTimeout");
            Invoke("CheckTimeout", 5f);

            // repeat call to request featured assets
            StartDownload();
        }

        void Awake() {
            didFirstFrame = false;
        }

        void Update() {
            // we're doing this request on the first frame update to give PolyApi time to do its auth and setup (if required)..
            if (!didFirstFrame && PolyApi.IsInitialized) {
                // if we already downloaded the featured assets, we don't need to do that again..
                if (results != null) {
                    Debug.Log("PolyImageManager>Already got featured assets list, so skipping re-download of that.");
                    AlreadyGotAssetList();
                    didFirstFrame = true;
                    return;
                }

                // we're here because we need to download the assets list.. 
                StartDownload();
                didFirstFrame = true;
            }

            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began) {
                return;
            }

            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
            TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit)) {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(FPCAMERA.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0) {
                    Debug.Log("Hit at back of the current DetectedPlane");
                } else {
                    // Instantiate Andy model at the hit pose.
                    //var andyObject = Instantiate(AndyAndroidPrefab, hit.Pose.position, hit.Pose.rotation);

                    // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                    //andyObject.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                    // world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    GameObject.Find("PolyImport").transform.SetPositionAndRotation(hit.Pose.position, hit.Pose.rotation);

                    // Make Andy model a child of the anchor.
                    GameObject.Find("PolyImport").transform.parent = anchor.transform;
                    GameObject.Find("Plane Generator").GetComponent<DetectedPlaneGenerator>().placed = true;
                }
            }
        }

        void StartDownload() {
            Debug.Log("Getting featured assets...");
            if (_statusText != null)
                _statusText.text = "Requesting...";

            didCompleteRequest = false;

            PolyListAssetsRequest request = PolyListAssetsRequest.Featured();

            // Limit requested models to those of medium complexity or lower.
            request.maxComplexity = PolyMaxComplexityFilter.SIMPLE;
            PolyApi.ListAssets(request, ListAssetsCallback);

            Invoke("CheckTimeout", 10f);
        }

        // Callback invoked when the featured assets results are returned.
        private void ListAssetsCallback(PolyStatusOr<PolyListAssetsResult> result) {
            results = result;

            didCompleteRequest = true;

            if (!result.Ok) {
                Debug.LogError("Failed to get featured assets. :( Reason: " + result.Status);
                if (_statusText != null)
                    _statusText.text = "ERROR: " + result.Status;
                return;
            }
            Debug.Log("Successfully got featured assets!");

            if (_statusText == null)
                return;

            _statusText.text = "Importing...";

            // Set the import options.
            PolyImportOptions options = PolyImportOptions.Default();
            // We want to rescale the imported meshes to a specific size.
            options.rescalingMode = PolyImportOptions.RescalingMode.FIT;
            // The specific size we want assets rescaled to (fit in a 1x1x1 box):
            options.desiredSize = 5.0f;
            // We want the imported assets to be recentered such that their centroid coincides with the origin:
            options.recenter = true;

            int i = Random.Range(0, result.Value.assets.Count - 1);
            PolyApi.Import(result.Value.assets[i], options, ImportAssetCallback);

            // Now let's get the first 5 featured assets and put them on the scene.
            //List<PolyAsset> assetsInUse = new List<PolyAsset> ();
            //for ( int i = 0; i < Mathf.Min (5, result.Value.assets.Count); i++ )
            //{
            //	// Import this asset.
            //	PolyApi.Import (result.Value.assets[i], options, ImportAssetCallback);
            //	assetsInUse.Add (result.Value.assets[i]);
            //}

            // Show attributions for the asset we display.
            //_attributionsText.text = PolyApi.GenerateAttributions (includeStatic: true, runtimeAssets: assetsInUse);
        }

        // Callback invoked when an asset has just been imported.
        private void ImportAssetCallback(PolyAsset asset, PolyStatusOr<PolyImportResult> result) {
            if (!result.Ok) {
                Debug.LogError("Failed to import asset. :( Reason: " + result.Status);
                _statusText.text = "ERROR: Import failed: " + result.Status;
                return;
            }
            Debug.Log("Successfully imported asset!");

            // need to make sure that the scene hasn't been destroyed by the time this callback comes in, so we just check
            // the validity of the status text object and if it's null, drop out
            if (_statusText == null) {
                Debug.Log("PolyImageManager>Dropping out because scene appears to have been unloaded!!");
                return;
            }

            _statusText.text = "";

            // Show attribution (asset title and author).
            _attributionsText.text = asset.displayName + "\nby " + asset.authorName;

            // Here, you would place your object where you want it in your scene, and add any
            // behaviors to it as needed by your app. As an example, let's just make it
            // slowly rotate:
            result.Value.gameObject.AddComponent<Rotate>();

            // make the new object a child of this gameobject
            result.Value.gameObject.transform.parent = transform;
            result.Value.gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            result.Value.gameObject.transform.localRotation = FPCAMERA.transform.localRotation;
            result.Value.gameObject.transform.Translate(result.Value.gameObject.transform.forward * 10000f);
        }

        private void AlreadyGotAssetList() {
            if (_statusText == null)
                return;

            _statusText.text = "Importing...";

            // Set the import options.
            PolyImportOptions options = PolyImportOptions.Default();
            // We want to rescale the imported meshes to a specific size.
            options.rescalingMode = PolyImportOptions.RescalingMode.FIT;
            // The specific size we want assets rescaled to (fit in a 1x1x1 box):
            options.desiredSize = 5.0f;
            // We want the imported assets to be recentered such that their centroid coincides with the origin:
            options.recenter = true;

            int i = Random.Range(0, results.Value.assets.Count - 1);
            PolyApi.Import(results.Value.assets[i], options, ImportAssetCallback);
        }
    }
}
