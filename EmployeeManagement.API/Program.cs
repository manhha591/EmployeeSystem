using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.Repositories;
using EmployeeManagement.API.Services;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API;
using Microsoft.OpenApi;

// Tạo ứng dụng web với cấu hình mặc định (appsettings.json, env vars, ...)
var builder = WebApplication.CreateBuilder(args);

// Cho phép tất cả domain gọi API (CORS mở hoàn toàn)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Đăng ký Controllers, bỏ qua vòng lặp tham chiếu khi serialize JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
// Đọc connection string từ config, nếu là dạng URI (postgres://...) thì parse sang key-value
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
if (connStr != null && connStr.StartsWith("postgres"))
{
    var uri = new Uri(connStr);
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var db = uri.AbsolutePath.TrimStart('/');
    var userInfo = uri.UserInfo.Split(':');
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";
    connStr = $"Host={host};Port={port};Database={db};Username={username};Password={password}";
}
// Đăng ký DbContext với PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connStr));
// Đăng ký các Repository và Service để Inject (DI)
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
// Đăng ký AutoMapper với MappingProfile (chuyển Entity <-> DTO)
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Cấu hình xác thực JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,               // Kiểm tra nhà phát hành token
            ValidateAudience = true,             // Kiểm tra đối tượng nhận token
            ValidateLifetime = true,             // Kiểm tra thời hạn token
            ValidateIssuerSigningKey = true,     // Kiểm tra chữ ký
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Đăng ký Swagger (tài liệu API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Thêm nút "Authorize" trên Swagger UI để nhập JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
    });
});

// Xây dựng ứng dụng
var app = builder.Build();

// Tự động chạy migration khi khởi động (tạo bảng nếu chưa có)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbConnStr = db.Database.GetConnectionString();
    logger.LogInformation("Connection string: {ConnStr}", dbConnStr);
    try
    {
        db.Database.Migrate();
        logger.LogInformation("Migration completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Migration failed, trying EnsureCreated");
        db.Database.EnsureCreated();
    }
}

// Middleware pipeline (thứ tự xử lý request)
if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<SwaggerSecurityFix>();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();        // Phục vụ file tĩnh (avatar, ...)

app.UseAuthentication();     // Xác thực: kiểm tra token
app.UseAuthorization();      // Phân quyền: kiểm tra role

app.MapControllers();        // Ánh xạ request vào Controllers

app.Run();
