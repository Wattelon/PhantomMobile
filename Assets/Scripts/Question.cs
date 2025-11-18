using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Question
{
    public string Text;
    public List<string> Answers;
    public Texture2D Image;

    public bool CheckAnswer(string answer)
    {
        return Answers.Contains(answer.ToLower().Trim());
    }
}