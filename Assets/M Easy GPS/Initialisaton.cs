using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MEasyGPS.Management
{
    
    public class Initialisaton : MonoBehaviour
    {
        public float maxWaitTime = 20;
        private float _maxWaitTime;
        protected bool didFail;

        public bool TryToStartGPSService()
        {
            StartCoroutine(StartGPSService());
            return didFail;
        }

        private IEnumerator StartGPSService()
        {
#if UNITY_ANDROID
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.CoarseLocation))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.CoarseLocation);
                StartCoroutine(StartGPSService());
                yield break;
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

            Input.compass.enabled = true;

            //start location service before querying location
            Input.location.Start();

            _maxWaitTime = maxWaitTime; // set max time

            while(Input.location.status == LocationServiceStatus.Initializing && _maxWaitTime < 0) // wait the service till the max wait time ends
            {
                yield return new WaitForSeconds(1);
                _maxWaitTime--;
            }

            if(_maxWaitTime <= 0)
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
    }
}
