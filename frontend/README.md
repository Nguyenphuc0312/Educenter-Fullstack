# EduCenter User UI

Frontend Vue 3 cho hệ thống EduCenter, gồm landing page, cổng học viên, cổng giảng viên và trang quản trị.

## Stack

- Vue 3 + Vite
- Vue Router
- Pinia
- Tailwind CSS
- Ant Design Vue cho bảng quản trị
- Vuetify assets/icons
- Axios API client

## Chạy local

```bash
npm install
npm run dev
```

## Build

```bash
npm run build
npm run lint
```

## Ghi chú

- Frontend gọi backend qua API Gateway tại `http://localhost:5000/gateway`.
- Giao diện hiện tại được giữ lại trong quá trình chuyển đổi từ React sang Vue.
- Các bảng admin sử dụng Ant Design Vue `a-table` với phân trang, chọn dòng và thao tác hàng loạt.
