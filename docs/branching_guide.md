# 📖 Quy Tắc Branching Git

Tài liệu này định nghĩa quy trình làm việc với Git (**Git Flow**) cho dự án **Gomoku**.  
Việc tuân thủ quy trình này là **bắt buộc** để đảm bảo sự nhất quán, giảm thiểu xung đột (merge conflicts) và giữ cho code luôn ở trạng thái ổn định.

---

> ✅ **Luôn có một nhánh chính (`main`)** chứa code ổn định, sẵn sàng để demo hoặc nộp bài.  
> ✅ **Có một nhánh phát triển (`develop`)** để tích hợp các tính năng đã hoàn thành.  
> ✅ **Quản lý việc sửa lỗi** một cách rõ ràng.

<br>
<br>
<br>

# 🏛️ 1. Các Nhánh Chính (Long-Lived Branches)

Dự án sẽ có **2 nhánh tồn tại vĩnh viễn**: `main` và `develop`.

---

### 1.1 🧱 Nhánh `main`

**Mục đích:**  
Đây là nhánh "sản phẩm" (**production**).  
Nó **LUÔN LUÔN** chứa code đã ổn định, đã được kiểm thử và sẵn sàng để demo cho giảng viên hoặc nộp bài.

**Quy tắc:**

- 🚫 **CẤM TUYỆT ĐỐI** commit hoặc push trực tiếp lên `main`.
- `main` chỉ nhận code merge từ:
  - `develop` (khi nhóm quyết định “chốt” phiên bản lớn)
  - `hotfix/*` (khi cần sửa lỗi khẩn cấp)
- Mỗi lần merge vào `main` nên được **đánh tag** (ví dụ: `v1.0`, `v1.1`, `final-demo`).

---

### 1.2 🧩 Nhánh `develop`

**Mục đích:**  
Đây là nhánh phát triển chính (**integration branch**).  
Tất cả các tính năng mới sau khi hoàn thành sẽ được merge vào đây.  
Nhánh này chứa code **mới nhất** của dự án.

**Quy tắc:**

- 🚫 **Không code trực tiếp** trên `develop`.
- Đây là nhánh “nguồn” để tạo ra tất cả các nhánh `feature/*`.
- Tất cả nhánh `feature/*` và `bugfix/*` đều merge vào `develop`.

<br>
<br>
<br>

# 🌿 2. Các Nhánh Hỗ Trợ (Supporting Branches)

Đây là các nhánh có vòng đời ngắn, dùng để phát triển tính năng hoặc sửa lỗi.  
Chúng sẽ **bị xóa sau khi được merge**.

---

### 2.1 🌟 `feature/*` — Nhánh Tính Năng

**Mục đích:** Phát triển một tính năng mới (ví dụ: Login, Tìm phòng, Xử lý logic game).

- **Nguồn (Branch from):** `develop`
- **Merge vào (Merge to):** `develop`

**Quy tắc:**

- Mỗi thành viên tự tạo nhánh `feature/*` cho tính năng mình làm.
- ❌ Không merge nhánh `feature` này vào `feature` khác.

---

### 2.2 🐞 `bugfix/*` — Nhánh Sửa Lỗi

**Mục đích:** Sửa các lỗi được phát hiện trên `develop`.

- **Nguồn:** `develop`
- **Merge vào:** `develop`

---

### 2.3 🚨 `hotfix/*` — Nhánh Sửa Lỗi Khẩn Cấp

**Mục đích:** Sửa lỗi nghiêm trọng được phát hiện trên `main` (ví dụ: code demo bị crash).

- **Nguồn:** `main`
- **Merge vào:** `main` **và** `develop` (để cả hai cùng nhận bản vá).

<br>
<br>
<br>

# 🏷️ 3. Quy Ước Đặt Tên Nhánh (Naming Convention)

Giúp cả nhóm biết ai làm gì, rõ ràng và thống nhất.

> **Cú pháp:** [loại]/[tên-viết-tắt]/[mô-tả-ngắn]

**Trong đó:**

- `[loại]`: `feature`, `bugfix`, `hotfix`
- `[tên-viết-tắt]`: viết tắt tên thành viên
- `[mô-tả-ngắn]`: mô tả ngắn gọn, dùng dấu `-` thay cho khoảng trắng

**Ví dụ:**

- **`feature/truong/login-screen-ui` → Phan Thiên Trường làm UI màn hình Login**
- **`feature/tuan-huynh/gameplay-logic` → Huỳnh Anh Tuấn làm logic ván đấu**
- **`hotfix/tuan-hoang/fix-null-password` → Hoàng Đức Anh Tuấn sửa lỗi password null**
- **`hotfix/tu/fix-demo-crash` → Phan Anh Tú sửa lỗi crash khi demo**

<br>
<br>
<br>

# 🔄 4. Luồng Làm Việc (Workflow) — Step by Step

### 🧭 Bước 1: Bắt đầu một tính năng mới

Giả sử bạn làm tính năng **Đăng nhập**.

**Luôn đảm bảo code ở `develop` là mới nhất:**

```bash
# Chuyển về nhánh develop
git checkout develop

# Kéo code mới nhất
git pull origin develop
```

**Tạo nhánh feature mới từ develop:**

```bash
# checkout : chuyển nhánh
# -b : tạo mới nếu chưa có
git checkout -b feature/at/login-screen
```

### 💻 Bước 2: Làm việc và Commit

Ví dụ : Thực hiện code trên nhánh `feature/at/login-screen`

**Commit thường xuyên với message rõ ràng:**

```bash
git add .
git commit -m "[Login] Add UI components for login form"
```

**Đẩy (push) nhánh của bạn lên server:**

```bash
git push origin feature/at/login-screen
```

### 🔀 Bước 3: Hoàn thành tính năng và Tạo Pull Request

Khi đã code xong và test kỹ ở local:

**Cập nhật develop & rebase để tránh xung đột:**

```bash
git checkout develop
git pull origin develop
git checkout feature/at/login-screen
git rebase develop

# Nếu có conflict:
#   git add .
#   git rebase --continue
git push origin feature/at/login-screen --force-with-lease

```

**Tiếp theo**

- **Lên GitHub/GitLab tạo Pull Request:**

- **Source branch: feature/at/login-screen**

- **Target branch: develop**

- **Ghi mô tả chi tiết PR và tag 1–2 thành viên (ví dụ: @AnhTuan) để review code.**

### 👀 Bước 4: Review Code và Merge

- Thành viên được tag vào sẽ review code.

- Nếu có vấn đề, họ comment trực tiếp trên PR.

- Bạn sửa code → commit → push lại (PR tự cập nhật).

- Khi review OK, PR được merge vào develop.

- Sau khi merge → xóa nhánh feature.

### 🚀 Bước 5: "Chốt" Phiên Bản (Release)

**Khi các tính năng trong develop đã ổn định:**

1. Trưởng nhóm tạo PR từ develop → main

2. Cả nhóm kiểm tra lại lần cuối

3. Merge PR vào main

4. Tạo tag phiên bản

```bash
# Đảm bảo đang ở main
git checkout main
git pull origin main

# Tạo tag
git tag -a v1.0 -m "Release v1.0 - Login, Register, Profile"

# Đẩy tag lên server
git push origin v1.0

```

<br>
<br>
<br>

# > 📝 Hướng Dẫn Nhanh (Cheat Sheet)

| Tình huống                                       | Lệnh nhanh                                                                          |
| ------------------------------------------------ | ----------------------------------------------------------------------------------- |
| 🚫 Không bao giờ code trên `main` hoặc `develop` |                                                                                     |
| 🔄 Cập nhật code mới nhất                        | `git checkout develop` → `git pull origin develop`                                  |
| 🌱 Bắt đầu tính năng mới                         | `git checkout -b feature/<tên-bạn>/<tính-năng>`                                     |
| 📤 Push nhánh lên server                         | `git push origin feature/<tên-bạn>/<tính-năng>`                                     |
| 🧩 Tạo Pull Request                              | Từ `feature/*` → `develop`                                                          |
| 🐞 Sửa lỗi trên `develop`                        | `git checkout -b bugfix/<tên-bạn>/<tên-lỗi>`                                        |
| 🚨 Sửa lỗi khẩn trên `main`                      | `git checkout -b hotfix/<tên-bạn>/<tên-lỗi>` → merge vào **cả `main` và `develop`** |
