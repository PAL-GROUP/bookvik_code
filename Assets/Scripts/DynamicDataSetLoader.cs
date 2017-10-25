using UnityEngine;
using System.Collections;

using Vuforia;
using System.Collections.Generic;


public class DynamicDataSetLoader : MonoBehaviour
{
    // specify these in Unity Inspector
    public GameObject augmentationObject = null;  // you can use teapot or other object
    public string dataSetName = "";  //  Assets/StreamingAssets/QCAR/DataSetName

    // Use this for initialization
    void Start()
    {
        // Vuforia 6.2+
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(LoadDataSet);
    }

    void LoadDataSet()
    {

        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

        DataSet dataSet = objectTracker.CreateDataSet();
   
        if (dataSet.Load(dataSetName))
        {
            objectTracker.Stop();  // stop tracker so that we can add new dataset

            if (!objectTracker.ActivateDataSet(dataSet))
            {
                // Note: ImageTracker cannot have more than 100 total targets activated
                Debug.Log("<color=yellow>Failed to Activate DataSet: " + dataSetName + "</color>");
            }

            if (!objectTracker.Start())
            {
                Debug.Log("<color=yellow>Tracker Failed to Start.</color>");
            }

            int counter = 0;

            IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();


            GameObject go = new GameObject("ImageTargets");
            foreach (TrackableBehaviour tb in tbs)
            {
                if (tb.name == "New Game Object" )
                {
                    // change generic name to include trackable name
                    tb.gameObject.name = tb.TrackableName;
                    tb.gameObject.transform.SetParent(go.transform);
                    // add additional script components for trackable
                    tb.gameObject.AddComponent<ModelAppearingEventHandler>();
                    tb.gameObject.AddComponent<TargetID>();
                }
            }
            go.AddComponent<GetTargetsType>();
            
        }
        else
        {
            Debug.LogError("<color=yellow>Failed to load dataset: '" + dataSetName + "'</color>");
        }
    }
}