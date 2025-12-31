import type { Configuration } from "@azure/msal-browser";
import { PublicClientApplication } from "@azure/msal-browser";

export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_ENTRA_CLIENT_ID!,
    authority: `https://${
      import.meta.env.VITE_ENTRA_TENANT_SUBDOMAIN
    }.ciamlogin.com/${
      import.meta.env.VITE_ENTRA_TENANT_SUBDOMAIN
    }.onmicrosoft.com`,
    knownAuthorities: [
      `${import.meta.env.VITE_ENTRA_TENANT_SUBDOMAIN}.ciamlogin.com`,
    ],
    redirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: "localStorage", // dev-friendly
    storeAuthStateInCookie: false,
  },
};

export const msalInstance = new PublicClientApplication(msalConfig);

const scopes = [`api://${import.meta.env.VITE_AUTH_AUDIENCE}/access_as_user`];

export const acquireAccessToken = async () => {
  await msalInstance.initialize();
  const activeAccount = msalInstance.getActiveAccount(); // This will only return a non-null value if you have logic somewhere else that calls the setActiveAccount API
  const accounts = msalInstance.getAllAccounts();

  if (!activeAccount && accounts.length === 0) {
    throw Error("NoAccessToken");
    /*
     * User is not signed in. Throw error or wait for user to login.
     * Do not attempt to log a user in outside of the context of MsalProvider
     */
  }
  const request = {
    scopes,
    account: activeAccount || accounts[0],
  };

  const authResult = await msalInstance.acquireTokenSilent(request);

  return authResult.accessToken;
};
