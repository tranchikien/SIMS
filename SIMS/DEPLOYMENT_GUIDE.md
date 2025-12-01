# Hướng dẫn Deploy SIMS lên Web

## Mục lục
1. [Chuẩn bị ứng dụng](#1-chuẩn-bị-ứng-dụng)
2. [Deploy lên Azure App Service](#2-deploy-lên-azure-app-service)
3. [Deploy lên IIS (Windows Server)](#3-deploy-lên-iis-windows-server)
4. [Deploy lên Linux Server](#4-deploy-lên-linux-server)
5. [Cấu hình Database trên Cloud](#5-cấu-hình-database-trên-cloud)
6. [Cấu hình Production Settings](#6-cấu-hình-production-settings)

---

## 1. Chuẩn bị ứng dụng

### Bước 1: Tạo file appsettings.Production.json

Tạo file `SIMS/appsettings.Production.json` để cấu hình cho môi trường production:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_PRODUCTION_CONNECTION_STRING_HERE"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Bước 2: Publish ứng dụng

#### Cách 1: Publish bằng Visual Studio
1. Click phải vào project `SIMS` → **Publish**
2. Chọn target (Azure, Folder, IIS, etc.)
3. Click **Publish**

#### Cách 2: Publish bằng Command Line

```bash
# Publish cho Windows
dotnet publish SIMS/SIMS.csproj -c Release -o ./publish

# Publish cho Linux
dotnet publish SIMS/SIMS.csproj -c Release -r linux-x64 -o ./publish

# Publish cho self-contained (bao gồm .NET runtime)
dotnet publish SIMS/SIMS.csproj -c Release -r win-x64 --self-contained true -o ./publish
```

---

## 2. Deploy lên Azure App Service

### Bước 1: Tạo Azure App Service

1. Đăng nhập vào [Azure Portal](https://portal.azure.com)
2. Click **Create a resource** → **Web App**
3. Điền thông tin:
   - **Subscription**: Chọn subscription của bạn
   - **Resource Group**: Tạo mới hoặc chọn existing
   - **Name**: Tên ứng dụng (ví dụ: `sims-app`)
   - **Publish**: Code
   - **Runtime stack**: .NET 8
   - **Operating System**: Windows hoặc Linux
   - **Region**: Chọn region gần nhất
4. Click **Review + create** → **Create**

### Bước 2: Tạo Azure SQL Database

1. Trong Azure Portal, click **Create a resource** → **SQL Database**
2. Điền thông tin:
   - **Database name**: `SIMSDB`
   - **Server**: Tạo server mới hoặc chọn existing
   - **Pricing tier**: Chọn tier phù hợp (Basic/Standard)
3. Click **Create**
4. Sau khi tạo xong, vào **Connection strings** và copy connection string

### Bước 3: Cấu hình Connection String trong Azure

1. Vào **App Service** → **Configuration** → **Connection strings**
2. Thêm connection string mới:
   - **Name**: `DefaultConnection`
   - **Value**: Connection string từ Azure SQL Database
   - **Type**: SQLAzure
3. Click **Save**

### Bước 4: Deploy code lên Azure

#### Cách 1: Deploy từ Visual Studio
1. Click phải project → **Publish**
2. Chọn **Azure** → **Azure App Service (Windows)** hoặc **Azure App Service (Linux)**
3. Chọn App Service đã tạo
4. Click **Publish**

#### Cách 2: Deploy bằng Azure CLI

```bash
# Login vào Azure
az login

# Deploy
az webapp deploy --resource-group <resource-group-name> --name <app-name> --src-path ./publish
```

#### Cách 3: Deploy bằng Git

1. Vào App Service → **Deployment Center**
2. Chọn **Local Git** hoặc **GitHub**
3. Follow hướng dẫn để setup

### Bước 5: Chạy Migration Database

Sau khi deploy, cần tạo database và tables. Có 2 cách:

#### Cách 1: Tự động (nếu code đã có EnsureCreated)
- Ứng dụng sẽ tự động tạo database khi chạy lần đầu

#### Cách 2: Manual bằng Azure Cloud Shell

```bash
# Kết nối đến database và chạy migration
# Hoặc dùng Azure Data Studio để kết nối và chạy SQL script
```

---

## 3. Deploy lên IIS (Windows Server)

### Bước 1: Cài đặt .NET 8 Hosting Bundle

1. Download từ: https://dotnet.microsoft.com/download/dotnet/8.0
2. Cài đặt **.NET 8.0 Hosting Bundle** trên Windows Server

### Bước 2: Tạo Website trong IIS

1. Mở **IIS Manager**
2. Click phải **Sites** → **Add Website**
3. Điền thông tin:
   - **Site name**: `SIMS`
   - **Physical path**: Đường dẫn đến folder publish
   - **Binding**: Chọn port (ví dụ: 80, 443)
4. Click **OK**

### Bước 3: Cấu hình Application Pool

1. Vào **Application Pools**
2. Chọn Application Pool của website
3. Set **.NET CLR Version** = **No Managed Code**
4. Set **Managed Pipeline Mode** = **Integrated**

### Bước 4: Cấu hình Connection String

1. Tạo file `appsettings.Production.json` trong folder publish
2. Hoặc dùng **Web.config** để override connection string:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <connectionStrings>
    <add name="DefaultConnection" 
         connectionString="YOUR_PRODUCTION_CONNECTION_STRING" 
         providerName="System.Data.SqlClient" />
  </connectionStrings>
</configuration>
```

### Bước 5: Set Permissions

1. Click phải folder publish → **Properties** → **Security**
2. Thêm user `IIS_IUSRS` với quyền **Read & Execute**

---

## 4. Deploy lên Linux Server

### Bước 1: Cài đặt .NET 8 trên Linux

```bash
# Ubuntu/Debian
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 8.0.0

# Hoặc dùng package manager
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

### Bước 2: Publish ứng dụng

```bash
dotnet publish SIMS/SIMS.csproj -c Release -r linux-x64 -o ./publish
```

### Bước 3: Copy files lên server

```bash
# Sử dụng SCP hoặc SFTP
scp -r ./publish/* user@server:/var/www/sims/
```

### Bước 4: Cấu hình Systemd Service

Tạo file `/etc/systemd/system/sims.service`:

```ini
[Unit]
Description=SIMS Application
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/sims
ExecStart=/usr/bin/dotnet /var/www/sims/SIMS.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=sims
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

Khởi động service:

```bash
sudo systemctl enable sims
sudo systemctl start sims
sudo systemctl status sims
```

### Bước 5: Cấu hình Nginx (Reverse Proxy)

Tạo file `/etc/nginx/sites-available/sims`:

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable site:

```bash
sudo ln -s /etc/nginx/sites-available/sims /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## 5. Cấu hình Database trên Cloud

### Azure SQL Database

1. Tạo Azure SQL Database (xem Bước 2 ở phần Azure App Service)
2. Lấy connection string từ Azure Portal
3. Format connection string:
   ```
   Server=tcp:your-server.database.windows.net,1433;Initial Catalog=SIMSDB;Persist Security Info=False;User ID=your-username;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   ```

### SQL Server trên Cloud (AWS RDS, Google Cloud SQL)

1. Tạo SQL Server instance trên cloud provider
2. Lấy connection string từ dashboard
3. Cập nhật vào `appsettings.Production.json`

### Connection String Format

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=SIMSDB;User Id=your-user;Password=your-password;TrustServerCertificate=true;MultipleActiveResultSets=true;"
  }
}
```

---

## 6. Cấu hình Production Settings

### Bước 1: Tắt Developer Exception Page

File `Program.cs` đã có code này, nhưng đảm bảo:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
```

### Bước 2: Cấu hình HTTPS

Đảm bảo có SSL certificate và cấu hình HTTPS redirect.

### Bước 3: Set Environment Variable

Trên server, set environment variable:

```bash
# Linux
export ASPNETCORE_ENVIRONMENT=Production

# Windows (PowerShell)
$env:ASPNETCORE_ENVIRONMENT="Production"
```

### Bước 4: Cấu hình Logging

Trong `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Error"
    }
  }
}
```

---

## Checklist trước khi Deploy

- [ ] Đã test ứng dụng local
- [ ] Đã tạo `appsettings.Production.json`
- [ ] Đã cấu hình connection string cho production database
- [ ] Đã publish ứng dụng
- [ ] Đã setup database trên cloud
- [ ] Đã cấu hình SSL/HTTPS
- [ ] Đã test đăng nhập sau khi deploy
- [ ] Đã kiểm tra logs nếu có lỗi

---

## Troubleshooting

### Lỗi: "Invalid object name 'TableName'"
- **Nguyên nhân**: Database chưa có bảng
- **Giải pháp**: Chạy migration hoặc `EnsureCreated()` (đã có trong code)

### Lỗi: "Cannot connect to database"
- **Nguyên nhân**: Connection string sai hoặc firewall chặn
- **Giải pháp**: 
  - Kiểm tra connection string
  - Cho phép IP của server trong Azure SQL Firewall rules

### Lỗi: "500 Internal Server Error"
- **Nguyên nhân**: Nhiều nguyên nhân có thể
- **Giải pháp**: 
  - Kiểm tra logs trong Azure Portal hoặc server logs
  - Kiểm tra application pool trong IIS
  - Kiểm tra permissions

---

## Liên kết hữu ích

- [ASP.NET Core Deployment](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/)
- [Azure App Service Documentation](https://learn.microsoft.com/en-us/azure/app-service/)
- [IIS Deployment Guide](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/)

