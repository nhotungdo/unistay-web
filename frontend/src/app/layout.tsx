import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import Navbar from "@/components/Navbar";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "Unistay - Nền tảng tìm phòng trọ sinh viên",
  description: "Tìm phòng trọ, người ở ghép nhanh chóng, dễ dàng cho sinh viên.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi">
      <body className={inter.className}>
        <Navbar />
        <main className="main-content">
          {children}
        </main>
      </body>
    </html>
  );
}
