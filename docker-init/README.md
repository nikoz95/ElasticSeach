# SQL Server Initialization Scripts

ეს დირექტორია შეიცავს SQL Server initialization scripts-ებს რომლებიც ავტომატურად ასრულდება container-ის პირველ გაშვებისას.

## 📁 ფაილები

### `entrypoint.sh`
- Custom entrypoint script SQL Server container-სთვის
- იწყებს SQL Server-ს background-ში
- ელოდება რომ SQL Server ready იყოს
- ასრულებს ყველა `*.sql` ფაილს დირექტორიიდან

### `01-create-admin-user.sql`
- ქმნის `admin` login-ს პაროლით `Password1234`
- ქმნის `ElasticsearchDemo` database-ს
- ქმნის `admin` user-ს database-ში
- ანიჭებს `db_owner` role-ს

## 🔑 Credentials

| Account | Username | Password | Role |
|---------|----------|----------|------|
| SA Account | `sa` | `YourStrong@Password123` | sysadmin (setup only) |
| Application User | `admin` | `Password1234` | db_owner |

## 🚀 როგორ მუშაობს

1. **Docker Compose up** → SQL Server container ეშვება
2. **entrypoint.sh** → SQL Server იწყებს background-ში
3. **Wait 30s** → SQL Server ready იქნება
4. **Run *.sql files** → `admin` user შეიქმნება
5. **API & Jobs** → უკავშირდებიან `admin` user-ით

## ⚠️ მნიშვნელოვანი

- Scripts ასრულდება **მხოლოდ პირველ გაშვებისას** (თუ volume ცარიელია)
- თუ გსურთ თავიდან გაშვება: `docker-compose down -v`
- SA პაროლი საჭიროა მხოლოდ setup-სთვის
- Application იყენებს `admin` user-ს (არა `sa`-ს)

## 🔧 ცვლილებები

თუ გსურთ სხვა credentials:

1. **შეცვალეთ `01-create-admin-user.sql`:**
   ```sql
   CREATE LOGIN [myuser] WITH PASSWORD = 'MyPassword123';
   ```

2. **განაახლეთ `docker-compose.yml`:**
   ```yaml
   - ConnectionStrings__SqlServer=Server=sqlserver,1433;Database=ElasticsearchDemo;User Id=myuser;Password=MyPassword123;...
   ```

3. **Clean restart:**
   ```bash
   docker-compose down -v
   docker-compose up -d
   ```

## 📝 დამატებითი Scripts

შეგიძლიათ დაამატოთ დამატებითი SQL scripts:

```bash
docker-init/
├── entrypoint.sh
├── 01-create-admin-user.sql
├── 02-create-tables.sql       # თქვენი script
├── 03-seed-data.sql           # თქვენი script
└── 99-final-setup.sql         # თქვენი script
```

Scripts ასრულდება alphabetically (01, 02, 03, ...).

## 🐛 Troubleshooting

### Scripts არ ასრულდა?

```bash
# Check logs
docker-compose logs sqlserver

# Check if admin user exists
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U admin -P Password1234
```

### Permission errors?

Windows-ზე entrypoint.sh-ის line endings უნდა იყოს LF (არა CRLF):
```bash
# Git-ში:
git config core.autocrlf false
```

ან VS Code-ში: "LF" ღილაკი ქვედა ზოლზე.

