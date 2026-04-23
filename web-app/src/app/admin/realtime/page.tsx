"use client";
import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { API_URL, getToken, getRealtimeCount } from "@/lib/api";
import Sidebar from "@/components/Sidebar";

export default function RealtimePage() {
  const [onlineCount, setOnlineCount] = useState<number | null>(null);
  const [connected, setConnected] = useState(false);
  const [history, setHistory] = useState<{ time: string; count: number }[]>([]);

  useEffect(() => {
    getRealtimeCount().then(data => setOnlineCount(data.count)).catch(() => {});

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/admin`, {
        accessTokenFactory: () => getToken() ?? "",
      })
      .withAutomaticReconnect()
      .build();

    connection.on("OnlineCount", (count: number) => {
      setOnlineCount(count);
      setHistory(prev => [
        ...prev.slice(-19),
        { time: new Date().toLocaleTimeString("vi-VN"), count },
      ]);
    });

    connection.start()
      .then(() => setConnected(true))
      .catch(() => setConnected(false));

    return () => { connection.stop(); };
  }, []);

  return (
    <div className="flex h-full">
      <Sidebar role="Admin" />
      <main className="flex-1 p-6 overflow-auto">
        <div className="flex items-center gap-3 mb-6">
          <h1 className="text-xl font-bold">Trực tuyến</h1>
          <span className={`flex items-center gap-1 text-xs font-medium px-2 py-0.5 rounded-full ${connected ? "bg-green-100 text-green-700" : "bg-red-100 text-red-500"}`}>
            <span className={`w-1.5 h-1.5 rounded-full ${connected ? "bg-green-500" : "bg-red-400"}`} />
            {connected ? "Đã kết nối" : "Mất kết nối"}
          </span>
        </div>

        <div className="bg-white rounded-2xl p-8 shadow-sm mb-6 text-center">
          <p className="text-sm text-gray-500 mb-2">Người dùng đang trực tuyến</p>
          <p className="text-6xl font-bold text-orange-500">
            {onlineCount === null ? "..." : onlineCount}
          </p>
        </div>

        <div className="bg-white rounded-2xl p-5 shadow-sm">
          <h2 className="font-semibold text-sm text-gray-700 mb-3">Lịch sử (20 điểm gần nhất)</h2>
          <div className="flex flex-col gap-1.5 max-h-64 overflow-auto">
            {history.slice().reverse().map((h, i) => (
              <div key={i} className="flex justify-between text-sm text-gray-600">
                <span className="text-gray-400 text-xs">{h.time}</span>
                <span className="font-medium">{h.count} người</span>
              </div>
            ))}
            {history.length === 0 && <p className="text-gray-400 text-xs">Chờ dữ liệu...</p>}
          </div>
        </div>
      </main>
    </div>
  );
}
