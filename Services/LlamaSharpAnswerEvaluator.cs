using LLama;
using LLama.Common;
using LLama.Sampling;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Noteflow.Services
{
    public class LlamaSharpAnswerEvaluator : IAnswerEvaluator
    {
        private readonly string _modelPath;
        private readonly SemaphoreSlim _mutex = new SemaphoreSlim(1, 1);
        private LLamaWeights? _weights;
        private LLamaContext? _context;
        private InteractiveExecutor? _executor;

        public LlamaSharpAnswerEvaluator(string modelPath)
        {
            _modelPath = modelPath;
        }

        public async Task<AnswerEvaluationResult> EvaluateAsync(string front, string back, string userAnswer)
        {
            if (!File.Exists(_modelPath))
            {
                return AnswerEvaluationResult.NotConfigured(
                    $"Kein lokales Modell gefunden. Lege das Modell unter '{_modelPath}' ab.");
            }

            await _mutex.WaitAsync();
            try
            {
                EnsureModelLoaded();

                var history = new ChatHistory();
                history.AddMessage(AuthorRole.System, BuildSystemPrompt());

                var session = new ChatSession(_executor!, history);
                var inferenceParams = new InferenceParams
                {
                    MaxTokens = 256,
                    SamplingPipeline = new DefaultSamplingPipeline(),
                    AntiPrompts = new[] { "User:" }
                };

                var output = new StringBuilder();
                var userMessage = new ChatHistory.Message(
                    AuthorRole.User,
                    BuildUserPrompt(front, back, userAnswer));
                await foreach (var text in session.ChatAsync(userMessage, inferenceParams))
                {
                    output.Append(text);
                }

                return ParseResult(output.ToString());
            }
            catch (Exception ex)
            {
                return new AnswerEvaluationResult
                {
                    Verdict = AnswerVerdict.Error,
                    Message = $"LLM-Fehler: {ex.Message}"
                };
            }
            finally
            {
                _mutex.Release();
            }
        }

        private void EnsureModelLoaded()
        {
            if (_executor != null)
            {
                return;
            }

            var parameters = new ModelParams(_modelPath)
            {
                ContextSize = 2048,
                GpuLayerCount = 0
            };

            _weights = LLamaWeights.LoadFromFile(parameters);
            _context = _weights.CreateContext(parameters);
            _executor = new InteractiveExecutor(_context);
        }

        private static string BuildSystemPrompt()
        {
            return "Du bist ein strenger Korrektor. Vergleiche die Antwort des Nutzers mit der Referenz. " +
                   "Antworte ausschliesslich als JSON: {\"verdict\":\"korrekt\"|\"falsch\",\"reason\":\"...\"}. " +
                   "Sei kurz und konkret.";
        }

        private static string BuildUserPrompt(string front, string back, string userAnswer)
        {
            return $"Vorderseite: {front}\n" +
                   $"Rueckseite: {back}\n" +
                   $"Antwort: {userAnswer}\n" +
                   "Bewerte die Antwort und gib nur JSON zurueck.";
        }

        private static AnswerEvaluationResult ParseResult(string raw)
        {
            var trimmed = raw.Trim();
            var jsonStart = trimmed.IndexOf('{');
            var jsonEnd = trimmed.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                return new AnswerEvaluationResult
                {
                    Verdict = AnswerVerdict.Error,
                    Message = $"Unerwartete Antwort: {trimmed}"
                };
            }

            var json = trimmed.Substring(jsonStart, jsonEnd - jsonStart + 1);
            try
            {
                using var doc = JsonDocument.Parse(json);
                var verdictRaw = doc.RootElement.GetProperty("verdict").GetString() ?? "";
                var reason = doc.RootElement.TryGetProperty("reason", out var reasonProp)
                    ? reasonProp.GetString() ?? ""
                    : "";

                var verdict = verdictRaw.ToLowerInvariant() switch
                {
                    "korrekt" => AnswerVerdict.Correct,
                    "richtig" => AnswerVerdict.Correct,
                    "correct" => AnswerVerdict.Correct,
                    "falsch" => AnswerVerdict.Incorrect,
                    "incorrect" => AnswerVerdict.Incorrect,
                    _ => AnswerVerdict.Error
                };

                var message = verdict switch
                {
                    AnswerVerdict.Correct => $"Korrekt. {reason}".Trim(),
                    AnswerVerdict.Incorrect => $"Falsch. {reason}".Trim(),
                    _ => $"Unerwartetes Urteil: {verdictRaw}. {reason}".Trim()
                };

                return new AnswerEvaluationResult
                {
                    Verdict = verdict,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                return new AnswerEvaluationResult
                {
                    Verdict = AnswerVerdict.Error,
                    Message = $"Konnte JSON nicht lesen: {ex.Message}"
                };
            }
        }
    }
}
