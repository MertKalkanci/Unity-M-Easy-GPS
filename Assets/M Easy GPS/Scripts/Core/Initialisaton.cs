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
        [Tooltip("Maximum wait time to wait gps service initialisation (seconds)")]  public float maxWaitTime = 20;
        private float _maxWaitTime;
        public bool didFail { get; private set; }

        public void TryToStartGPSService()
        {
            StartCoroutine(StartGPSService());
            return;
        }

        private IEnumerator StartGPSService()
        {
            didFail = false;

            RequestPermissionAndroid();

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
                if (Input.location.isEnabledByUser)
                {
                    didFail = false;
                    TryToStartGPSService();
                }
                else
                {
                    RequestPermissionAndroid();
                }
            }
        }
        private void RequestPermissionAndroid()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.CoarseLocation))
            {
                Permission.RequestUserPermission(UnityEngine.Android.Permission.CoarseLocation);
            }
#endif
            return;
        }
    }
}
