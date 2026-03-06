```text
AudioTravellingSystem (Solution)
│
├── 📂 AudioTravelling.Shared (Class Library)
│   ├── 📂 Entities          <-- Các bảng DB anh vừa vẽ (Tour, POI, Media...)
│   ├── 📂 Enums             <-- MediaType, LanguageCode...
│   └── 📂 Models            <-- Data Transfer Objects (DTOs)
│
├── 📂 AudioTravelling.WebAdmin (ASP.NET Core MVC - Giống Repo GitHub)
│   ├── 📂 Controllers       <-- Quản lý Tour, POI cho Web
│   ├── 📂 Models            <-- ViewModel cho trang Web
│   ├── 📂 Views             <-- Giao diện Admin (HTML/Razor)
│   ├── 📂 wwwroot           <-- CSS, JS, Ảnh của trang Web
│   └── 📄 Program.cs        <-- Cấu hình chạy Web
│
├── 📂 AudioTravelling.Api (ASP.NET Core Web API)
│   ├── 📂 Controllers       <-- Trả về dữ liệu JSON cho Mobile App
│   └── 📂 Services          <-- Logic tính toán Geofencing, xử lý file Audio
│
└── 📂 AudioTravelling.Mobile (.NET MAUI - Phần anh muốn thêm)
    ├── 📂 Platforms         <-- Cấu hình riêng cho Android/iOS
    ├── 📂 Resources         <-- Icon, Splash screen, Fonts
    ├── 📂 Views             <-- Giao diện App (XAML)
    ├── 📂 ViewModels        <-- Logic điều khiển giao diện App
    ├── 📂 Services          <-- Gọi API lấy dữ liệu Tour/POI
    └── 📄 MauiProgram.cs    <-- Cấu hình chạy App Mobile
```
