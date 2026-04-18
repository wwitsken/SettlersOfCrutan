/** Must match SettlersOfCrutan.Presentation.Auth.DevUserImpersonation */
export const DEV_SESSION_USER_STORAGE_KEY = "settlers-dev-user-id";
export const DEV_USER_HEADER = "X-Dev-User-Id";
export const DEV_USER_QUERY_PARAM = "dev_user_id";

export function readDevSessionUserIdFromStorage(): string {
  if (!import.meta.env.DEV || typeof sessionStorage === "undefined") return "";
  return sessionStorage.getItem(DEV_SESSION_USER_STORAGE_KEY)?.trim() ?? "";
}

export function isDevImpersonationActive(): boolean {
  return readDevSessionUserIdFromStorage().length > 0;
}
