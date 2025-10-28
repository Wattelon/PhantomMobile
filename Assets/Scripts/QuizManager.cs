using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class QuestionData
{
    public string questionText;
    public string correctAnswer;
    public Texture2D questionImage;
}

public class QuizManager : MonoBehaviour
{
    [Header("Questions")]
    public List<QuestionData> questions = new();

    [Header("UI Elements")]
    private Label _questionText;
    private TextField _answerField;
    private Button _submitButton;
    private Label _questionCount;
    private Label _timerLabel;
    private VisualElement _questionImage;
    private VisualElement _resultsPanel;
    private VisualElement _testPanel;
    private Label _resultsText;

    private int _currentQuestionIndex;
    private int _correctAnswers;
    private float _elapsedTime;
    private bool _isTestActive = true;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        _questionText   = root.Q<Label>("QuestionText");
        _answerField    = root.Q<TextField>("AnswerField");
        _submitButton   = root.Q<Button>("SubmitButton");
        _questionCount  = root.Q<Label>("QuestionCount");
        _timerLabel     = root.Q<Label>("Timer");
        _questionImage  = root.Q<VisualElement>("QuestionImage");
        _resultsPanel   = root.Q<VisualElement>("ResultsPanel");
        _testPanel   = root.Q<VisualElement>("TestPanel");
        _resultsText    = root.Q<Label>("ResultsText");

        
    }

    private void Start()
    {
        ShowQuestion();
    }

    private void OnEnable()
    {
        _submitButton.RegisterCallback<ClickEvent>(OnSubmitSubmitted);
    }
    
    private void OnDisable()
    {
        _submitButton.UnregisterCallback<ClickEvent>(OnSubmitSubmitted);
    }

    private void Update()
    {
        if (_isTestActive)
        {
            _elapsedTime += Time.deltaTime;
            _timerLabel.text = $"Время: {_elapsedTime:F1} c";
        }
    }

    private void ShowQuestion()
    {
        if (_currentQuestionIndex >= questions.Count)
        {
            EndTest();
            return;
        }

        var q = questions[_currentQuestionIndex];
        _questionText.text = q.questionText;
        _answerField.value = "";

        _questionCount.text = $"Вопрос {_currentQuestionIndex + 1}/{questions.Count}";
        
        if (q.questionImage != null)
        {
            _questionImage.style.backgroundImage = new StyleBackground(q.questionImage);
            _questionImage.style.display = DisplayStyle.Flex;
        }
        else
        {
            _questionImage.style.display = DisplayStyle.None;
        }
    }

    private void OnSubmitSubmitted(ClickEvent evt)
    {
        var userAnswer = _answerField.value.Trim().ToLower();
        var correct = questions[_currentQuestionIndex].correctAnswer.Trim().ToLower();

        if (userAnswer == correct)
            _correctAnswers++;

        _currentQuestionIndex++;

        if (_currentQuestionIndex < questions.Count)
            ShowQuestion();
        else
            EndTest();
    }

    private void EndTest()
    {
        _isTestActive = false;
        _testPanel.style.display = DisplayStyle.None;
        _resultsPanel.style.display = DisplayStyle.Flex;
        _resultsText.text = $"Тест завершён!\n" +
                           $"Правильных ответов: {_correctAnswers} из {questions.Count}\n" +
                           $"Время прохождения: {_elapsedTime:F1} секунд";
    }
}
