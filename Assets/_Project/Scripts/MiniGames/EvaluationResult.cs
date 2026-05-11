namespace UnityFramework.MiniGames.Gameplay
{
    public readonly struct EvaluationResult
    {
        public EvaluationResult(bool correct, string[] conceptKeys, float latencySeconds)
        {
            Correct = correct;
            ConceptKeys = conceptKeys ?? System.Array.Empty<string>();
            LatencySeconds = latencySeconds;
        }

        public bool Correct { get; }
        public string[] ConceptKeys { get; }
        public float LatencySeconds { get; }
    }
}
