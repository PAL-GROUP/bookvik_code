using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SecurityController : MonoBehaviour
{
    public static SecurityController Instance;

    [Header("Fields For Fill")]
    public Text Question;

    public InputField InputAnswer;

    [Space(10f)]
    public GameObject SecurityScreen;
    public Button SendButton;
    private string JSonToFile;
    public SecurityQuestion SecurityObject;

    public Security RandomQuestion;

    public List<Security> SecurityQuestionList = new List<Security>();
    public Security[] items1 = new Security[3]
    {
        new Security("9*8=","72"),
        new Security("2/2=","1"),
        new Security("2+3=","5"),
    };
    
    private void Awake()
    {
        Instance = this;
        //SecurityObject.questionList = items1.ToList();
        CreateJSonStructure();
    }

    private void Start()
    {
        int randomNumberOfQuestion = UnityEngine.Random.Range(0, SecurityObject.questionList.Count);
        RandomQuestion = SecurityObject.questionList[randomNumberOfQuestion];
        FillSecurityPanel();
    }

    public void FillSecurityPanel()
    {
        Question.text = RandomQuestion.Question;
    }

    public void CheckCorrectAnswer(GameObject window)
    {
        if (InputAnswer.text == RandomQuestion.Answer)
        {
            SecurityScreen.GetComponent<PopUp>().Disable();
            InputAnswer.text = "";
            window.SetActive(true);
        }
        else
        {
            RefreshPanel();
        }
    }

    public void RefreshPanel()
    {
        int randomNumberOfQuestion = UnityEngine.Random.Range(0, SecurityObject.questionList.Count);
        RandomQuestion = SecurityObject.questionList[randomNumberOfQuestion];
        InputAnswer.text = "";
        FillSecurityPanel();
    }

    private void CreateJSonStructure()
    {
        var pathToFile = "";
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            pathToFile = Application.dataPath + "/Resources/SecurityJson.json";

            if (!File.Exists(pathToFile))
            {
                JSonToFile = JsonUtility.ToJson(SecurityObject);

                StreamWriter sw = File.CreateText(pathToFile);
                sw.Close();
                File.WriteAllText(pathToFile, JSonToFile);
            }

            SecurityObject = JsonUtility.FromJson<SecurityQuestion>(File.ReadAllText(pathToFile));
        }
        else
        {
            TextAsset textAsset = Resources.Load("SecurityJson", typeof(TextAsset)) as TextAsset;
            var contentFromFile = textAsset.text;
            SecurityObject = JsonUtility.FromJson<SecurityQuestion>(contentFromFile);
        }
    }


    [Serializable]
    public class SecurityQuestion
    {
        public List<Security> questionList = new List<Security>(); 
    }

    [Serializable]
    public class Security
    {
        public string Question;
        public string Answer;

        public Security(string question, string answer)
        {
            Question = question;
            Answer = answer;
        }
    }
}
