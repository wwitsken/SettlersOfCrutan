import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import Home from "./pages/Home.tsx";
import "./index.css";

import { createBrowserRouter, RouterProvider } from "react-router";
import AppLayout from "./layouts/AppLayout.tsx";
import Forbidden from "./pages/Forbidden.tsx";
import Lobby from "./pages/Lobby.tsx";
import Game from "./pages/Game.tsx";

import { api } from "./api/client.ts";
import { LobbyLayout } from "./layouts/LobbyLayout.tsx";
import { MsalProvider } from "@azure/msal-react";

import { msalInstance, acquireAccessToken } from "../authConfig.ts";

const router = createBrowserRouter([
  {
    Component: AppLayout,
    children: [
      { index: true, Component: Home },
      { path: "forbidden", Component: Forbidden },
      { path: "game/:gameId", Component: Game },
      {
        path: "lobby/:lobbyId",
        Component: LobbyLayout,
        children: [
          {
            index: true,
            loader: async ({ params }) => {
              if (!params.lobbyId) return { status: 404 };
              const { data, response } = await api.GET("/api/lobby/{lobbyId}", {
                params: {
                  path: {
                    lobbyId: params.lobbyId,
                  },
                },
                accessToken: await acquireAccessToken(),
              });
              return { data, status: response.status };
            },
            Component: Lobby,
          },
        ],
      },
      { path: "preferences", element: <p>User Preferences</p> },
    ],
  },
]);

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <MsalProvider instance={msalInstance}>
      <RouterProvider router={router} />
    </MsalProvider>
  </StrictMode>
);
