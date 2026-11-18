namespace StepByStepCoding.Security.Models
{
    public sealed class RefreshToken
    {
        public Guid Id
        {
            get; private set;
        }

        public string TokenHash { get; private set; } = null!;

        public Guid UserId
        {
            get; private set;
        }

        public DateTimeOffset CreatedAt
        {
            get; private set;
        }

        public DateTimeOffset ExpiresAt
        {
            get; private set;
        }

        private RefreshToken()

        {
        }

        public RefreshToken(Guid userId, string tokenHash, DateTimeOffset createdAt, DateTimeOffset expiresAt)
        {
            EnforceTokenBusinessRules(userId, tokenHash, createdAt, expiresAt);
            Id = Guid.CreateVersion7();
            UserId = userId;
            TokenHash = tokenHash;
            CreatedAt = createdAt.ToUniversalTime();
            ExpiresAt = expiresAt.ToUniversalTime();
        }

        private static void EnforceTokenBusinessRules(Guid userId, string tokenHash,
            DateTimeOffset createdAt, DateTimeOffset expiresAt)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("A user is required.", nameof(userId));

            if (string.IsNullOrWhiteSpace(tokenHash) || tokenHash.Length != 64 || !tokenHash.All(Uri.IsHexDigit))
                throw new ArgumentException("A SHA-256 token hash is required.", nameof(tokenHash));

            if (expiresAt <= createdAt)
                throw new ArgumentException("Expiration must follow creation.", nameof(expiresAt));
        }
    }
}
