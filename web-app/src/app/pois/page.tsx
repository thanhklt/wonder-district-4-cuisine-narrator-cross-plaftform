"use client";
import { useEffect, useState } from "react";
import { getMyPois, createPoi, submitPoi, deletePoi, getPackages } from "@/lib/api";
import Sidebar from "@/components/Sidebar";

export default function OwnerPoisPage() {
  const [pois, setPois] = useState<any[]>([]);
  const [packages, setPackages] = useState<any[]>([]);
  const [form, setForm] = useState({ name: "", lat: "", lng: "", packageId: "", description: "" });
  const [showForm, setShowForm] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    getMyPois().then(setPois);
    getPackages().then(setPackages);
  }, []);

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    try {
      await createPoi({ ...form, lat: +form.lat, lng: +form.lng, packageId: +form.packageId });
      const updated = await getMyPois();
      setPois(updated);
      setShowForm(false);
      setForm({ name: "", lat: "", lng: "", packageId: "", description: "" });
    } finally { setLoading(false); }
  }

  async function handleSubmit(id: string) {
    await submitPoi(id);
    const updated = await getMyPois();
    setPois(updated);
  }

  async function handleDelete(id: string) {
    if (!confirm("Xoá POI này?")) return;
    await deletePoi(id);
    setPois(pois.filter(p => p.id !== id));
  }

  const statusColor: Record<string, string> = {
    Draft: "text-gray-500", Pending: "text-yellow-600",
    Approved: "text-green-600", Rejected: "text-red-500",
  };

  return (
    <div className="flex h-full">
      <Sidebar role="Owner" />
      <main className="flex-1 p-6 overflow-auto">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-xl font-bold">POI của tôi</h1>
          <button onClick={() => setShowForm(!showForm)}
            className="bg-orange-500 text-white px-4 py-2 rounded-xl text-sm font-medium hover:bg-orange-600">
            + Tạo POI
          </button>
        </div>

        {showForm && (
          <form onSubmit={handleCreate} className="bg-white rounded-2xl p-6 mb-6 shadow-sm flex flex-col gap-3">
            <h2 className="font-semibold text-gray-700">POI mới</h2>
            <input required placeholder="Tên quán" value={form.name}
              onChange={e => setForm({ ...form, name: e.target.value })}
              className="border rounded-xl px-3 py-2 text-sm" />
            <div className="flex gap-2">
              <input required placeholder="Vĩ độ (Lat)" value={form.lat} type="number" step="any"
                onChange={e => setForm({ ...form, lat: e.target.value })}
                className="border rounded-xl px-3 py-2 text-sm flex-1" />
              <input required placeholder="Kinh độ (Lng)" value={form.lng} type="number" step="any"
                onChange={e => setForm({ ...form, lng: e.target.value })}
                className="border rounded-xl px-3 py-2 text-sm flex-1" />
            </div>
            <select required value={form.packageId} onChange={e => setForm({ ...form, packageId: e.target.value })}
              className="border rounded-xl px-3 py-2 text-sm">
              <option value="">Chọn gói</option>
              {packages.map((p: any) => (
                <option key={p.id} value={p.id}>{p.name} — {p.radiusMeters}m — {p.price.toLocaleString()}đ</option>
              ))}
            </select>
            <textarea required placeholder="Mô tả (tiếng Việt)" value={form.description}
              onChange={e => setForm({ ...form, description: e.target.value })} rows={4}
              className="border rounded-xl px-3 py-2 text-sm resize-none" />
            <div className="flex gap-2">
              <button type="submit" disabled={loading}
                className="bg-orange-500 text-white px-4 py-2 rounded-xl text-sm font-medium hover:bg-orange-600 disabled:bg-gray-300">
                {loading ? "Đang lưu..." : "Lưu"}
              </button>
              <button type="button" onClick={() => setShowForm(false)}
                className="text-gray-500 px-4 py-2 rounded-xl text-sm hover:bg-gray-100">Huỷ</button>
            </div>
          </form>
        )}

        <div className="flex flex-col gap-3">
          {pois.map((poi: any) => (
            <div key={poi.id} className="bg-white rounded-2xl p-4 shadow-sm flex items-center gap-4">
              <div className="flex-1 min-w-0">
                <p className="font-semibold text-sm">{poi.name}</p>
                <p className="text-xs text-gray-400">{poi.lat}, {poi.lng} · {poi.radiusMeters}m</p>
                <span className={`text-xs font-medium ${statusColor[poi.status] ?? "text-gray-500"}`}>
                  {poi.status}
                </span>
              </div>
              <div className="flex gap-2">
                {(poi.status === "Draft" || poi.status === "Rejected") && (
                  <button onClick={() => handleSubmit(poi.id)}
                    className="text-xs bg-blue-500 text-white px-3 py-1 rounded-full hover:bg-blue-600">
                    Submit
                  </button>
                )}
                {poi.status === "Draft" && (
                  <button onClick={() => handleDelete(poi.id)}
                    className="text-xs text-red-500 hover:text-red-700">Xoá</button>
                )}
              </div>
            </div>
          ))}
          {pois.length === 0 && <p className="text-gray-400 text-sm">Chưa có POI nào.</p>}
        </div>
      </main>
    </div>
  );
}
