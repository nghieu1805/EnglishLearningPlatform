-- Chạy bằng tài khoản quản trị trong MySQL Workbench.
-- Thay CHANGE_THIS_LOCAL_PASSWORD trước khi chạy; không commit mật khẩu thật.
CREATE DATABASE IF NOT EXISTS EnglishLearningDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER IF NOT EXISTS 'eapp'@'localhost' IDENTIFIED BY 'CHANGE_THIS_LOCAL_PASSWORD';
GRANT ALL PRIVILEGES ON EnglishLearningDb.* TO 'eapp'@'localhost';
-- Nếu tài khoản eapp đã tồn tại, CREATE USER IF NOT EXISTS không thay mật khẩu.
-- Dùng mật khẩu hiện có hoặc chủ động đổi bằng ALTER USER trong Workbench.
