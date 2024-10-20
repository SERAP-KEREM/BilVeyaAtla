using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections.Generic;

public interface IQuestionUI
{
    void SetQuestionUI(Question question);
    void ShowCorrectAnswerFeedback();
    void ShowIncorrectAnswerFeedback();
    void UpdatePlayerScores(Dictionary<int, int> playerScores);
}

