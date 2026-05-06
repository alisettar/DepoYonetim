using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Catalog.Migrations;

/// <summary>
/// Initial Catalog DB schema
/// </summary>
public partial class InitialCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "super_admins",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "citext", nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: false),
                mfa_secret = table.Column<string>(type: "text", nullable: true),
                is_locked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_super_admins", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tenants",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "text", maxLength: 100, nullable: false),
                name = table.Column<string>(type: "text", maxLength: 255, nullable: false),
                status = table.Column<string>(type: "text", nullable: false, defaultValue: "Active"),
                plan = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tenants", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "user_lookups",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "citext", nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                mfa_secret = table.Column<string>(type: "text", nullable: true),
                failed_attempts = table.Column<int>(type: "integer", nullable: false),
                is_locked = table.Column<bool>(type: "boolean", nullable: false),
                last_login_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_lookups", x => x.id);
                table.ForeignKey(
                    name: "fk_user_lookups_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "tenant_databases",
            columns: table => new
            {
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                host = table.Column<string>(type: "text", nullable: false),
                port = table.Column<int>(type: "integer", nullable: false, defaultValue: 5432),
                db_name = table.Column<string>(type: "text", nullable: false),
                username = table.Column<string>(type: "text", nullable: false),
                password_enc = table.Column<byte[]>(type: "bytea", nullable: false),
                region = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tenant_databases", x => x.tenant_id);
                table.ForeignKey(
                    name: "fk_tenant_databases_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "audit_global",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                occurred_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                actor_type = table.Column<string>(type: "text", nullable: false),
                action = table.Column<string>(type: "text", nullable: false),
                target_type = table.Column<string>(type: "text", nullable: false),
                target_id = table.Column<Guid>(type: "uuid", nullable: false),
                ip = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_audit_global", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_super_admins_email",
            table: "super_admins",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_tenants_code",
            table: "tenants",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_lookups_email",
            table: "user_lookups",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_lookups_tenant_id",
            table: "user_lookups",
            column: "tenant_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "super_admins");
        migrationBuilder.DropTable(name: "tenant_databases");
        migrationBuilder.DropTable(name: "user_lookups");
        migrationBuilder.DropTable(name: "audit_global");
        migrationBuilder.DropTable(name: "tenants");
    }
}
