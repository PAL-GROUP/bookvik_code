using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModelController : MonoBehaviour
{
    public static ModelController Instance;
    public GameObject ParticlePrefab;
    public GameObject MainModel;

    public GameObject lose;
    public GameObject win;

    public string WinModelNameUkr;
    public string LoseModelNameUkr;

    public string WinModelNameEng;
    public string LoseModelNameEng;

    public bool IsActionOnScene;
    public bool TwoGamesTarget;
    public RecognizeColorController RecognizeColorComponent;
    public UseWithCodeSceneManager OpenGameComponent;

    [Header("Video Objects")]
    public UniversalMediaPlayer MediaPlayer;
    public GameObject UniWebPLayerObject;

    [Header("Recognize Objects")]
    public GameObject RegionCapture;
    public GameObject RecognizeButton;

    [Header("Recognize Objects")]
    public AudioSource ModelAudioSource;

    public AudioClip SelectModelAudioClip;
    private Animation _modelAnimaition;

    public List<AudioClip> Soundtracks = new List<AudioClip>();
    public List<GameObject> GameModels = new List<GameObject>();

    private IEnumerator _gameCoroutine;

    private void Start()
    {
        Instance = this;
        FindObjectOfType<AssetBundlesManager>().InstantiateModels();
    }

    //private void OnDestroy()
    //{
    //    AssetBundlesManager.Instance.UnloadAssets();
    //}

    public void GetObjectsOnStartGame(List<GameObject> models)
    {
        ParticlePrefab = models[0];
        GameModels.AddRange(models.GetRange(1, models.Count - 1));
    }

    public void GetObjectsOnStartGame(GameObject model)
    {
        GameModels.Clear();
        GameModels.Add(model);
    }

    private void GetModel(string modelName)
    {
        var modelNumber = GetNumberOfModelFromName(modelName);
        //AssetBundlesManager.Instance.InstantiateModels();
        SetInfoForModel(modelNumber);

    }

    public void SetInfoForModel(string modelNumber)
    {

        MainModel = GameModels.Find(x => x.name == modelNumber);

        if (MainModel == null)
        {
            MainModel = GameModels.Find(x => x.name == modelNumber);
        }

        DataTargetManager.Instance.ModelSelectorComponent = MainModel.GetComponent<Selector>();
        DataTargetManager.Instance.ModelSelectorComponent.TargetType = DataTargetManager.Instance.CurrentTargetType;

        _modelAnimaition = MainModel.GetComponent<Animation>();
        List<AnimationState> states = new List<AnimationState>(_modelAnimaition.Cast<AnimationState>());
        _modelAnimaition.clip = states[0].clip;
        GetSoundtrackForModel(DataTargetManager.Instance.ModelName);
 
        IEnumerator newCoroutine = ShowModel();
        StartCoroutine(newCoroutine);
    }

    public string GetNumberOfModelFromName(string modelName)
    {
        string modelNumber = "";

        foreach (char ch in modelName)
        {
            if (Char.IsDigit(ch))
            {
                modelNumber += ch;
            }
        }

        return modelNumber;
    }

    private void GetSoundtrackForModel(string modelName)
    {
        ModelAudioSource = MainModel.GetComponent<AudioSource>();
        print(Soundtracks.Find(x => x.name == modelName));
        ModelAudioSource.clip = Soundtracks.Find(x => x.name == modelName);

    }

    #region MODEL APPEARING

    private void PlayAnimation()
    {
        _modelAnimaition.Play();
        ModelAudioSource.Play();
    }

    public void StopAnimation()
    {
        if (_modelAnimaition != null)
            _modelAnimaition.clip.SampleAnimation(MainModel, 0f);
    }

    public void Show(bool is3D)
    {
        IsActionOnScene = true;

        if (is3D)
        {
            if (DataTargetManager.Instance.currentTargetObject != null)
                DataTargetManager.Instance.currentTargetObject.Scaner.SetActive(false);
            if (DataTargetManager.Instance.ImagesTargetObject != null)
                DataTargetManager.Instance.ImagesTargetObject.Scaner.SetActive(false);
            if (DataTargetManager.Instance.VuMarkTargetObject != null)
                DataTargetManager.Instance.VuMarkTargetObject.Scaner.SetActive(false);
            GetModel(DataTargetManager.Instance.ModelName);
            DataTargetManager.Instance.IsCanScanTarget = false;
            //GetModel(DataTargetManager.Instance.ModelName);
        }
        else
        {
            ShowNo3DAction(DataTargetManager.Instance.CurrentTargetType);
        }
    }

    public IEnumerator ShowModel()
    {
        if (!MainModel.activeInHierarchy)
        {
            if (RecognizeColorComponent.IsTryAgainAvailable || RecognizeColorComponent.IsSuccessfull)
            {
                RecognizeButton.SetActive(false);
            }
            ParticlePrefab.SetActive(true);
            ParticlePrefab.GetComponent<ParticleSystem>().Play();

            yield return new WaitForSeconds(0.5f);

            MainModel.SetActive(true);
            ActivateDeactivateSwipe(DataTargetManager.Instance.CurrentTargetType);
            MainModel.GetComponent<Selector>().IsCanTap = false;

            yield return new WaitForSeconds(3f);
            StopAnimation();
            PlayAnimation();
            while (_modelAnimaition.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }

            ActionAfterAnimationEnd(DataTargetManager.Instance.CurrentTargetType);
            MainModel.GetComponent<Selector>().IsCanTap = true;
        }
        yield return null;
    }

    public void ShowNo3DAction(ModelEnumerators.TargetActionType curTypeOfTarget)
    {
        switch (curTypeOfTarget)
        {
            case ModelEnumerators.TargetActionType.V:
            case ModelEnumerators.TargetActionType.Z:
                StartCoroutine(OpenCloseTwoVideo());
                StartCoroutine(OpenGame());
                break;
        }
    }

    public void HideAllObjects()
    {
        if (MainModel != null)
            MainModel.SetActive(false);

        ParticlePrefab.SetActive(false);

        if (DataTargetManager.Instance.currentTargetObject != null)
        {
            DataTargetManager.Instance.currentTargetObject.Scaner.SetActive(true);
        }

        if (DataTargetManager.Instance.VuMarkTargetObject != null)
        {
            DataTargetManager.Instance.VuMarkTargetObject.Scaner.SetActive(true);
        }

        if (DataTargetManager.Instance.ImagesTargetObject != null)
        {
            DataTargetManager.Instance.ImagesTargetObject.Scaner.SetActive(true);
        }

        if (RecognizeColorComponent.IsRecognizeAvailable)
        {
            if (DataTargetManager.Instance.currentTargetObject != null)
            {
                DataTargetManager.Instance.currentTargetObject.Scaner.SetActive(false);
            }
        }

        UniWebPLayerObject.SetActive(false);
    }

    public void HideRecoFunctional()
    {
        StopAllCoroutines();

        if (RecognizeColorComponent.IsRecognizeAvailable)
        {
            DeactivateRecognizeFunctional(false);
        }
    }

    public void StopCoroutines()
    {
        StopAllCoroutines();
    }

    #endregion



    #region MODEL ACTIONS

    private void ActivateDeactivateSwipe(ModelEnumerators.TargetActionType curTypeOfTarget)
    {
        switch (curTypeOfTarget)
        {
            case ModelEnumerators.TargetActionType.K:
            case ModelEnumerators.TargetActionType.R:
            case ModelEnumerators.TargetActionType.VuMark:
                MainModel.GetComponent<SwipeModel>().enabled = true;
                break;

            case ModelEnumerators.TargetActionType.W:
            case ModelEnumerators.TargetActionType.Q:
                MainModel.GetComponent<SwipeModel>().enabled = false;
                break;
        }
    }

    private void ActionAfterAnimationEnd(ModelEnumerators.TargetActionType curTypeOfTarget)
    {
        switch (curTypeOfTarget)
        {
            case ModelEnumerators.TargetActionType.Q:
                if (!RecognizeColorComponent.IsTryAgainAvailable && !RecognizeColorComponent.IsSuccessfull)
                {
                    //if (!string.IsNullOrEmpty(DataTargetManager.Instance.CurrentGameLink2))
                    if (!string.IsNullOrEmpty(DataTargetManager.Instance.CurrentGameLink1))
                    {
                        TwoGamesTarget = true;
                        _gameCoroutine = OpenGame();
                        StartCoroutine(_gameCoroutine);
                    }
                    else
                    {
                        StartCoroutine(OpenCloseTwoVideo());
                    }
                    StartCoroutine(WaitForTurnOnOffRecognizeFunctionality());
                }
                else if (RecognizeColorComponent.IsTryAgainAvailable && !RecognizeColorComponent.IsSuccessfull)
                {
                    HideAllObjects();
                    RecognizeButton.SetActive(true);
                    DataTargetManager.Instance.IsCanScanTarget = true;
                    print(DataTargetManager.Instance.IsCanScanTarget);
                }
                else if (!RecognizeColorComponent.IsTryAgainAvailable && RecognizeColorComponent.IsSuccessfull)
                {
#if !UNITY_EDITOR
                    if (!string.IsNullOrEmpty(DataTargetManager.Instance.CurrentGameLink2))
                    {
                        Debug.LogWarning("Open");
                        OpenGameComponent.OpenExternalGame(DataTargetManager.Instance.CurrentGameLink2);
                    }
#endif
                    DeactivateRecognizeFunctional(false);
                }
                break;

            case ModelEnumerators.TargetActionType.W:
                StartCoroutine(OpenCloseTwoVideo());
                StartCoroutine(OpenGame());
                break;
        
        }
    }

    public IEnumerator OpenCloseTwoVideo()
    {
        IsActionOnScene = true;
        TwoGamesTarget = false;
        MediaPlayer.IsVideoEnded = false;
        if (!string.IsNullOrEmpty(DataTargetManager.Instance.VideoLink1))
        {
            UniWebPLayerObject.SetActive(true);
            MediaPlayer._path = DataTargetManager.Instance.VideoLink1;

            MediaPlayer.Play();

            while (!MediaPlayer.IsVideoEnded)
            {
                yield return new WaitForEndOfFrame();
            }

            UniWebPLayerObject.SetActive(false);
        }

        MediaPlayer.IsVideoEnded = false;
        if (!string.IsNullOrEmpty(DataTargetManager.Instance.VideoLink2))
        {
            UniWebPLayerObject.SetActive(true);
            MediaPlayer._path = DataTargetManager.Instance.VideoLink2;

            MediaPlayer.Play();

            while (!MediaPlayer.IsVideoEnded)
            {
                yield return new WaitForEndOfFrame();
            }

            UniWebPLayerObject.SetActive(false);
        }
        MediaPlayer.IsVideoEnded = true;

        if (!RecognizeColorComponent.IsRecognizeAvailable)
        {
            IsActionOnScene = false;
        }

        HideAllObjects();

        yield return null;
    }

    public IEnumerator OpenGame()
    {
        if (!string.IsNullOrEmpty(DataTargetManager.Instance.CurrentGameLink1))
        {
            while (!MediaPlayer.IsVideoEnded)
            {
                yield return new WaitForEndOfFrame();
            }
#if !UNITY_EDITOR
            OpenGameComponent.OpenExternalGame(DataTargetManager.Instance.CurrentGameLink1);
#endif
        }
        yield return null;
    }

    public IEnumerator OpenCloseVideo()
    {
        IsActionOnScene = true;

        while (DataTargetManager.Instance.VideoLink1 == "")
        {
            yield return new WaitForEndOfFrame();
        }

        UniWebPLayerObject.SetActive(true);
        MediaPlayer.IsVideoEnded = false;
        MediaPlayer._path = DataTargetManager.Instance.VideoLink1;

        MediaPlayer.Play();

        while (!MediaPlayer.IsVideoEnded)
        {
            yield return new WaitForEndOfFrame();
        }

        UniWebPLayerObject.SetActive(false);

        if (!RecognizeColorComponent.IsRecognizeAvailable)
        {
            IsActionOnScene = false;
        }

        HideAllObjects();

        yield return null;
    }

    public IEnumerator WaitForTurnOnOffRecognizeFunctionality()
    {
        while (!MediaPlayer.IsVideoEnded)
        {
            yield return new WaitForEndOfFrame();
        }

        while (OpenGameComponent.IsGameOpen)
        {
            yield return new WaitForEndOfFrame();
        }

        RegionCapture.GetComponent<Region_Capture>().ImageTarget =
            GameObject.FindGameObjectWithTag("Targets").GetComponentInChildren<Transform>().transform.FindChild(DataTargetManager.Instance.CurrentTargetID.ToString()).gameObject;
        DeactivateRecognizeFunctional(true);
        HideAllObjects();
    }


    public void DeactivateRecognizeFunctional(bool activate)
    {

        if (activate)
        {
            RegionCapture.SetActive(activate);
            RecognizeButton.SetActive(activate);
            DataTargetManager.Instance.IsCanScanTarget = true;

        }
        else
        {
            RegionCapture.SetActive(activate);
            RecognizeButton.SetActive(activate);
            RecognizeColorComponent.IsTryAgainAvailable = activate;
            RecognizeColorComponent.IsSuccessfull = activate;
            RecognizeColorComponent.Result.text = "";
            RecognizeColorComponent.ColorRecognizeBorder.text = "";
            IsActionOnScene = false;
        }
    }

    #endregion
}
