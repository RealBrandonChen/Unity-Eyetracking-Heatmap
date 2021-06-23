using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ObjBehaviour;
using System.IO;
using Proyecto26;
using System;
using Firebase;
using Firebase.Database;

namespace PupilLabs
{   // Create the class for recording the gazepoint in 2D plane.
    [Serializable]
    public class GazePoint2D
    {
        public float GazePoint2DX;
        public float GazePoint2DY;
        public float GazePoint2DZ;

        public GazePoint2D(float gazePoint2DX, float gazePoint2DY, float gazePoint2DZ)
        {
            this.GazePoint2DX = gazePoint2DX;
            this.GazePoint2DY = gazePoint2DY;
            this.GazePoint2DZ = gazePoint2DZ;
        }
    }

    public class GazeVisualizer : MonoBehaviour
    {
        public Transform gazeOrigin;
        public GazeController gazeController;

        [Header("HeatMapOutputFilePath")]
        public string FilePath;

        [Header("Settings")]
        [Range(0f, 1f)]
        public float confidenceThreshold = 0.6f;
        public bool binocularOnly = true;

        [Header("Projected Visualization")]
        public Transform projectionMarker;
        public Transform gazeDirectionMarker;
        [Range(0.01f, 0.1f)]
        public float sphereCastRadius = 0.1f;
        public bool isMarkerProjected = false;

        private int cubeLayerMask;
        private int heatMapLayerMask;

        Vector3 localGazeDirection;
        float gazeDistance;
        bool isGazing = false;

        bool errorAngleBasedMarkerRadius = true;
        float angleErrorEstimate = 2f;

        Vector3 origMarkerScale;
        MeshRenderer targetRenderer;
        GameObject[] allCubes;
        float minAlpha = 0.2f;
        float maxAlpha = 0.8f;

        float lastConfidence;

        private int fileUploadTime = 0;
        private const string projectId = "unitytesting-5ab13-default-rtdb"; // Can find this in the Firebase project settings
        private static readonly string databaseURL = $"https://unitytesting-5ab13-default-rtdb.asia-southeast1.firebasedatabase.app/";


        private void Start()
        {
            allCubes = GameObject.FindGameObjectsWithTag("Cube");
            
        }

        void OnEnable()
        {
            bool allReferencesValid = true;
            if (projectionMarker == null)
            {
                Debug.LogError("ProjectionMarker reference missing!");
                allReferencesValid = false;
            }
            if (gazeDirectionMarker == null)
            {
                Debug.LogError("GazeDirectionMarker reference missing!");
                allReferencesValid = false;
            }
            if (gazeOrigin == null)
            {
                Debug.LogError("GazeOrigin reference missing!");
                allReferencesValid = false;
            }
            if (gazeController == null)
            {
                Debug.LogError("GazeController reference missing!");
                allReferencesValid = false;
            }
            if (!allReferencesValid)
            {
                Debug.LogError("GazeVisualizer is missing required references to other components. Please connect the references, or the component won't work correctly.");
                enabled = false;
                return;
            }

            origMarkerScale = gazeDirectionMarker.localScale;
            targetRenderer = gazeDirectionMarker.GetComponent<MeshRenderer>();

            StartVisualizing();

            // Hitpoint Output
            TextWriter tw = new StreamWriter(FilePath, false);       //write the cube position into .CSV file
            tw.WriteLine("TimeStamp" + "," + "HitPointX" + "," + "HitPointY" + "," + "HitPointZ");
            tw.Close();

        }

        void OnDisable()
        {
            if (gazeDirectionMarker != null)
            {
                gazeDirectionMarker.localScale = origMarkerScale;
            }

            StopVisualizing();
        }

        void Update()
        {

            if (!isGazing)
            {
                return;
            }

            VisualizeConfidence();

            ShowProjected();
            if (RecordingController.IsRecording)
            {
                HeatMapData();
            }
            
        }

        public void StartVisualizing()
        {
            if (!enabled)
            {
                Debug.LogWarning("Component not enabled.");
                return;
            }

            if (isGazing)
            {
                Debug.Log("Already gazing!");
                return;
            }

            Debug.Log("Start Visualizing Gaze");

            gazeController.OnReceive3dGaze += ReceiveGaze;

            projectionMarker.gameObject.SetActive(isMarkerProjected);
            gazeDirectionMarker.gameObject.SetActive(isMarkerProjected);
            isGazing = true;
        }

        public void StopVisualizing()
        {
            if (!isGazing || !enabled)
            {
                Debug.Log("Nothing to stop.");
                return;
            }

            if (projectionMarker != null)
            {
                projectionMarker.gameObject.SetActive(false);
            }
            if (gazeDirectionMarker != null)
            {
                gazeDirectionMarker.gameObject.SetActive(false);
            }

            isGazing = false;

            gazeController.OnReceive3dGaze -= ReceiveGaze;
        }

        void ReceiveGaze(GazeData gazeData)
        {
            if (binocularOnly && gazeData.MappingContext != GazeData.GazeMappingContext.Binocular)
            {
                return;
            }

            lastConfidence = gazeData.Confidence;

            if (gazeData.Confidence < confidenceThreshold)
            {
                return;
            }

            localGazeDirection = gazeData.GazeDirection;    //  *Gaze direction corresponding to the 3d gaze point. Normalized vector in local camera space.
            gazeDistance = gazeData.GazeDistance;   //  *Distance in meters between VR camera and the 3d gaze point.
        }

        void VisualizeConfidence()
        {
            if (targetRenderer != null)
            {
                Color c = targetRenderer.material.color;
                c.a = MapConfidence(lastConfidence);
                targetRenderer.material.color = c;
            }
        }

        void ShowProjected()
        {
            gazeDirectionMarker.localScale = origMarkerScale;

            Vector3 origin = gazeOrigin.position;
            Vector3 direction = gazeOrigin.TransformDirection(localGazeDirection);


            cubeLayerMask = (1 << 8) | (1 << 9) | (1 << 10) | (1 << 11) | (1 << 12) | (1 << 13)| (1 << 14) | (1 << 15) | (1 << 16) | (1 << 17);
            if (Physics.SphereCast(origin, sphereCastRadius, direction, out RaycastHit hit, Mathf.Infinity, cubeLayerMask))
            {
                Debug.DrawRay(origin, direction * hit.distance, Color.yellow);

                projectionMarker.position = hit.point;

                gazeDirectionMarker.position = origin + direction * hit.distance;
                gazeDirectionMarker.LookAt(origin);

                if (errorAngleBasedMarkerRadius)
                {
                    gazeDirectionMarker.localScale = GetErrorAngleBasedScale(origMarkerScale, hit.distance, angleErrorEstimate);
                }
                // Enable the script <TrackingTarget> when gazing the target cubes
                TrackingTarget scriptTarget = hit.collider.gameObject.GetComponent<TrackingTarget>();
                scriptTarget.enabled = true;
            }
            else
            {
                Debug.DrawRay(origin, direction * 10, Color.white);
                
                foreach (GameObject cube in allCubes)
                {
                    TrackingTarget scriptTarget = cube.GetComponent<TrackingTarget>();
                    scriptTarget.enabled = false;
                }
            }
        }

        // Record hitpoint of the raycast into .CSV for heatmap data
        void HeatMapData()
        { 
            Vector3 origin = gazeOrigin.position;
            Vector3 direction = gazeOrigin.TransformDirection(localGazeDirection);
            Vector3 hitPoint;
            heatMapLayerMask = (1 << 22);
            if (Physics.SphereCast(origin, sphereCastRadius, direction, out RaycastHit hit, Mathf.Infinity, heatMapLayerMask))
            {
                hitPoint = origin + direction * hit.distance;
                hitPoint = gazeOrigin.InverseTransformPoint(hitPoint);
               
                CSVWriter(hitPoint.x, hitPoint.y, hitPoint.z);

                GazePoint2D gazePoint2D = new GazePoint2D(hitPoint.x, hitPoint.y, hitPoint.z);
                if ((int)Time.realtimeSinceStartup != fileUploadTime)
                {
                    FireBaseUpload(gazePoint2D, ((int)Time.realtimeSinceStartup).ToString());
                    fileUploadTime = (int)Time.realtimeSinceStartup;
                }
                
            }
        }

        public static void FireBaseUpload(GazePoint2D gazePoint2D, string timeStamp)
        {
            
            RestClient.Put($"{databaseURL}GazePoints/{timeStamp}.json", gazePoint2D).Then(response => {
                Debug.Log("The user was successfully uploaded to the database");
            });
            
        }

        void CSVWriter(float x, float y, float z)
        {
            TextWriter tw = new StreamWriter(FilePath, true);
            tw.WriteLine(Time.realtimeSinceStartup + "," + x + "," + y + "," + z);
            tw.Close();
        }


        Vector3 GetErrorAngleBasedScale(Vector3 origScale, float distance, float errorAngle)
        {
            Vector3 scale = origScale;
            float scaleXY = distance * Mathf.Tan(Mathf.Deg2Rad * angleErrorEstimate) * 2;
            scale.x = scaleXY;
            scale.y = scaleXY;
            return scale;
        }

        float MapConfidence(float confidence)
        {
            return Mathf.Lerp(minAlpha, maxAlpha, confidence);
        }
    }
}