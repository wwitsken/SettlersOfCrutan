import { useState } from "react";
import type { ChatMessage } from "../../types/catan";
import { COLOR_MAP, COLOR_TEXT_MAP } from "../../constants/catanMeta";

export default function ChatPanel({ messages }: { messages: ChatMessage[] }) {
  const [input, setInput] = useState("");
  return (
    <div className="flex flex-col h-full">
      <div className="text-xs font-semibold text-stone-500 uppercase tracking-widest mb-2 px-1">Live Chat</div>

      <div className="flex-1 overflow-y-auto space-y-2 pr-1 mb-3">
        {messages.map((msg) => (
          <div key={msg.id} className="flex gap-2 items-start">
            <div className={`w-2 h-2 rounded-full flex-shrink-0 mt-1.5 ${COLOR_MAP[msg.color]}`} />
            <div>
              <span className={`text-xs font-semibold ${COLOR_TEXT_MAP[msg.color]}`}>{msg.player} </span>
              <span className="text-xs text-stone-500">{msg.time}</span>
              <p className="text-xs text-stone-300 mt-0.5 leading-snug">{msg.text}</p>
            </div>
          </div>
        ))}
      </div>

      <div className="flex gap-2">
        <input
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="Send a message…"
          className="flex-1 bg-stone-800 border border-stone-700 rounded-lg px-3 py-1.5 text-xs text-stone-200 placeholder:text-stone-600 focus:outline-none focus:border-stone-500"
        />
        <button
          onClick={() => setInput("")}
          className="px-3 py-1.5 bg-stone-700 hover:bg-stone-600 text-stone-200 text-xs rounded-lg transition-colors"
        >
          Send
        </button>
      </div>
    </div>
  );
}
