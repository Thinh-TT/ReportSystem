# System Objectives Checklist

## 0) Current status

- [x] Da tao migration thanh cong.
- [x] Da ket noi SQL Server thanh cong.

## 1) Seed du lieu toi thieu

- [x] Seed bang `roles`: `EMPLOYEE`, `MANAGER`, `ADMIN`.
- [x] Seed it nhat 1 user admin + gan `ADMIN` qua `user_roles`.
- [x] Seed 2 template mau:
- [x] `PH_METER_DAILY_CHECK` voi field va rule `slope 85-115`.
- [x] `DISTILLED_WATER_QUALITY_CHECK` voi rule `ph`, `tpc`, `ec`, `chlorine`.
- [x] Seed theo mo hinh version: `report_templates` -> `report_template_versions(PUBLISHED)` -> `template_fields` -> `field_rules`.
- [x] Bao dam seed idempotent (chay lai khong tao trung).

## 2) Core workflow MVP

### 2.1) Chot contract nghiep vu (quy tac bat buoc khi lam phan 2)

- [x] Khoa state machine submission: chi cho phep `DRAFT -> SUBMITTED -> AUTO_EVALUATED -> APPROVED/REJECTED`.
- [x] Chi cho phep sua `report_field_values` khi submission o trang thai `DRAFT`.
- [x] `Submit` chi ap dung cho submission `DRAFT`, va phai du cac field `is_required`.
- [x] `AutoEvaluate` chi ap dung cho submission `SUBMITTED`, phai luu `rule_snapshot_json` cho tung field va cap nhat `evaluated_at`.
- [x] `Approve/Reject` chi ap dung cho submission `AUTO_EVALUATED`, phai luu `approved_by_user_id`, `approved_at`, `manager_result`, `manager_note`.
- [x] Chan `Approve` khi con bat ky rule `severity = ERROR` bi fail (neu chua co co che override chinh thuc).
- [x] Validate `report_field_values.field_id` thuoc dung `template_version_id` cua submission.
- [x] Moi transition phai ghi log vao `approval_logs` voi `action`, `from_status`, `to_status`, `action_by_user_id`, `action_at`.
- [x] `report_template_versions` da `PUBLISHED` va da co submission thi khong cho sua/xoa `template_fields` va `field_rules` cua version do.
- [x] Tat ca timestamp nghiep vu dung UTC (`created_at`, `updated_at`, `submitted_at`, `evaluated_at`, `approved_at`, `action_at`).

### 2.2) Checklist trien khai Core Workflow MVP (chia theo stack)

#### Stack A) Application Layer (business use cases)

- [x] Thiet ke use case vong doi template: tao template, tao version, them/sua field, them/sua rule.
- [x] Thiet ke use case vong doi submission: tao draft, cap nhat field values, submit, auto evaluate, approve, reject, reopen.
- [x] Thiet ke response model thong nhat cho workflow (`status`, `auto_result`, `manager_result`, timestamps, logs).

#### Stack B) Workflow API (luong nghiep vu chinh)

- [x] `POST /api/submissions/draft`
- [x] `PUT /api/submissions/{id}/fields`
- [x] `POST /api/submissions/{id}/submit`
- [x] `POST /api/submissions/{id}/evaluate`
- [x] `POST /api/submissions/{id}/approve`
- [x] `POST /api/submissions/{id}/reject`
- [x] `GET /api/submissions` (filter theo template, date, status, created_by)

#### Stack C) CRUD Management API (CRUD toan bang)

- [x] Chuan hoa endpoint CRUD cho moi resource: `GET list`, `GET by id`, `POST`, `PUT`, `DELETE`.
- [x] CRUD nhom quyen/nguoi dung: `users`, `roles`, `user_roles`.
- [x] CRUD nhom template: `report_templates`, `report_template_versions`, `template_fields`, `field_rules`.
- [x] CRUD nhom van hanh bao cao: `report_submissions`, `report_field_values`, `report_attachments`, `approval_logs`.

#### Stack D) UI theo vai tro

- [x] Employee: tao draft, nhap du lieu, submit bao cao.
- [x] Manager: xem danh sach, xem chi tiet, approve/reject, nhap ghi chu quan ly.
- [x] Admin: quan tri template/version/field/rule bang man hinh CRUD.

#### Stack E) Guardrails + Test + DoD

- [x] Them guardrails nghiep vu o API/Application de CRUD khong pha workflow.
- [x] Viet integration test cho luong end-to-end: `draft -> submit -> evaluate -> approve/reject`.
- [x] Viet smoke test cho CRUD toan bang (it nhat case create/update/delete/list co ban).
- [x] Dinh nghia tieu chi hoan thanh phan 2 (DoD): workflow chay end-to-end + CRUD toan bang + guardrails + test pass.

## 3) Rule engine va rang buoc nghiep vu

- [ ] Implement cac rule type: `RANGE`, `LT`, `LTE`, `GT`, `GTE`, `REGEX`, `IN_SET`.
- [ ] Luu `rule_snapshot_json` khi evaluate.
- [ ] Tong hop `auto_result` cua submission tu ket qua tung field.
- [ ] Chan duyet khi chua qua `SUBMITTED` va `AUTO_EVALUATED`.
- [ ] Validate `report_field_values.field_id` thuoc dung `template_version_id` cua submission.

## 4) Attachment + Export

- [x] Upload/luu metadata anh minh chung (`report_attachments`).
- [x] Danh sach bao cao co filter theo template, date, status, created_by.
- [x] Export Excel cho bao cao.
- [x] Export PDF (sau Excel).

## 5) Hardening truoc production

- [x] Hoan thien authentication + authorization theo role.
- [ ] Them approval log day du cho moi transition.
- [ ] Viet test:
- [ ] Unit test cho rule engine.
- [ ] Integration test cho workflow.
- [ ] Thiet lap logging va error handling chuan.
- [ ] Chuan bi Docker/deploy script + huong dan van hanh.

## 6) Thu tu uu tien sprint gan nhat

### Huong di moi (uu tien theo yeu cau hien tai)

- [x] Uu tien 1: Authentication + Authorization theo role.
- [x] Uu tien 2: Attachment + Export.
- [ ] Uu tien 3: Sua UI cho phu hop voi auth/attachment/export.

### Ke hoach sprint cap nhat

- [x] Sprint 1: Seed toi thieu.
- [x] Sprint 2: Submission workflow end-to-end + CRUD quan tri toan bang (khong export).
- [x] Sprint 3: Authentication + Authorization theo role (protect API + UI by role).
- [x] Sprint 4: Attachment + Export (upload/list/filter/export Excel/PDF).
- [ ] Sprint 5: UI refactor theo role, toi uu UX cho luong moi.
- [ ] Sprint 6: Rule engine hoan chinh + manager approval nang cao.
