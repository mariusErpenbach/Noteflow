using System.IO;
using System.Threading.Tasks;

namespace Noteflow.Services
{
    public class StubAnswerEvaluator : IAnswerEvaluator
    {
        private readonly string _modelPath;

        public StubAnswerEvaluator(string modelPath)
        {
            _modelPath = modelPath;
        }

        public Task<AnswerEvaluationResult> EvaluateAsync(string front, string back, string userAnswer)
        {
            if (!File.Exists(_modelPath))
            {
                return Task.FromResult(AnswerEvaluationResult.NotConfigured(
                    $"Kein lokales Modell gefunden. Lege ein GGUF-Modell unter '{_modelPath}' ab."));
            }

            return Task.FromResult(AnswerEvaluationResult.NotConfigured(
                "LLM-Integration ist noch nicht aktiviert."));
        }
    }
}
