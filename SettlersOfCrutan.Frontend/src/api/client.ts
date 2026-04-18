import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./types";
import { getAccessTokenForApi } from "../authConfig";
import {
  DEV_USER_HEADER,
  readDevSessionUserIdFromStorage,
} from "./devSessionUser";

let accessToken: string | undefined = undefined;

const authMiddleware: Middleware = {
  async onRequest({ request }) {
    const devId = readDevSessionUserIdFromStorage();
    if (import.meta.env.DEV && devId)
      request.headers.set(DEV_USER_HEADER, devId);
    else request.headers.delete(DEV_USER_HEADER);

    if (!accessToken) {
      accessToken = await getAccessTokenForApi();
      if (!accessToken && !devId) {
        console.warn(
          "No access token found for request. Attempting unauthenticated request.",
        );
      }
    }

    if (accessToken)
      request.headers.set("Authorization", `Bearer ${accessToken}`);
    else request.headers.delete("Authorization");

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
