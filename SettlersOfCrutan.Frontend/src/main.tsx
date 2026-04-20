import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import HomePage from "./pages/HomePage.tsx";
import "./index.css";

import { createBrowserRouter, RouterProvider } from "react-router";
import AppLayout from "./layouts/AppLayout.tsx";
import ForbiddenPage from "./pages/ForbiddenPage.tsx";
import LobbyPage, { LobbyLoader } from "./pages/LobbyPage.tsx";
import GamePage, { GameLoader } from "./pages/GamePage.tsx";

import { MsalProvider } from "@azure/msal-react";

import { msalInstance } from "./authConfig.ts";
import { DevSessionUserProvider } from "./context/DevSessionUserContext";
import { SignalRProvider } from "./context/SignalRProvider";

await msalInstance.initialize();

const router = createBrowserRouter([
  {
    Component: AppLayout,
    children: [
      { index: true, Component: HomePage },
      { path: "forbidden", Component: ForbiddenPage },
      {
        path: "game/:gameId",
        loader: GameLoader,
        Component: GamePage,
      },
      {
        path: "lobby/:lobbyId",
        loader: LobbyLoader,
        Component: LobbyPage,
      },
      { path: "preferences", element: <p>User Preferences</p> },
    ],
  },
]);

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <MsalProvider instance={msalInstance}>
      <DevSessionUserProvider>
        <SignalRProvider>
          <RouterProvider router={router} />
        </SignalRProvider>
      </DevSessionUserProvider>
    </MsalProvider>
  </StrictMode>
);
