# EnglishLearningPlatform — VS Code

Project C# ASP.NET Core MVC theo README đính kèm. Dùng **MySQL local**, không cần Docker, không dùng database cloud. Đây là bản khởi đầu có mã nguồn cho Phase 1 và các luồng chính Phase 2; không phải toàn bộ roadmap tương lai.

**Bắt đầu bằng [HUONG-DAN-CHAY.md](HUONG-DAN-CHAY.md).** Bản yêu cầu đầy đủ được giữ nguyên tại [README-ORIGINAL.md](README-ORIGINAL.md).

## Công nghệ

- .NET 8 / C# 12, ASP.NET Core MVC + Identity.
- EF Core/Identity 8.0.13; Pomelo MySQL 8.0.3.
- MySQL Server 8.x trên localhost:3306, database EnglishLearningDb.
- HTML, Razor, CSS, JavaScript; Bootstrap 5.3.3 qua CDN. Giao diện có CSS riêng nên vẫn sử dụng được khi CDN/font không tải.
- xUnit: kiểm thử chấm điểm, dữ liệu giả mạo, snapshot kết quả.

Các version được ghim để cùng major tương thích, không tuyên bố là bản vá mới nhất. Cần kiểm tra cập nhật bảo mật trước triển khai thực tế. global.json cho phép dùng SDK .NET 8 đã cài ở feature band mới hơn.

## Kiến trúc

```
EnglishLearningPlatform.sln
src/
  EnglishLearning.Domain/          Entities, enums
  EnglishLearning.Application/     Interfaces, DTOs, QuizService
  EnglishLearning.Infrastructure/  EF Core, MySQL, Identity, repository, seeder
  EnglishLearning.Web/             MVC controllers, Razor views, CSS/JS
 tests/EnglishLearning.Tests/      xUnit
 .vscode/                         F5, build, run, extensions
 scripts/                         Setup Windows / Linux
 database/                        SQL tạo database và user local
```

Dependency: Application → Domain; Infrastructure → Application; Web → Application + Infrastructure. Các luồng học dùng repository; admin CRUD và truy vấn dashboard dùng DbContext trực tiếp ở Web để giữ bản đầu dễ hiểu. Không tuyên bố Clean Architecture tuyệt đối.

## Đã có mã nguồn

- Lựa chọn 12 lớp; chủ đề theo lớp, thứ tự học, bản nháp/xuất bản.
- Từ vựng, nghĩa Việt, loại từ, CEFR, UK/US phonetics, ví dụ/dịch, họ từ và từ liên quan.
- Nhiều nguồn SGK/Unit cho một chủ đề; mỗi từ trong bài chính thức phải trỏ đúng nguồn của chủ đề. Nguồn ghi tên sách và phiên bản do admin nhập.
- Bản ghi âm HTTPS UK/US; nếu chưa có, nút đọc bằng giọng máy chỉ hoạt động khi thiết bị có đúng giọng tương ứng. Không gán nhãn giọng máy là bản ghi chuẩn.
- Flashcard và quiz trắc nghiệm; chấm điểm trên server, kiểm tra đáp án thuộc đúng câu, lưu đáp án và nội dung snapshot.
- Đăng ký, đăng nhập, đăng xuất bằng Identity, khoá tạm sau 5 lần đăng nhập sai, hồ sơ/lớp hiện tại.
- Lưu trạng thái New / Learning / Learned / NeedsReview, tổng hợp tiến độ theo chủ đề, danh sách từ đã lưu, lịch sử 100 quiz gần nhất.
- Từ điển nội bộ, có từ độc lập không cần thuộc bài học; tìm tối đa 50 kết quả.
- Admin CRUD lớp/chủ đề/nguồn/từ/ví dụ/audio/quiz/câu hỏi/đáp án, danh sách người dùng chỉ đọc, số liệu tổng quan.
- Chống CSRF toàn bộ POST, role Admin, kiểm tra dữ liệu, mật khẩu do Identity hash, không lấy UserId hay điểm quiz từ client.

## Dữ liệu khởi tạo

Seed tạo 12 lớp, 1 chủ đề **School — DEMO** ở lớp 6 với 4 từ, quiz 2 câu và từ điển độc lập `sustainability`. Câu ví dụ do người tạo project tự viết. Đây **không phải** bộ nội dung SGK chính thức. Không tự gán nguồn sách, CEFR, IPA hay audio chưa xác minh. Lớp khác hiển thị trạng thái chờ nội dung.

Tắt dữ liệu demo trước seed bằng cấu hình `Seed:DemoContent=false`. Tắt cờ không xoá dữ liệu demo đã nhập trước đó.

Quy trình nội dung chính thức: tạo topic bản nháp → thêm nguồn SGK/Unit → thêm từ và nguồn của từng từ → thêm ví dụ/audio/quiz → kiểm tra và xuất bản. Giữ tên chủ đề gốc trong TopicSource khi gộp chủ đề. Cần con người đối chiếu SGK và quyền sử dụng nội dung; ứng dụng chỉ kiểm tra liên kết dữ liệu.

## Phần chưa triển khai

- Quên mật khẩu/email phục hồi, xác nhận email, quản lý quyền/xoá người dùng từ giao diện.
- Từ điển API/AI trả kết quả cho mọi từ; kho SGK đầy đủ lớp 1–12.
- Matching, điền từ, listening/spelling/word ordering; hiện có flashcard + trắc nghiệm.
- Streak, daily goal, SRS, XP/badges, grammar, reading, writing, speaking, AI Tutor, teacher/parent dashboards.
- Phân trang/khả năng mở rộng cho admin; bản này dành cho dữ liệu local nhỏ.
- UserTopicProgress được tính từ trạng thái từ, không tạo bảng tổng hợp riêng. Chưa có entity GrammarLesson vì thuộc phase sau.
- Migration .cs và snapshot chưa sinh sẵn: setup script sinh bằng EF Core trên máy bạn rồi cập nhật DB.

## Kiểm tra và giới hạn xác minh

Môi trường tạo gói không có `dotnet`, `mysql` hay PowerShell và không tải được SDK. Vì vậy **chưa chạy dotnet restore/build/test, migration hoặc kiểm thử MySQL/giao diện**. Đã kiểm tra cấu trúc solution, tham chiếu project, JSON/XML, đường dẫn view, cú pháp JavaScript và script shell. Các bài xUnit được cung cấp để chạy trên máy có .NET SDK; không coi chúng là đã pass.

Chạy `scripts/setup.ps1` để restore → build → sinh migration → cập nhật DB → seed → test. Script dừng ngay ở bước lỗi. Xem checklist tại [docs/TESTING.md](docs/TESTING.md).
