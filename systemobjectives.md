# System Objectives Checklist

## 0) Current status
- [x] Đã tạo migration thành công.
- [x] Đã kết nối SQL Server thành công.

## 1) Chốt toolchain (làm ngay)
- [ ] Thêm `global.json` để pin đúng SDK .NET 8 cho toàn solution.
- [ ] Chuẩn hóa `Program.cs`:
- [ ] Đăng ký `AddDbContext<ReportSystemDbContext>()` trước `builder.Build()`.
- [ ] Tách registration service theo extension method (Application/Infrastructure) để dễ mở rộng.
- [ ] Bổ sung cấu hình môi trường (`appsettings.Development.json`, `appsettings.Production.json`) rõ ràng cho connection string.
- [ ] Quy ước thời gian UTC cho toàn bộ trường `datetime2` (create/update/submit/approve/evaluate).

## 2) Seed dữ liệu tối thiểu (làm ngay sau toolchain)
- [ ] Seed bảng `roles`: `EMPLOYEE`, `MANAGER`, `ADMIN`.
- [ ] Seed ít nhất 1 user admin + gán `ADMIN` qua `user_roles`.
- [ ] Seed 2 template mẫu:
- [ ] `PH_METER_DAILY_CHECK` với field và rule `slope 85-115`.
- [ ] `DISTILLED_WATER_QUALITY_CHECK` với rule `ph`, `tpc`, `ec`, `chlorine`.
- [ ] Seed theo mô hình version: `report_templates` -> `report_template_versions(PUBLISHED)` -> `template_fields` -> `field_rules`.
- [ ] Bảo đảm seed idempotent (chạy lại không tạo trùng).

## 3) Core workflow MVP
- [ ] Thiết kế use case lớp Application:
- [ ] Tạo template/version/field/rule.
- [ ] Tạo submission draft, submit, approve/reject.
- [ ] Auto evaluate submission theo rule.
- [ ] Xây API REST cho workflow chính.
- [ ] Xây UI cho 3 vai trò:
- [ ] Employee: tạo và gửi báo cáo.
- [ ] Manager: xem, duyệt/từ chối.
- [ ] Admin: quản lý template/rule.

## 4) Rule engine và ràng buộc nghiệp vụ
- [ ] Implement các rule type: `RANGE`, `LT`, `LTE`, `GT`, `GTE`, `REGEX`, `IN_SET`.
- [ ] Lưu `rule_snapshot_json` khi evaluate.
- [ ] Tổng hợp `auto_result` của submission từ kết quả từng field.
- [ ] Chặn duyệt khi chưa qua `SUBMITTED` và `AUTO_EVALUATED`.
- [ ] Validate `report_field_values.field_id` thuộc đúng `template_version_id` của submission.

## 5) Attachment + Export
- [ ] Upload/lưu metadata ảnh minh chứng (`report_attachments`).
- [ ] Danh sách báo cáo có filter theo template, date, status, created_by.
- [ ] Export Excel cho báo cáo.
- [ ] Export PDF (sau Excel).

## 6) Hardening trước production
- [ ] Hoàn thiện authentication + authorization theo role.
- [ ] Thêm approval log đầy đủ cho mọi transition.
- [ ] Viết test:
- [ ] Unit test cho rule engine.
- [ ] Integration test cho workflow.
- [ ] Thiết lập logging và error handling chuẩn.
- [ ] Chuẩn bị Docker/deploy script + hướng dẫn vận hành.

## 7) Thứ tự ưu tiên sprint gần nhất
- [ ] Sprint 1: Chốt toolchain + seed tối thiểu.
- [ ] Sprint 2: Submission workflow end-to-end (không export).
- [ ] Sprint 3: Rule engine hoàn chỉnh + manager approval.
- [ ] Sprint 4: Export + hardening cơ bản.
