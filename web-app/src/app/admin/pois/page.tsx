"use client";
import { useEffect, useState } from "react";
import { getPendingPois, approvePoi, rejectPoi } from "@/lib/api";
import Sidebar from "@/components/Sidebar";

export default function AdminPoisPage() {
  const [pois, setPois] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => { getPendingPois().then(setPois); }, []);

  async function handleApprove(id: string) {
    setLoading(true);
    await approvePoi(id);
    setPois(pois.filter(p => p.id !== id));
    setLoading(false);
  }

  async function handleReject(id: string) {
    const note = prompt("Lý do từ chối (tùy chọn):");
    setLoading(true);
    await rejectPoi(id, note ?? undefined);
    setPois(pois.filter(p => p.id !== id));
    setLoading(false);
  }

  return (
    <div className="flex h-full">
      <Sidebar role="Admin" />
      <main className="flex-1 p-6 overflow-auto">
        <h1 className="text-xl font-bold mb-6">Duyệt POI ({pois.length})</h1>
        <div className="flex flex-col gap-4">
          {pois.map((poi: any) => (
            <div key={poi.id} className="bg-white rounded-2xl p-5 shadow-sm">
              <div className="flex justify-between items-start gap-4">
                <div className="flex-1 min-w-0">
                  <p className="font-semibold">{poi.name}</p>
                  <p className="text-xs text-gray-400">{poi.lat}, {poi.lng}</p>
                  <p className="text-xs text-gray-500 mt-1">
                    Owner: {poi.owner?.email}
                  </p>
                  {poi.localizations?.find((l: any) => l.language === "vi") && (
                    <p className="text-sm text-gray-700 mt-2 line-clamp-3">
                      {poi.localizations.find((l: any) => l.language === "vi")?.textContent}
                    </p>
                  )}
                </div>
                <div className="flex flex-col gap-2">
                  <button onClick={() => handleApprove(poi.id)} disabled={loading}
                    className="bg-green-500 text-white text-xs px-3 py-1.5 rounded-full hover:bg-green-600 disabled:bg-gray-300">
                    Duyệt
                  </button>
                  <button onClick={() => handleReject(poi.id)} disabled={loading}
                    className="bg-red-500 text-white text-xs px-3 py-1.5 rounded-full hover:bg-red-600 disabled:bg-gray-300">
                    Từ chối
                  </button>
                </div>
              </div>
            </div>
          ))}
          {pois.length === 0 && <p className="text-gray-400 text-sm">Không có POI nào chờ duyệt.</p>}
        </div>
      </main>
    </div>
  );
}
