using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Net.Sockets;

public class GameManager : MonoBehaviour
{
    public static bool levelStarted, levelFailed, cheatOn;
    public static float pageTurnSpeed = 270; // Changes throughout game from 4.5s, to 2.7s (or 3s) 60 * 4.5 = 270 to 180 or 162

    [SerializeField]
    Camera myCam;

    [SerializeField] Transform spawnFolder;

    [SerializeField] SpriteRenderer gameTitle, gameTitleBG, controlsTitle, controllsTitleBG, whiteSquare, highscoreTitle, highscoreBG;

    [SerializeField] Transform[] retryTexts;

    bool restartLogic, startLogic;

    [SerializeField] TextMeshPro currentScore;
    int score, inci;

    [SerializeField] GameObject[] pages2, pages3;
    List<GameObject> livePages = new List<GameObject>();

    public static bool bottomsTriggered, moveBottom;

    [SerializeField] Animator camShake, scoreAnim;

    [SerializeField] SpriteRenderer[] soundIcons;

    [SerializeField] SoundManagerLogic mySoundManager;

    [SerializeField] AudioSource mainMenuMusic;

    // Sounds: GameManager.cs, PlayerController.cs

    [SerializeField] GameObject[] socks;

    [SerializeField] Material bgMat;
    private void Awake()
    {
        Application.targetFrameRate = 60;

        levelStarted = false;
        levelFailed = false;

        restartLogic = false;
        startLogic = false;
        bottomsTriggered = false;
        moveBottom = false;

        cheatOn = false;

        inci = 0;
        score = 0;

        bgMat.color = new Color(0.2434f, 0.7803f, 0.7669f);

        currentScore.text = PlayerPrefs.GetInt("highScore", 0) + "";

        if (PlayerPrefs.GetInt("eggyEnabled", 0) == 0) // is off
        {
            for (int i = 0; i < socks.Length; i++)
                socks[i].SetActive(false);

            cheatOn = false;
        }
        else if (PlayerPrefs.GetInt("eggyEnabled", 0) == 1) // is on
        {
            for (int i = 0; i < socks.Length; i++)
                socks[i].SetActive(true);

            cheatOn = true;
        }

        // spawn starting 6 pages
        for (int i = 5; i >= 0; i--)
        {
            GameObject tempObj = Instantiate(pages3[Random.Range(0, pages3.Length)], Vector3.zero, Quaternion.identity, spawnFolder);
            tempObj.GetComponent<PageLogic>().turnState = i;
            switch (i)
            {
                case 0:
                    tempObj.transform.localPosition = new Vector3(-0.12f, -4.4f, 0);
                    tempObj.transform.localEulerAngles = new Vector3(0, 0, 121.55f);
                    break;
                case 1:
                    tempObj.transform.localPosition = new Vector3(-0.02f, -4.344f, 0);
                    tempObj.transform.localEulerAngles = new Vector3(0, 0, 121.55f);
                    break;
                case 2:
                    tempObj.transform.localPosition = new Vector3(0.076f, -4.29f, 0);
                    tempObj.transform.localEulerAngles = new Vector3(0, 0, 121.55f);
                    break;
                case 3:
                    tempObj.transform.localPosition = new Vector3(0.076f, -4.29f, 0);
                    tempObj.transform.localEulerAngles = Vector3.zero;
                    break;
                case 4:
                    tempObj.transform.localPosition = new Vector3(0.076f, -4.4f, 0);
                    tempObj.transform.localEulerAngles = Vector3.zero;
                    break;
                case 5:
                    tempObj.transform.localPosition = new Vector3(0.076f, -4.51f, 0);
                    tempObj.transform.localEulerAngles = Vector3.zero;
                    break;
                default:
                    Debug.Log("error");
                    break;
            }
            livePages.Add(tempObj);
        }
    }

    private void Start()
    {
        StartCoroutine(StartLogic());
        PlayerPrefs.SetInt("GamesSinceLastAdPop", PlayerPrefs.GetInt("GamesSinceLastAdPop", 0)+1);
    }

    IEnumerator SpawnNewPage()
    {
        yield return new WaitForSeconds(Mathf.Clamp((((Mathf.Min(score, 30) - 30) * -1) / 30) * 0.7f + 0.3f, 0.5f, 1)); // wait time in between page landing and a new page spawning (1 -> 0.3f)
        if (!levelFailed)
        {
            mySoundManager.Play("PageTurning" + Random.Range(1, 5)); // page turning

            // instantiate page
            GameObject tempObj;
            if (score < 7)
                tempObj = Instantiate(pages3[Random.Range(0, pages3.Length)], Vector3.zero, Quaternion.identity, spawnFolder);
            else
                tempObj = Instantiate(pages2[Random.Range(0, pages3.Length)], Vector3.zero, Quaternion.identity, spawnFolder);
            tempObj.GetComponent<PageLogic>().turnState = 0;
            tempObj.transform.localPosition = new Vector3(-0.12f, -4.4f, 0);
            tempObj.transform.localEulerAngles = new Vector3(0, 0, 121.55f);

            livePages.Add(tempObj);

            // move top two pages
            livePages[3].GetComponent<PageLogic>().move = true;
            livePages[4].GetComponent<PageLogic>().move = true;
            livePages[5].GetComponent<PageLogic>().move = true;
        }
    }

    IEnumerator DelayScore()
    {
        yield return new WaitForSeconds(8f / 60f);

        if (!levelFailed)
            score++;

        if (score % 10 == 0 && score > 1)
        {
            StartCoroutine(ChangeMatColor());
        }
    }

    IEnumerator ChangeMatColor()
    {
        float timer = 0, totalTimer = Random.Range(10, 25);
        Color startColor = bgMat.color;
        Color endColor = startColor;

        inci++;

        switch (inci)
        {
            case 0:
                endColor = new Color(0.2434f, 0.7803f, 0.7669f);
                break;
            case 1:
                endColor = new Color(0.7118544f, 0.7803922f, 0.4869646f);
                break;
            case 2:
                endColor = new Color(0.7803922f, 0.4571017f, 0.4073647f);
                break;
            case 3:
                endColor = new Color(0.7676717f, 0.5259843f, 0.7803922f);
                break;
            case 4:
                endColor = new Color(0.3925373f, 0.7803922f, 0.4571797f);
                break;
            case 5:
                endColor = new Color(0.3269843f, 0.4006146f, 0.7803922f);
                inci = -1;
                break;
            default:
                endColor = new Color(0.2434f, 0.7803f, 0.7669f);
                break;
        }

        while (timer <= totalTimer)
        {
            bgMat.color = Color.Lerp(startColor, endColor, timer / totalTimer);
            timer++;
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator FadeOutAudio(AudioSource myAudio)
    {
        float timer = 0, totalTime = 24;
        float startingLevel = myAudio.volume;
        while (timer <= totalTime)
        {
            myAudio.volume = Mathf.Lerp(startingLevel, 0, timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    private void Update()
    {
        if (cheatOn && PlayerPrefs.GetInt("eggyEnabled", 0) == 0) // turn on
        {
            for (int i = 0; i < socks.Length; i++)
                socks[i].SetActive(true);

            PlayerPrefs.SetInt("eggyEnabled", 1);
        }
        else if (!cheatOn && PlayerPrefs.GetInt("eggyEnabled", 0) == 1) // turn off
        {
            for (int i = 0; i < socks.Length; i++)
                socks[i].SetActive(false);

            PlayerPrefs.SetInt("eggyEnabled", 0);
        }

        if (moveBottom)
        {
            livePages[2].GetComponent<PageLogic>().move = true;
            livePages[1].GetComponent<PageLogic>().move = true;
            moveBottom = false;
        }
        if (bottomsTriggered)
        {
            camShake.SetTrigger("shake");

            StartCoroutine(DelayScore());
            scoreAnim.SetTrigger("bump");
            GameObject tempTrash = livePages[0];
            livePages.RemoveAt(0);
            Destroy(tempTrash);
            StartCoroutine(SpawnNewPage());

            mySoundManager.Play("PageLanding"+Random.Range(1,5)); // page landed

            bottomsTriggered = false;
        }
        if (levelStarted)
        {
            currentScore.text = score + "";
        }
        if (!restartLogic && levelFailed)
        {
            Transform tempObj = retryTexts[Random.Range(0, retryTexts.Length)].transform;
            SpriteRenderer retryTitle, retryBg;
            retryTitle = tempObj.GetComponent<SpriteRenderer>();
            retryBg = tempObj.GetComponentsInChildren<SpriteRenderer>()[1];

            if (score > PlayerPrefs.GetInt("highScore", 0))
            {
                PlayerPrefs.SetInt("highScore", score);
            }

            StartCoroutine(RetryLiterature(retryTitle, retryBg));
            StartCoroutine(RestartWait());
            restartLogic = true;
        }

        if (!startLogic && levelStarted)
        {
            foreach (SpriteRenderer sprite in soundIcons)
            {
                StartCoroutine(FadeImageOut(sprite));
            }

            StartCoroutine(FadeOutAudio(mainMenuMusic));

            StartCoroutine(FadeImageOut(gameTitle));
            StartCoroutine(FadeImageOut(gameTitleBG));
            StartCoroutine(FadeImageOut(controlsTitle));
            StartCoroutine(FadeImageOut(controllsTitleBG));
            StartCoroutine(FadeImageOut(highscoreTitle));
            StartCoroutine(FadeImageOut(highscoreBG));

            StartCoroutine(SpawnNewPage());

            startLogic = true;
        }
        pageTurnSpeed = Mathf.Clamp((((Mathf.Min(score, 30)-30)*-1)/30)*180+90, 162, 270); // from 270 to 90
    }

    IEnumerator StartLogic()
    {
        whiteSquare.enabled = true;
        whiteSquare.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FadeImageOut(whiteSquare));
    }

    IEnumerator RetryLiterature(SpriteRenderer mainText, SpriteRenderer bgText)
    {
        float timer = 0, totalTime = 40;
        Color startingColor1 = mainText.color;
        Color startingColor2 = bgText.color;
        Transform textTransform = mainText.gameObject.transform.parent.transform;

        Vector3 startingScale = textTransform.localScale;

        while (timer <= totalTime)
        {
            if (timer <= 18)
                textTransform.localScale = Vector3.Lerp(startingScale*0.1f, startingScale * 1.7f, timer / (totalTime-18));

            if (timer < totalTime * 0.75f)
            {
                mainText.color = Color.Lerp(startingColor1, new Color(startingColor1.r, startingColor1.g, startingColor1.b, 1), timer / (totalTime*0.7f));
                bgText.color = Color.Lerp(startingColor2, new Color(startingColor2.r, startingColor2.g, startingColor2.b, 1), timer / (totalTime*0.7f));
            }

            yield return new WaitForFixedUpdate();
            timer++;
        }

        timer = 0;
        totalTime = 80;
        startingScale = textTransform.localScale;
        while (timer <= totalTime)
        {
            textTransform.localScale = Vector3.Lerp(startingScale, new Vector3(startingScale.x*1.15f, startingScale.y*1.5f, startingScale.z*1.5f), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    IEnumerator RestartWait()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        StartCoroutine(RestartLevel(whiteSquare));
    }

    IEnumerator RestartLevel(SpriteRenderer myImage)
    {
        float timer = 0, totalTime = 24;
        Color startingColor = myImage.color;
        myImage.enabled = true;
        while (timer <= totalTime)
        {
            myImage.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 0), new Color(startingColor.r, startingColor.g, startingColor.b, 1), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }



    IEnumerator FadeImageOut(SpriteRenderer myImage)
    {
        float timer = 0, totalTime = 24;
        Color startingColor = myImage.color;
        myImage.enabled = true;
        while (timer <= totalTime)
        {
            myImage.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 1), new Color(startingColor.r, startingColor.g, startingColor.b, 0), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
        myImage.enabled = false;
    }

    IEnumerator FadeImageIn(SpriteRenderer myImage, float totalTime)
    {
        float timer = 0;
        Color startingColor = myImage.color;
        myImage.enabled = true;
        while (timer <= totalTime)
        {
            myImage.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 0), new Color(startingColor.r, startingColor.g, startingColor.b, 1), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    IEnumerator FadeTextOut(TextMeshPro myTtext)
    {
        float timer = 0, totalTime = 24;
        Color startingColor = myTtext.color;
        while (timer <= totalTime)
        {
            myTtext.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 1), new Color(startingColor.r, startingColor.g, startingColor.b, 0), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    IEnumerator FadeTextIn(TextMeshPro myTtext)
    {
        float timer = 0, totalTime = 24;
        Color startingColor = myTtext.color;
        while (timer <= totalTime)
        {
            myTtext.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 0), new Color(startingColor.r, startingColor.g, startingColor.b, 1), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }
}
