"use client";
import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { getSessionToken, clearSessionToken, verifySession } from "@/lib/api";
import dynamic from "next/dynamic";

const MapView = dynamic(() => import("@/components/MapView"), { ssr: false });

export default function MapPage() {
  const router = useRouter();

  useEffect(() => {
    if (!getSessionToken()) {
      router.replace("/scan");
      return;
    }

    // Verify ngầm, không block map render
    verifySession().then((result) => {
      if (result.status === "invalid") {
        clearSessionToken();
        router.replace("/scan");
      }
    });

    if ("wakeLock" in navigator) {
      (navigator as any).wakeLock.request("screen").catch(() => {});
    }
  }, [router]);

  return (
    <div className="flex flex-col h-full">
      <header className="bg-orange-500 text-white px-4 py-2.5 flex items-center gap-2 z-10 shadow">
        <span className="text-xl">🎙️</span>
        <span className="font-bold text-sm">AudioTravelling — Vĩnh Khánh</span>
      </header>
      <div className="flex-1 relative">
        <MapView />
      </div>
    </div>
  );
}
