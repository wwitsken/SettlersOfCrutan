import { Outlet, Link } from "react-router";
import { useMsal } from "@azure/msal-react";
import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";
import { useDevSessionUser } from "../context/DevSessionUserContext";

export default function AppLayout() {
  const { instance } = useMsal();
  const { devUserId, setDevUserId } = useDevSessionUser();
  return (
    <div className="flex min-h-screen flex-col bg-slate-50">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
          <Link
            to="/"
            className="text-lg font-semibold tracking-tight text-slate-900"
          >
            Settlers of Crutan
          </Link>
          <nav className="flex flex-wrap items-center gap-4 text-sm">
            <Link to="/" className="text-slate-600 hover:text-slate-900">
              Home
            </Link>
            {import.meta.env.DEV && (
              <label className="flex items-center gap-2 text-xs text-amber-900">
                <span className="whitespace-nowrap font-medium">Dev user id</span>
                <input
                  type="text"
                  value={devUserId}
                  onChange={(e) => setDevUserId(e.target.value)}
                  placeholder="e.g. player-2"
                  className="w-36 rounded border border-amber-300 bg-amber-50 px-2 py-1 text-slate-900 placeholder:text-slate-400"
                  autoComplete="off"
                  spellCheck={false}
                />
              </label>
            )}
            <UnauthenticatedTemplate>
              <button
                type="button"
                className="rounded-lg bg-slate-900 px-3 py-1.5 text-white hover:bg-slate-800"
                onClick={async () => await instance.loginRedirect()}
              >
                Sign in
              </button>
            </UnauthenticatedTemplate>
            <AuthenticatedTemplate>
              <button
                type="button"
                className="text-slate-600 hover:text-slate-900"
                onClick={async () => await instance.logout()}
              >
                Sign out
              </button>
            </AuthenticatedTemplate>
          </nav>
        </div>
      </header>
      <main className="flex-1">
        <div className="mx-auto max-w-6xl p-4 md:p-6">
          <Outlet />
        </div>
      </main>
      <footer className="border-t border-slate-200 bg-white">
        <div className="mx-auto max-w-6xl px-4 py-3 text-xs text-slate-500">
          {new Date().getFullYear()} Settlers of Crutan
        </div>
      </footer>
    </div>
  );
}
