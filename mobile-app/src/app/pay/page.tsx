"use client";
import { Suspense, useState } from "react";
import { useSearchParams, useRouter } from "next/navigation";
import { initiatePayment } from "@/lib/api";

function PayContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const code = searchParams.get("code") ?? "";
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  if (!code) {
    router.replace("/scan");
    return null;
  }

  async function handlePay() {
    setLoading(true);
    setError("");
    try {
      const { paymentUrl } = await initiatePayment(code);
      window.location.href = paymentUrl;
    } catch {
      setError("QR code không hợp lệ hoặc đã bị vô hiệu hoá.");
      setLoading(false);
    }
  }

  return (
    <div className="flex flex-col h-full items-center justify-center p-6 gap-6">
      <div className="text-center">
        <div className="text-5xl mb-4">🎙️</div>
        <h1 className="text-2xl font-bold text-orange-500">AudioTravelling</h1>
        <p className="text-gray-500 text-sm mt-1">Phố ẩm thực Vĩnh Khánh</p>
      </div>

      <div className="w-full max-w-sm bg-white rounded-2xl shadow-md p-6 flex flex-col gap-4">
        <div className="text-center">
          <div className="text-4xl mb-2">✅</div>
          <p className="font-semibold text-gray-800">Quét mã QR thành công</p>
          <p className="text-xs text-gray-400 mt-1 font-mono">{code}</p>
        </div>

        <div className="bg-orange-50 rounded-xl p-4 text-center">
          <p className="text-sm text-gray-600">Phí truy cập</p>
          <p className="text-3xl font-bold text-orange-500 mt-1">10.000 ₫</p>
          <p className="text-xs text-gray-400 mt-1">Hiệu lực 24 giờ · Không giới hạn</p>
        </div>

        {error && <p className="text-red-500 text-xs text-center">{error}</p>}

        <button
          onClick={handlePay}
          disabled={loading}
          className="bg-orange-500 hover:bg-orange-600 disabled:bg-gray-300 text-white font-semibold py-3 rounded-xl transition-colors"
        >
          {loading ? "Đang xử lý..." : "Thanh toán qua VNPay"}
        </button>

        <button
          onClick={() => router.replace("/scan")}
          className="text-gray-400 text-sm text-center hover:text-gray-600 transition-colors"
        >
          Quét lại
        </button>
      </div>
    </div>
  );
}

export default function PayPage() {
  return (
    <Suspense
      fallback={
        <div className="flex h-full items-center justify-center text-gray-400">
          Đang tải...
        </div>
      }
    >
      <PayContent />
    </Suspense>
  );
}
