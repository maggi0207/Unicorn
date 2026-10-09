using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components;

/// <summary>
/// SubjectivityQuestionNode
/// </summary>
public partial class SubjectivityQuestionNode
{
    /// <summary>
    /// Question
    /// </summary>
    [Parameter]
    public EmployerPortalQuestionProxy Question { get; set; } = default!;
    /// <summary>
    /// AllQuestions
    /// </summary>
    [Parameter]
    public EmployerPortalQuestionProxy[]? AllQuestions { get; set; }
    /// <summary>
    /// Answers
    /// </summary>
    [Parameter]
    public Dictionary<int, string> Answers { get; set; } = new();
    /// <summary>
    /// AnswersChanged
    /// </summary>
    [Parameter]
    public EventCallback<Dictionary<int, string>> AnswersChanged { get; set; }
    /// <summary>
    /// FormSubmitted
    /// </summary>
    [Parameter]
    public bool FormSubmitted { get; set; }

    private bool _isTouched;

    private string CurrentAnswer
    {
        get => Answers.TryGetValue(Question.EmployerPortalQuestionSK, out var val) ? val : string.Empty;

        set => Answers[Question.EmployerPortalQuestionSK] = value;
    }

    private void HandleBlur()
    {
        _isTouched = true;
    }

    private bool ShouldShowError()
    {
        return Question.ControlTypeCodeSK is not 0 and not 9 && (FormSubmitted || _isTouched) && (IsRequiredAndEmpty() || IsInvalidDate() || IsInvalidQuarterYear() || IsTextTooLong());
    }

    private bool IsRequiredAndEmpty()
    {
        return Question.ControlTypeCodeSK != 0 &&
               Question.ControlTypeCodeSK != 9 &&
               string.IsNullOrWhiteSpace(CurrentAnswer);
    }

    private bool IsInvalidDate()
    {
        if (Question.ControlTypeCodeSK != 2 || string.IsNullOrWhiteSpace(CurrentAnswer))
        {
            return false;
        }

        if (!DateTime.TryParse(CurrentAnswer, out var date))
        {
            return true;
        }

        var minYear = DateTime.Today.Year - 4;
        var minDate = new DateTime(minYear, 1, 1);
        return date < minDate || date > DateTime.Today;
    }

    private bool IsInvalidQuarterYear()
    {
        if (!IsQuarterYearQuestion() || string.IsNullOrWhiteSpace(CurrentAnswer))
        {
            return false;
        }

        var cleaned = CurrentAnswer.Trim().Replace("Q", "", StringComparison.OrdinalIgnoreCase);
        var parts = cleaned.Split('-');
        if (parts.Length != 2 || parts[0].Length != 1 || !char.IsDigit(parts[0][0]) || parts[1].Length != 4 || !parts[1].All(char.IsDigit))
        {
            return true;
        }

        if (!int.TryParse(parts[0], out var q) || q < 1 || q > 4)
        {
            return true;
        }

        if (!int.TryParse(parts[1], out var year))
        {
            return true;
        }

        var minYear = DateTime.Today.Year - 4;
        return year < minYear || year > DateTime.Today.Year;
    }

    private bool IsTextTooLong()
    {
        return Question.ControlTypeCodeSK == 1 &&
               !string.IsNullOrWhiteSpace(CurrentAnswer) &&
               CurrentAnswer.Length > 1000;
    }

    private async Task OnTextInput(string newValue)
    {
        CurrentAnswer = newValue;
        if (AnswersChanged.HasDelegate)
        {
            await AnswersChanged.InvokeAsync(Answers);
        }
    }

    private async Task OnNumericInput(string raw)
    {
        var digitsOnly = new string(raw.Where(char.IsDigit).ToArray());
        CurrentAnswer = digitsOnly.Length > 25 ? digitsOnly[..25] : digitsOnly;
        if (AnswersChanged.HasDelegate)
        {
            await AnswersChanged.InvokeAsync(Answers);
        }
    }

    private async Task OnAnswerChanged(string newValue)
    {
        var previousValue = CurrentAnswer;
        CurrentAnswer = newValue;
        _isTouched = true;

        if (!string.Equals(previousValue, newValue, StringComparison.Ordinal))
        {
            if (Question.SubsequentQuestions != null && Question.SubsequentQuestions.Length > 0)
            {
                foreach (var childQuestion in Question.SubsequentQuestions)
                {
                    PruneDescendants(childQuestion);
                }
            }
        }

        if (AnswersChanged.HasDelegate)
        {
            await AnswersChanged.InvokeAsync(Answers);
        }
    }

    private void PruneDescendants(EmployerPortalQuestionProxy question)
    {
        if (!IsChildVisible(question))
        {
            Answers.Remove(question.EmployerPortalQuestionSK);
        }

        if (question.SubsequentQuestions != null)
        {
            foreach (var child in question.SubsequentQuestions)
            {
                PruneDescendants(child);
            }
        }
    }

    private async Task OnSingleCheckboxToggle(string option)
    {
        _isTouched = true;
        var newAnswer = string.Equals(CurrentAnswer, option, StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : option;

        await OnAnswerChanged(newAnswer);
    }

    private (string Opt1Val, string Opt1Lbl, string Opt2Val, string Opt2Lbl) GetBinaryCheckboxLabels()
    {
        var isTrueFalse = Question.AllowableResponses != null &&
                          Question.AllowableResponses.Any(r => string.Equals(r, "true", StringComparison.OrdinalIgnoreCase));

        return isTrueFalse
            ? ("true", "True", "false", "False")
            : ("yes", "Yes", "no", "No");
    }

    private async Task OnMultiCheckboxToggle(string option)
    {
        _isTouched = true;
        var currentValues = (CurrentAnswer ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (currentValues.Contains(option, StringComparer.OrdinalIgnoreCase))
        {
            currentValues.RemoveAll(v => string.Equals(v, option, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            currentValues.Add(option);
        }

        var newAnswer = string.Join(",", currentValues);
        await OnAnswerChanged(newAnswer);
    }

    private bool IsMultiCheckboxSelected(string option)
    {
        return !string.IsNullOrWhiteSpace(CurrentAnswer) &&
               CurrentAnswer.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Contains(option, StringComparer.OrdinalIgnoreCase);
    }

    private List<string> GetCheckboxOptions()
    {
        return Question.AllowableResponses != null && Question.AllowableResponses.Length > 0
            ? Question.AllowableResponses.ToList()
            : ["yes", "no"];
    }

    private bool IsQuarterYearQuestion()
    {
        return string.Equals(Question.QuestionText, "Quarter and Year:", StringComparison.OrdinalIgnoreCase) ||
               (Question.AllowableResponses != null &&
                Question.AllowableResponses.Length == 1 &&
                string.Equals(Question.AllowableResponses[0], "Q-YYYY", StringComparison.OrdinalIgnoreCase));
    }

    private async Task OnQuarterYearInput(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString() ?? string.Empty;
        var digitsOnly = new string(raw.Where(char.IsDigit).ToArray());
        digitsOnly = digitsOnly.Length > 5 ? digitsOnly[..5] : digitsOnly;

        var formatted = digitsOnly.Length switch
        {
            0 => string.Empty,
            1 => digitsOnly + "-",
            _ => digitsOnly[..1] + "-" + digitsOnly[1..]
        };

        CurrentAnswer = formatted;
        if (AnswersChanged.HasDelegate)
        {
            await AnswersChanged.InvokeAsync(Answers);
        }
    }

    private List<RadioOption<string>> GetRadioOptions()
    {
        return Question.AllowableResponses != null && Question.AllowableResponses.Length > 0
            ? Question.AllowableResponses.Select(resp => new RadioOption<string>
            {
                Value = resp,
                Label = MapOptionLabel(resp)
            }).ToList()
            : [
            new() { Value = "yes", Label = "Yes" },
        new() { Value = "no", Label = "No" }
        ];
    }

    private static string MapOptionLabel(string resp)
    {
        return resp.ToLowerInvariant() switch
        {
            "yes" => "Yes",
            "no" => "No",
            "true" => "True",
            "false" => "False",
            _ => resp
        };
    }

    private List<SelectOption> GetDropdownOptions()
    {
        var options = new List<SelectOption>
    {
        new() { Value = "", Text = "-- Select --" }
    };

        if (Question.AllowableResponses != null)
        {
            foreach (var resp in Question.AllowableResponses)
            {
                options.Add(new SelectOption { Value = resp, Text = resp });
            }
        }

        return options;
    }

    private bool IsChildVisible(EmployerPortalQuestionProxy childQuestion)
    {
        if (string.IsNullOrWhiteSpace(CurrentAnswer))
        {
            return false;
        }

        if (Question.Conditions == null || Question.Conditions.Length == 0)
        {
            return true;
        }

        var specificConditions = Question.Conditions
            .Where(c => c.QuestionSK == childQuestion.EmployerPortalQuestionSK)
            .ToList();

        if (specificConditions.Count > 0)
        {
            foreach (var cond in specificConditions)
            {
                if (string.Equals(cond.Input, CurrentAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if ((cond.Input.Equals("dateValue", StringComparison.OrdinalIgnoreCase) ||
                     cond.Input.Equals("textValue", StringComparison.OrdinalIgnoreCase) ||
                     cond.Input.Equals("listValue", StringComparison.OrdinalIgnoreCase)) &&
                    !string.IsNullOrWhiteSpace(CurrentAnswer))
                {
                    return true;
                }
            }

            return false;
        }

        foreach (var cond in Question.Conditions.Where(c => c.QuestionSK == 0))
        {
            if (string.Equals(cond.Input, CurrentAnswer, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if ((cond.Input.Equals("dateValue", StringComparison.OrdinalIgnoreCase) ||
                 cond.Input.Equals("textValue", StringComparison.OrdinalIgnoreCase) ||
                 cond.Input.Equals("listValue", StringComparison.OrdinalIgnoreCase)) &&
                !string.IsNullOrWhiteSpace(CurrentAnswer))
            {
                return true;
            }
        }

        return false;
    }


    private DateTime? DateValue
    {
        get => DateTime.TryParse(CurrentAnswer, out var d) ? d : null;
        set
        {
            CurrentAnswer = value?.ToString("MM/dd/yyyy") ?? string.Empty;
            if (AnswersChanged.HasDelegate)
            {
                _ = AnswersChanged.InvokeAsync(Answers);
            }
        }
    }

    private bool IsSingleSelectCheckbox()
    {
        return Question.AllowableResponses == null ||
               Question.AllowableResponses.Length == 0 ||
               (Question.AllowableResponses.Length <= 2 &&
                (Question.AllowableResponses.Any(r => string.Equals(r, "true", StringComparison.OrdinalIgnoreCase)) ||
                 Question.AllowableResponses.Any(r => string.Equals(r, "yes", StringComparison.OrdinalIgnoreCase))));
    }
}
