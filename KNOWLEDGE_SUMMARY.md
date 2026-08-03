# Employee Management System — Tổng hợp kiến thức

> Tài liệu tổng hợp toàn bộ kiến thức và kiến trúc của dự án học tập
> **ASP.NET Core 10 + EF Core + PostgreSQL + React/TypeScript + Docker + CI/CD**

---

## 1. Tổng quan dự án

**Mục tiêu:** Xây dựng hệ thống quản lý nhân viên (CRUD phòng ban & nhân viên) để học các công nghệ:

| Công nghệ | Vai trò |
|---|---|
| ASP.NET Core 10 | Backend API (REST) |
| EF Core 10 + Npgsql | ORM kết nối PostgreSQL |
| PostgreSQL 18 | Cơ sở dữ liệu |
| React 19 + TypeScript + Vite | Frontend SPA |
| Tailwind CSS v4 | Styling |
| Docker / Docker Compose | Đóng gói & chạy multi-container |
| GitHub Actions | CI (build + test) |
| Render.com | Deploy (Web Service + PostgreSQL) |
| Python (psycopg2) | Script đọc dữ liệu DB |

**Tính năng chính:**
- Đăng nhập JWT (access token 15 phút + refresh token 7 ngày)
- CRUD Phòng ban (Department)
- CRUD Nhân viên (Employee) + upload avatar
- Phân trang, tìm kiếm, lọc theo phòng ban
- Dashboard thống kê (tổng nhân viên, tổng phòng ban, ...)

---

## 2. Kiến trúc tổng thể

```
┌─────────────┐      HTTP/JSON       ┌──────────────┐      SQL      ┌──────────────┐
│   React     │ ───────────────────► │  .NET API    │ ────────────► │ PostgreSQL   │
│  Frontend   │ ◄─────────────────── │  :8080       │ ◄──────────── │  (Docker)    │
└─────────────┘  token + data        └──────────────┘              └──────────────┘
```

**Backend — Kiến trúc phân lớp (Layered Architecture):**

```
Controllers  →  nhận request HTTP, trả HTTP response
     ↓
Services     →  business logic (chỉ làm việc với DTO)
     ↓
Repositories →  thao tác DB qua DbContext (trả Entity)
     ↓
DbContext    →  EF Core map Entity ↔ bảng PostgreSQL
```

> Nguyên tắc: **Controller không biết DB, Repository không biết HTTP.** Mỗi tầng chỉ phụ thuộc tầng dưới liền kề qua **interface** (inversion of dependency).

**Frontend — Kiến trúc FSD (Feature-Sliced Design):**

```
app/       →  cấu hình app: routing (App.tsx), entry point (main.tsx), css
pages/     →  các trang: Login, Dashboard, Departments, Employees
widgets/   →  component dùng chung cho nhiều trang: Navbar
features/  →  tính năng: api client của từng module (departmentApi, employeeApi...)
entities/  →  model type của dữ liệu (Department, Employee)
shared/    →  dùng chung toàn app: axios instance, UI components (Card)
```

---

## 3. Backend — .NET API

### 3.1 Models (Entity)

```csharp
// Models/Department.cs
public class Department
{
    public int Id { get; set; }
    [Required] [MaxLength(100)]
    public string Name { get; set; }
    public ICollection<Employee> Employees { get; set; }  // 1-N
}

// Models/Employee.cs
public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public decimal Salary { get; set; }
    public string? Avatar { get; set; }        // đường dẫn ảnh
    public int DepartmentId { get; set; }      // FK
    public Department? Department { get; set; }// navigation
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### 3.2 DbContext

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}
```

- Mỗi `DbSet<T>` tương ứng 1 bảng
- EF Core dùng **Convention** (quy ước đặt tên) để tự map bảng/cột — không cần Fluent API

### 3.3 DTO + AutoMapper

| DTO | Mục đích |
|---|---|
| `DepartmentDto` / `CreateDepartmentDto` / `UpdateDepartmentDto` | 3 loại: đọc / tạo / cập nhật |
| `EmployeeDto` / `CreateEmployeeDto` / `UpdateEmployeeDto` | tương tự |
| `PagedResult<T>` | kết quả phân trang `{ items, totalCount, page, pageSize }` |
| `EmployeeFilterDto` | bộ lọc: `search`, `departmentId`, `page`, `pageSize` |
| `DashboardDto` | thống kê: tổng nhân viên, tổng phòng ban, ... |

**Vì sao cần DTO?**
- Không lộ field nhạy cảm của Entity
- Trả đúng dữ liệu client cần (VD: `DepartmentName` thay vì `DepartmentId`)
- Tách contract API khỏi cấu trúc DB

**MappingProfile** định nghĩa cách map:
```csharp
CreateMap<Employee, EmployeeDto>()
    .ForMember(d => d.DepartmentName, opt => opt.MapFrom(s => s.Department!.Name));
```

### 3.4 Repository Pattern

```csharp
public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<Department> CreateAsync(Department d);
    Task UpdateAsync(Department d);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
```

**Lợi ích:**
- Tách truy vấn DB khỏi business logic → dễ test bằng Mock
- Tập trung thay đổi query 1 nơi

### 3.5 Service Layer

```csharp
public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repo;
    private readonly IMapper _mapper;

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        var department = _mapper.Map<Department>(dto);
        var created = await _repo.CreateAsync(department);
        return _mapper.Map<DepartmentDto>(created);
    }
}
```

**Vai trò:** chứa business logic, nhận DTO từ Controller, map sang Entity, gọi Repository, map ngược về DTO.

### 3.6 Controllers

```csharp
[Authorize]                       // yêu cầu JWT hợp lệ
[ApiController]
[Route("api/[controller]")]       // => api/departments
public class DepartmentsController : ControllerBase
```

| HTTP | Endpoint | Mô tả | Response |
|---|---|---|---|
| GET | `/api/departments` | danh sách | 200 |
| GET | `/api/departments/{id}` | chi tiết | 200 / 404 |
| POST | `/api/departments` | tạo mới | 201 + Location |
| PUT | `/api/departments/{id}` | cập nhật | 204 |
| DELETE | `/api/departments/{id}` | xóa | 204 / 404 |
| GET | `/api/employees?search=&departmentId=&page=&pageSize=` | phân trang + lọc | 200 |
| POST | `/api/employees/{id}/avatar` | upload avatar | 200 |
| POST | `/api/auth/login` | đăng nhập | access + refresh token |
| POST | `/api/auth/refresh` | refresh token | cặp token mới |
| GET | `/api/dashboard` | thống kê | 200 |

### 3.7 JWT Authentication

```
Login → GenerateAccessToken (15 phút) + GenerateRefreshToken (7 ngày)
        │
        ├─ AccessToken: JWT ký bằng Jwt:Key (config), chứa claims
        └─ RefreshToken: chuỗi ngẫu nhiên, LƯU VÀO DB (bảng RefreshTokens)

Request có token → header: Authorization: Bearer <token>
                  → ValidateIssuer/Audience/Lifetime/SigningKey
```

- `Program.cs` cấu hình `AddAuthentication().AddJwtBearer()` với `TokenValidationParameters`
- Vì sao dùng RefreshToken: access token ngắn hạn (giảm rủi ro bị đánh cắp), refresh token dài hạn để cấp token mới mà không phải đăng nhập lại
- Frontend interceptor tự gọi `/auth/refresh` khi nhận 401 (1 lần), thành công thì thử lại request gốc

### 3.8 Program.cs — Pipeline & Dependency Injection

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(...);   // DI DbContext
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>(); // DI Repository
builder.Services.AddScoped<IDepartmentService, DepartmentService>();       // DI Service
builder.Services.AddAutoMapper(typeof(MappingProfile));     // DI AutoMapper
builder.Services.AddAuthentication(...);                    // JWT
builder.Services.AddSwaggerGen(...);                        // Swagger

var app = builder.Build();
// tự động migration khi khởi động:
db.Database.Migrate();   // tạo/cập nhật bảng từ Migrations

// Middleware pipeline (thứ tự quan trọng):
app.UseCors();              // 1. CORS
app.UseStaticFiles();       // 2. file tĩnh (avatar)
app.UseAuthentication();    // 3. xác thực
app.UseAuthorization();     // 4. phân quyền
app.MapControllers();       // 5. routing
```

**Scoped vs Singleton:** Repository/Service đăng ký `Scoped` → 1 instance mỗi request (đúng chu kỳ sống của DbContext).

### 3.9 Migrations

```bash
dotnet ef migrations add AddRefreshToken
dotnet ef database update
```

- `Migrations/` chứa lịch sử thay đổi schema, EF tự áp dụng khi `Migrate()`
- Khi deploy, API tự chạy migration → **không cần tạo bảng thủ công**

---

## 4. Frontend — React + TypeScript

### 4.1 Axios Instance + Interceptor

```ts
// shared/api/axios.ts
const baseURL = (window as any).__API_URL__ || import.meta.env.VITE_API_URL

const api = axios.create({ baseURL })

// Request: tự đính token
api.interceptors.request.use(config => {
  const token = localStorage.getItem("accessToken")
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Response: gặp 401 → tự động refresh token → thử lại request
api.interceptors.response.use(res => res, async err => {
  if (err.response?.status === 401 && !err.config._retry) {
    // gọi /Auth/refresh, lưu token mới, retry request gốc
  }
})
```

**2 điểm quan trọng nhất của frontend:**
1. `VITE_API_URL` lấy từ **runtime** `config.js` (do nginx tạo khi container start) — vì env build-time không đổi được sau khi deploy
2. `window.location.href` dùng cho redirect thay vì `useNavigate` — vì `useNavigate` không đáng tin ở production build

### 4.2 Routing & Token Guard

```tsx
// app/App.tsx
const token = localStorage.getItem("accessToken")

<Route path="/dashboard" element={token ? <DashboardPage/> : <Navigate to="/login"/>} />
```

- Không có token → tự chuyển về `/login`
- Các route: `/login`, `/dashboard`, `/departments`, `/employees`, `/` redirect → `/dashboard`

### 4.3 Login Flow

```ts
const res = await api.post("/Auth/login", { username, password })
localStorage.setItem("accessToken", res.data.accessToken)
localStorage.setItem("refreshToken", res.data.refreshToken)
window.location.href = "/dashboard"
```

### 4.4 Features API

```ts
// features/department/api/departmentApi.ts
export const departmentApi = {
  getAll: () => api.get<Department[]>("/Departments"),
  create: (dto) => api.post("/Departments", dto),
  update: (id, dto) => api.put(`/Departments/${id}`, dto),
  delete: (id) => api.delete(`/Departments/${id}`),
}
```

---

## 5. Docker

### 5.1 Dockerfile (API) — Multi-stage build

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build   # stage 1: compile
COPY EmployeeManagement.API/EmployeeManagement.API.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0          # stage 2: chạy
COPY --from=build /app .
CMD dotnet EmployeeManagement.API.dll --urls http://0.0.0.0:${PORT:-8080}
```

**Bài học quan trọng:**
- Image cuối chỉ chứa runtime (không SDK) → image nhỏ hơn rất nhiều
- `COPY docker-publish/ .` (file binary build tay) KHÔNG hoạt động trên Render vì thư mục không có trên GitHub → phải multi-stage build
- Render dùng biến `PORT` → phải chạy `--urls http://0.0.0.0:${PORT:-8080}` thay vì port cố định

### 5.2 Dockerfile (Frontend)

```dockerfile
FROM node:22-alpine AS build        # stage 1: build React
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/. .
RUN npm run build

FROM nginx:alpine                   # stage 2: serve bằng nginx
COPY --from=build /app/dist /usr/share/nginx/html
CMD echo "window.__API_URL__ = '${VITE_API_URL:-/api}';" > /usr/share/nginx/html/config.js \
  && sed -i "s/listen 80;/listen ${PORT:-80};/g" /etc/nginx/conf.d/default.conf \
  && nginx -g "daemon off;"
```

**Build context:** phải là root repo (`COPY frontend/...`), vì Render build với context là root — không được đặt `frontend/` trong `.dockerignore` root.

### 5.3 docker-compose.yml

```yaml
services:
  db:        # PostgreSQL 18, port 5433 (host) → 5432 (container), volume pgdata
  api:       # build từ Dockerfile, port 8080, env ConnectionStrings__DefaultConnection
  frontend:  # build frontend/Dockerfile, port 3000 → 80
```

**Lưu ý:**
- Host trong container network: dùng tên service (`db`, `api`) thay vì `localhost`
- `depends_on` + `healthcheck` đảm bảo API chỉ start sau khi DB sẵn sàng
- Cần `docker builder prune -a -f` khi build bị cache cũ

---

## 6. CI/CD — GitHub Actions + Render

### 6.1 CI (.github/workflows/ci.yml)

```yaml
on:
  push: { branches: [main] }
  pull_request: { branches: [main] }

jobs:
  api:       # restore → build → test (dotnet test)
  frontend:  # npm ci → lint → build
```

### 6.2 Deploy (Render)

- **API**: Web Service, Dockerfile `/Dockerfile`, env: `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `ASPNETCORE_ENVIRONMENT=Production`
- **Frontend**: Web Service, Dockerfile `/frontend/Dockerfile`, env: `VITE_API_URL=https://<api-url>/api`
- **PostgreSQL**: Render DB, connection string dạng URI `postgresql://...` → Program.cs tự parse sang key-value

**Luồng làm việc:**
```
push feature → CI chạy → tạo PR → CI pass mới merge main → Render auto-deploy
```

### 6.3 Những lỗi deploy đã gặp (kinh nghiệm)

| Lỗi | Nguyên nhân | Cách xử lý |
|---|---|---|
| `"/docker-publish": not found` | Dockerfile copy thư mục không có trên GitHub | multi-stage build |
| `inotify instances limit reached` | .NET watch file config, Render giới hạn | `ASPNETCORE_hostBuilder__reloadConfigOnChange=false` |
| `Format of the initialization string...` | Npgsql không parse được connection string dạng URI | parse URI → key-value trong Program.cs |
| `405 Method Not Allowed` | Frontend gọi `/api/...` qua nginx, nginx không có proxy | dùng `VITE_API_URL` trỏ thẳng tới API |
| `host not found in upstream "api"` | nginx.conf proxy tới host `api` không tồn tại trên Render | bỏ proxy, dùng URL API trực tiếp |
| `VITE_API_URL` không đổi được sau build | Vite env là build-time | tạo `config.js` runtime khi container start |

---

## 7. Python Script

```python
# python/read_employees.py
import psycopg2
conn = psycopg2.connect(host="localhost", port=5433, dbname="employeemanagement", user="postgres", password="admin")
cur = conn.cursor()
cur.execute("SELECT * FROM employees")
rows = cur.fetchall()
```

- Dùng thư viện `psycopg2` (cần `pip install psycopg2-binary`)
- Có thể đọc DB Docker (port 5433) hoặc DB Render (connection string)

---

## 8. Testing

**xUnit + Moq** — Unit test cho Service layer, mock Repository:

```csharp
[Fact]
public async Task GetByIdAsync_ExistingId_ReturnsDepartment()
{
    _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Department { Id = 1, Name = "IT" });
    var result = await _service.GetByIdAsync(1);
    Assert.Equal("IT", result!.Name);
}
```

- 6 test: GetAll, GetById (có/không có), Create, Update, Delete
- Chạy: `dotnet test EmployeeManagement.API.Tests/`

---

## 9. Kiến thức tổng hợp theo chủ đề

### REST API
- URL động từ thuộc tính: `[controller]` = tên Controller bỏ "Controller"
- HTTP status code chuẩn: 200/201/204/400/401/404
- `[ApiController]` tự động: validation 400, binding từ body/route/query

### Dependency Injection
- Đăng ký: `AddScoped` (mỗi request 1 instance)
- Constructor injection: framework tự tạo object theo interface

### JWT
- 3 phần: Header (thuật toán) + Payload (claims) + Signature (ký bằng secret)
- Cấu hình: Issuer, Audience, Key, Lifetime

### EF Core
- Migration: tạo/áp dụng thay đổi schema
- `Include()` để join table (nạp navigation property)
- `Skip()/Take()` phân trang, `Where()` lọc, `OrderBy()` sắp xếp

### React
- Hooks: `useState`, `useEffect`
- axios interceptor: xử lý token tập trung
- LocalStorage: lưu token (đơn giản, không an toàn tuyệt đối)

### Docker
- Multi-stage build: build 1 stage, chạy stage khác
- Network nội bộ: container gọi nhau bằng service name
- Env var runtime vs build-time

### CI/CD
- CI: kiểm tra code tự động (build + test) khi push/PR
- Deploy tự động: merge main → deploy lên cloud

---

## 10. Các lệnh thường dùng

```bash
# Backend
dotnet run                       # chạy API (dev)
dotnet ef migrations add <Name>  # tạo migration mới
dotnet ef database update        # áp dụng migration
dotnet test                      # chạy test

# Frontend
npm run dev                      # dev server
npm run build                    # build production
npm run lint                     # lint code

# Docker
docker compose up -d             # chạy toàn bộ (db + api + frontend)
docker compose down              # dừng
docker builder prune -a -f       # xóa cache build

# Deploy Render
git push origin main             # tự động CI + deploy
```

---

## 11. Tham khảo nhanh

| Chủ đề | File chính |
|---|---|
| Cấu hình & pipeline | `EmployeeManagement.API/Program.cs` |
| Routing API | `Controllers/*` |
| Business logic | `Services/*` |
| Truy vấn DB | `Repositories/*` |
| Model | `Models/*` |
| DTO + Map | `DTOs/*` |
| Routing SPA | `frontend/src/app/App.tsx` |
| Gọi API + token | `frontend/src/shared/api/axios.ts` |
| Docker | `Dockerfile`, `frontend/Dockerfile`, `docker-compose.yml` |
| CI | `.github/workflows/ci.yml` |
| Test | `EmployeeManagement.API.Tests/` |
