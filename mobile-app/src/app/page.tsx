"use client";
import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { getSessionToken, setSessionToken, clearSessionToken, verifySession } from "@/lib/api";

export default function Home() {
  const router = useRouter();

  useEffect(() => {
    async function check() {
      // VNPay callback: token vừa được cấp → lưu và vào map ngay
      const params = new URLSearchParams(window.location.search);
      const urlToken = params.get("token");
      if (urlToken) {
        setSessionToken(urlToken);
        router.replace("/map");
        return;
      }

      // Không có token → đi quét QR
      if (!getSessionToken()) {
        router.replace("/scan");
        return;
      }

      // Có token → xác minh với server
      const result = await verifySession();
      if (result.status === "valid") {
        router.replace("/map");
      } else if (result.status === "invalid") {
        clearSessionToken();
        router.replace("/scan");
      } else {
        // offline: tin localStorage, vào map
        router.replace("/map");
      }
    }
    check();
  }, [router]);

  return (
    <div className="flex h-full items-center justify-center">
      <div className="text-orange-500 text-lg font-medium animate-pulse">Đang tải...</div>
    </div>
  );
}
