// GameAreaLayout.tsx
import { Outlet } from "react-router";
import { useSignalR } from "../hooks/useSignalR";

export function LobbyLayout() {
  const { isConnected, isConnecting, error } = useSignalR("/api/realtime-hub", {
    autoConnect: true,
    handlers: {
      LobbyReceive: (lobbyId: string, eventName: string, payload: object) =>
        console.log(
          `lobbyId: ${lobbyId}\n eventName: ${eventName}\n payload: ${JSON.stringify(
            payload
          )}`
        ),
    },
  });

  if (isConnecting && !isConnected) {
    return <div className="p-4">Connecting to game services...</div>;
  }

  if (error && !isConnected) {
    return (
      <div className="p-4 text-red-600">
        Could not connect to game services: {error}
      </div>
    );
  }

  return <Outlet />;
}
