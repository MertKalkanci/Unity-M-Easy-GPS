using MEasyGPS.Management;
using UnityEngine;
using System.Collections;

namespace MEasyGPS.Utils
{
    public class GPSObjectPlace : MonoBehaviour
    {
        private SceneGPSManager manager;
        private GameObject instance;

        public GameObject GPSObject;
        [Space]
        public float latitude; // write like this to preventing the space between this two
        public float longtitude; 
        [Space]
        [Tooltip("Instance object manually")] public bool instanceManually = false;
        [Tooltip("Update location of instance")] public bool updateLocationAfterInstance = false;
        [Space]
        [Tooltip("Maximum wait time to place instance (seconds)")] public float maxWaitTimeForInitialisation = 20f;
        [Space]
        [Tooltip("Don't change this value if you arent in a differnt planet or working on some experimental features")]  [SerializeField] private double latitudeMeterConstant = 111320;
        [Space]
        [SerializeField] private DebugMode debugMode = DebugMode.None;
        [SerializeField] private string debugTextTag = "DebugInfoTextMiscellaneous";
        private TMPro.TMP_Text debugText;

        private void Awake()
        {
            if (debugMode != DebugMode.None)
                debugText = GameObject.FindGameObjectWithTag(debugTextTag).GetComponent<TMPro.TMP_Text>();

            if(!instanceManually)
                Instance();
        }

        public void Instance()
        {
            if (instance)
                return;
            StartCoroutine(InstanceWork());
        }
        private IEnumerator InstanceWork()
        {
            float waitTime = maxWaitTimeForInitialisation;
            bool tryAgain = false, failed = false,isLocationServiceRunning = false;


            #region Check Availability

            while (waitTime > 0) //try to find and check manager in the wait time
            {
                tryAgain = false;
                isLocationServiceRunning = Input.location.status == LocationServiceStatus.Running;

                try
                {
                    manager = FindObjectOfType<SceneGPSManager>();
                }
                catch
                {
                    Debug.Log("Failed to Find SceneGPSManager");

                    if (debugMode != DebugMode.None)
                        debugText.text = "Failed to Find SceneGPSManager";

                    tryAgain = true;
                }

                if (!GPSObject && !tryAgain)
                {
                    Debug.Log("Failed to Find GPSObject");

                    if (debugMode != DebugMode.None)
                        debugText.text = "Failed to Find GPSObject";

                    tryAgain = true;
                }

                
                if (manager && !tryAgain)
                {
                    tryAgain = !isLocationServiceRunning;

                    if (debugMode != DebugMode.None)
                        debugText.text = "IS GPS MANAGER WORKING: " + isLocationServiceRunning;

                    if (tryAgain)
                        Debug.Log("GPS Manager Found But Not Working");
                }
#if UNITY_EDITOR // to test object placement
                tryAgain = false;
                break;
#endif


                if (tryAgain)
                {
                    yield return new WaitForSeconds(1);
                    waitTime--;
                }
                else
                {
                    break;
                }
            }

            failed = tryAgain;



            if (failed)
            {
                Debug.Log("Failed To Instatiate Object");

                if (debugMode != DebugMode.None)
                {
                    debugText.text = "Failed To Instatiate Object";
                }

                yield break;
            }

#endregion

            float diffLatMet, diffLonMet;

            this.DiffMeters(manager.latitude, manager.longtitude, latitude, longtitude, out diffLatMet, out diffLonMet);

            instance = Instantiate(GPSObject, RotateVectorForCordinateSystem(diffLonMet, diffLatMet, manager.trueHeading, Camera.main.transform), Quaternion.identity, this.gameObject.transform);
        }

        private void Update()
        {
            if(instance && updateLocationAfterInstance)
            {
                float diffLatMet = 0f, diffLonMet;

                this.DiffMeters(manager.latitude, manager.longtitude, latitude, longtitude, out diffLatMet, out diffLonMet);

                //in start Ar Session rotates the game world towards the angle player looks to solve it we must rotate the object realtive to the session origin

                

                instance.transform.position = RotateVectorForCordinateSystem(diffLonMet, diffLatMet, manager.trueHeading, Camera.main.transform);

                if (debugMode != DebugMode.None)
                {
                    debugText.text = "CameraPos:\n" + Camera.main.transform.position + "\n" + "TargetPos:\n" + instance.transform.position;
                }
            }
        }

        private Vector3 RotateVectorForCordinateSystem(float diffLonMet,float diffLatMet, float trueHeading, Transform CameraTransform) 
        {
            //in start Ar Session rotates the game world towards the angle player looks to solve it we must rotate the object realtive to the session origin

            float diffrenceToRotateDegrees = trueHeading - CameraTransform.rotation.eulerAngles.y;
            float diffrenceToRotateRadians = Mathf.Deg2Rad * diffrenceToRotateDegrees;

            Vector3 rawPosition = new Vector3(diffLonMet, 0, diffLatMet);
            Vector3 cameraPosition = CameraTransform.position;

            Vector3 rotatedPosition = 
                new Vector3
            (Mathf.Cos(diffrenceToRotateRadians) * rawPosition.x - Mathf.Sin(diffrenceToRotateRadians) * rawPosition.z,
            0,
            Mathf.Sin(diffrenceToRotateRadians) * rawPosition.x + Mathf.Cos(diffrenceToRotateRadians) * rawPosition.z)
            +
            cameraPosition;

            return rotatedPosition;
        }

        private void DiffMeters(double originLat, double originLon, double TargetLat, double TargetLon, out float diffLatMet, out float diffLonMet) 
        {
            double oLatMet, oLonMet, tLatMet, tLonMet;

            oLatMet = originLat * latitudeMeterConstant;
            oLonMet = originLon * 40075000 * Mathf.Cos(Mathf.Deg2Rad * (float)originLat) / 360;
            tLatMet = TargetLat * latitudeMeterConstant;
            tLonMet = TargetLon * 40075000 * Mathf.Cos(Mathf.Deg2Rad * (float)TargetLat) / 360;

            diffLatMet = (float)(tLatMet - oLatMet);
            diffLonMet = (float)(tLonMet - oLonMet);
        } // also have this function in ARMapTest to make scripts independent

        
    }
}