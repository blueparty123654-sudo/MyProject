$(document).ready(function () {

    // --- ตัวแปรสำหรับ Modal โปรไฟล์ (ถ้ามี) ---
    // const profileModal = $('#profileModal');
    // ...

    // --- ตัวแปรสำหรับ Modal ประวัติการจอง ---
    const historyModalElement = document.getElementById('bookingHistoryModal');
    const historyModalInstance = historyModalElement ? (bootstrap.Modal.getInstance(historyModalElement) || new bootstrap.Modal(historyModalElement)) : null;
    const historyListContainer = $('#bookingHistoryListContainer');
    const historyErrorAlert = $('#bookingHistoryErrorAlert');

    // ----- ปุ่มเปิด Modal ประวัติการจอง -----
    $('#viewBookingHistoryBtn').on('click', function (e) {
        e.preventDefault();

        if (!historyModalInstance) {
            console.error("Booking history modal element not found or Bootstrap Modal not initialized.");
            Swal.fire('เกิดข้อผิดพลาด', 'ไม่สามารถเปิดหน้าต่างประวัติการจองได้', 'error');
            return;
        }

        historyListContainer.html('<p class="text-center text-muted mt-3"><i class="fas fa-spinner fa-spin fa-2x"></i><br/>กำลังโหลดข้อมูล...</p>');
        historyErrorAlert.hide().text('');
        historyModalInstance.show();
        console.log("History modal shown. Requesting data...");

        $.ajax({
            url: '/Booking/GetUserBookings',
            type: 'GET',
            dataType: 'json',
            success: function (bookings) {
                console.log("AJAX Success. Bookings received:", bookings); // <<<--- DEBUG: ดูข้อมูลที่ได้รับทั้งหมด
                historyListContainer.empty();

                if (bookings && bookings.length > 0) {
                    const listGroup = $('<div class="list-group list-group-flush"></div>');
                    try {
                        bookings.forEach(booking => {
                            // *** ตรวจสอบ Property ชื่อ 'orderId' (camelCase) ***
                            const currentOrderId = (booking && typeof booking.orderId === 'number' && booking.orderId > 0) ? booking.orderId : 0;
                            console.log(`Processing booking object:`, booking, ` | Extracted Order ID: ${currentOrderId}`); // <<<--- DEBUG: ดู object และ ID ที่ดึงได้

                            // --- Format วันที่ (th-TH) ---
                            let pickupDateFormatted = 'N/A';
                            let returnDateFormatted = 'N/A';
                            try {
                                if (booking.pickupDate && typeof booking.pickupDate === 'string') {
                                    pickupDateFormatted = new Date(booking.pickupDate).toLocaleDateString('th-TH', { day: '2-digit', month: '2-digit', year: 'numeric' });
                                }
                                if (booking.returnDate && typeof booking.returnDate === 'string') {
                                    returnDateFormatted = new Date(booking.returnDate).toLocaleDateString('th-TH', { day: '2-digit', month: '2-digit', year: 'numeric' });
                                }
                            } catch (e) { console.error("Error formatting date for order " + currentOrderId + ":", e); }

                            // --- กำหนดสี Badge ตาม Status (ตัวอย่าง) ---
                            let statusBadgeClass = 'bg-secondary'; // Default
                            // *** อ่าน booking.status (camelCase) ***
                            if (booking.status === 'เสร็จสิ้น') { statusBadgeClass = 'bg-success'; }
                            else if (booking.status === 'กำลังดำเนินการ') { statusBadgeClass = 'bg-warning text-dark'; }
                            else if (booking.status === 'ยกเลิก') { statusBadgeClass = 'bg-danger'; }

                            // --- สร้าง HTML สำหรับแต่ละรายการ ---
                            const itemHtml = `
                                <div class="list-group-item px-0 py-3 booking-history-item" data-order-id="${currentOrderId}">
                                    <div class="row align-items-center g-2">
                                        <div class="col-auto text-center" style="width: 80px;">
                                             @* ใช้ booking.productImageUrl (camelCase) *@
                                            <img src="${booking.productImageUrl || '/images/placeholder.png'}" alt="${booking.productName}" class="img-fluid rounded" style="max-height: 50px;">
                                        </div>
                                        <div class="col">
                                             @* ใช้ booking.productName (camelCase) *@
                                            <h6 class="mb-1 fw-bold">${booking.productName || 'N/A'}</h6>
                                            <small class="text-muted d-block">เช่า: ${pickupDateFormatted} | คืน: ${returnDateFormatted}</small>
                                        </div>
                                        <div class="col-md-auto text-center col-sm-6 mt-2 mt-md-0">
                                            <span class="badge ${statusBadgeClass}">${booking.status || 'N/A'}</span>
                                            <br/><small class="text-muted">ID: ${currentOrderId}</small>
                                        </div>
                                        <div class="col-md-auto text-end col-sm-6 mt-2 mt-md-0 history-buttons">
                                            <a href="/Booking/Details/${currentOrderId}" target="_blank" class="btn btn-sm btn-outline-primary me-1 ${currentOrderId === 0 ? 'disabled' : ''}" title="ดูรายละเอียด"><i class="fas fa-eye"></i></a>
                                            <button class="btn btn-sm btn-outline-danger delete-booking-btn" data-order-id="${currentOrderId}" title="ยกเลิกรายการนี้" ${currentOrderId === 0 ? 'disabled' : ''}><i class="fas fa-trash-alt"></i></button>
                                        </div>
                                    </div>
                                </div>`;
                            listGroup.append(itemHtml);
                        }); // จบ forEach
                        historyListContainer.append(listGroup);
                    } catch (htmlError) {
                        console.error("Error creating booking list HTML:", htmlError);
                        historyErrorAlert.text('เกิดข้อผิดพลาดในการแสดงผลรายการ').show();
                        historyListContainer.html('<p class="text-center text-danger">ไม่สามารถแสดงรายการได้</p>');
                    }
                } else {
                    console.log("No bookings found.");
                    historyListContainer.html('<p class="text-center text-muted mt-3">ยังไม่มีประวัติการจอง</p>');
                }
            }, // จบ success
            error: function (jqXHR, textStatus, errorThrown) {
                console.error("AJAX Error fetching booking history:", textStatus, errorThrown, jqXHR);
                let msg = jqXHR.responseJSON?.message || errorThrown;
                historyErrorAlert.text('เกิดข้อผิดพลาดในการโหลดข้อมูล: ' + msg).show();
                historyListContainer.html('<p class="text-center text-danger">ไม่สามารถโหลดข้อมูลได้</p>');
            } // จบ error
        }); // จบ ajax
    }); // จบ #viewBookingHistoryBtn click

    // ----- ปุ่มลบรายการจอง (ใช้ Event Delegation + SweetAlert2) -----
    historyListContainer.on('click', '.delete-booking-btn', function () {
        const button = $(this);
        const orderId = button.data('order-id'); // Should be correct number if button is enabled
        const listItem = button.closest('.booking-history-item');
        const productName = listItem.find('h6').text() || 'รายการนี้';

        // No need to check orderId === 0 here again because button should be disabled if it was 0

        Swal.fire({
            title: 'ยืนยันการลบ',
            html: `คุณต้องการลบรายการจองสำหรับ<br><b>${productName}</b> (ID: ${orderId}) หรือไม่?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d',
            confirmButtonText: '<i class="fas fa-trash-alt me-1"></i> ใช่, ลบเลย!',
            cancelButtonText: 'ยกเลิก',
            showLoaderOnConfirm: true,
            preConfirm: () => {
                return new Promise((resolve) => {
                    console.log("Deletion confirmed for Order ID:", orderId);

                    let token = $('input[name="__RequestVerificationToken"]').val();
                    if (!token) { token = $('#profileForm input[name="__RequestVerificationToken"]').val(); }

                    if (!token) {
                        console.error("AntiForgeryToken not found!");
                        Swal.showValidationMessage('เกิดข้อผิดพลาด: ไม่พบ Token ความปลอดภัย');
                        resolve(false);
                        return;
                    }
                    console.log("Using AntiForgeryToken for delete:", token ? "Found" : "Not Found");

                    $.ajax({
                        url: '/Booking/DeleteBooking',
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ OrderId: parseInt(orderId) }),
                        headers: { 'RequestVerificationToken': token },
                        success: function (response) {
                            console.log("Delete response:", response);
                            if (response && response.success) {
                                resolve(response); // Resolve with success response
                            } else {
                                Swal.showValidationMessage(response?.message || 'เกิดข้อผิดพลาดในการลบ');
                                resolve(false); // Resolve indicating failure
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            console.error("Error deleting booking:", textStatus, errorThrown, jqXHR);
                            let msg = 'เกิดข้อผิดพลาดในการเชื่อมต่อ';
                            try { const res = JSON.parse(jqXHR.responseText); if (res?.message) msg = res.message; } catch (e) { }
                            Swal.showValidationMessage(msg);
                            resolve(false); // Resolve indicating failure
                        }
                    }); // จบ ajax
                }); // จบ Promise
            },
            allowOutsideClick: () => !Swal.isLoading()
        }).then((result) => {
            if (result.isConfirmed && result.value && result.value.success) {
                listItem.fadeOut(400, function () {
                    $(this).remove();
                    if (historyListContainer.find('.booking-history-item').length === 0) {
                        historyListContainer.html('<p class="text-center text-muted mt-3">ยังไม่มีประวัติการจอง</p>');
                    }
                });
                Swal.fire('ลบสำเร็จ!', result.value.message || `รายการจอง #${orderId} ถูกลบแล้ว`, 'success');
            } else if (!result.isDismissed) {
                // Error handled by Swal.showValidationMessage in preConfirm
                button.prop('disabled', false).html('<i class="fas fa-trash-alt"></i>'); // Restore button
            } else {
                console.log("Deletion cancelled for Order ID:", orderId);
            }
        }); // จบ Swal.fire().then()
    }); // จบ delete-booking-btn click

    // ... (โค้ด JS อื่นๆ ของ Profile Modal ถ้ามี) ...

}); // ปิด document ready