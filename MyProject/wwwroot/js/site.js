// รอให้เอกสาร HTML ทั้งหมดโหลดเสร็จก่อนเริ่มทำงาน
$(document).ready(function () {

    // ===================================
    // == ส่วนควบคุมการแสดง Modal Login/Register ==
    // ===================================
    $('.show-auth-modal').on('click', function (e) {

        // ป้องกันไม่ให้ลิงก์ทำงานตามปกติ
        e.preventDefault();

        // ดึงค่าจาก attribute 'data-form' ของปุ่มที่ถูกคลิก
        var formType = $(this).data('form');

        // เปิด Modal ที่ถูกต้อง
        if (formType === 'login') {
            $('#loginModal').modal('show');
        }
        else if (formType === 'signup') {
            $('#registerModal').modal('show');
        }
    });

    // ===================================
    // ==   ส่วนของ jQuery UI Datepicker  ==
    // ===================================

    // **สำคัญ:** ตั้งค่าปฏิทินให้เป็นภาษาอังกฤษ (en-GB) เสมอ
    // เพื่อแก้ปัญหาปี พ.ศ. เพี้ยน เมื่อเปลี่ยนภาษาเว็บเป็นไทย
    $.datepicker.setDefaults($.datepicker.regional["en-GB"]);

    // สั่งให้ element ที่มี id="datepicker" กลายเป็นช่องเลือกวันที่แบบพิเศษ
    $("#datepicker").datepicker({
        changeMonth: true,     // เปิดให้เลือกเดือนจาก Dropdown
        changeYear: true,      // เปิดให้เลือกปีจาก Dropdown
        yearRange: "c-100:c",  // กำหนดช่วงปีให้เลือกได้ (100 ปีก่อนหน้า ถึงปีปัจจุบัน)
        dateFormat: "yy-mm-dd", // กำหนด Format วันที่เป็น "ปี-เดือน-วัน"
        showAnim: "slideDown"  // เพิ่ม Animation เล็กน้อยตอนเปิด
    });

    // ===================================
    // ==  ทำให้ Alert หายไปเองใน 5 วินาที  ==
    // ===================================
    // ใช้ setTimeout เพื่อหน่วงเวลาทำงาน
    setTimeout(function () {
        // ค้นหา div ที่มี class 'alert' แล้วค่อยๆ เลื่อนปิด (slideUp)
        // 500 คือความเร็วในการเลื่อนปิด (0.5 วินาที)
        $('.alert').slideUp(500);
    }, 5000); // 5000 milliseconds = 5 วินาที

});