import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./pages/App.tsx";
import "./index.css";

import { createBrowserRouter, RouterProvider } from "react-router";
import Login from "./pages/Login";
import CreateUser from "./pages/CreateUser";
import ResetPassword from "./pages/ResetPassword";
import Logout from "./pages/Logout";
import AppLayout from "./layouts/AppLayout.tsx";

const router = createBrowserRouter([
  {
    Component: AppLayout,
    children: [
      { index: true, Component: App },
      { path: "login", Component: Login },
      { path: "create-user", Component: CreateUser },
      { path: "reset-password", Component: ResetPassword },
      { path: "logout", Component: Logout },
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
    <RouterProvider router={router} />,
  </StrictMode>
);
