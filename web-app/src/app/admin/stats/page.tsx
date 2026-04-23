"use client";
import { useEffect, useState } from "react";
import { getStats } from "@/lib/api";
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from "recharts";
import Sidebar from "@/components/Sidebar";

const PERIODS = [
  { value: "day", label: "Hôm nay" },
  { value: "3days", label: "3 ngày" },
  { value: "week", label: "Tuần" },
  { value: "month", label: "Tháng" },
  { value: "year", label: "Năm" },
];

export default function StatsPage() {
  const [period, setPeriod] = useState("day");
  const [data, setData] = useState<any[]>([]);
  const [total, setTotal] = useState(0);

  useEffect(() => {
    getStats(period).then((res) => {
      setData(res);
      setTotal(res.reduce((sum: number, r: any) => sum + r.count, 0));
    });
  }, [period]);

  return (
    <div className="flex h-full">
      <Sidebar role="Admin" />
      <main className="flex-1 p-6 overflow-auto">
        <h1 className="text-xl font-bold mb-4">Thống kê truy cập</h1>

        <div className="flex gap-2 mb-6">
          {PERIODS.map(p => (
            <button key={p.value} onClick={() => setPeriod(p.value)}
              className={`px-4 py-1.5 rounded-full text-sm font-medium transition-colors ${period === p.value ? "bg-orange-500 text-white" : "bg-white text-gray-600 hover:bg-gray-100"}`}>
              {p.label}
            </button>
          ))}
        </div>

        <div className="bg-white rounded-2xl p-5 shadow-sm mb-6">
          <p className="text-sm text-gray-500 mb-1">Tổng lượt truy cập</p>
          <p className="text-3xl font-bold text-orange-500">{total.toLocaleString()}</p>
        </div>

        <div className="bg-white rounded-2xl p-5 shadow-sm">
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={data} margin={{ top: 5, right: 10, bottom: 5, left: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis dataKey="date" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip />
              <Bar dataKey="count" fill="#f97316" radius={[4, 4, 0, 0]} name="Lượt truy cập" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </main>
    </div>
  );
}
