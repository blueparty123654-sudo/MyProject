$(document).ready(function () {

    // --- ส่วนจัดการการเปิด Modals ---
    // (โค้ดส่วนนี้เหมือนเดิม)

    // --- ส่วนตั้งค่า Datepicker ---
    // (โค้ดส่วนนี้เหมือนเดิม)


    // --- ส่วนจัดการการส่งฟอร์มด้วย AJAX (ฉบับปรับปรุง) ---
    function handleFormSubmit(form) {
        // Unobtrusive validation จะทำงานก่อน AJAX โดยอัตโนมัติ
        // ถ้าฟอร์มไม่ผ่าน client-side validation, AJAX จะไม่ถูกส่ง
        if (!form.valid()) {
            return;
        }

        var alertBoxId = form.attr('id').replace('Form', 'Alert');
        var alertBox = $('#' + alertBoxId);
        alertBox.hide();

        var formData = new FormData(form[0]);

        $.ajax({
            url: form.attr('action'),
            type: form.attr('method'),
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                // ล้าง error เก่าๆ ที่แสดงอยู่
                form.find(".text-danger").text("");

                if (response.success) {
                    alertBox.removeClass('alert-danger').addClass('alert-success').text(response.message).show();
                    setTimeout(function () {
                        window.location.href = response.redirectUrl || '/';
                    }, 1500);
                } else {
                    if (response.message) {
                        // แสดง error ทั่วไป (เช่น รหัสผ่านผิด)
                        alertBox.removeClass('alert-success').addClass('alert-danger').text(response.message).show();
                    }
                    if (response.errors) {
                        // แสดง error ของแต่ละช่อง
                        $.each(response.errors, function (key, value) {
                            var field = form.find('[name="' + key + '"]');
                            var validationSpan = field.next('.text-danger');
                            if (validationSpan) {
                                validationSpan.text(value[0]);
                            }
                        });
                    }
                }
            },
            error: function () {
                alertBox.removeClass('alert-success').addClass('alert-danger').text('เกิดข้อผิดพลาดในการเชื่อมต่อกับเซิร์ฟเวอร์').show();
            }
        });
    }

    // ผูก Event กับฟอร์มทั้ง 3
    $('#loginForm, #registerForm, #profileForm').on('submit', function (e) {
        e.preventDefault();
        handleFormSubmit($(this));
    });

    // --- ส่วนจัดการ Floating Alert ที่มาจาก TempData ---
    // (โค้ดส่วนนี้เหมือนเดิม)

});