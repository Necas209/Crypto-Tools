namespace CryptoLib;

public sealed record RegisterRequest(string UserName, string Password);

public sealed record LoginRequest(string UserName, string Password);

public sealed record LoginResponse(string AccessToken);

public sealed record SignatureRequest(string Hash);

public sealed record VerifyRequest(string Hash, string Signature);

public sealed record ExchangeRequest(byte[] PublicKey);