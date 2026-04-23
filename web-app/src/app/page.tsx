"use client";
import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { getToken } from "@/lib/api";

export default function Home() {
  const router = useRouter();
  useEffect(() => {
    const role = localStorage.getItem("admin_role");
    if (getToken()) {
      router.replace(role === "Admin" ? "/admin/pois" : "/pois");
    } else {
      router.replace("/login");
    }
  }, [router]);
  return null;
}
