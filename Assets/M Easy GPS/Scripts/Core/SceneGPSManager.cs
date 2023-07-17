using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEasyGPS.Management;

namespace MEasyGPS.Management
{
    public class SceneGPSManager : MonoBehaviour
    {
        public static SceneGPSManager instance;
        public double latitude { get; private set; }
        public double longtitude { get; private set; }
        public float altitude { get; private set; }
        public float horizontalAccuracy { get; private set; }
        public float magneticHeading { get; private set; }
        public float trueHeading { get; private set; }
        public float headingAccuracy { get; private set; }
        [Tooltip("Experimental location update ping")] public float locationPing { get; private set; }

        private Initialisaton init;
        private void Awake()
        {
            if (instance)
                Destroy(this);

            instance = this;

            DontDestroyOnLoad(this.gameObject);
            try
            {
                init = GetComponent<Initialisaton>();
            }
            catch
            {
                if (init == null)
                    init = gameObject.AddComponent<Initialisaton>();
            }

            init.TryToStartGPSService();
        }

        private void Update()
        {
            if (init && !init.didFail)
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