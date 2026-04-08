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

### 2.1) Chốt contract nghiệp vụ (quy tắc bắt buộc khi làm phần 2)

- [ ] Khóa state machine submission: chỉ cho phép `DRAFT -> SUBMITTED -> AUTO_EVALUATED -> APPROVED/REJECTED`.
- [ ] Chỉ cho phép sửa `report_field_values` khi submission ở trạng thái `DRAFT`.
- [ ] `Submit` chỉ áp dụng cho submission `DRAFT`, và phải đủ các field `is_required`.
- [ ] `AutoEvaluate` chỉ áp dụng cho submission `SUBMITTED`, phải lưu `rule_snapshot_json` cho từng field và cập nhật `evaluated_at`.
- [ ] `Approve/Reject` chỉ áp dụng cho submission `AUTO_EVALUATED`, phải lưu `approved_by_user_id`, `approved_at`, `manager_result`, `manager_note`.
- [ ] Chặn `Approve` khi còn bất kỳ rule `severity = ERROR` bị fail (nếu chưa có cơ chế override chính thức).
- [ ] Validate `report_field_values.field_id` thuộc đúng `template_version_id` của submission.
- [ ] Mọi transition phải ghi log vào `approval_logs` với `action`, `from_status`, `to_status`, `action_by_user_id`, `action_at`.
- [ ] `report_template_versions` đã `PUBLISHED` và đã có submission thì không cho sửa/xóa `template_fields` và `field_rules` của version đó.
- [ ] Tất cả timestamp nghiệp vụ dùng UTC (`created_at`, `updated_at`, `submitted_at`, `evaluated_at`, `approved_at`, `action_at`).

### 2.2) Checklist triển khai Core Workflow MVP (chia theo stack)

#### Stack A) Application Layer (business use cases)

- [ ] Thiết kế use case vòng đời template: tạo template, tạo version, thêm/sửa field, thêm/sửa rule.
- [ ] Thiết kế use case vòng đời submission: tạo draft, cập nhật field values, submit, auto evaluate, approve, reject, reopen.
- [ ] Thiết kế response model thống nhất cho workflow (`status`, `auto_result`, `manager_result`, timestamps, logs).

#### Stack B) Workflow API (luồng nghiệp vụ chính)

- [ ] `POST /api/submissions/draft`
- [ ] `PUT /api/submissions/{id}/fields`
- [ ] `POST /api/submissions/{id}/submit`
- [ ] `POST /api/submissions/{id}/evaluate`
- [ ] `POST /api/submissions/{id}/approve`
- [ ] `POST /api/submissions/{id}/reject`
- [ ] `GET /api/submissions` (filter theo template, date, status, created_by)

#### Stack C) CRUD Management API (CRUD toàn bảng)

- [ ] Chuẩn hóa endpoint CRUD cho mọi resource: `GET list`, `GET by id`, `POST`, `PUT`, `DELETE`.
- [ ] CRUD nhóm quyền/người dùng: `users`, `roles`, `user_roles`.
- [ ] CRUD nhóm template: `report_templates`, `report_template_versions`, `template_fields`, `field_rules`.
- [ ] CRUD nhóm vận hành báo cáo: `report_submissions`, `report_field_values`, `report_attachments`, `approval_logs`.

#### Stack D) UI theo vai trò

- [ ] Employee: tạo draft, nhập dữ liệu, submit báo cáo.
- [ ] Manager: xem danh sách, xem chi tiết, approve/reject, nhập ghi chú quản lý.
- [ ] Admin: quản trị template/version/field/rule bằng màn hình CRUD.

#### Stack E) Guardrails + Test + DoD

- [ ] Thêm guardrails nghiệp vụ ở API/Application để CRUD không phá workflow.
- [ ] Viết integration test cho luồng end-to-end: `draft -> submit -> evaluate -> approve/reject`.
- [ ] Viết smoke test cho CRUD toàn bảng (ít nhất case create/update/delete/list cơ bản).
- [ ] Định nghĩa tiêu chí hoàn thành phần 2 (DoD): workflow chạy end-to-end + CRUD toàn bảng + guardrails + test pass.

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
- [ ] Sprint 2: Submission workflow end-to-end + CRUD quản trị toàn bảng (không export).
- [ ] Sprint 3: Rule engine hoàn chỉnh + manager approval.
- [ ] Sprint 4: Export + hardening cơ bản.
