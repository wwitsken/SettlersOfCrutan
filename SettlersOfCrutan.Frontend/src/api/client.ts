import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./types";
import { acquireAccessToken } from "../../authConfig";

let accessToken: string | undefined = undefined;

const authMiddleware: Middleware = {
  async onRequest({ request }) {
    // fetch token, if it doesn’t exist
    if (!accessToken) {
      try {
        accessToken = await acquireAccessToken();
      } catch {
        console.warn(
          "No access token found for request. Attempting unauthenticated request."
        );
        accessToken = undefined;
      }
    }

    request.headers.set("Authorization", `Bearer ${accessToken}`);
    return request;
  },
};

const api = createClient<paths>({
  baseUrl: "",
  fetch: async (req: Request) => {
    const headers = new Headers(req.headers);
    const newReq = new Request(req, { headers });
    return fetch(newReq);
  },
});

api.use(authMiddleware);

export { api };
