import type { Configuration, SilentRequest } from "@azure/msal-browser";
import { PublicClientApplication } from "@azure/msal-browser";
import { isDevImpersonationActive } from "./api/devSessionUser";

export const msalConfig: Configuration = {
  auth: {
    clientId: "767bc2e9-1317-41b2-87c6-f77fa8683dc3",
    authority:
      "https://settlersofcrutan.ciamlogin.com/63ba9d01-deb0-4f35-bca3-756e31e2c98c",
  },
  cache: {
    cacheLocation: "localStorage", // dev-friendly
    storeAuthStateInCookie: false,
  },
};

export const msalInstance = new PublicClientApplication(msalConfig);
//await createStandardPublicClientApplication(msalConfig);

export const loginRequest = {
  scopes: ["api://76494224-a444-443a-9dee-39ac7271494e/access_as_user"],
};

export const acquireAccessToken = async () => {
  const activeAccount = msalInstance.getActiveAccount(); // This will only return a non-null value if you have logic somewhere else that calls the setActiveAccount API
  const accounts = msalInstance.getAllAccounts();

  if (!activeAccount && accounts.length === 0) {
    throw Error("NoAccessToken");
    /*
     * User is not signed in. Throw error or wait for user to login.
     * Do not attempt to log a user in outside of the context of MsalProvider
     */
  }
  const apiScope = import.meta.env.VITE_AUTH_AUDIENCE as string | undefined;
  const request: SilentRequest = {
    account: activeAccount || accounts[0]!,
    scopes: apiScope ? [apiScope] : loginRequest.scopes,
  };

  const authResult = await msalInstance.acquireTokenSilent(request);

  return authResult.accessToken;
};

/** Silent MSAL token for API/SignalR; returns undefined if not signed in or silent acquisition fails. */
export async function getAccessTokenForApi(): Promise<string | undefined> {
  try {
    return await acquireAccessToken();
  } catch {
    return undefined;
  }
}

/** Token for openapi-fetch calls: in dev with mock user id, MSAL is optional. */
export async function getAccessTokenForOpenApi(): Promise<string | undefined> {
  if (isDevImpersonationActive()) return getAccessTokenForApi();
  try {
    return await acquireAccessToken();
  } catch {
    return undefined;
  }
}
