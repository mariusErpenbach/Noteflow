namespace Noteflow.Services
{
    public enum AnswerVerdict
    {
        Correct,
        Incorrect,
        NotConfigured,
        Error
    }

    public class AnswerEvaluationResult
    {
        public AnswerVerdict Verdict { get; set; }
        public string Message { get; set; } = string.Empty;

        public static AnswerEvaluationResult NotConfigured(string message)
        {
            return new AnswerEvaluationResult
            {
                Verdict = AnswerVerdict.NotConfigured,
                Message = message
            };
        }
    }
}
