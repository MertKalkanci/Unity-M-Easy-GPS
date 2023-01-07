using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEasyGPS.Management;

namespace MEasyGPS.Management
{
    [RequireComponent(typeof(Initialisaton))]
    public class SceneGPSManager : MonoBehaviour
    {
        public float latitude, longtitude, altitude, horizontalAccuracy,
            magneticHeading, trueHeading, headingAccuracy;
        [Tooltip("Experimental location update ping")] public float locationPing;

        private Initialisaton init;
        private void Awake()
        {
            try
            {
                init = GetComponent<Initialisaton>();
            }
            catch
            {
                if (init == null)
                    init = gameObject.AddComponent<Initialisaton>();
            }

            if(!init.TryToStartGPSService())
            {
                Awake();
            }
        }

        private void Update()
        {
            if (init && !init.TryToStartGPSService())
                UpdateLocation();
        }
        private void UpdateLocation()
        {
            if(Input.location.status == LocationServiceStatus.Running)
            {
                magneticHeading = Input.compass.magneticHeading;
                trueHeading = Input.compass.trueHeading;
                headingAccuracy = Input.compass.headingAccuracy;

                latitude = Input.location.lastData.latitude;
                longtitude = Input.location.lastData.longitude;
                altitude = Input.location.lastData.altitude;
                horizontalAccuracy = Input.location.lastData.horizontalAccuracy;
                TimeSpan myTimeSpan = DateTime.Now - new DateTime(1970, 0, 0, 0, 0, 0);
                locationPing = (float)(myTimeSpan.Seconds - Input.location.lastData.timestamp);
            }
        }
    }
}