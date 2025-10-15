$(document).ready(function () {

    // ... (โค้ดเปิด modal, datepicker, AJAX for Login/Register ของเดิม) ...
    // ... (ตรวจสอบให้แน่ใจว่าโค้ดเก่าทั้งหมดอยู่ครบ) ...

    // --- Datepicker สำหรับฟอร์ม Profile ---
    $("#profileDatepicker").datepicker({
        changeMonth: true, changeYear: true, yearRange: "c-100:c",
        dateFormat: "yy-mm-dd", showAnim: "slideDown"
    });

    // ===========================================
    // ==   เปิด Profile Modal และดึงข้อมูลเก่า   ==
    // ===========================================
    $('#showProfileModalBtn').on('click', function (e) {
        e.preventDefault();

        // ดึงข้อมูลโปรไฟล์ปัจจุบันจากเซิร์ฟเวอร์
        $.ajax({
            type: "GET",
            url: "/Account/GetProfile",
            success: function (data) {
                // นำข้อมูลที่ได้ไปใส่ในฟอร์ม
                $('#UserName').val(data.userName);
                $('#Email').val(data.email);
                $('#profileDatepicker').val(data.dateOfBirth);

                // ซ่อนข้อความแจ้งเตือนเก่าๆ แล้วค่อยเปิด Modal
                $('#profileErrorContainer').hide();
                $('#profileSuccessContainer').hide();
                $('#profileModal').modal('show');
            },
            error: function () {
                alert("ไม่สามารถโหลดข้อมูลโปรไฟล์ได้");
            }
        });
    });

    // ===================================
    // ==   AJAX for Profile Form      ==
    // ===================================
    $('#profileForm').on('submit', function (e) {
        e.preventDefault();

        var form = $(this);
        var errorContainer = $('#profileErrorContainer');
        var successContainer = $('#profileSuccessContainer');
        errorContainer.hide();
        successContainer.hide();

        var formData = new FormData(this); // ใช้ FormData สำหรับการอัปโหลดไฟล์

        $.ajax({
            type: "POST",
            url: form.attr('action'),
            data: formData,
            processData: false, // จำเป็นสำหรับ FormData
            contentType: false, // จำเป็นสำหรับ FormData
            success: function (response) {
                if (response.success) {
                    successContainer.text(response.message).show();
                    // หน่วงเวลา 2 วินาทีแล้วรีโหลดหน้าเพื่อให้เห็นชื่อใหม่บน Navbar
                    setTimeout(function () {
                        window.location.reload();
                    }, 2000);
                } else {
                    errorContainer.text(response.message).show();
                }
            },
            error: function () {
                errorContainer.text('เกิดข้อผิดพลาดในการเชื่อมต่อ').show();
            }
        });
    });

});