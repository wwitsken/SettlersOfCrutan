import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./pages/App.tsx";
import "./index.css";

import { createBrowserRouter } from "react-router";
import { RouterProvider } from "react-router/dom";

const router = createBrowserRouter([
  {
    path: "/",
    Component: App,
  },
  {
    path: "/login",
    element: <p>log in</p>,
  },
  {
    path: "/create-user",
    element: <p>create user</p>,
  },
  {
    path: "/reset-password",
    element: <p>reset user password</p>,
  },
  {
    path: "/preferences",
    element: <p>User Preferences</p>,
  },
  {
    path: "/lobby/:lobbyId",
    loader: async ({ params }) => {
      const team = await fetchLobby(params.lobbyId);
      return { name: team.name };
    },
    element: <p>Lobby</p>,
  },
]);

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <RouterProvider router={router} />,
  </StrictMode>
);
