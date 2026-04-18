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
    <div
      className="cursor-default flex min-h-screen w-full flex-col"
      style={{ background: "var(--parchment)", color: "var(--ink)" }}
    >
      <div className="w-3/4 mx-auto">
        <header
          className="border-b-2 border-(--ink) shadow-[0_2px_0_var(--ink)]"
          style={{ background: "var(--parchment-2)" }}
        >
          <div className="mx-auto flex  items-center justify-between gap-4 px-4 py-3">
            {/* Brand */}
            <Link to="/" className="flex items-center gap-3 no-underline">
              <div
                className="h-9 w-8 shrink-0 border-2 border-(--ink) shadow-[2px_2px_0_var(--ink)]"
                style={{
                  background: "var(--catan-accent)",
                  clipPath: "polygon(0 0, 100% 0, 100% 60%, 50% 100%, 0 60%)",
                }}
              />
              <span
                className="leading-tight whitespace-nowrap"
                style={{
                  fontFamily: "var(--font-serif)",
                  fontSize: "1.5rem",
                  color: "var(--ink)",
                }}
              >
                Settlers of Crutan
              </span>
            </Link>

            {/* Nav */}
            <nav className="flex flex-wrap items-center gap-4">
              <Link
                to="/"
                className="text-sm hover:underline"
                style={{
                  fontFamily: "var(--font-hand)",
                  fontSize: "1.1rem",
                  color: "var(--ink-soft)",
                }}
              >
                Home
              </Link>

              {import.meta.env.DEV && (
                <label
                  className="flex items-center gap-2 text-xs"
                  style={{ color: "var(--ink-faint)" }}
                >
                  <span className="whitespace-nowrap font-medium">
                    Dev user id
                  </span>
                  <input
                    type="text"
                    value={devUserId}
                    onChange={(e) => setDevUserId(e.target.value)}
                    placeholder="e.g. player-2"
                    className="w-36 rounded border border-(--ink-soft) bg-white/40 px-2 py-1 text-sm text-(--ink) placeholder:text-(--ink-faint) outline-none focus:border-(--catan-accent)"
                    autoComplete="off"
                    spellCheck={false}
                  />
                </label>
              )}

              <UnauthenticatedTemplate>
                <button
                  type="button"
                  className="rounded-xl border-2 border-(--ink) bg-(--catan-accent) px-3 py-1.5 text-sm font-medium text-[#fff7e3] shadow-[2px_2px_0_var(--ink)] transition-transform hover:-translate-x-px hover:-translate-y-px"
                  style={{ fontFamily: "var(--font-hand)", fontSize: "1.1rem" }}
                  onClick={async () => await instance.loginRedirect()}
                >
                  Sign in
                </button>
              </UnauthenticatedTemplate>
              <AuthenticatedTemplate>
                <button
                  type="button"
                  className="cursor-pointer text-sm transition-opacity hover:opacity-70"
                  style={{
                    fontFamily: "var(--font-hand)",
                    fontSize: "1.1rem",
                    color: "var(--ink-soft)",
                  }}
                  onClick={async () => await instance.logout()}
                >
                  Sign out
                </button>
              </AuthenticatedTemplate>
            </nav>
          </div>
        </header>

        <main className="flex-1">
          <div className="mx-auto p-4 md:p-6">
            <Outlet />
          </div>
        </main>

        <footer
          className="border-t-2 border-(--ink)"
          style={{ background: "var(--parchment-2)" }}
        >
          <div
            className="mx-auto px-4 py-3 text-xs"
            style={{
              fontFamily: "var(--font-mono)",
              color: "var(--ink-faint)",
              letterSpacing: "0.05em",
            }}
          >
            {new Date().getFullYear()} · Settlers of Crutan
          </div>
        </footer>
      </div>
    </div>
  );
}
