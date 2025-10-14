import { useState, type FormEvent } from "react";
import { api } from "../api/client";
import { RequireRole } from "../components/guards";

export default function CreateUser() {
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [tempPassword, setTempPassword] = useState("");

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setLoading(true);
    setMessage(null);
    try {
      const { error, data } = await api.POST("/api/auth/admin/create-user", {
        body: { email },
      });
      if (error) throw error;
      setTempPassword(data);
      setMessage("User created with temporary password.");
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Failed to create user";
      setMessage(msg);
    } finally {
      setLoading(false);
    }
  }

  return (
    <RequireRole roles={["Admin"]}>
      <div className="flex flex-col max-w-md m-auto">
        <form onSubmit={onSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Email
            </label>
            <input
              type="email"
              className="mt-1 w-full rounded border px-3 py-2"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          {message && <p className="text-sm text-gray-700">{message}</p>}
          {tempPassword && (
            <>
              <div className="flex items-stretch gap-2">
                <input
                  type="text"
                  readOnly
                  value={tempPassword}
                  className="flex-1 rounded border bg-white px-3 py-2 font-mono text-sm text-gray-800"
                />
                <button
                  type="button"
                  className="rounded bg-gray-800 px-3 py-2 text-sm text-white hover:bg-gray-700"
                  onClick={() => navigator.clipboard.writeText(tempPassword)}
                >
                  Copy
                </button>
              </div>
              <a
                href="/create-user"
                className="text-blue-700 text-sm hover:underline"
              >
                Create Another
              </a>
            </>
          )}
          {!tempPassword && (
            <button
              type="submit"
              className="w-full rounded bg-blue-600 px-4 py-2 text-white disabled:opacity-50"
              disabled={loading}
            >
              {loading ? "Creating…" : "Create"}
            </button>
          )}
        </form>
      </div>
    </RequireRole>
  );
}
