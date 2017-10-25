using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class DataTargetManager : MonoBehaviour
{
    public static DataTargetManager Instance;

    public int CurrentTargetID;
    public ModelEnumerators.TargetActionType CurrentTargetType;
    public ModelEnumerators.ActionFile CurrentFileAction;
    public string ModelName;

    public Selector ModelSelectorComponent;

    public ModelAppearingEventHandler currentTargetObject;
    public VuMarkEventHandler VuMarkTargetObject;
    public ImagesEventHandler ImagesTargetObject;
    public string VideoLink1;
    public string VideoLink2;
    public string CurrentGameLink1;
    public string CurrentGameLink2;

    public bool IsCanScanTarget = true;

    private void Awake()
    {
        Instance = this;
    }

    public void SetTypeTargetForModel(ModelEnumerators.TargetActionType type, ModelAppearingEventHandler modelHandler)
    {
        CurrentTargetType = type;
        //ModelSelectorComponent.TargetType = type;

        currentTargetObject = modelHandler;
    }

    public void SetDataFromVuMark(ModelEnumerators.TargetActionType type, VuMarkEventHandler vuMarkHandler)
    {
        CurrentTargetType = type;
        //ModelSelectorComponent.TargetType = type;
        VuMarkTargetObject = vuMarkHandler;
    }

    public void SetDataFromImages(ModelEnumerators.TargetActionType type, ImagesEventHandler imagesHandler)
    {
        CurrentTargetType = type;
        //ModelSelectorComponent.TargetType = type;

        ImagesTargetObject = imagesHandler;
    }
}
