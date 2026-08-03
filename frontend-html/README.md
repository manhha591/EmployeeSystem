# frontend-html — Phiên bản HTML/CSS/JS thuần

Phiên bản không cần framework (không React), dùng HTML + CSS + Vanilla JS thuần.

## Tính năng

- Đăng nhập / đăng xuất (JWT + refresh token tự động)
- Dashboard thống kê (nhân viên, phòng ban, tổng lương, biểu đồ thanh)
- CRUD Phòng ban (modal thêm/sửa, xóa có xác nhận)
- CRUD Nhân viên (phân trang, tìm kiếm, lọc theo phòng ban, upload avatar)

## Cấu trúc

```
frontend-html/
├── index.html        → tự chuyển hướng login/dashboard
├── login.html        → trang đăng nhập
├── dashboard.html    → trang tổng quan
├── departments.html  → trang phòng ban
├── employees.html    → trang nhân viên
├── css/style.css     → toàn bộ style
└── js/
    ├── api.js        → fetch wrapper (token, refresh, helpers)
    ├── login.js
    ├── dashboard.js
    ├── departments.js
    └── employees.js
```

## Cấu hình API URL

Sửa hằng số ở đầu file `js/api.js`:

```js
const API_URL = "/api";  // hoặc https://employee-api-zkyo.onrender.com/api
```

## Cách chạy

Vì dùng `fetch` tới API khác origin nên cần chạy bằng HTTP server (không mở file trực tiếp):

```bash
# Python
python -m http.server 5173 --directory frontend-html

# hoặc Node
npx serve frontend-html
```

Mở `http://localhost:5173` → đăng nhập `admin` / `admin123`.

Nếu frontend và API khác domain (VD: Render), cần đảm bảo CORS trên API đã bật (dự án này đã bật AllowAny).

## Khác biệt với bản React

| | Bản React | Bản HTML thuần |
|---|---|---|
| Framework | React 19 + Vite | Không (Vanilla JS) |
| Routing | react-router-dom | Chuyển trang bằng file .html |
| Gọi API | axios + interceptor | fetch wrapper tự viết |
| Build | npm run build → bundle | Không cần build |
