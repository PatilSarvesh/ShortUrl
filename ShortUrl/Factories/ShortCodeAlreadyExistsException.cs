namespace ShortUrl.Factories
{
    public sealed class ShortCodeAlreadyExistsException : Exception
    {
        public ShortCodeAlreadyExistsException(string shortCode)
            : base($"The short code '{shortCode}' is already in use.")
        {
        }
    }
}
