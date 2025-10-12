import { useState, type FormEvent } from "react";
import { api } from "../api/client";

export default function ResetPassword() {
  const [email, setEmail] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<string | null>(null);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setLoading(true);
    setMessage(null);
    try {
      const { error } = await api.POST("/api/auth/admin/reset-password", {
        body: { email, newPassword },
      });
      if (error) throw error;
      setMessage("Password reset successfully.");
    } catch (err: unknown) {
      const msg =
        err instanceof Error ? err.message : "Failed to reset password";
      setMessage(msg);
    } finally {
      setLoading(false);
    }
  }

  return (
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
        <div>
          <label className="block text-sm font-medium text-gray-700">
            New password
          </label>
          <input
            type="password"
            className="mt-1 w-full rounded border px-3 py-2"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            required
            minLength={6}
          />
        </div>
        {message && <p className="text-sm text-gray-700">{message}</p>}
        <button
          type="submit"
          className="w-full rounded bg-blue-600 px-4 py-2 text-white disabled:opacity-50"
          disabled={loading}
        >
          {loading ? "Resetting…" : "Reset password"}
        </button>
      </form>
    </div>
  );
}
