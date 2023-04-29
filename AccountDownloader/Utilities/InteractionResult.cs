namespace AccountDownloader
{
    // I didn't like the way interaction error handling was meant to be handled(With a global handler), this will let us pass the result back and forth more easily
    public struct InteractionResult<T>
    {
        public readonly T? Result { get; }
        public readonly string? Error { get; }
        public readonly bool HasResult { get; }

        private InteractionResult(T result)
        {
            Result = result;
            Error = null;
            HasResult = true;
        }
        private InteractionResult(bool hasResult, string error)
        {
            HasResult = hasResult;
            Result = default(T);
            Error = error;
        }

        public static InteractionResult<T> WithError(string error)
        {
            return new InteractionResult<T>(false, error);
        }
        public static InteractionResult<T> WithResult(T error)
        {
            return new InteractionResult<T>(error);
        }
    }
}
