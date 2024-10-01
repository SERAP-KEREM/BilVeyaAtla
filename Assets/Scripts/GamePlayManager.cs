using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
public class GamePlayManager : Singleton<GamePlayManager>
{
    public GameObject[] options;
    public TextMeshProUGUI questionText;
    public RawImage questionImage;
    private DBManager DB;
    private string correctOption;
    void Start()
    {
        DB = DBManager.Instance;
        StartCoroutine(CheckIfIsDoneTrue());


    }
    private IEnumerator CheckIfIsDoneTrue()
    {
        if (DB.isDone)
        {
            StartCoroutine(DB.GetQuestions());
        }
        else
        {

            yield return new WaitForSeconds(1);
            StartCoroutine(CheckIfIsDoneTrue());
        }


    }

    public void GetQuestion(string question)
    {
        questionText.text = question;
    }
    public void GetOptions(string a, string b, string c, string d, string correct)
    {
        options[0].transform.GetChild(0).GetComponent<Text>().text = a;
        options[1].transform.GetChild(0).GetComponent<Text>().text = b;
        options[2].transform.GetChild(0).GetComponent<Text>().text = c;
        options[3].transform.GetChild(0).GetComponent<Text>().text = d;
        correctOption = correct;
    }
    public IEnumerator LoadQuestionImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);

        else
        {
            questionImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }
    public void Answer(string option)
    {

        Dictionary<string, int> optionsDict = new Dictionary<string, int>();
        optionsDict["A"] = 1;
        optionsDict["B"] = 2;
        optionsDict["C"] = 3;
        optionsDict["D"] = 4;
        if (option = correctOption)
        {

            // do?ru
            int index = optionsDict[option];
            options[index - 1].GetComponent<Image>().color = Color.green;
        }
        else
        {
            // yanl??
            int index = optionsDict[option];
            options[index - 1].GetComponent<Image>().color = Color.red;
            StartCoroutine(CloseWrongAnswer(index));
        }


    }
    private IEnumerator CloseWrongAnswer(int index)
    {
        yield return new WaitForSeconds(1);
        options[index - 1].gameObject.SetActive(false);
    }
}