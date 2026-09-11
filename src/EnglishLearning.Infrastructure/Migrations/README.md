# EF Core migrations
Migration InitialCreate và model snapshot sẽ được EF Core tạo bằng scripts/setup.ps1 (hoặc setup.sh) ở lần thiết lập đầu tiên. Chưa có migration .cs được sinh sẵn vì môi trường tạo project không có .NET SDK.

Script chỉ tạo InitialCreate khi chưa tồn tại; các lần tiếp theo áp dụng migration hiện có. Sau khi sinh, commit toàn bộ file .cs và snapshot để các máy khác dùng chung lịch sử. Không xoá migration đã áp dụng.

Sau khi thay đổi entities:
```
dotnet ef migrations add DescribeYourChange --project src/EnglishLearning.Infrastructure --startup-project src/EnglishLearning.Web --output-dir Migrations
dotnet ef database update --project src/EnglishLearning.Infrastructure --startup-project src/EnglishLearning.Web
```
