# Project Tích hợp Bitrix24

Dự án này minh họa việc tích hợp với Bitrix24 sử dụng .NET 8, với các tính năng quản lý liên hệ và thiết lập ứng dụng.

---

## Yêu cầu

Trước khi bắt đầu, hãy đảm bảo bạn đã cài đặt các công cụ sau:

* **Visual Studio 2022**
* **Ngrok**
* **.NET 8 SDK**

Bạn cũng sẽ cần 2 cổng trống: `5010` và `7269`.

---

## Cài đặt

Thực hiện theo các bước sau để thiết lập và chạy dự án:

1.  **Clone project** về máy, sau đó vào thư mục và chạy file `Bitrix24Integration.sln`.
2.  Trong Visual Studio, chọn **Project** -> **Configure Startup Projects...**

    ![Step 1](images/step1.png)

3.  Chọn **Multiple startup projects** và bật **Action** -> **Start** cho cả hai project. Nhấn **OK**.

    ![Step 2](images/step2.png)

4.  **Chạy project** (F5 hoặc nút Start).
5.  **Bật Ngrok** bằng cách mở terminal hoặc command prompt và chạy lệnh sau:

    ```bash
    ngrok http 5010
    ```

    ![Step 3](images/step3.png)

6.  Truy cập trang Bitrix24 cá nhân của bạn, sau đó chọn **Developer resources**.

    ![Step 4](images/step4.png)

7.  Chọn **other** -> **local application**.

    ![Step 5](images/step5.png)

8.  Copy link từ Ngrok (ví dụ: `https://your-ngrok-url.ngrok-free.app`) và điền vào trường **Install path** với `/api/Install` ở cuối. Sau đó, nhấn **Save**.

    ![Step 6-1](images/step6-1.png)

    ![Step 6](images/step6.png)

---

## Hướng dẫn sử dụng

Dự án này bao gồm hai bài tập chính: tương tác với Bitrix24 API qua Swagger và quản lý liên hệ thông qua giao diện web.

### Bài 1: Sử dụng Bitrix24 API qua Swagger

1.  Truy cập trang Swagger UI tại `https://localhost:7289/swagger`.

    ![Step 7](images/step7.png)

2.  Tìm và mở rộng API **CallApi**. Nhấn **Try It Out**.

    ![Step 8](images/step8.png)

3.  Quay lại trang ứng dụng local Bitrix24 để lấy **Application ID (Client ID)** và **Application Key (Client Secret)**. Điền thông tin này vào các trường tương ứng trong Swagger.

    ![Step 9-1](images/step9-1.png)

    ![Step 9](images/step9.png)

4.  Nhấn **Execute**. Kéo xuống để xem kết quả API.

    ![Step 10](images/step10.png)

---

### Bài 2: Quản lý liên hệ qua giao diện web

1.  Truy cập trang quản lý liên hệ tại `https://localhost:7269/`.

    ![Step 11](images/step11.png)

2.  Điền **Application ID (Client ID)** và **Application Key (Client Secret)** từ Bitrix24 của bạn, sau đó nhấn **Lưu**.

    ![Step 12](images/step12.png)

3.  Trên trang chính, bạn có thể thực hiện các hành động: **xóa** (biểu tượng thùng rác), **tìm kiếm**, hoặc xem **chi tiết liên hệ** (biểu tượng bên cạnh thùng rác).

    ![Step 13](images/step13.png)

4.  Để tạo một liên hệ mới, nhấn **Thêm Liên Hệ** từ trang chính.

    ![Step 13](images/step13.png)

5.  Điền vào các trường thông tin liên hệ và nhấn **Tạo**.

    ![Step 14](images/step14.png)

    ![Step 15](images/step15.png)

6.  Nếu thành công, bạn sẽ tự động được dẫn đến trang chi tiết của liên hệ đó.

    ![Step 16](images/step16.png)

7.  Tại đây, bạn có thể **chỉnh sửa thông tin cơ bản** như Tên, Email, Số điện thoại và Website.

    ![Step 17](images/step17.png)

    ![Step 18](images/step18.png) -> ![Step 19](images/step19.png)

8.  Kéo xuống dưới là phần thông tin **Bank** và **Address**. Trước tiên, bạn phải tạo một hồ sơ để thêm thông tin này.

    ![Step 20](images/step20.png) -> ![Step 21](images/step21.png) -> ![Step 22](images/step22.png)

9.  Từ đó, bạn có thể **thêm/xóa/sửa** các thông tin Bank hoặc Address, hoặc **xóa toàn bộ hồ sơ** (lưu ý: thao tác này cũng sẽ xóa luôn thông tin Bank và Address kèm theo).

    ![Step 23](images/step23.png) -> ![Step 24](images/step24.png)

    ![Step 25](images/step25.png) -> ![Step 26](images/step26.png) -> ![Step 27](images/step27.png)

10. **Lưu ý quan trọng:** Nếu có thông báo lỗi, thường là do thiếu **Application ID (Client ID)** và **Application Key (Client Secret)**. Nhấn vào nút **Thay đổi Credentials** để cập nhật chúng.