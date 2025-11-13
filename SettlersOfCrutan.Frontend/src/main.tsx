import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import Home from "./pages/Home.tsx";
import "./index.css";

import { createBrowserRouter, RouterProvider } from "react-router";
import Login from "./pages/Login";
import CreateUser from "./pages/CreateUser";
import ResetPassword from "./pages/ResetPassword";
import Logout from "./pages/Logout";
import AppLayout from "./layouts/AppLayout.tsx";
import AppBootstrap from "./components/AppBootstrap.tsx";
import Forbidden from "./pages/Forbidden.tsx";
import Lobby, { type LobbyData } from "./pages/Lobby.tsx";
import { api } from "./api/client.ts";

const router = createBrowserRouter([
  {
    Component: AppLayout,
    children: [
      { index: true, Component: Home },
      { path: "forbidden", Component: Forbidden },
      { path: "login", Component: Login },
      { path: "create-user", Component: CreateUser },
      { path: "reset-password", Component: ResetPassword },
      { path: "logout", Component: Logout },
      {
        path: "lobby/:lobbyId",
        loader: async ({ params }): Promise<LobbyData> => {
          if (!params.lobbyId) return { status: 404 };
          const response = await api.GET("/api/lobby/{lobbyId}", {
            params: {
              path: {
                lobbyId: params.lobbyId,
              },
            },
          });
          return { data: response.data, status: response.response.status };
        },
        Component: Lobby,
      },
      { path: "preferences", element: <p>User Preferences</p> },
    ],
  },

  // {
  //   path: "/lobby/:lobbyId",
  //   loader: async ({ params }) => {
  //     return { name: params.lobbyId ?? "" };
  //   },
  //   element: <p>Lobby</p>,
  // },
]);

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <AppBootstrap>
      <RouterProvider router={router} />
    </AppBootstrap>
  </StrictMode>
);
