import { createContext, useContext } from "react";
import type * as signalR from "@microsoft/signalr";

export type HandlerMap = Record<string, (...args: unknown[]) => void>;

export interface SignalRContextValue {
  connection: signalR.HubConnection | null;
  isConnected: boolean;
  isConnecting: boolean;
  error: string | null;
  start: () => Promise<void>;
  stop: () => Promise<void>;
  invoke: (methodName: string, ...args: unknown[]) => Promise<unknown>;
  send: (methodName: string, ...args: unknown[]) => Promise<void>;
  registerHandlers: (handlers: HandlerMap) => void;
}

export const SignalRContext = createContext<SignalRContextValue | undefined>(
  undefined
);

export function useSignalRContext() {
  const ctx = useContext(SignalRContext);
  if (!ctx)
    throw new Error("useSignalRContext must be used within SignalRProvider");
  return ctx;
}
