# System Objectives Checklist

## 0) Current status

- [x] Đã tạo migration thành công.
- [x] Đã kết nối SQL Server thành công.

## 1) Seed dữ liệu tối thiểu

- [x] Seed bảng `roles`: `EMPLOYEE`, `MANAGER`, `ADMIN`.
- [x] Seed ít nhất 1 user admin + gán `ADMIN` qua `user_roles`.
- [x] Seed 2 template mẫu:
- [x] `PH_METER_DAILY_CHECK` với field và rule `slope 85-115`.
- [x] `DISTILLED_WATER_QUALITY_CHECK` với rule `ph`, `tpc`, `ec`, `chlorine`.
- [x] Seed theo mô hình version: `report_templates` -> `report_template_versions(PUBLISHED)` -> `template_fields` -> `field_rules`.
- [x] Bảo đảm seed idempotent (chạy lại không tạo trùng).

## 2) Core workflow MVP

- [ ] Thiết kế use case lớp Application:
- [ ] Tạo template/version/field/rule.
- [ ] Tạo submission draft, submit, approve/reject.
- [ ] Auto evaluate submission theo rule.
- [ ] Xây API REST cho workflow chính.
- [ ] Xây UI cho 3 vai trò:
- [ ] Employee: tạo và gửi báo cáo.
- [ ] Manager: xem, duyệt/từ chối.
- [ ] Admin: quản lý template/rule.

## 3) Rule engine và ràng buộc nghiệp vụ

- [ ] Implement các rule type: `RANGE`, `LT`, `LTE`, `GT`, `GTE`, `REGEX`, `IN_SET`.
- [ ] Lưu `rule_snapshot_json` khi evaluate.
- [ ] Tổng hợp `auto_result` của submission từ kết quả từng field.
- [ ] Chặn duyệt khi chưa qua `SUBMITTED` và `AUTO_EVALUATED`.
- [ ] Validate `report_field_values.field_id` thuộc đúng `template_version_id` của submission.

## 4) Attachment + Export

- [ ] Upload/lưu metadata ảnh minh chứng (`report_attachments`).
- [ ] Danh sách báo cáo có filter theo template, date, status, created_by.
- [ ] Export Excel cho báo cáo.
- [ ] Export PDF (sau Excel).

## 5) Hardening trước production

- [ ] Hoàn thiện authentication + authorization theo role.
- [ ] Thêm approval log đầy đủ cho mọi transition.
- [ ] Viết test:
- [ ] Unit test cho rule engine.
- [ ] Integration test cho workflow.
- [ ] Thiết lập logging và error handling chuẩn.
- [ ] Chuẩn bị Docker/deploy script + hướng dẫn vận hành.

## 6) Thứ tự ưu tiên sprint gần nhất

- [x] Sprint 1: Seed tối thiểu.
- [ ] Sprint 2: Submission workflow end-to-end (không export).
- [ ] Sprint 3: Rule engine hoàn chỉnh + manager approval.
- [ ] Sprint 4: Export + hardening cơ bản.
