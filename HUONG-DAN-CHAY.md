# Mở và chạy project bằng VS Code trên Windows

## 1. Chuẩn bị

Cài **.NET 8 SDK** (SDK, không chỉ Runtime), VS Code với C# Dev Kit, MySQL Community Server 8.x và MySQL Workbench. Kiểm tra trong terminal mới:

```powershell
dotnet --list-sdks
mysql --version
```

Nếu `mysql` không được nhận trong terminal nhưng Workbench kết nối được thì vẫn dùng Workbench chạy SQL; không cần thêm MySQL vào PATH để app chạy.

Giải nén vào ví dụ `F:\EnglishLearningPlatform`, mở **đúng thư mục có EnglishLearningPlatform.sln** bằng File → Open Folder. Mở Terminal → New Terminal.

## 2. Tạo database

Trong MySQL Workbench, đăng nhập server local bằng tài khoản quản trị. Mở `database/create-database.sql`, thay `CHANGE_THIS_LOCAL_PASSWORD` bằng mật khẩu local bạn chọn và chạy SQL. Không lưu mật khẩu thật vào file để commit.

Database: EnglishLearningDb; host localhost; port 3306; user eapp.

## 3. Cấu hình kết nối và tài khoản Admin

Ở terminal tại thư mục solution:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=EnglishLearningDb;User=eapp;Password=YOUR_MYSQL_PASSWORD;" --project src/EnglishLearning.Web

dotnet user-secrets set "Seed:AdminEmail" "admin@example.com" --project src/EnglishLearning.Web

dotnet user-secrets set "Seed:AdminPassword" "YOUR_ADMIN_PASSWORD" --project src/EnglishLearning.Web
```

Thay YOUR_MYSQL_PASSWORD bằng mật khẩu của user eapp. Thay YOUR_ADMIN_PASSWORD bằng mật khẩu ít nhất 8 ký tự, có chữ hoa, chữ thường, số, ký tự đặc biệt. `admin@example.com` chỉ là ví dụ: có thể đổi thành email bạn muốn dùng đăng nhập. Không có tài khoản admin/mật khẩu cố định trong mã nguồn.

User Secrets được nạp ở môi trường Development. Đặt biến môi trường trước khi chạy setup:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
```

User Secrets nằm ngoài source nhưng không phải kho mã hoá. Không gửi cho người khác; với triển khai thật dùng cơ chế secrets của máy chủ.

## 4. Thiết lập lần đầu

```powershell
.\scripts\setup.ps1
```

Script thực hiện restore packages, build, sinh InitialCreate kèm snapshot nếu chưa có, cập nhật MySQL, seed và chạy unit tests. Cần Internet lần đầu để tải NuGet. Giữ MySQL service đang chạy.

Nếu Windows chặn chạy `.ps1`, không cần đổi policy toàn máy: chạy lần lượt các lệnh tương đương dưới đây:

```powershell
dotnet tool restore
dotnet restore EnglishLearningPlatform.sln
dotnet build EnglishLearningPlatform.sln
dotnet ef migrations add InitialCreate --project src/EnglishLearning.Infrastructure --startup-project src/EnglishLearning.Web --output-dir Migrations
dotnet ef database update --project src/EnglishLearning.Infrastructure --startup-project src/EnglishLearning.Web
dotnet run --project src/EnglishLearning.Web --no-launch-profile -- --seed
dotnet test EnglishLearningPlatform.sln
```

Chỉ chạy `migrations add InitialCreate` khi chưa có migration đó. Sau khi sinh, giữ và commit các migration .cs cùng snapshot. Không dùng EnsureCreated cho DB này.

## 5. Chạy website

```powershell
dotnet watch --project src/EnglishLearning.Web run
```

Mở **http://localhost:5000** (hoặc URL terminal báo). Có thể nhấn **F5** trong VS Code để debug sau khi setup xong. Dừng bằng Ctrl+C.

Không chạy `dotnet run` trống tại thư mục solution vì có nhiều project. Luôn chỉ rõ project Web.

## 6. Thử nhanh

1. Trang chủ → Lớp 6 → School — DEMO: xem 4 từ và flashcard.
2. Đăng ký tài khoản học sinh → lưu trạng thái từ → làm quiz → xem Tiến độ.
3. Từ điển → tìm `sustainability`: từ này độc lập với bài học.
4. Đăng xuất → đăng nhập bằng Admin đã cấu hình → Quản trị.
5. Tạo topic bản nháp → thêm TopicSource → Vocabulary → Examples/Audio → xuất bản.
6. Tạo Exercise → Question → tối thiểu 2 Answer và đúng 1 IsCorrect; quiz sẽ chỉ mở khi tất cả câu hợp lệ.

Sau khi đã tạo admin, có thể xoá mật khẩu bootstrap khỏi secrets:

```powershell
dotnet user-secrets remove "Seed:AdminPassword" --project src/EnglishLearning.Web
```

Tài khoản đã tạo vẫn còn và đăng nhập bằng mật khẩu đã chọn. Chạy seed lại không đặt lại mật khẩu của tài khoản đã tồn tại. Đăng ký thông thường không tạo quyền Admin.

## Lỗi thường gặp

| Lỗi | Kiểm tra |
|---|---|
| dotnet không được nhận | Cài SDK, mở terminal mới; `dotnet --list-sdks` phải có 8.x |
| SDK version not found | Cài .NET 8 SDK, không chỉ .NET 9/10; global.json đang chọn 8 |
| Unable to connect to MySQL hosts | MySQL service, localhost:3306, tài khoản, mật khẩu trong secrets |
| Access denied for user eapp | Mật khẩu và user/host đúng; CREATE USER IF NOT EXISTS không đổi mật khẩu user cũ |
| Table doesn't exist | Chạy EF database update rồi seed |
| No project was found | Mở thư mục solution và dùng --project/--startup-project đúng như hướng dẫn |
| Admin không có mục Quản trị | Đặt cả AdminEmail/AdminPassword và chạy seed ở Development, sau đó đăng nhập lại |
| Không phát âm | Cài giọng en-GB/en-US ở hệ điều hành hoặc thêm URL audio HTTPS hợp lệ |
| Quiz 404/chưa sẵn sàng | Topic phải Published; có câu hỏi; mỗi câu ≥2 đáp án và đúng 1 đáp án đúng |
| NuGet restore thất bại | Kiểm tra Internet/proxy/nuget.org; giữ version EF/Identity/Pomelo tương thích |

## Linux/macOS

Có thể dùng `bash scripts/setup.sh` sau khi cấu hình MySQL và secrets tương tự. Trước đó đặt `export ASPNETCORE_ENVIRONMENT=Development`. Project dùng MySQL local nên không cần Docker.
