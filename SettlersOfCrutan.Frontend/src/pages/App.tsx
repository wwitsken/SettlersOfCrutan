import { useState } from "react";
import { api } from "../api/client";

async function echo(content: string) {
  const { data, error } = await api.POST("/api/echo", {
    body: content,
  });

  if (error) throw error;
  return data;
}

function App() {
  const [statusText, setStatusText] = useState("");
  const [inputText, setInputText] = useState("");
  const [echoResponse, setEchoResponse] = useState("");

  return (
    <div className="min-h-screen bg-gray-100 flex flex-col items-center justify-center">
      <h1 className="text-4xl font-bold text-blue-600 mb-6">Vite + React</h1>
      <div className="card bg-white shadow-lg rounded-lg p-8 flex flex-col items-center">
        <button
          className="bg-blue-500 hover:bg-blue-600 text-white font-semibold py-2 px-4 rounded transition-colors mb-4"
          onClick={async () => {
            try {
              const res = await fetch("api/health");
              const txt = await res.text();
              setStatusText(txt);
            } catch (e: any) {
              setStatusText(`Error: ${e?.message ?? String(e)}`);
            }
          }}
        >
          Api Status: {statusText}
        </button>

        <div className="w-full max-w-md space-y-2">
          <label
            htmlFor="echo-input"
            className="block text-sm font-medium text-gray-700"
          >
            Enter text
          </label>
          <input
            id="echo-input"
            type="text"
            className="w-full border rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Type something..."
            value={inputText}
            onChange={(e) => setInputText(e.target.value)}
          />

          <button
            className="bg-green-500 hover:bg-green-600 text-white font-semibold py-2 px-4 rounded transition-colors"
            onClick={async () => {
              try {
                const data = await echo(inputText);
                setEchoResponse(data ?? "");
              } catch (e: any) {
                setEchoResponse(`Request failed: ${e?.message ?? String(e)}`);
              }
            }}
          >
            Send to /api/echo
          </button>

          <label
            htmlFor="echo-output"
            className="block text-sm font-medium text-gray-700"
          >
            Response
          </label>
          <textarea
            id="echo-output"
            className="w-full border rounded px-3 py-2 h-24 resize-y focus:outline-none focus:ring-2 focus:ring-blue-500"
            readOnly
            value={echoResponse}
          />
        </div>

        <p className="text-gray-700">
          Edit{" "}
          <code className="bg-gray-200 px-1 rounded">src/pages/App.tsx</code>{" "}
          and save to test HMR
        </p>
      </div>
    </div>
  );
}

export default App;
