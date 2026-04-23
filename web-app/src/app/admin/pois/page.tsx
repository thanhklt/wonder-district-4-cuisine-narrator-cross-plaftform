"use client";
import { useEffect, useState } from "react";
import { getPendingPois, approvePoi, rejectPoi } from "@/lib/api";
import Sidebar from "@/components/Sidebar";

export default function AdminPoisPage() {
  const [pois, setPois] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [rejectDialog, setRejectDialog] = useState<{ id: string; name: string } | null>(null);
  const [rejectNote, setRejectNote] = useState("");

  useEffect(() => { getPendingPois().then(setPois); }, []);

  async function handleApprove(id: string) {
    setLoading(true);
    await approvePoi(id);
    setPois(pois.filter((p: any) => p.id !== id));
    setLoading(false);
  }

  function openRejectDialog(id: string, name: string) {
    setRejectNote("");
    setRejectDialog({ id, name });
  }

  function closeRejectDialog() {
    setRejectDialog(null);
    setRejectNote("");
  }

  async function confirmReject() {
    if (!rejectDialog) return;
    setLoading(true);
    await rejectPoi(rejectDialog.id, rejectNote.trim() || undefined);
    setPois(pois.filter((p: any) => p.id !== rejectDialog.id));
    setLoading(false);
    closeRejectDialog();
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
                  <p className="text-xs text-gray-500 mt-1">Owner: {poi.owner?.email}</p>
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
                  <button onClick={() => openRejectDialog(poi.id, poi.name)} disabled={loading}
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

      {/* Reject dialog */}
      {rejectDialog && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md mx-4 p-6 flex flex-col gap-4">
            <h2 className="font-bold text-gray-800">Từ chối POI</h2>
            <p className="text-sm text-gray-500">
              POI: <span className="font-medium text-gray-700">{rejectDialog.name}</span>
            </p>
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-gray-700">
                Lý do từ chối <span className="text-gray-400 font-normal">(tùy chọn)</span>
              </label>
              <textarea
                value={rejectNote}
                onChange={e => setRejectNote(e.target.value)}
                placeholder="Nhập lý do từ chối..."
                rows={3}
                className="border rounded-xl px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-red-200"
              />
            </div>
            <div className="flex gap-2 justify-end">
              <button
                onClick={closeRejectDialog}
                disabled={loading}
                className="px-4 py-2 rounded-xl text-sm text-gray-600 hover:bg-gray-100 disabled:opacity-50">
                Hủy
              </button>
              <button
                onClick={confirmReject}
                disabled={loading}
                className="px-4 py-2 rounded-xl text-sm font-semibold bg-red-500 text-white hover:bg-red-600 disabled:bg-gray-300">
                {loading ? "Đang xử lý..." : "Xác nhận từ chối"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
