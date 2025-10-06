import createClient from "openapi-fetch";
import type { paths } from "./types";

// Including cookies automatically
export const api = createClient<paths>({
  baseUrl: "",
  credentials: "include",
});
