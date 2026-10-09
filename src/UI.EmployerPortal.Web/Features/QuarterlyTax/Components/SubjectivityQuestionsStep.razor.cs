using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components;

/// <summary>
/// SubjectivityQuestionsStep
/// </summary>
public partial class SubjectivityQuestionsStep
{
    /// <summary>
    /// Categories
    /// </summary>
    [Parameter]
    public EmployerPortalPayrollQuestionsCategoryProxy[] Categories { get; set; } = [];
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
    /// ShowValidation
    /// </summary>
    [Parameter]
    public bool ShowValidation { get; set; }
    /// <summary>
    /// FieldIds
    /// </summary>
    [Parameter]
    public Dictionary<string, string> FieldIds { get; set; } = new();
    /// <summary>
    /// EditContext
    /// </summary>
    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    private ValidationMessageStore? _messageStore;

    private readonly HashSet<int> _submittedQuestionSKs = [];

    private string BannerMessage => GetBannerMessage();

    private string GetBannerMessage()
    {
        if (Categories == null || Categories.Length == 0)
        {
            return string.Empty;
        }

        foreach (var category in Categories)
        {
            if (category?.TaxThresholds == null)
            {
                continue;
            }

            foreach (var threshold in category.TaxThresholds)
            {
                if (threshold == null)
                {
                    continue;
                }

                // 1. Direct Message on threshold if populated by backend
                if (!string.IsNullOrWhiteSpace(threshold.Message))
                {
                    return threshold.Message;
                }

            }
        }

        return string.Empty;
    }

    /// <summary>
    /// OnParametersSet
    /// </summary>
    protected override void OnParametersSet()
    {

        if (EditContext != null && _messageStore == null)
        {
            _messageStore = new ValidationMessageStore(EditContext);
        }

        if (ShowValidation && _submittedQuestionSKs.Count == 0)
        {
            RecordSubmittedSnapshot();
        }

    }

    /// <summary>
    /// IsValid
    /// </summary>
    /// <returns></returns>
    public bool IsValid()
    {
        RecordSubmittedSnapshot();

        ShowValidation = true;

        return Revalidate(scrollToTopOnInvalid: true);
    }

    private void RecordSubmittedSnapshot()
    {

        _submittedQuestionSKs.Clear();

        if (Categories != null)
        {

            foreach (var category in Categories)
            {

                if (category.TaxThresholds == null)
                {
                    continue;
                }

                foreach (var threshold in category.TaxThresholds)
                {

                    if (threshold.EmployerPortalPayrollQuestions == null)
                    {
                        continue;
                    }

                    var active = GetActiveQuestions(threshold.EmployerPortalPayrollQuestions);

                    foreach (var q in active)
                    {
                        _submittedQuestionSKs.Add(q.EmployerPortalQuestionSK);
                    }
                }
            }
        }
    }

    private bool Revalidate(bool scrollToTopOnInvalid)
    {
        FieldIds.Clear();

        _messageStore?.Clear();

        var firstMissingInputId = (string?) null;

        var hasMissingRequired = false;

        var formatErrors = new List<(string Key, string InputId, string Message)>();

        if (Categories != null)
        {
            foreach (var category in Categories)
            {
                if (category.TaxThresholds == null)
                {
                    continue;
                }

                foreach (var threshold in category.TaxThresholds)
                {
                    if (threshold.EmployerPortalPayrollQuestions == null)
                    {
                        continue;
                    }

                    var active = GetActiveQuestions(threshold.EmployerPortalPayrollQuestions);

                    foreach (var q in active)
                    {
                        if (q.ControlTypeCodeSK is 0 or 9)
                        {
                            continue;
                        }

                        var inputId = GetInputId(q);

                        if (!Answers.TryGetValue(q.EmployerPortalQuestionSK, out var val) || string.IsNullOrWhiteSpace(val))
                        {
                            hasMissingRequired = true;

                            firstMissingInputId ??= inputId;

                        }
                        else
                        {
                            if (q.ControlTypeCodeSK == 2 && !IsValidDate(val))
                            {
                                var minYear = DateTime.Today.Year - 4;

                                var dateKey = $"Question_Date_{q.EmployerPortalQuestionSK}";

                                var msg = $"'{q.QuestionText}' must be a valid date between 01/01/{minYear} and today.";

                                FieldIds[dateKey] = inputId;

                                formatErrors.Add((dateKey, inputId, msg));

                                if (EditContext != null && _messageStore != null)
                                {
                                    _messageStore.Add(EditContext.Field(dateKey), msg);
                                }

                            }

                            else if (IsQuarterYearQuestion(q) && !IsValidQuarterYear(val))
                            {
                                var minYear = DateTime.Today.Year - 4;

                                var qyKey = $"Question_QY_{q.EmployerPortalQuestionSK}";

                                var msg = $"'{q.QuestionText}' must be a valid Quarter and Year between 1-{minYear} and 4-{DateTime.Today.Year} (e.g. 1-2024).";

                                FieldIds[qyKey] = inputId;

                                formatErrors.Add((qyKey, inputId, msg));

                                if (EditContext != null && _messageStore != null)
                                {
                                    _messageStore.Add(EditContext.Field(qyKey), msg);
                                }

                            }
                            else if (q.ControlTypeCodeSK == 1 && val.Length > 1000)
                            {
                                var textKey = $"Question_Text_{q.EmployerPortalQuestionSK}";

                                var msg = $"'{q.QuestionText}' cannot exceed 1000 characters.";

                                FieldIds[textKey] = inputId;

                                formatErrors.Add((textKey, inputId, msg));

                                if (EditContext != null && _messageStore != null)
                                {
                                    _messageStore.Add(EditContext.Field(textKey), msg);
                                }
                            }
                        }
                    }
                }
            }
        }
        if (hasMissingRequired && firstMissingInputId != null)
        {
            var summaryKey = "Subjectivity_Summary";

            FieldIds[summaryKey] = firstMissingInputId;

            if (EditContext != null && _messageStore != null)
            {
                _messageStore.Add(EditContext.Field(summaryKey), "Please complete all required subjectivity questions.");
            }

        }
        foreach (var (key, inputId, message) in formatErrors)
        {
            FieldIds[key] = inputId;

            if (EditContext != null && _messageStore != null)
            {
                _messageStore.Add(EditContext.Field(key), message);
            }

        }

        if (EditContext != null)
        {
            EditContext.NotifyValidationStateChanged();
        }

        var overallValid = !hasMissingRequired && formatErrors.Count == 0;

        if (!overallValid && scrollToTopOnInvalid)
        {

            _ = JSRuntime.InvokeVoidAsync("scrollToTop");

        }

        return overallValid;

    }

    private static string GetInputId(EmployerPortalQuestionProxy q)
    {
        return q.ControlTypeCodeSK switch
        {
            5 => $"question_radio_{q.EmployerPortalQuestionSK}_opt0",
            4 => $"question_input_{q.EmployerPortalQuestionSK}_opt1",
            _ => $"question_input_{q.EmployerPortalQuestionSK}"
        };
    }

    private static bool IsQuarterYearQuestion(EmployerPortalQuestionProxy q)
    {
        return string.Equals(q.QuestionText, "Quarter and Year:", StringComparison.OrdinalIgnoreCase) ||
               (q.AllowableResponses != null &&
                q.AllowableResponses.Length == 1 &&
                string.Equals(q.AllowableResponses[0], "Q-YYYY", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsValidDate(string val)
    {
        if (string.IsNullOrWhiteSpace(val))
        {
            return false;
        }

        if (!DateTime.TryParse(val, out var date))
        {
            return false;
        }

        var minYear = DateTime.Today.Year - 4;

        var minDate = new DateTime(minYear, 1, 1);

        return date >= minDate && date <= DateTime.Today;
    }

    private static bool IsValidQuarterYear(string val)
    {
        if (string.IsNullOrWhiteSpace(val))
        {
            return false;
        }

        var cleaned = val.Trim().Replace("Q", "", StringComparison.OrdinalIgnoreCase);

        var parts = cleaned.Split('-');

        if (parts.Length != 2 || parts[0].Length != 1 || !char.IsDigit(parts[0][0]) || parts[1].Length != 4 || !parts[1].All(char.IsDigit))
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var q) || q < 1 || q > 4)
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var year))
        {
            return false;
        }

        var minYear = DateTime.Today.Year - 4;
        return year >= minYear && year <= DateTime.Today.Year;
    }

    private List<EmployerPortalQuestionProxy> GetActiveQuestions(EmployerPortalQuestionProxy[] allQuestions)
    {
        var result = new List<EmployerPortalQuestionProxy>();

        if (allQuestions == null || allQuestions.Length == 0)
        {
            return result;
        }

        var visitedSKs = new HashSet<int>();

        var rootQuestions = allQuestions
            .Where(q => !IsConditionalChild(q, allQuestions))
            .OrderBy(q => q.SortOrder);

        foreach (var root in rootQuestions)
        {
            CollectActiveTree(root, allQuestions, result, visitedSKs);

        }
        return result;
    }

    private void CollectActiveTree(
        EmployerPortalQuestionProxy current,
        EmployerPortalQuestionProxy[] allQuestions,
        List<EmployerPortalQuestionProxy> result,
        HashSet<int> visitedSKs)
    {
        if (current == null || !visitedSKs.Add(current.EmployerPortalQuestionSK))
        {
            return;
        }

        result.Add(current);

        if (current.SubsequentQuestions == null || current.SubsequentQuestions.Length == 0)
        {
            return;
        }

        var currentAnswer = Answers.TryGetValue(current.EmployerPortalQuestionSK, out var val) ? val : string.Empty;

        if (string.IsNullOrWhiteSpace(currentAnswer))
        {
            return;
        }

        var visibleChildren = current.SubsequentQuestions
            .Where(child => IsConditionMet(current, child, currentAnswer))
            .OrderBy(child => child.SortOrder);

        foreach (var child in visibleChildren)
        {
            CollectActiveTree(child, allQuestions, result, visitedSKs);
        }
    }

    private bool IsConditionalChild(EmployerPortalQuestionProxy question, EmployerPortalQuestionProxy[] allQuestions)
    {
        foreach (var parent in allQuestions)
        {
            if (parent.EmployerPortalQuestionSK == question.EmployerPortalQuestionSK)
            {
                continue;
            }

            if (parent.SubsequentQuestions != null && parent.SubsequentQuestions.Any(c => c.EmployerPortalQuestionSK == question.EmployerPortalQuestionSK))
            {
                if (parent.Conditions != null && parent.Conditions.Any(c => c.QuestionSK == 0 || c.QuestionSK == question.EmployerPortalQuestionSK))
                {
                    return true;
                }

            }

        }

        return false;
    }

    private bool IsConditionMet(EmployerPortalQuestionProxy parent, EmployerPortalQuestionProxy child, string parentAnswer)
    {
        if (parent.Conditions == null || parent.Conditions.Length == 0)
        {
            return true;
        }

        var specificConditions = parent.Conditions
            .Where(c => c.QuestionSK == child.EmployerPortalQuestionSK)
            .ToList();

        if (specificConditions.Count > 0)
        {
            foreach (var cond in specificConditions)
            {
                if (string.Equals(cond.Input, parentAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if ((cond.Input.Equals("dateValue", StringComparison.OrdinalIgnoreCase) ||
                     cond.Input.Equals("textValue", StringComparison.OrdinalIgnoreCase) ||
                     cond.Input.Equals("listValue", StringComparison.OrdinalIgnoreCase)) &&
                    !string.IsNullOrWhiteSpace(parentAnswer))
                {
                    return true;
                }
            }

            return false;
        }

        // Fallback for non-branching questions where QuestionSK is 0
        foreach (var cond in parent.Conditions.Where(c => c.QuestionSK == 0))
        {
            if (string.Equals(cond.Input, parentAnswer, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if ((cond.Input.Equals("dateValue", StringComparison.OrdinalIgnoreCase) ||
                 cond.Input.Equals("textValue", StringComparison.OrdinalIgnoreCase) ||
                 cond.Input.Equals("listValue", StringComparison.OrdinalIgnoreCase)) &&
                !string.IsNullOrWhiteSpace(parentAnswer))
            {
                return true;
            }
        }

        return false;
    }


    private async Task OnAnswersChanged(Dictionary<int, string> updatedAnswers)
    {
        Answers = updatedAnswers;

        if (AnswersChanged.HasDelegate)
        {
            await AnswersChanged.InvokeAsync(Answers);

        }

        if (_submittedQuestionSKs.Count > 0)
        {
            Revalidate(scrollToTopOnInvalid: false);
        }
    }
}
