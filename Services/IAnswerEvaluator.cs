using System.Threading.Tasks;

namespace Noteflow.Services
{
    public interface IAnswerEvaluator
    {
        Task<AnswerEvaluationResult> EvaluateAsync(string front, string back, string userAnswer);
    }
}
