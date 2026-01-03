import { Outlet, Link } from "react-router";
import { useMsal } from "@azure/msal-react";
import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";

export default function AppLayout() {
  const { instance } = useMsal();
  return (
    <div className="min-h-screen bg-gray-100 flex flex-col">
      <header className="bg-white shadow">
        <div className="mx-auto max-w-6xl px-4 py-3 flex items-center justify-between">
          <Link to="/" className="text-lg font-semibold text-gray-900">
            Settlers Of Crutan
          </Link>
          <nav className="flex items-center gap-4 text-sm">
            <Link to="/" className="text-gray-700 hover:text-gray-900">
              Home
            </Link>
            <span className="text-gray-300">|</span>
            <UnauthenticatedTemplate>
              <button
                className="text-gray-700 hover:text-gray-900 cursor-pointer"
                onClick={async () => await instance.loginRedirect()}
              >
                Login
              </button>
            </UnauthenticatedTemplate>
            <AuthenticatedTemplate>
              <button
                className="text-gray-700 hover:text-gray-900 cursor-pointer"
                onClick={async () => await instance.logout()}
              >
                Logout
              </button>
            </AuthenticatedTemplate>
          </nav>
        </div>
      </header>
      <main className="flex-1">
        <div className="mx-auto max-w-6xl p-4 pt-20">
          <Outlet />
        </div>
      </main>
      <footer className="bg-white border-t">
        <div className="mx-auto max-w-6xl px-4 py-3 text-xs text-gray-500">
          {new Date().getFullYear()} Settlers Of Crutan
        </div>
      </footer>
    </div>
  );
}
