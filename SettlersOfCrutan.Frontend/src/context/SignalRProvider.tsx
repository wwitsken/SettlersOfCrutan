import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import type { PropsWithChildren } from "react";
import * as signalR from "@microsoft/signalr";
import { getAccessTokenForApi } from "../authConfig";
import { DEV_USER_QUERY_PARAM } from "../api/devSessionUser";
import {
  SignalRContext,
  type SignalRContextValue,
  type HandlerMap,
} from "./SignalRContext";
import { useDevSessionUser } from "./DevSessionUserContext";

interface SignalRProviderProps {
  hubUrl?: string; // default to "/api/realtime-hub"
  handlers?: HandlerMap; // optional initial handlers to register once
}

export function SignalRProvider({
  hubUrl = "/api/realtime-hub",
  handlers,
  children,
}: PropsWithChildren<SignalRProviderProps>) {
  const { devUserId } = useDevSessionUser();
  const resolvedHubUrl = useMemo(() => {
    const trimmed = devUserId.trim();
    if (!import.meta.env.DEV || !trimmed) return hubUrl;
    const sep = hubUrl.includes("?") ? "&" : "?";
    return `${hubUrl}${sep}${DEV_USER_QUERY_PARAM}=${encodeURIComponent(trimmed)}`;
  }, [hubUrl, devUserId]);

  // Single connection instance owned by the provider
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  // Idempotent start() management to avoid parallel starts
  const startPromiseRef = useRef<Promise<void> | null>(null);
  // Ensure handlers are registered once per connection instance
  const handlersRegisteredRef = useRef(false);
  // Track methods already registered to avoid duplicate `on` subscriptions
  const registeredMethodsRef = useRef<Set<string>>(new Set());
  // Queue handlers if registration is requested before connection exists
  const pendingHandlersRef = useRef<HandlerMap | null>(null);

  const [isConnecting, setIsConnecting] = useState(false);
  const [isConnected, setIsConnected] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Build a new HubConnection for the given hubUrl
  const buildHubConnection = useCallback(() => {
    return new signalR.HubConnectionBuilder()
      .withUrl(resolvedHubUrl, {
        accessTokenFactory: async () => (await getAccessTokenForApi()) ?? "",
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();
  }, [resolvedHubUrl]);

  // Attach lifecycle event handlers and register any pending handlers
  const configureConnection = useCallback((conn: signalR.HubConnection) => {
    // Lifecycle events
    conn.onreconnecting(() => {
      setIsConnected(false);
      setIsConnecting(true);
      setError(null);
    });

    conn.onreconnected(() => {
      setIsConnected(true);
      setIsConnecting(false);
      setError(null);
    });

    conn.onclose((err) => {
      setIsConnected(false);
      setIsConnecting(false);
      if (err) setError(err.message);
    });

    // If any handlers were queued before connection existed, register them now
    if (pendingHandlersRef.current) {
      for (const [methodName, handler] of Object.entries(
        pendingHandlersRef.current,
      )) {
        conn.off(methodName);
        conn.on(methodName, handler);
        registeredMethodsRef.current.add(methodName);
      }
      handlersRegisteredRef.current = true;
      pendingHandlersRef.current = null;
    }
  }, []);

  // Build the connection exactly once per hubUrl (eager init)
  useEffect(() => {
    if (typeof window === "undefined") return;
    const methodsSet = registeredMethodsRef.current;

    const connection = buildHubConnection();
    connectionRef.current = connection;
    handlersRegisteredRef.current = false; // reset for new connection instance
    methodsSet.clear();

    configureConnection(connection);

    // Cleanup on unmount / hub URL change
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
          handlersRegisteredRef.current = false;
          methodsSet.clear();
          setIsConnected(false);
          setIsConnecting(false);
        }
      })();
    };
  }, [resolvedHubUrl, buildHubConnection, configureConnection]);

  const registerHandlers = useCallback((map: HandlerMap) => {
    if (!map) return;
    const conn = connectionRef.current;
    // If no connection yet, queue handlers for later registration
    if (!conn) {
      pendingHandlersRef.current = {
        ...(pendingHandlersRef.current ?? {}),
        ...map,
      };
      return;
    }

    for (const [methodName, handler] of Object.entries(map)) {
      conn.off(methodName);
      conn.on(methodName, handler);
      registeredMethodsRef.current.add(methodName);
    }
    handlersRegisteredRef.current = true;
  }, []);

  // If initial handlers are provided, register them once
  useEffect(() => {
    if (handlers) registerHandlers(handlers);
    // intentionally do not include registerHandlers in deps to avoid re-register
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [handlers]);

  const start = useCallback(async () => {
    let conn = connectionRef.current;
    // Lazily build the connection if it doesn't exist yet
    if (!conn) {
      conn = buildHubConnection();
      connectionRef.current = conn;
      handlersRegisteredRef.current = false;
      registeredMethodsRef.current.clear();
      configureConnection(conn);
    }

    // Already connected
    if (conn.state === signalR.HubConnectionState.Connected) return;

    // If a start is in-flight, await it
    if (startPromiseRef.current) {
      await startPromiseRef.current;
      return;
    }

    // Avoid parallel starts with StrictMode / route transitions
    setIsConnecting(true);
    setError(null);

    const startPromise = conn
      .start()
      .then(() => {
        setIsConnected(true);
        setIsConnecting(false);
      })
      .catch((err) => {
        console.error("SignalR start error:", err);
        setError(err?.message ?? "Failed to connect");
        setIsConnected(false);
        setIsConnecting(false);
        throw err;
      })
      .finally(() => {
        startPromiseRef.current = null;
      });

    startPromiseRef.current = startPromise;
    await startPromise;
  }, [buildHubConnection, configureConnection]);

  const stop = useCallback(async () => {
    const conn = connectionRef.current;
    if (!conn) return;
    try {
      await conn.stop();
      setIsConnected(false);
      setIsConnecting(false);
    } catch (err) {
      console.error("SignalR stop error:", err);
    }
  }, []);

  const invoke = useCallback(async (methodName: string, ...args: unknown[]) => {
    const conn = connectionRef.current;
    if (!conn) throw new Error("No SignalR connection.");
    return conn.invoke(methodName, ...args);
  }, []);

  const send = useCallback(async (methodName: string, ...args: unknown[]) => {
    const conn = connectionRef.current;
    if (!conn) throw new Error("No SignalR connection.");
    return conn.send(methodName, ...args);
  }, []);

  const value: SignalRContextValue = useMemo(
    () => ({
      connection: connectionRef.current,
      isConnected,
      isConnecting,
      error,
      start,
      stop,
      invoke,
      send,
      registerHandlers,
    }),
    [
      isConnected,
      isConnecting,
      error,
      start,
      stop,
      invoke,
      send,
      registerHandlers,
    ],
  );

  return (
    <SignalRContext.Provider value={value}>{children}</SignalRContext.Provider>
  );
}
