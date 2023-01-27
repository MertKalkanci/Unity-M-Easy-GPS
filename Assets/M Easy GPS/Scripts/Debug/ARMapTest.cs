using MEasyGPS.Management;
using TMPro;
using UnityEngine;

namespace MEasyGPS.Utils
{
    public class ARMapTest : MonoBehaviour
    {
        private SceneGPSManager manager;
        private GPSObjectPlace[] objectPlacers;
        private TMP_Text debugText;
        private RectTransform magneticDirectionTransform, trueDirectionTransform;
        private RectTransform[] targetTransforms;
        public double Playerlatitude, Playerlongtitude, CurrentHeading, CurrentMagneticHeading, CurrentAccuracy;
        public float ping;

        [Space]

        [SerializeField] private GameObject TargetPositionUI;
        [SerializeField] private DebugMode debugMode;
        [SerializeField] private double latitudeMeterConstant = 111320;
        [SerializeField] private double targetLatitude, targetLongitude;
        [SerializeField] private string TargetTag = "DebugTargetLocation", DirectionTrueTag = "DebugDirectionTrue", DirectionMagneticTag = "DebugDirectionMagnetic", DebugTextTag = "DebugInfoText";

        void Start()
        {
            manager = FindObjectOfType<SceneGPSManager>();
            debugText = GameObject.FindGameObjectWithTag(DebugTextTag).GetComponent<TMP_Text>();
            trueDirectionTransform = GameObject.FindGameObjectWithTag(DirectionTrueTag).GetComponent<RectTransform>();
            magneticDirectionTransform = GameObject.FindGameObjectWithTag(DirectionMagneticTag).GetComponent<RectTransform>();

            if (trueDirectionTransform == null || magneticDirectionTransform == null || debugText == null)
            {
                Debug.Log("Error Finding Target transforms found with tag: ' " + TargetTag + " ' , Destroying This Script Instance...");
                Destroy(this);
            }

            objectPlacers = FindObjectsOfType<GPSObjectPlace>();

            targetTransforms = new RectTransform[objectPlacers.Length];
            for (int i = 0; i < objectPlacers.Length; i++)
            {
                targetTransforms[i] = Instantiate(TargetPositionUI, trueDirectionTransform.parent).GetComponent<RectTransform>();
            }

            targetLatitude = objectPlacers[0].latitude;
            targetLongitude = objectPlacers[0].longtitude;
        }

        void Update()
        {
            if (debugMode == DebugMode.None)
                return;

            if (debugMode == DebugMode.Info)
            {
                CurrentHeading = manager.trueHeading;
                CurrentMagneticHeading = manager.magneticHeading;
                CurrentAccuracy = manager.headingAccuracy;
                Playerlatitude = manager.latitude;
                Playerlongtitude = manager.longtitude;
                ping = manager.locationPing;
            }

            //Hesaplamalar
            for (int i = 0; i < objectPlacers.Length; i++)
            {
                double diffLatMet, diffLonMet;

                DiffMeters(Playerlatitude, Playerlongtitude, objectPlacers[i].latitude, objectPlacers[i].longtitude, out diffLatMet, out diffLonMet);

                if (Mathf.Abs((float)diffLatMet) < 100 && Mathf.Abs((float)diffLonMet) < 100)
                    targetTransforms[i].anchoredPosition = new Vector3((float)diffLonMet, (float)diffLatMet);
                else
                    targetTransforms[i].anchoredPosition = Vector3.zero;
            }

            magneticDirectionTransform.rotation = Quaternion.Euler(0, 0, (float)-CurrentMagneticHeading);
            trueDirectionTransform.rotation = Quaternion.Euler(0, 0, (float)-CurrentHeading);
            debugText.text =
                "PlayerLat: " + Playerlatitude + "\nPlayerLon: " + Playerlongtitude +
                "\n===================" +
                "\nTargetLat: " + targetLatitude + "\nTargetLon: " + targetLongitude +
                "\n===================" +
                "\nMagneticDir: " + CurrentMagneticHeading + "\nTrueDir: " + CurrentHeading + "\nAccuracy: " + CurrentAccuracy +
                "\n===================" +
                "\nLocationPing: " + ping
                ;
        }
        public void DiffMeters(double originLat, double originLon, double TargetLat, double TargetLon, out double diffLatMet, out double diffLonMet)
        {
            double oLatMet, oLonMet, tLatMet, tLonMet;

            oLatMet = originLat * latitudeMeterConstant;
            oLonMet = originLon * 40075000 * Mathf.Cos(Mathf.Deg2Rad * (float)originLat) / 360;
            tLatMet = TargetLat * latitudeMeterConstant;
            tLonMet = TargetLon * 40075000 * Mathf.Cos(Mathf.Deg2Rad * (float)TargetLat) / 360;

            diffLatMet = tLatMet - oLatMet;
            diffLonMet = tLonMet - oLonMet;
        } // also have this function in GPSObjectPlace to make scripts independent
        public void RandomiseTarget()
        {
            targetLatitude = Playerlatitude + Random.Range(9f, -9f) * 0.00001f;
            targetLongitude = Playerlongtitude + Random.Range(8.7f, -8.7f) * 0.00001f;

            try
            {
                objectPlacers[0].latitude = (float)targetLatitude;
                objectPlacers[0].longtitude = (float)targetLongitude;
            }
            catch
            {
                Debug.Log("Failed to Find GPS Object Instance");
            }
        }
    }
}