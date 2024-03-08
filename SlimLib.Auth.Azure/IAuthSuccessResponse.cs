namespace SlimLib.Auth.Azure
{
    public interface IAuthSuccessResponse
    {
        string AccessToken { get; }
        int ExpiresIn { get; }
        string TokenType { get; }
    }
}