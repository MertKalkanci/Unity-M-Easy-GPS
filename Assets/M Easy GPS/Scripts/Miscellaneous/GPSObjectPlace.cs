using MEasyGPS.Management;
using UnityEngine;
using System.Collections;

namespace MEasyGPS.Miscellaneous
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
        [Tooltip("Update Location")] public bool updateLocationAfterInstance = false;
        [Space]
        [Tooltip("Max Wait Time To Place Instance")] public float maxWaitTimeForInitialisation = 20f;
        [Space]
        [SerializeField] private double latitudeMeterConstant = 111320;


        private void Awake()
        {
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
            bool tryAgain = false, failed = false;


            while(waitTime > 0) //try to find and check manager in the wait time
            {
                tryAgain = false;
                try
                {
                    manager = FindObjectOfType<SceneGPSManager>();
                }
                catch
                {
                    Debug.Log("Failed to Find SceneGPSManager");
                    tryAgain = true;
                }

                if (manager && !tryAgain)
                {
                    tryAgain = !manager.IsWorking;

                    if(tryAgain)
                        Debug.Log("GPS Manager Found But Not Working");
                }

                if (!GPSObject && !tryAgain)
                {
                    Debug.Log("Failed to Find GPSObject");
                    tryAgain = true;
                }


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
                yield break;
            }


            float diffLatMet, diffLonMet;


            this.DiffMeters(manager.latitude, manager.longtitude, latitude, longtitude, out diffLatMet, out diffLonMet);

            instance = Instantiate(
                    GPSObject,
                    (new Vector3(diffLonMet, 0, diffLatMet) + Camera.main.transform.position)
                    , Quaternion.identity, this.gameObject.transform);
            
        }

        private void Update()
        {
            if(instance && updateLocationAfterInstance)
            {
                float diffLatMet = 0f, diffLonMet;

                this.DiffMeters(manager.latitude, manager.longtitude, latitude, longtitude, out diffLatMet, out diffLonMet);

                instance.transform.position = (new Vector3(diffLonMet, 0, diffLatMet) + Camera.main.transform.position);
            }
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