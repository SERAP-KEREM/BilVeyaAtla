using System.Collections.Generic;

public class Question
{
    public string QuestionText { get; set; }
    public List<string> Options { get; set; }
    public string CorrectAnswer { get; set; }

    public Question(string questionText, List<string> options, string correctAnswer)
    {
        QuestionText = questionText;
        Options = options;
        CorrectAnswer = correctAnswer;
    }

}
