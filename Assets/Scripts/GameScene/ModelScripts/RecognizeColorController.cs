using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class RecognizeColorController : MonoBehaviour
{
    public int percentAccuracy;
    public int AccuracyOfColorLimit;

    [Space(10f)]

    public bool IsShowDrawedRegion;
    public List<Color> getColor0;
    public Color ColorLimit;
    public Text Result;
    public Text ColorRecognizeBorder;
    public Text TergetNotFound;

    public float CurrentResultRecognizing;
    public List<Pixels> PixelsArray = new List<Pixels>();

    public RenderTextureCamera RenderTextureComponent;
    public DataTargetManager DataTarget;

    public bool IsSuccessfull;
    public bool IsTryAgainAvailable;
    public bool IsRecognizeAvailable;
    public bool IsTargetFound;

    private int wrongScanCount = 0;

    private void OnEnable()
    {
        ColorRecognizeBorder.text = "Необхідно - " + percentAccuracy;

        IsRecognizeAvailable = true;
    }

    private void OnDisable()
    {
        IsRecognizeAvailable = false;
    }

    public void StartRecognizeColor()
    {
        if (DataTarget.currentTargetObject.IsTargetFound)
        {
            StopAllCoroutines();
            StartCoroutine(CheckAndTryChangePixelsColor());
        }
        else
        {
            StartCoroutine(TargetNotFoundText());
        }
    }

    private IEnumerator TargetNotFoundText()
    {
        TergetNotFound.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        TergetNotFound.gameObject.SetActive(false);
    }

    private IEnumerator CheckAndTryChangePixelsColor()
    {
        getColor0.Clear();

        if (RenderTextureComponent.Render_Texture_Camera != null && getColor0.Count == 0)
        {
            RenderTextureComponent.Render_Texture_Camera.GetComponent<Camera>().enabled = false;

            Texture2D texture2d = GetRTPixels(RenderTextureComponent.Render_Texture_Camera.GetComponent<Camera>().targetTexture);

            var elementNumber = 0;

            for (int i = 0; i < PixelsArray.Count; i++)
            {
                if (PixelsArray[i].TargetID == DataTarget.CurrentTargetID)
                {
                    elementNumber = i;
                    break;
                }
            }


            for (int i = 0; i < PixelsArray[elementNumber].ColorCoordinates.Length; i++)
            {
                for (int j = (int)PixelsArray[elementNumber].ColorCoordinates[i].x - PixelsArray[elementNumber].regionRaidus; j <= (int)PixelsArray[elementNumber].ColorCoordinates[i].x + PixelsArray[elementNumber].regionRaidus; j++)
                {
                    for (int k = (int)PixelsArray[elementNumber].ColorCoordinates[i].y - PixelsArray[elementNumber].regionRaidus; k <= (int)PixelsArray[elementNumber].ColorCoordinates[i].y + PixelsArray[elementNumber].regionRaidus; k++)
                    {
                        getColor0.Add(texture2d.GetPixel(j, k));

                        if (IsShowDrawedRegion)
                            texture2d.SetPixel(j, k, Color.magenta);
                    }
                }
            }

            ColorResultEstimating(getColor0);
            Debug.Log("Color under limit line!");

            yield return null;

            texture2d.Apply();

            yield return null;

            // Save to render texture
            Graphics.Blit(texture2d, RenderTextureComponent.Render_Texture_Camera.targetTexture);
            RenderTexture.active = null;
            RenderTextureComponent.Render_Texture_Camera.GetComponent<Camera>().enabled = true;
        }
    }


    private void ColorResultEstimating(List<Color> colorsArray)
    {
        int successfullColors = 0;
        int failedColors = 0;

        for (int i = 0; i < colorsArray.Count; i++)
        {
            int red = (int)(colorsArray[i].r * 255);
            int blue = (int)(colorsArray[i].b * 255);
            int green = (int)(colorsArray[i].g * 255);
            int average = (red + blue + green) / 3;

            //if ((red <= average + AccuracyOfColorLimit && red >= average - AccuracyOfColorLimit) &&
            //    (green <= average + AccuracyOfColorLimit && green >= average - AccuracyOfColorLimit) &&
            //    (blue <= average + AccuracyOfColorLimit && blue >= average - AccuracyOfColorLimit))
            //{
            //    failedColors++;
            //}
            //// check for fake blue
            //else if ((red <= 160 && red >= 120) &&
            //    (green <= 200 && green >= 160) &&
            //    (blue <= 240 && blue >= 200))
            //{
            //    failedColors++;
            //}
            //else if (red >= average + AccuracyOfColorLimit || red <= average - AccuracyOfColorLimit ||
            //    green >= average + AccuracyOfColorLimit || green <= average - AccuracyOfColorLimit ||
            //    blue >= average + AccuracyOfColorLimit || blue <= average - AccuracyOfColorLimit)
            //{
            //   // print("red " + red + " green " + green + " blue " + blue );
            //    successfullColors++;

            //}

            if ((red <= average + AccuracyOfColorLimit && red >= average - AccuracyOfColorLimit) &&
                (green <= average + AccuracyOfColorLimit && green >= average - AccuracyOfColorLimit) &&
                (blue <= average + AccuracyOfColorLimit && blue >= average - AccuracyOfColorLimit))
            {
                failedColors++;
            }
            // check for fake blue
            else if ((red <= 160 && red >= 120) &&
                (green <= 200 && green >= 160) &&
                (blue <= 250 && blue >= 190))
            {
                failedColors++;
            }
            else if (red >= average + AccuracyOfColorLimit || red <= average - AccuracyOfColorLimit ||
                green >= average + AccuracyOfColorLimit || green <= average - AccuracyOfColorLimit ||
                blue >= average + AccuracyOfColorLimit || blue <= average - AccuracyOfColorLimit)
            {
               //  print("red " + red + " green " + green + " blue " + blue );
                successfullColors++;
            }
        }

        CurrentResultRecognizing = ((float)successfullColors / (successfullColors + failedColors)) * 100;

        if (CurrentResultRecognizing > percentAccuracy)
        {
            print("Good job! You painted about " + CurrentResultRecognizing + " percent of image!");
            IsTryAgainAvailable = false;
            IsSuccessfull = true;
            DataTarget.currentTargetObject.IsTargetFound = false;
            // hard input engilsh workbook
            if(DataTargetManager.Instance.CurrentTargetID < 21)
                DataTargetManager.Instance.ModelName = ModelController.Instance.WinModelNameUkr;
            else
            {
                DataTargetManager.Instance.ModelName = ModelController.Instance.WinModelNameEng;

            }
            wrongScanCount = 0;

            ModelController.Instance.Show(true);
        }
        else
        {
            print("Sorry! You painted about " + CurrentResultRecognizing + " percent of image! It's not enough!");
            IsTryAgainAvailable = true;
            IsSuccessfull = false;
            // hard input engilsh workbook

            if (DataTargetManager.Instance.CurrentTargetID < 21)
                DataTargetManager.Instance.ModelName = ModelController.Instance.LoseModelNameUkr;
            else
            {
                DataTargetManager.Instance.ModelName = ModelController.Instance.LoseModelNameEng;

            }
            if (wrongScanCount < 2)
                wrongScanCount++;
            else
            {
                IsTryAgainAvailable = false;
                IsSuccessfull = true;
                DataTarget.currentTargetObject.IsTargetFound = false;
                // hard input engilsh workbook
                if (DataTargetManager.Instance.CurrentTargetID < 21)
                    DataTargetManager.Instance.ModelName = ModelController.Instance.WinModelNameUkr;
                else
                {
                    DataTargetManager.Instance.ModelName = ModelController.Instance.WinModelNameEng;
                }
                wrongScanCount = 0;
            }
            ModelController.Instance.Show(true);
        }

        //IEnumerator curCoroutine = ModelController.Instance.ShowModel();
        //StartCoroutine(curCoroutine);
        Result.text = "Результат распізнавання - " + CurrentResultRecognizing;
        print("Success: " + successfullColors + " Failed: " + failedColors + " Sum: " + (successfullColors + failedColors));


    }

    public Texture2D GetRTPixels(RenderTexture rt)
    {
        Debug.Log("Read pixels data!");

        // Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        RenderTexture.active = rt;

        // Create a new Texture2D and read the RenderTexture image into it
        Texture2D tex = new Texture2D(rt.width, rt.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        // Restorie previously active render texture
        RenderTexture.active = currentActiveRT;
        return tex;
    }


    [System.Serializable]
    public class Pixels
    {
        public int TargetID;
        public Vector2[] ColorCoordinates;
        public int regionRaidus;
    }
}
