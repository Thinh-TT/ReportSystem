using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "report_templates",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    template_code = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    template_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    employee_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "report_template_versions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    template_id = table.Column<long>(type: "bigint", nullable: false),
                    version_no = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    effective_from = table.Column<DateTime>(type: "datetime2", nullable: true),
                    effective_to = table.Column<DateTime>(type: "datetime2", nullable: true),
                    published_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    published_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_template_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_template_versions_report_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "report_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_template_versions_users_published_by",
                        column: x => x.published_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_submissions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    submission_no = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    template_version_id = table.Column<long>(type: "bigint", nullable: false),
                    report_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    performed_by_text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    auto_result = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "PENDING"),
                    manager_result = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "PENDING"),
                    manager_note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    approved_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    evaluated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_submissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_submissions_report_template_versions_template_version_id",
                        column: x => x.template_version_id,
                        principalTable: "report_template_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_report_submissions_users_approved_by_user_id",
                        column: x => x.approved_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_report_submissions_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "template_fields",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    template_version_id = table.Column<long>(type: "bigint", nullable: false),
                    field_code = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    field_label = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    data_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_required = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    placeholder = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    options_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_fields", x => x.id);
                    table.ForeignKey(
                        name: "FK_template_fields_report_template_versions_template_version_id",
                        column: x => x.template_version_id,
                        principalTable: "report_template_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "approval_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    submission_id = table.Column<long>(type: "bigint", nullable: false),
                    action = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    from_status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    to_status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    action_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    metadata_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    action_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approval_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_approval_logs_report_submissions_submission_id",
                        column: x => x.submission_id,
                        principalTable: "report_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_approval_logs_users_action_by_user_id",
                        column: x => x.action_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "report_attachments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    submission_id = table.Column<long>(type: "bigint", nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    captured_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    uploaded_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_attachments", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_attachments_report_submissions_submission_id",
                        column: x => x.submission_id,
                        principalTable: "report_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_attachments_users_uploaded_by_user_id",
                        column: x => x.uploaded_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "field_rules",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    field_id = table.Column<long>(type: "bigint", nullable: false),
                    rule_order = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    rule_type = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    min_value = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    max_value = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    threshold_value = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    expected_text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    severity = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "ERROR"),
                    fail_message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_field_rules_template_fields_field_id",
                        column: x => x.field_id,
                        principalTable: "template_fields",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_field_values",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    submission_id = table.Column<long>(type: "bigint", nullable: false),
                    field_id = table.Column<long>(type: "bigint", nullable: false),
                    value_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    value_number = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    value_date = table.Column<DateOnly>(type: "date", nullable: true),
                    value_datetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    value_bool = table.Column<bool>(type: "bit", nullable: true),
                    normalized_value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    auto_result = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "PENDING"),
                    evaluation_note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    rule_snapshot_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_field_values", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_field_values_report_submissions_submission_id",
                        column: x => x.submission_id,
                        principalTable: "report_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_field_values_template_fields_field_id",
                        column: x => x.field_id,
                        principalTable: "template_fields",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_approval_logs_action_by_user_id",
                table: "approval_logs",
                column: "action_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_approval_logs_submission_id_action_at",
                table: "approval_logs",
                columns: new[] { "submission_id", "action_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_field_rules_field_id_rule_order",
                table: "field_rules",
                columns: new[] { "field_id", "rule_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_attachments_submission_id_created_at",
                table: "report_attachments",
                columns: new[] { "submission_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_report_attachments_uploaded_by_user_id",
                table: "report_attachments",
                column: "uploaded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_field_values_field_id",
                table: "report_field_values",
                column: "field_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_field_values_submission_id_field_id",
                table: "report_field_values",
                columns: new[] { "submission_id", "field_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_submissions_approved_by_user_id",
                table: "report_submissions",
                column: "approved_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_submissions_created_by_user_id_created_at",
                table: "report_submissions",
                columns: new[] { "created_by_user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_report_submissions_submission_no",
                table: "report_submissions",
                column: "submission_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_submissions_template_version_id_report_date_status",
                table: "report_submissions",
                columns: new[] { "template_version_id", "report_date", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_report_template_versions_published_by",
                table: "report_template_versions",
                column: "published_by");

            migrationBuilder.CreateIndex(
                name: "IX_report_template_versions_template_id_version_no",
                table: "report_template_versions",
                columns: new[] { "template_id", "version_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_templates_template_code",
                table: "report_templates",
                column: "template_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_code",
                table: "roles",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_template_fields_template_version_id_display_order",
                table: "template_fields",
                columns: new[] { "template_version_id", "display_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_template_fields_template_version_id_field_code",
                table: "template_fields",
                columns: new[] { "template_version_id", "field_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_employee_code",
                table: "users",
                column: "employee_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "approval_logs");

            migrationBuilder.DropTable(
                name: "field_rules");

            migrationBuilder.DropTable(
                name: "report_attachments");

            migrationBuilder.DropTable(
                name: "report_field_values");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "report_submissions");

            migrationBuilder.DropTable(
                name: "template_fields");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "report_template_versions");

            migrationBuilder.DropTable(
                name: "report_templates");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
