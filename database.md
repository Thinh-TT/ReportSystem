# Database Design (Text Specification)

## 1) Muc tieu thiet ke

- Ho tro nhieu loai bao cao, moi loai co bo truong du lieu khac nhau.
- Ho tro cau hinh quy tac danh gia pass/fail theo tung truong.
- Luu duoc nguoi tao, ngay lap, anh minh chung, nguoi quan ly xac nhan.
- Bao toan lich su khi form/rule thay doi theo thoi gian (versioning).

## 2) Nguyen tac chon mo hinh

- Dung mo hinh dong: `Template -> TemplateVersion -> Field -> Rule -> Submission -> FieldValue`.
- Khong tao cot cung theo tung form bao cao.
- Moi submission luon tro den 1 `template_version` cu the de giu tinh nhat quan lich su.

## 3) Danh sach bang va cot chinh

### 3.1 `users`

- `id` (uniqueidentifier, PK)
- `employee_code` (nvarchar(50), unique, not null)
- `full_name` (nvarchar(200), not null)
- `email` (nvarchar(200), unique, null)
- `is_active` (bit, default 1)
- `created_at` (datetime2, not null)
- `updated_at` (datetime2, not null)

### 3.2 `roles`

- `id` (int, PK, identity)
- `code` (varchar(50), unique, not null) -> `EMPLOYEE`, `MANAGER`, `ADMIN`
- `name` (nvarchar(100), not null)

### 3.3 `user_roles`

- `user_id` (uniqueidentifier, FK -> users.id, not null)
- `role_id` (int, FK -> roles.id, not null)
- PK: (`user_id`, `role_id`)

### 3.4 `report_templates`

- `id` (bigint, PK, identity)
- `template_code` (varchar(100), unique, not null)
- `template_name` (nvarchar(255), not null)
- `description` (nvarchar(1000), null)
- `is_active` (bit, default 1)
- `created_at` (datetime2, not null)
- `updated_at` (datetime2, not null)

### 3.5 `report_template_versions`

- `id` (bigint, PK, identity)
- `template_id` (bigint, FK -> report_templates.id, not null)
- `version_no` (int, not null)
- `status` (varchar(20), not null) -> `DRAFT`, `PUBLISHED`, `ARCHIVED`
- `effective_from` (datetime2, null)
- `effective_to` (datetime2, null)
- `published_by` (uniqueidentifier, FK -> users.id, null)
- `published_at` (datetime2, null)
- `created_at` (datetime2, not null)
- `updated_at` (datetime2, not null)
- Unique: (`template_id`, `version_no`)

### 3.6 `template_fields`

- `id` (bigint, PK, identity)
- `template_version_id` (bigint, FK -> report_template_versions.id, not null)
- `field_code` (varchar(100), not null)
- `field_label` (nvarchar(255), not null)
- `data_type` (varchar(20), not null) -> `NUMBER`, `TEXT`, `DATE`, `DATETIME`, `BOOLEAN`, `SELECT`
- `unit` (nvarchar(50), null)
- `is_required` (bit, default 0)
- `display_order` (int, not null)
- `placeholder` (nvarchar(255), null)
- `options_json` (nvarchar(max), null) (cho truong `SELECT`)
- `is_active` (bit, default 1)
- `created_at` (datetime2, not null)
- `updated_at` (datetime2, not null)
- Unique: (`template_version_id`, `field_code`)
- Unique: (`template_version_id`, `display_order`)

### 3.7 `field_rules`

- `id` (bigint, PK, identity)
- `field_id` (bigint, FK -> template_fields.id, not null)
- `rule_order` (int, not null, default 1)
- `rule_type` (varchar(30), not null) -> `RANGE`, `LT`, `LTE`, `GT`, `GTE`, `REGEX`, `IN_SET`
- `min_value` (decimal(18,6), null)
- `max_value` (decimal(18,6), null)
- `threshold_value` (decimal(18,6), null)
- `expected_text` (nvarchar(500), null)
- `severity` (varchar(20), not null, default `ERROR`) -> `ERROR`, `WARNING`
- `fail_message` (nvarchar(500), null)
- `is_active` (bit, default 1)
- `created_at` (datetime2, not null)
- `updated_at` (datetime2, not null)
- Unique: (`field_id`, `rule_order`)

### 3.8 `report_submissions`

- `id` (bigint, PK, identity)
- `submission_no` (varchar(50), unique, not null)
- `template_version_id` (bigint, FK -> report_template_versions.id, not null)
- `report_date` (date, not null)
- `created_by_user_id` (uniqueidentifier, FK -> users.id, not null)
- `performed_by_text` (nvarchar(200), null)
- `status` (varchar(30), not null) -> `DRAFT`, `SUBMITTED`, `AUTO_EVALUATED`, `APPROVED`, `REJECTED`
- `auto_result` (varchar(10), not null, default `PENDING`) -> `PENDING`, `PASS`, `FAIL`
- `manager_result` (varchar(10), not null, default `PENDING`) -> `PENDING`, `PASS`, `FAIL`
- `manager_note` (nvarchar(1000), null)
- `approved_by_user_id` (uniqueidentifier, FK -> users.id, null)
- `approved_at` (datetime2, null)
- `submitted_at` (datetime2, null)
- `evaluated_at` (datetime2, null)
- `created_at` (datetime2, not null)
- `updated_at` (datetime2, not null)

### 3.9 `report_field_values`

- `id` (bigint, PK, identity)
- `submission_id` (bigint, FK -> report_submissions.id, not null)
- `field_id` (bigint, FK -> template_fields.id, not null)
- `value_text` (nvarchar(max), null)
- `value_number` (decimal(18,6), null)
- `value_date` (date, null)
- `value_datetime` (datetime2, null)
- `value_bool` (bit, null)
- `normalized_value` (nvarchar(255), null)
- `auto_result` (varchar(10), not null, default `PENDING`) -> `PENDING`, `PASS`, `FAIL`, `NA`
- `evaluation_note` (nvarchar(500), null)
- `rule_snapshot_json` (nvarchar(max), null)
- `created_at` (datetime2, not null)
- `updated_at` (datetime2, not null)
- Unique: (`submission_id`, `field_id`)

### 3.10 `report_attachments`

- `id` (bigint, PK, identity)
- `submission_id` (bigint, FK -> report_submissions.id, not null)
- `file_path` (nvarchar(500), not null)
- `file_name` (nvarchar(255), not null)
- `content_type` (varchar(100), null)
- `file_size_bytes` (bigint, null)
- `captured_at` (datetime2, null)
- `uploaded_by_user_id` (uniqueidentifier, FK -> users.id, not null)
- `created_at` (datetime2, not null)

### 3.11 `approval_logs`

- `id` (bigint, PK, identity)
- `submission_id` (bigint, FK -> report_submissions.id, not null)
- `action` (varchar(30), not null) -> `SUBMIT`, `AUTO_EVALUATE`, `APPROVE`, `REJECT`, `REOPEN`
- `from_status` (varchar(30), null)
- `to_status` (varchar(30), null)
- `action_by_user_id` (uniqueidentifier, FK -> users.id, null) (null khi he thong auto evaluate)
- `comment` (nvarchar(1000), null)
- `metadata_json` (nvarchar(max), null)
- `action_at` (datetime2, not null)

## 4) Quan he tong quan (ERD text)

- `users` 1-n `report_submissions` (created_by_user_id)
- `users` 1-n `report_submissions` (approved_by_user_id)
- `users` n-n `roles` qua `user_roles`
- `report_templates` 1-n `report_template_versions`
- `report_template_versions` 1-n `template_fields`
- `template_fields` 1-n `field_rules`
- `report_template_versions` 1-n `report_submissions`
- `report_submissions` 1-n `report_field_values`
- `report_submissions` 1-n `report_attachments`
- `report_submissions` 1-n `approval_logs`

## 5) Index de xuat

- `report_submissions(template_version_id, report_date, status)`
- `report_submissions(created_by_user_id, created_at desc)`
- `report_field_values(submission_id, field_id)` (unique index)
- `report_attachments(submission_id, created_at desc)`
- `approval_logs(submission_id, action_at desc)`
- `template_fields(template_version_id, display_order)`
- `field_rules(field_id, rule_order)`

## 6) Rang buoc nghiep vu

- Khong cho `APPROVED` neu `status` chua qua `AUTO_EVALUATED` hoac chua `SUBMITTED`.
- Moi `report_field_values` phai thuoc 1 field cua chinh `template_version_id` ma submission dang dung.
- Khi publish version moi, version cu khong duoc sua field/rule (chi cho archive).
- `auto_result` cua submission tinh theo tong hop ket qua field:
- Tat ca field bat buoc hop le + khong co rule `ERROR` fail -> `PASS`.
- Nguoc lai -> `FAIL`.

## 7) Seed du lieu toi thieu cho 2 form mau

- Template 1: `PH_METER_DAILY_CHECK`
- Fields: `date`, `ph_1`, `ph_2`, `ph_3`, `slope`, `clean`, `conclusion`, `performed_by`, `remark`
- Rule: `slope` trong khoang `85` den `115`.

- Template 2: `DISTILLED_WATER_QUALITY_CHECK`
- Fields: `date`, `batch`, `ph`, `tpc`, `ec`, `chlorine`, `conclusion`, `performed_by`, `remark`
- Rules:
- `ph` trong khoang `5.0` den `7.5`.
- `tpc` < `100`.
- `ec` < `25`.
- `chlorine` < `0.1`.

## 8) Trang thai workflow de xai ngay

1. Nhan vien tao phieu -> `DRAFT`
2. Nhan vien gui phieu -> `SUBMITTED`
3. He thong chay rule -> `AUTO_EVALUATED` + cap nhat `auto_result`
4. Quan ly xem va xac nhan:

- Duyet -> `APPROVED`, set `manager_result` (`PASS`/`FAIL`)
- Tu choi -> `REJECTED`

## 9) Ghi chu ky thuat

- Kieu du lieu tren dang theo SQL Server (co the map sang PostgreSQL neu can).
- Nen su dung UTC cho truong `datetime2`.
- Anh minh chung chi luu metadata + `file_path`; file that luu o object storage hoac file server.
