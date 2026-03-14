using System;

public class Class1
{
	public void Main(string[] args)
	{
        var builder = WebApplication.CreateBuilder(args);

        // 1. ĐĂNG KÝ DỊCH VỤ (SERVICES)
        // Dòng này cho phép C# hiểu và điều hướng các View (.cshtml) anh vừa tách
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // 2. CẤU HÌNH PIPELINE (MIDDLEWARE)
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        // QUAN TRỌNG: Cho phép truy cập file tĩnh trong thư mục wwwroot
        // Nếu không có dòng này, style.css và app.js sẽ bị lỗi 404
        app.UseStaticFiles();

        app.UseRouting();

        // Cần thiết khi anh làm chức năng Đăng nhập với Google (Social Login)
        app.UseAuthorization();

        // 3. ĐỊNH TUYẾN (ROUTING)
        // Cấu hình để khi mở web, nó sẽ tự tìm đến HomeController -> Index()
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
