$(document).ready(function () {
    // ... (ส่วนเปิด Modal และ Datepicker เหมือนเดิม) ...
    $('.show-auth-modal').on('click', function (e) {
        e.preventDefault();
        var formType = $(this).data('form');
        if (formType === 'login') {
            $('#loginErrorContainer').hide();
            $('#loginModal').modal('show');
        } else if (formType === 'signup') {
            $('#registerErrorContainer').hide();
            $('#registerModal').modal('show');
        }
    });
    $('#showProfileModalBtn').on('click', function (e) {
        e.preventDefault();
        $.ajax({
            type: "GET", url: "/Account/GetProfile",
            success: function (data) {
                $('#UserName').val(data.userName);
                $('#Email').val(data.email);
                $('#profileDatepicker').val(data.dateOfBirth);
                $('#profileErrorContainer').hide();
                $('#profileSuccessContainer').hide();
                $('#profileModal').modal('show');
            },
            error: function () { alert("ไม่สามารถโหลดข้อมูลโปรไฟล์ได้"); }
        });
    });
    $.datepicker.setDefaults($.datepicker.regional["en-GB"]);
    $("#datepicker, #profileDatepicker").datepicker({
        changeMonth: true, changeYear: true, yearRange: "c-100:c",
        dateFormat: "yy-mm-dd", showAnim: "slideDown"
    });

    // ===========================================
    // ==   AJAX SUBMISSION WITH ANTI-FORGERY   ==
    // ===========================================

    // --- AJAX for Login Form ---
    $('#loginForm').on('submit', function (e) {
        e.preventDefault();
        var form = $(this);
        var errorContainer = $('#loginErrorContainer');
        errorContainer.hide();
        var token = form.find('input[name="__RequestVerificationToken"]').val(); // **ดึงรหัสลับ**

        $.ajax({
            type: "POST", url: form.attr('action'), data: form.serialize(),
            headers: { 'RequestVerificationToken': token }, // **แนบรหัสลับไปกับ Header**
            success: function (response) {
                if (response.success) {
                    window.location.reload();
                } else {
                    errorContainer.text(response.message).show();
                }
            },
            error: function () { errorContainer.text('เกิดข้อผิดพลาดในการเชื่อมต่อ').show(); }
        });
    });

    // --- AJAX for Register Form ---
    $('#registerForm').on('submit', function (e) {
        e.preventDefault();
        var form = $(this);
        var errorContainer = $('#registerErrorContainer');
        errorContainer.hide();
        var formData = new FormData(this);
        var token = form.find('input[name="__RequestVerificationToken"]').val(); // **ดึงรหัสลับ**

        $.ajax({
            type: "POST", url: form.attr('action'), data: formData,
            processData: false, contentType: false,
            headers: { 'RequestVerificationToken': token }, // **แนบรหัสลับไปกับ Header**
            success: function (response) {
                if (response.success) {
                    window.location.reload();
                } else {
                    errorContainer.text(response.message).show();
                }
            },
            error: function () { errorContainer.text('เกิดข้อผิดพลาดในการเชื่อมต่อ').show(); }
        });
    });

    // --- AJAX for Profile Form ---
    $('#profileForm').on('submit', function (e) {
        e.preventDefault();
        var form = $(this);
        var errorContainer = $('#profileErrorContainer');
        var successContainer = $('#profileSuccessContainer');
        errorContainer.hide(); successContainer.hide();
        var formData = new FormData(this);
        var token = form.find('input[name="__RequestVerificationToken"]').val(); // **ดึงรหัสลับ**

        $.ajax({
            type: "POST", url: form.attr('action'), data: formData,
            processData: false, contentType: false,
            headers: { 'RequestVerificationToken': token }, // **แนบรหัสลับไปกับ Header**
            success: function (response) {
                if (response.success) {
                    successContainer.text(response.message).show();
                    setTimeout(function () { window.location.reload(); }, 2000);
                } else {
                    errorContainer.text(response.message).show();
                }
            },
            error: function () { errorContainer.text('เกิดข้อผิดพลาดในการเชื่อมต่อ').show(); }
        });
    });
});