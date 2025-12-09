// hooks/useSignalR.ts
import { useEffect, useRef, useState, useCallback } from "react";
import * as signalR from "@microsoft/signalr";

type HandlerMap = Record<string, (...args: any[]) => void>;

interface UseSignalROptions {
  /** Whether to connect automatically on mount (default: true) */
  autoConnect?: boolean;
  /** Extra handlers to register: { ServerMethodName: (...args) => void } */
  handlers?: HandlerMap;
  /** Called when connection successfully starts */
  onConnected?: (connection: signalR.HubConnection) => void;
  /** Called when connection closes */
  onDisconnected?: (error?: Error) => void;
}

/**
 * useSignalR
 * @param hubUrl The relative or absolute URL to your hub (e.g. "/hub/game")
 */
export function useSignalR(hubUrl: string, options: UseSignalROptions = {}) {
  const { autoConnect = true, handlers, onConnected, onDisconnected } = options;

  const connectionRef = useRef<signalR.HubConnection | null>(null);

  const [isConnecting, setIsConnecting] = useState(false);
  const [isConnected, setIsConnected] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Build the connection once per hubUrl/options.accessTokenFactory
  useEffect(() => {
    // In case of SSR, do nothing
    if (typeof window === "undefined") return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { withCredentials: true })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    connectionRef.current = connection;

    // Register handlers
    if (handlers) {
      for (const [methodName, handler] of Object.entries(handlers)) {
        connection.on(methodName, handler);
      }
    }

    // Lifecycle events
    connection.onreconnecting(() => {
      setIsConnected(false);
      setIsConnecting(true);
      setError(null);
    });

    connection.onreconnected(() => {
      setIsConnected(true);
      setIsConnecting(false);
      setError(null);
    });

    connection.onclose((err) => {
      setIsConnected(false);
      setIsConnecting(false);
      if (err) {
        setError(err.message);
      }
      onDisconnected?.(err ?? undefined);
    });

    // Cleanup on unmount / hubUrl change
    return () => {
      (async () => {
        try {
          if (connectionRef.current) {
            await connectionRef.current.stop();
          }
        } catch {
          // ignore
        } finally {
          connectionRef.current = null;
          setIsConnected(false);
          setIsConnecting(false);
        }
      })();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hubUrl]); // note: handlers intentionally not in deps so they don't re-subscribe on every render

  const start = useCallback(async () => {
    if (!connectionRef.current) return;

    if (
      connectionRef.current.state === signalR.HubConnectionState.Connected ||
      connectionRef.current.state === signalR.HubConnectionState.Connecting
    ) {
      return;
    }

    setIsConnecting(true);
    setError(null);
    try {
      await connectionRef.current.start();
      setIsConnected(true);
      onConnected?.(connectionRef.current);
    } catch (err: any) {
      console.error("SignalR start error:", err);
      setError(err?.message ?? "Failed to connect");
      setIsConnected(false);
      setIsConnecting(false);
      throw err;
    }
  }, [onConnected]);

  const stop = useCallback(async () => {
    if (!connectionRef.current) return;
    try {
      await connectionRef.current.stop();
      setIsConnected(false);
      setIsConnecting(false);
    } catch (err) {
      console.error("SignalR stop error:", err);
    }
  }, []);

  const invoke = useCallback(async (methodName: string, ...args: any[]) => {
    if (!connectionRef.current) throw new Error("No SignalR connection.");
    return connectionRef.current.invoke(methodName, ...args);
  }, []);

  const send = useCallback(async (methodName: string, ...args: any[]) => {
    if (!connectionRef.current) throw new Error("No SignalR connection.");
    return connectionRef.current.send(methodName, ...args);
  }, []);

  // Auto-connect if requested
  useEffect(() => {
    if (!autoConnect) return;
    (async () => {
      try {
        await start();
      } catch {
        // error already handled
      }
    })();
  }, [autoConnect, start]);

  return {
    connection: connectionRef.current,
    isConnected,
    isConnecting,
    error,
    start,
    stop,
    invoke,
    send,
  };
}
