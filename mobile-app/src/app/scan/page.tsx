"use client";
import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";

export default function ScanPage() {
  const router = useRouter();
  const [error, setError] = useState("");
  const stoppedRef = useRef(false);

  useEffect(() => {
    stoppedRef.current = false;
    let scanner: import("html5-qrcode").Html5Qrcode | null = null;

    const start = async () => {
      const { Html5Qrcode } = await import("html5-qrcode");
      if (stoppedRef.current) return;

      scanner = new Html5Qrcode("qr-reader");

      try {
        await scanner.start(
          { facingMode: "environment" },
          { fps: 10, qrbox: { width: 260, height: 260 } },
          (decodedText: string) => {
            if (stoppedRef.current) return;
            stoppedRef.current = true;

            let code = decodedText.trim();
            try {
              const url = new URL(decodedText);
              code = url.searchParams.get("code") ?? code;
            } catch {
              // không phải URL — dùng thẳng giá trị quét được
            }

            scanner?.stop().finally(() =>
              router.push(`/pay?code=${encodeURIComponent(code.toUpperCase())}`)
            );
          },
          undefined
        );
      } catch {
        setError("Không thể truy cập camera. Vui lòng cấp quyền camera.");
      }
    };

    start();

    return () => {
      stoppedRef.current = true;
      scanner?.stop().catch(() => {});
    };
  }, [router]);

  return (
    <div className="flex flex-col h-full items-center justify-center p-6 gap-6">
      <div className="text-center">
        <div className="text-5xl mb-4">🎙️</div>
        <h1 className="text-2xl font-bold text-orange-500">AudioTravelling</h1>
        <p className="text-gray-500 text-sm mt-1">Phố ẩm thực Vĩnh Khánh</p>
      </div>

      <div className="w-full max-w-sm flex flex-col gap-3">
        <p className="text-gray-700 text-center text-sm">
          Hướng camera vào mã QR của cửa hàng
        </p>
        <div id="qr-reader" className="rounded-2xl overflow-hidden shadow-md" />
        {error && (
          <p className="text-red-500 text-xs text-center">{error}</p>
        )}
      </div>

      <p className="text-xs text-gray-400 text-center">
        Sau khi thanh toán, bạn có 24 giờ truy cập không giới hạn.
      </p>
    </div>
  );
}
