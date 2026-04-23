"use client";
import { useEffect, useState } from "react";
import { getQrCodes, createQr, toggleQr, deleteQr, regenerateQr } from "@/lib/api";
import Sidebar from "@/components/Sidebar";

export default function AdminQrPage() {
  const [codes, setCodes] = useState<any[]>([]);

  useEffect(() => { getQrCodes().then(setCodes); }, []);

  async function handleCreate() {
    const qr = await createQr();
    setCodes([qr, ...codes]);
  }

  async function handleToggle(id: string) {
    const updated = await toggleQr(id);
    setCodes(codes.map(c => c.id === id ? { ...c, isActive: updated.isActive } : c));
  }

  async function handleDelete(id: string) {
    if (!confirm("Xoá QR này?")) return;
    await deleteQr(id);
    setCodes(codes.filter(c => c.id !== id));
  }

  async function handleRegenerateAll() {
    if (!confirm("Tạo lại tất cả QR với URL hiện tại?")) return;
    await Promise.all(codes.map(c => regenerateQr(c.id)));
    alert("Đã tạo lại tất cả QR. Reload trang để xem ảnh mới.");
  }

  const API = process.env.NEXT_PUBLIC_WEBAPP_API_URL ?? "http://localhost:5000";

  return (
    <div className="flex h-full">
      <Sidebar role="Admin" />
      <main className="flex-1 p-6 overflow-auto">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-xl font-bold">Quản lý QR</h1>
          <div className="flex gap-2">
            {codes.length > 0 && (
              <button onClick={handleRegenerateAll}
                className="bg-blue-500 text-white px-4 py-2 rounded-xl text-sm font-medium hover:bg-blue-600">
                Tạo lại tất cả QR
              </button>
            )}
            <button onClick={handleCreate}
              className="bg-orange-500 text-white px-4 py-2 rounded-xl text-sm font-medium hover:bg-orange-600">
              + Tạo QR mới
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {codes.map((c: any) => (
            <div key={c.id} className={`bg-white rounded-2xl p-4 shadow-sm border-2 ${c.isActive ? "border-green-200" : "border-gray-200"}`}>
              <div className="flex justify-between items-start mb-3">
                <span className="font-mono text-sm font-bold tracking-wider">{c.code}</span>
                <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${c.isActive ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"}`}>
                  {c.isActive ? "Hoạt động" : "Tắt"}
                </span>
              </div>
              <p className="text-xs text-gray-400 mb-3">
                {new Date(c.createdAt).toLocaleDateString("vi-VN")}
              </p>
              <div className="flex gap-2">
                <button onClick={() => handleToggle(c.id)}
                  className="flex-1 text-xs border rounded-full py-1 hover:bg-gray-50">
                  {c.isActive ? "Tắt" : "Bật"}
                </button>
                <button onClick={() => handleDelete(c.id)}
                  className="text-xs text-red-500 hover:text-red-700 px-2">Xoá</button>
              </div>
            </div>
          ))}
          {codes.length === 0 && <p className="text-gray-400 text-sm col-span-3">Chưa có QR nào.</p>}
        </div>
      </main>
    </div>
  );
}
