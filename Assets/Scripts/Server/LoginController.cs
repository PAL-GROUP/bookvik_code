using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using platformer.network;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

[Serializable]
public class LoginController : MonoBehaviour
{


    [Header("Login")]
    public GameObject LoginMessage;
    public Text LoginMessageText;
    public GameObject Recconect;

    [Header("UserInfo")]
    public InputField Email;


    [Header("Nickname")]
    public GameObject NicknameChangeWindow;
    public InputField Nickname;
    public InputField DayOfBirth;
    public InputField MonthOfBirth;
    public InputField YearOfBirth;

    public Button LoginButton;
    public GameObject LoginScreen;
    public GameObject LoadingPanel;

    [Header("Progress Information")]
    public Text UserName;

    public static string UserToken;
    public static string TokenType;
    public bool IsLoggedOn = false;
    public static string AvatarType;

    public ExternalMailSender MailSender;
    private RequestSendHandler RequestSendHandler;
    private RequestLogin _rLogin;
    private RequestNickName _rNickname;
    private string _url;
    private int lengthOfBirth;
    private bool isSend;

    public const string MatchEmailPattern =
            @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
            + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
              + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
            + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";


    private void Awake()
    {
        RequestSendHandler = FindObjectOfType<RequestSendHandler>();
        RequestSendHandler.Reconnect = Recconect;
    }

    private void Start()
    {
        GetLoadingPanelObject();
        LoginControll();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //   Invoke("NickNameTrue",5f);
    }

    private void OnEnable()
    {
        WebAsync.OnLoginTrue += WebAsync_OnLoginTrue;
        WebAsync.OnTokenTrue += WebAsync_OnOnTokenTrue;
        WebAsync.OnNickNameChangeTrue += WebAsync_OnOnNickNameChangeTrue;

        //WebAsync.OnGetUserImageTrue += WebAsyncOnOnGetUserImageTrue;
    }

    private void LoginControll()
    {

        if (!RequestSendHandler.IsLoggedOn)
        {
            LoginTrue();
        }
        else
        {
            LoginScreen.SetActive(false);
            if (!string.IsNullOrEmpty(RequestSendHandler.UserName))
            {
                UserName.transform.parent.GetComponent<Button>().enabled = false;
                UserName.text = RequestSendHandler.UserName;
            }
        }
    }

    public void GetLoadingPanelObject()
    {
        RequestSendHandler.LoadingPanel = LoadingPanel;
    }

    private void WebAsync_OnOnTokenTrue(object sender, EventArgs eventArgs)
    {
        string str = WebAsync.WebResponseString.Replace(".", "");
        var token = JsonUtility.FromJson<TokenInfo>(str);
        UserToken = token.access_token;
        TokenType = token.token_type;
        RequestSendHandler.UserToken = TokenType + " " + UserToken;
        RequestSendHandler.IsLoggedOn = true;


    }

    private void OnDisable()
    {
        WebAsync.OnLoginTrue -= WebAsync_OnLoginTrue;
        WebAsync.OnTokenTrue -= WebAsync_OnOnTokenTrue;
        WebAsync.OnNickNameChangeTrue -= WebAsync_OnOnNickNameChangeTrue;


        //WebAsync.OnGetUserImageTrue -= WebAsyncOnOnGetUserImageTrue;
    }

    private void WebAsyncOnOnGetUserImageTrue(object sender, EventArgs eventArgs)
    {
        if (AvatarType == "UserAvatar")
        {
            string str = WebAsync.WebResponseString;
            var userAvatar = JsonUtility.FromJson<JResult>(str);
            byte[] image = Convert.FromBase64String(userAvatar.Bytes);
            var texture = new Texture2D(1, 1);
            //  UserAvatar.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            //    new Vector2(0f, 1f));
            //   UserAvatarStatic = texture;
            AvatarType = "";
        }
    }

    public static bool IsEmail(string email)
    {
        if (email != null) return Regex.IsMatch(email, MatchEmailPattern);
        else return false;
    }

    public void LoginTrue()
    {
        string requestUrl = string.Format(NetworkRequests.LoginRequest, RequestSendHandler.BaseServerUrl);

        var uri = new Uri(requestUrl);
        if (PlayerPrefs.HasKey("DeviceSession"))
        {
            Email.interactable = false;
            LoginButton.interactable = false;
            Email.text = PlayerPrefs.GetString("Email");
            _rLogin = new RequestLogin { DeviceSession = PlayerPrefs.GetString("DeviceSession"), Email = PlayerPrefs.GetString("Email") };

            RequestSendHandler.RequestTypeInt = 0;
            RequestSendHandler.SendRequest(uri, _rLogin, HttpMethod.Post, ContentType.ApplicationJson);
        }
        else if (Email.text != "")
        {
            string deviceSession = Guid.NewGuid().ToString();

            if (SceneManager.GetActiveScene().name.ToLower().Contains("rus")) PlayerPrefs.SetString("Localization", "rus");
            if (SceneManager.GetActiveScene().name.ToLower().Contains("ukr")) PlayerPrefs.SetString("Localization", "ukr");
            if (SceneManager.GetActiveScene().name.ToLower().Contains("eng")) PlayerPrefs.SetString("Localization", "eng");

            if (!IsEmail(Email.text))
            {
                switch (PlayerPrefs.GetString("Localization"))
                {
                    case "rus": ShowError.Show("Неправильно введен e-mail!"); break;
                    case "ukr": ShowError.Show("Неправильно введено e-mail!"); break;
                    case "eng": ShowError.Show("Incorrect e-mail!"); break;
                }
                return;

            }
            string email = Email.text;

            Email.interactable = false;
            LoginButton.interactable = false;
            _rLogin = new RequestLogin { DeviceSession = deviceSession, Email = email };
            PlayerPrefs.SetString("DeviceSession", deviceSession);
            PlayerPrefs.SetString("Email", email);


            RequestSendHandler.RequestTypeInt = 0;
            RequestSendHandler.SendRequest(uri, _rLogin, HttpMethod.Post, ContentType.ApplicationJson);
        }
        else
        {
            Debug.Log("DeviceSession is null and E-mail is empty");
        }
    }

    public void ChangeNickNameTrue()
    {
        string month, year, day;

        string[] time = YearOfBirth.text.Split('/');

        DateTime correctDate;
        if (time.Length < 3)
        {
            ShowError.Show("Не корректно введена дата народження!");
            return;
        }

        print(YearOfBirth.text + DateTime.TryParse(YearOfBirth.text, out correctDate));
        month = time[0];
        day = time[1];
        year = time[2];

        if (Nickname.text == "" || day == "" || day.Contains("/") || month == "" || month.Contains("/") || year == "" ||
        year.Contains("/") || DateTime.TryParse(YearOfBirth.text, out correctDate) == false)
        {
            ShowError.Show("Не корректно введена дата народження!");
            return;
        }

        string requestUrl = string.Format(NetworkRequests.NickNameChangeRequest, RequestSendHandler.BaseServerUrl);

        var uri = new Uri(requestUrl);

        _rNickname = new RequestNickName { Name = Nickname.text, DateOfBirth = year + "-" + month + "-" + day + " 00:00:00.00" };
        RequestSendHandler.RequestTypeInt = 2;
        RequestSendHandler.SendRequest(uri, _rNickname, HttpMethod.Post, ContentType.ApplicationJson, TokenType + " " + UserToken);
    }

    public void DateBirthFormating()
    {
        if (YearOfBirth.text.Length < lengthOfBirth)
        {
            if (YearOfBirth.text.Length > 1)
            {
                YearOfBirth.text.Remove(YearOfBirth.text.Length - 1, 1);
                lengthOfBirth = YearOfBirth.text.Length;
                return;
            }
        }

        if (YearOfBirth.text.Length < 10)
        {
            if (YearOfBirth.caretPosition == 5)
            {
                if (YearOfBirth.text[YearOfBirth.text.Length - 1].ToString() != "/" &&
                    SymbolsCounter(YearOfBirth.text, '/') < 2)
                {
                    print(YearOfBirth.caretPosition);
                    YearOfBirth.text = YearOfBirth.text.Insert(YearOfBirth.caretPosition, "/");
                }
            }
            else if (YearOfBirth.caretPosition == 2)
            {
                if (YearOfBirth.text[YearOfBirth.text.Length - 1].ToString() != "/" && SymbolsCounter(YearOfBirth.text, '/') < 2)
                {
                    print(YearOfBirth.caretPosition);
                    YearOfBirth.text = YearOfBirth.text.Insert(YearOfBirth.caretPosition, "/");
                }
            }

            if (YearOfBirth.caretPosition == 2 || YearOfBirth.caretPosition == 5)
            {
                YearOfBirth.caretPosition++;
            }

            lengthOfBirth = YearOfBirth.text.Length;
        }
    }

    private int SymbolsCounter(string str, char symbol)
    {
        int count = 0;

        foreach (var item in str.ToCharArray())
        {
            if (item == symbol)
            {
                count++;
            }
        }

        return count;
    }

    private void WebAsync_OnOnNickNameChangeTrue(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;
        //var userInfo = JsonUtility.FromJson<UserInfo>(str);
        if (str == "1")
        {
            UserName.text = _rNickname.Name;
            RequestSendHandler.UserName = _rNickname.Name;
            NicknameChangeWindow.GetComponent<PopUp>().Disable();
            UserName.transform.parent.GetComponent<Button>().enabled = false;
        }
        else
        {

        }
    }

    private void WebAsync_OnLoginTrue(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;
        var userInfo = JsonUtility.FromJson<UserInfo>(str);

        if (userInfo.Email == "")
        {
            if (!MailSender.isSend)
            {
                MailSender.Send(Email.text.Trim(), PlayerPrefs.GetString("DeviceSession"));
                LoginMessage.SetActive(true);
                switch (PlayerPrefs.GetString("Localization"))
                {
                    case "rus":
                        LoginMessageText.text =
            "Сообщение отправлено на указанный почтовый адрес. Проверьте и подтвердите, пожалуйста, Вашу регистрацию.";
                        break;
                    case "ukr":
                        LoginMessageText.text =
            "Повідомлення надіслано на вказану поштову адресу. Перевірте та підтвердіть, будь ласка, Вашу реєстрацію.";
                        break;
                    case "eng":
                        LoginMessageText.text =
            "Message was send on your e-mail. Please confirm your registration.";
                        break;
                }

            }
            StartCoroutine(CheckAutorization());
        }
        else
        {
            IEnumerator curCoroutine = CheckAutorization();
            StopCoroutine(curCoroutine);
            LoginScreen.SetActive(false);
            switch (PlayerPrefs.GetString("Localization"))
            {
                case "rus":
                    LoginMessageText.text =
        "Авторизация.";
                    break;
                case "ukr":
                    LoginMessageText.text =
        "Авторизація.";
                    break;
                case "eng":
                    LoginMessageText.text =
        "Authorization.";
                    break;
            }
            TokenRequest();

            SwipeController.Instance.CanShow = true;


            if (userInfo.Name != userInfo.Email)
            {
                UserName.transform.parent.GetComponent<Button>().enabled = false;
                UserName.text = userInfo.Name;
                RequestSendHandler.UserName = userInfo.Name;

            }
        }
    }

    private void TokenRequest()
    {
        string requestUrl = string.Format(NetworkRequests.TokenRequest, RequestSendHandler.BaseServerUrl);
        var uri = new Uri(requestUrl);

        string s =
            "grant_type=password&scope=offline_access profile email roles&resource=" + RequestSendHandler.BaseServerUrl + " &username=" +
            Email.text + " &password=" + Email.text;

        RequestSendHandler.RequestTypeInt = 1;
        RequestSendHandler.SendRequest(uri, s, HttpMethod.Post, ContentType.TextPlain);
    }

    private IEnumerator CheckAutorization()
    {
        yield return new WaitForSeconds(3f);

        LoginTrue();
    }

    public void ClearField()
    {
        StopAllCoroutines();
        PlayerPrefs.DeleteKey("Email");
        PlayerPrefs.DeleteKey("DeviceSession");
        Email.text = "";
        Email.interactable = true;
        LoginButton.interactable = true;
        MailSender.isSend = false;
        LoginMessage.SetActive(false);
    }

}


public class TokenInfo
{
    public string access_token;
    public string expires;
    public string expires_in;
    public string issued;
    public string token_type;
    public string userName;
}

[Serializable]
public class UserInfo
{
    public string Id;
    public string Email;
    public string Name;
    public string DateOfBirth;
    public string CanChangeName;
}

[Serializable]
public class UserStats
{
    public long BestLevelPasses;
    public long BestLevelPlays;
    public long BronzeCups;
    public long GoldenCups;
    public long LevelLikes;
    public long LevelPlayes;
    public long LevelPublishes;
    public long PassedLevels;
    public long SilverCups;
}

[Serializable]
public class JResult
{
    public string Bytes;
}

