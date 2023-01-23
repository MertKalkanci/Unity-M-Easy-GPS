using System.Collections;
using UnityEngine.Android;
using UnityEngine;
public enum DebugMode
{
    Editor,
    Info,
    None
}

namespace MEasyGPS.Management
{
    
    public class Initialisaton : MonoBehaviour
    {
        public bool didFail { get; private set; }

        [Tooltip("Maximum wait time to wait gps service initialisation (seconds)")] [Range(1, 10)] public float maxWaitTime = 5f;
        [Tooltip("Maximum wait time to retry to initialise gps service (seconds)")]  [SerializeField] [Range(5, 15)] private float retryTime = 10f;

        private float _maxWaitTime, _retryTime;

        private void Awake()
        {
            if(retryTime < maxWaitTime) //to prevent overloading
            {
                maxWaitTime = 5;
                retryTime = 10;
            }
            _maxWaitTime = maxWaitTime;
            _retryTime = retryTime;
        }

        public void TryToStartGPSService()
        {
            StartCoroutine(StartGPSService());
            return;
        }

        private IEnumerator StartGPSService()
        {
            didFail = false;

#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Permission.RequestUserPermission(Permission.FineLocation);
            }
#endif

            if (!Input.location.isEnabledByUser) // GPS Not Active
            {
                Debug.Log("GPS SERVICE NOT ACTIVE!");
                didFail = true;
                yield break;
            }

            if(Input.location.status == LocationServiceStatus.Running)
            {
                didFail = false;
                yield break;
            }


            //start location service before querying location
            Input.location.Start(6f,6f);

            Input.compass.enabled = true;

            _maxWaitTime = maxWaitTime; // set max time

            while(Input.location.status == LocationServiceStatus.Initializing && _maxWaitTime < 0) // wait the service till the max wait time ends
            {
                yield return new WaitForSeconds(1);
                _maxWaitTime--;
            }

            if(_maxWaitTime < 0)
            {
                Debug.Log("GPS SERVICE DIDN'T INITALIZED IN TIME!");
                didFail = true;
                yield break;
            }
            else if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.Log("GPS SERVICE INITALIZE FAILED!");
                didFail= true;
                yield break;
            }
            else
            {
                didFail = false;
                yield break;
            }

        }
        private void Update()
        {
            if(Input.location.status == LocationServiceStatus.Initializing || Input.location.status == LocationServiceStatus.Running)
            {
                didFail = false;
            }
            else if (didFail)
            {
                _retryTime -= Time.deltaTime;

                if(_retryTime < 0)
                {
                    _retryTime = retryTime;
                    TryToStartGPSService();
                }
            }
        }
    }
}
