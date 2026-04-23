"use client";
import { useEffect, useState } from "react";
import { getMyPois, createPoi, updatePoi, deletePoi, getPackages } from "@/lib/api";
import Sidebar from "@/components/Sidebar";

const STATUS_LABEL: Record<string, string> = {
  Draft: "Nháp", Pending: "Chờ duyệt", Approved: "Đã duyệt", Rejected: "Từ chối",
};
const STATUS_COLOR: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Pending: "bg-yellow-100 text-yellow-700",
  Approved: "bg-green-100 text-green-700",
  Rejected: "bg-red-100 text-red-500",
};
const FILTER_OPTIONS = ["Tất cả", "Draft", "Pending", "Approved", "Rejected"];

const EMPTY_FORM = { name: "", lat: "", lng: "", packageId: "", description: "" };

export default function AdminManagePoisPage() {
  const [pois, setPois] = useState<any[]>([]);
  const [packages, setPackages] = useState<any[]>([]);
  const [filter, setFilter] = useState("Tất cả");
  const [form, setForm] = useState(EMPTY_FORM);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    getMyPois().then(setPois);
    getPackages().then(setPackages);
  }, []);

  const filtered = filter === "Tất cả"
    ? pois
    : pois.filter((p: any) => p.status === filter);

  function openCreate() {
    setEditingId(null);
    setForm(EMPTY_FORM);
    setShowForm(true);
  }

  function openEdit(poi: any) {
    setEditingId(poi.id);
    setForm({
      name: poi.name,
      lat: String(poi.lat),
      lng: String(poi.lng),
      packageId: String(poi.packageId ?? ""),
      description: poi.localizations?.find((l: any) => l.language === "vi")?.textContent ?? "",
    });
    setShowForm(true);
  }

  function cancelForm() {
    setShowForm(false);
    setEditingId(null);
    setForm(EMPTY_FORM);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    try {
      if (editingId) {
        await updatePoi(editingId, {
          name: form.name, lat: +form.lat, lng: +form.lng, description: form.description,
        });
      } else {
        await createPoi({
          name: form.name, lat: +form.lat, lng: +form.lng,
          packageId: +form.packageId, description: form.description,
        });
      }
      setPois(await getMyPois());
      cancelForm();
    } finally {
      setLoading(false);
    }
  }

  async function handleDelete(id: string, name: string) {
    if (!confirm(`Xoá POI "${name}"?`)) return;
    await deletePoi(id);
    setPois(pois.filter((p: any) => p.id !== id));
  }

  return (
    <div className="flex h-full">
      <Sidebar role="Admin" />
      <main className="flex-1 p-6 overflow-auto">
        <div className="flex justify-between items-center mb-4">
          <h1 className="text-xl font-bold">Quản lý POI ({filtered.length})</h1>
          <button onClick={openCreate}
            className="bg-orange-500 text-white px-4 py-2 rounded-xl text-sm font-medium hover:bg-orange-600">
            + Tạo POI
          </button>
        </div>

        {/* Filter buttons */}
        <div className="flex gap-2 flex-wrap mb-5">
          {FILTER_OPTIONS.map(opt => (
            <button key={opt} onClick={() => setFilter(opt)}
              className={`px-3 py-1.5 rounded-full text-xs font-medium border transition-colors ${
                filter === opt
                  ? "bg-orange-500 text-white border-orange-500"
                  : "bg-white text-gray-600 border-gray-200 hover:border-orange-300 hover:text-orange-500"
              }`}>
              {opt === "Tất cả" ? `Tất cả (${pois.length})` : `${STATUS_LABEL[opt]} (${pois.filter((p: any) => p.status === opt).length})`}
            </button>
          ))}
        </div>

        {showForm && (
          <form onSubmit={handleSubmit}
            className="bg-white rounded-2xl p-6 mb-6 shadow-sm flex flex-col gap-3">
            <h2 className="font-semibold text-gray-700">{editingId ? "Chỉnh sửa POI" : "POI mới"}</h2>
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
            {!editingId && (
              <select required value={form.packageId}
                onChange={e => setForm({ ...form, packageId: e.target.value })}
                className="border rounded-xl px-3 py-2 text-sm">
                <option value="">Chọn gói</option>
                {packages.map((p: any) => (
                  <option key={p.id} value={p.id}>
                    {p.name} — {p.radiusMeters}m — {p.price.toLocaleString()}đ
                  </option>
                ))}
              </select>
            )}
            <textarea required placeholder="Mô tả (tiếng Việt)" value={form.description}
              onChange={e => setForm({ ...form, description: e.target.value })}
              rows={4} className="border rounded-xl px-3 py-2 text-sm resize-none" />
            <div className="flex gap-2">
              <button type="submit" disabled={loading}
                className="bg-orange-500 text-white px-4 py-2 rounded-xl text-sm font-medium hover:bg-orange-600 disabled:bg-gray-300">
                {loading ? "Đang lưu..." : editingId ? "Cập nhật" : "Tạo"}
              </button>
              <button type="button" onClick={cancelForm}
                className="text-gray-500 px-4 py-2 rounded-xl text-sm hover:bg-gray-100">Huỷ</button>
            </div>
          </form>
        )}

        <div className="flex flex-col gap-3">
          {filtered.map((poi: any) => (
            <div key={poi.id} className="bg-white rounded-2xl p-4 shadow-sm flex items-center gap-4">
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 mb-0.5">
                  <p className="font-semibold text-sm">{poi.name}</p>
                  <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${STATUS_COLOR[poi.status] ?? "bg-gray-100 text-gray-500"}`}>
                    {STATUS_LABEL[poi.status] ?? poi.status}
                  </span>
                </div>
                <p className="text-xs text-gray-400">{poi.lat}, {poi.lng} · {poi.radiusMeters}m</p>
                {poi.localizations?.find((l: any) => l.language === "vi") && (
                  <p className="text-xs text-gray-500 mt-1 line-clamp-1">
                    {poi.localizations.find((l: any) => l.language === "vi")?.textContent}
                  </p>
                )}
              </div>
              <div className="flex gap-2 shrink-0">
                <button onClick={() => openEdit(poi)}
                  className="text-xs bg-blue-50 text-blue-600 border border-blue-200 px-3 py-1 rounded-full hover:bg-blue-100">
                  Sửa
                </button>
                <button onClick={() => handleDelete(poi.id, poi.name)}
                  className="text-xs bg-red-50 text-red-500 border border-red-200 px-3 py-1 rounded-full hover:bg-red-100">
                  Xoá
                </button>
              </div>
            </div>
          ))}
          {filtered.length === 0 && (
            <p className="text-gray-400 text-sm">
              {pois.length === 0 ? "Chưa có POI nào." : "Không có POI nào ở trạng thái này."}
            </p>
          )}
        </div>
      </main>
    </div>
  );
}
