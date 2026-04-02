namespace SettlersOfCrutan.Presentation.Auth;

/// <summary>Names for development-only user impersonation (never honored outside Development).</summary>
public static class DevUserImpersonation
{
    public const string HeaderName = "X-Dev-User-Id";
    public const string SignalRQueryParameter = "dev_user_id";
}
