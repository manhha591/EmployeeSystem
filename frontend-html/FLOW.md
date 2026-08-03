# Luồng hoạt động chi tiết — frontend-html (HTML/CSS/JS thuần)

> Tài liệu mô tả toàn bộ luồng hoạt động của phiên bản frontend không dùng framework.

---

## 1. Cấu trúc và vai trò các file

| File | Vai trò |
|---|---|
| `index.html` | Cửa ngõ: tự chuyển hướng login/dashboard dựa vào token |
| `login.html` + `js/login.js` | Trang đăng nhập, lưu token |
| `dashboard.html` + `js/dashboard.js` | Thống kê tổng quan |
| `departments.html` + `js/departments.js` | CRUD phòng ban |
| `employees.html` + `js/employees.js` | CRUD nhân viên (phân trang, lọc, avatar) |
| `css/style.css` | Toàn bộ style |
| `js/api.js` | Phần lõi: gọi API, token, refresh token, guard |

**Điểm khác biệt lớn nhất với React:** mỗi lần chuyển trang là **load lại toàn bộ trang HTML** (không phải SPA). Dữ liệu "sống xuyên trang" chỉ nằm ở `localStorage` (token).

---

## 2. Luồng khởi động (index.html)

```
User mở http://localhost:5173/
        ↓
index.html chạy 1 script inline duy nhất:
    localStorage có "accessToken" ?  →  chuyển đến dashboard.html
    không có                         →  chuyển đến login.html
```

- Token là "chìa khóa" xác định trạng thái đăng nhập
- Không có API call nào ở bước này — chỉ check localStorage

---

## 3. Luồng đăng nhập (login.html)

```
User nhập admin / admin123 → bấm "Đăng nhập"
        ↓
login.js bắt sự kiện submit form:
  1. Validate: username, password không được rỗng
  2. api.post("/Auth/login", { username, password })
        ↓
api.js thực hiện fetch POST → https://<API_URL>/Auth/login
        (lần này CHƯA có token → không đính Authorization header)
        ↓
Server trả về: { accessToken, refreshToken, expiresIn: 900 }
        ↓
login.js:
  localStorage.setItem("accessToken", ...)
  localStorage.setItem("refreshToken", ...)
        ↓
window.location.href = "dashboard.html"
```

**Lỗi xảy ra:**
- Sai tài khoản → server trả 401 → hiện "Sai tài khoản hoặc mật khẩu"
- Nút bị disable + hiện "Đang xử lý..." trong lúc chờ (chống spam)

---

## 4. Bảo vệ trang — requireAuth()

```
Mỗi trang nội bộ (dashboard/departments/employees) gọi requireAuth() ở đầu JS:
    if (!localStorage.getItem("accessToken")) {
        window.location.href = "login.html";   // đá về trang đăng nhập
    }
```

**Vì sao cần?** HTML thuần không có router như React — user có thể gõ thẳng `employees.html` trên URL. Mỗi trang phải **tự kiểm tra** trước khi làm gì.

**Lưu ý:** đây chỉ là rào cản phía client. Bảo mật thật nằm ở server: token giả/hết hạn sẽ bị API trả 401.

---

## 5. Phần lõi — api.js

### 5.1 Cấu hình

```js
const API_URL = "/api";  // hoặc https://employee-api-zkyo.onrender.com/api
```

### 5.2 apiFetch() — gọi fetch kèm token

```
Mọi request đều đi qua apiFetch(path, options):
  1. Đọc accessToken từ localStorage
  2. Nếu có → thêm header: Authorization: Bearer <token>
  3. fetch(`${API_URL}${path}`, ...)
  4. Nếu response 401 VÀ chưa retry → gọi tryRefreshToken()
        → refresh thành công → retry request ban đầu với token mới
  5. Trả về Response
```

### 5.3 apiRequest() — gọi + parse JSON + xử lý lỗi

```
1. Gọi apiFetch()
2. response không ok (4xx/5xx) → parse message (JSON hoặc text) → ném Error
3. ok → parse JSON nếu content-type là application/json
```

### 5.4 Các helper

```js
api.get(path)              // GET
api.post(path, body)       // POST kèm JSON body
api.put(path, body)        // PUT
api.delete(path)           // DELETE
api.upload(path, formData) // POST dạng multipart (upload file, không JSON)
```

### 5.5 tryRefreshToken() — tự gia hạn phiên

```
Gặp 401 → lấy refreshToken từ localStorage:
        ↓
không có refreshToken? → logout() → về login.html
        ↓
POST /Auth/refresh { refreshToken }:
        ↓
thành công → server trả cặp token MỚI
        → ghi đè localStorage
        → return true → request gốc được retry
        ↓
thất bại → logout() (refresh token hết hạn 7 ngày / bị revoke)
```

### 5.6 logout() + formatVND()

```
logout(): xóa cả 2 token khỏi localStorage → window.location.href = "login.html"

formatVND(amount): 15000000 → "15.000.000 VND"
```

---

## 6. Luồng Dashboard

```
requireAuth() OK
        ↓
api.get("/Dashboard")
        ↓
Server trả: {
    totalEmployees: 25,
    totalDepartments: 4,
    totalSalary: 150000000,
    employeesByDepartment: [ { departmentName, count, totalSalary }, ... ]
}
        ↓
1. Điền 3 ô thống kê: Nhân viên / Phòng ban / Tổng lương
2. Vẽ bảng "Nhân viên theo phòng ban":
   - Thanh màu xanh: width = count / maxCount * 100 %
   - Số nhân viên + tổng lương (formatVND)
        ↓
Lỗi → hiển thị thông báo trong bảng
```

---

## 7. Luồng CRUD Phòng ban

### 7.1 Khởi tạo

```
requireAuth() → loadDepartments() → api.get("/Departments") → render bảng
```

### 7.2 Thêm mới

```
Bấm "+ Thêm phòng ban" → openCreateModal():
    editingId = null            ← đánh dấu đang THÊM
    xóa nội dung ô input
    mở modal (class "open")
        ↓
Nhập tên → bấm "Lưu" (sự kiện trên #saveBtn):
    name rỗng → alert
        ↓
api.post("/Departments", { name })
        ↓
Thành công → showAlert xanh → đóng modal → loadDepartments() (tải lại)
```

### 7.3 Sửa

```
Bấm "Sửa" → openEditModal(id, name):
    editingId = id              ← đánh dấu đang SỬA
    điền sẵn tên hiện tại
        ↓
Bấm "Lưu" → api.put(`/Departments/${editingId}`, { id, name })
```

### 7.4 Xóa

```
Bấm "Xóa" → confirm("Bạn có chắc...?")
        ↓ đồng ý
api.delete(`/Departments/${id}`) → tải lại danh sách
```

**Ý nghĩa biến `editingId`:**
- `null` → POST (tạo mới)
- có giá trị → PUT (cập nhật, gửi kèm id)

---

## 8. Luồng CRUD Nhân viên (phức tạp nhất)

### 8.1 Khởi tạo (init)

```
1. api.get("/Departments") → lưu vào biến `departments` (cache)
2. fillDeptFilter() → đổ phòng ban vào dropdown lọc (góc trái trang)
3. fillDeptSelect() → đổ phòng ban vào dropdown trong modal
4. loadEmployees() → tải trang 1
```

### 8.2 Phân trang + tìm kiếm + lọc

```
State:
    currentPage = 1
    PAGE_SIZE = 10
    search (ô "Tìm kiếm...")
    deptId (dropdown "Tất cả phòng ban")

loadEmployees() ghép URL:
    /Employees?page=1&pageSize=10&search=Nguyen&departmentId=2
        ↓
Server trả: { items: [10 nhân viên], totalCount: 25, page, totalPages: 3 }
        ↓
renderEmployees(items) → innerHTML vào <tbody>
renderPagination() → vẽ nút: ‹ 1 2 3 › (nút active màu xanh)
        ↓
Sự kiện kích hoạt load lại:
  - Gõ ô tìm kiếm → debounce 300ms → reset trang 1 → load
  - Đổi dropdown lọc → reset trang 1 → load
  - Bấm nút phân trang → đổi currentPage → load
```

**Vì sao có debounce?** Mỗi ký tự gõ sẽ gây 1 request API → debounce 300ms chỉ gọi sau khi ngừng gõ, giảm tải server.

### 8.3 Thêm/Sửa nhân viên

```
Bấm "Sửa" → openEditModal(emp):   // nhận cả object employee
    editingId = emp.id
    điền fullName, email, phone, salary
    avatarFile rỗng (không cần load ảnh cũ)
    chọn đúng phòng ban: departments.find(d => d.name === emp.departmentName)
        ↓
Bấm "Lưu" → gom form thành dto:
    { fullName, email, phone, salary: Number(salary), departmentId: Number(deptSelect) }
        ↓
editingId null → POST /Employees → trả về created.id
editingId ≠ null → PUT /Employees/{id}
        ↓
Nếu có chọn file ảnh:
    formData = new FormData(); formData.append("file", avatarFile)
    api.upload(`/Employees/${empId}/avatar`, formData)
    (KHÔNG dùng JSON — phải là multipart/form-data vì là file)
        ↓
Đóng modal → loadEmployees()
```

### 8.4 Hiển thị avatar

```
Server trả e.avatar dạng "/uploads/avatars/{guid}.png" (đường dẫn tương đối)
        ↓
Ghép origin API: new URL(API_URL, location.href).origin + e.avatar
    → https://api.../uploads/avatars/{guid}.png
        ↓
Không có ảnh → ô "—" màu xám
Ảnh tải lỗi (onerror) → tự ẩn thẻ img
```

---

## 9. Luồng Đăng xuất

```
api.js tự đăng ký sự kiện cho #logoutBtn (nếu tồn tại trên trang):
        ↓
localStorage.removeItem("accessToken")
localStorage.removeItem("refreshToken")
        ↓
window.location.href = "login.html"
```

---

## 10. Tổng kết toàn cục

```
Mở trang ──► index.html ──► có token? ──yes──► dashboard.html
                     │                        │
                     └──no──► login.html ──► đăng nhập ──► dashboard.html
                                                        │
                    ┌───────────────────────────────────┘
                    ▼
      Mỗi trang: requireAuth() → gọi API (kèm Bearer token)
                    │
                    ├── 401 → tự refresh token → retry → OK
                    └── refresh thất bại → logout → login.html
                    │
                    ▼
      Thao tác dữ liệu qua modal → gọi API → tải lại danh sách
```

**3 thành phần tạo nên hệ thống:**
1. `localStorage` — giữ phiên đăng nhập (access + refresh token)
2. `js/api.js` — tập trung mọi request: gắn token, xử lý 401, refresh, lỗi
3. Mỗi trang tự kiểm tra token + tự render dữ liệu bằng `innerHTML`

---

## 11. So sánh với bản React

| Hoạt động | Bản HTML thuần | Bản React |
|---|---|---|
| Chuyển trang | `location.href` (tải lại trang) | Router SPA (đổi component) |
| Gọi API | fetch wrapper trong `api.js` | axios + interceptor |
| Giữ token | localStorage | localStorage |
| Render dữ liệu | Template string + `innerHTML` | JSX + state |
| Auth guard | `requireAuth()` mỗi trang | `<Navigate to="/login" />` trong router |
| Phân trang | Tự viết HTML string | JSX map |
| API URL runtime | `const API_URL` trong api.js | `config.js` do nginx tạo |
