$(document).ready(function () {

    const historyModalElement = document.getElementById('bookingHistoryModal'); // หา Element ด้วย ID
    // ตรวจสอบว่า Element มีจริงก่อนสร้าง Instance
    const historyModalInstance = historyModalElement ? (bootstrap.Modal.getInstance(historyModalElement) || new bootstrap.Modal(historyModalElement)) : null;
    const historyListContainer = $('#bookingHistoryListContainer');
    const historyErrorAlert = $('#bookingHistoryErrorAlert'); // Alert ใน Modal ประวัติ

    const redemptionModalEl = document.getElementById('redemptionHistoryModal');
    const redemptionModalInstance = redemptionModalEl ? new bootstrap.Modal(redemptionModalEl) : null;
    const redemptionListContainer = $('#redemptionHistoryListContainer');
    const redemptionErrorAlert = $('#redemptionHistoryErrorAlert');

    const rentalModalEl = document.getElementById('rentalHistoryModal');
    const rentalModalInstance = rentalModalEl ? new bootstrap.Modal(rentalModalEl) : null;
    const rentalListContainer = $('#rentalHistoryListContainer');
    const rentalErrorAlert = $('#rentalHistoryErrorAlert');

    // ----- ปุ่มเปิด Modal ประวัติการจอง -----
    $('#viewBookingHistoryBtn').on('click', function (e) {
        e.preventDefault(); // ป้องกันการทำงานของ Button ปกติ

        if (!historyModalInstance) {
            console.error("Booking history modal element not found or Bootstrap Modal not initialized.");
            Swal.fire('เกิดข้อผิดพลาด', 'ไม่สามารถเปิดหน้าต่างประวัติการจองได้', 'error');
            return;
        }

        // แสดงสถานะ Loading และเคลียร์ข้อมูลเก่า/Error
        historyListContainer.html('<p class="text-center text-muted mt-3"><i class="fas fa-spinner fa-spin fa-2x"></i><br/>กำลังโหลดข้อมูล...</p>');
        historyErrorAlert.hide().text('');

        // เปิด Modal ประวัติ
        historyModalInstance.show();
        console.log("History modal shown. Requesting data..."); // DEBUG

        // AJAX GET ดึงประวัติ
        $.ajax({
            url: '/Booking/GetUserBookings', // URL Action GET
            type: 'GET',
            dataType: 'json',
            success: function (bookings) {
                console.log("AJAX Success. Bookings received:", bookings); // DEBUG
                historyListContainer.empty(); // เคลียร์ Loading

                if (bookings && bookings.length > 0) {
                    const listGroup = $('<div class="list-group list-group-flush"></div>');
                    try {
                        bookings.forEach(booking => {
                            // *** ตรวจสอบและใช้ OrderId อย่างระมัดระวัง ***
                            const currentOrderId = (booking && typeof booking.orderId === 'number' && booking.orderId > 0) ? booking.orderId : 0;
                            console.log(`Processing booking: ID=${currentOrderId}`); // DEBUG

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

                            // --- กำหนดสี Badge ตาม Status (ปรับปรุง) ---
                            let statusBadgeClass = 'bg-secondary'; // Default
                            // *** ใช้ค่า Status จาก Controller (น่าจะเป็น "รอชำระเงิน") ***
                            if (booking.status === 'รอชำระเงิน') {
                                statusBadgeClass = 'bg-warning text-dark'; // สีเหลือง
                            }
                            else if (booking.status === 'ยกเลิก') { // ถ้ามีสถานะยกเลิก
                                statusBadgeClass = 'bg-danger';
                            }
                            // เพิ่มเงื่อนไข Status อื่นๆ ถ้ามี

                            // --- สร้าง HTML ปุ่ม (แบบเดียวสำหรับรายการที่ยังไม่จ่าย) ---
                            const actionButtonsHtml = `
                                <a href="/Booking/Details?orderId=${currentOrderId}" target="_blank" class="btn btn-sm btn-outline-primary me-1 ${currentOrderId === 0 ? 'disabled' : ''}" title="ดูรายละเอียด"><i class="fas fa-eye"></i></a>
                                <button class="btn btn-sm btn-outline-danger delete-booking-btn" data-order-id="${currentOrderId}" title="ยกเลิกรายการนี้" ${currentOrderId === 0 ? 'disabled' : ''}><i class="fas fa-trash-alt"></i></button>
                            `;
                            // --- จบส่วนสร้างปุ่ม ---

                            // --- สร้าง HTML สำหรับแต่ละรายการ ---
                            const itemHtml = `
                                <div class="list-group-item px-0 py-3 booking-history-item" data-order-id="${currentOrderId}">
                                    <div class="row align-items-center g-2">
                                        <div class="col-auto text-center" style="width: 80px;">
                                            <img src="${booking.productImageUrl || '/images/placeholder.png'}" alt="${booking.productName}" class="img-fluid rounded" style="max-height: 50px; object-fit: contain;">
                                        </div>
                                        <div class="col">
                                            <h6 class="mb-1 fw-bold">${booking.productName || 'N/A'}</h6>
                                            <small class="text-muted d-block">เช่า: ${pickupDateFormatted} | คืน: ${returnDateFormatted}</small>
                                        </div>
                                        <div class="col-md-auto text-center col-sm-6 mt-2 mt-md-0">
                                            <span class="badge ${statusBadgeClass}">${booking.status || 'N/A'}</span>
                                            <br/><small class="text-muted">ID: ${currentOrderId}</small>
                                        </div>
                                        <div class="col-md-auto text-end col-sm-6 mt-2 mt-md-0 history-buttons">
                                            ${actionButtonsHtml}
                                        </div>
                                    </div>
                                </div>`;

                            console.log("Appending item:", currentOrderId); // DEBUG
                            listGroup.append(itemHtml);

                        }); // จบ forEach
                        console.log("Appending list group to container"); // DEBUG
                        historyListContainer.append(listGroup);
                    } catch (htmlError) {
                        console.error("Error creating booking list HTML:", htmlError); // DEBUG
                        historyErrorAlert.text('เกิดข้อผิดพลาดในการแสดงผลรายการ').show();
                        historyListContainer.html('<p class="text-center text-danger">ไม่สามารถแสดงรายการได้</p>');
                    }
                } else {
                    console.log("No bookings found."); // DEBUG
                    historyListContainer.html('<p class="text-center text-muted mt-3">ยังไม่มีประวัติการจอง (ที่ยังไม่ชำระเงิน)</p>'); // ปรับข้อความ
                }
            }, // จบ success
            error: function (jqXHR, textStatus, errorThrown) {
                console.error("AJAX Error fetching booking history:", textStatus, errorThrown, jqXHR); // DEBUG
                let msg = jqXHR.responseJSON?.message || errorThrown;
                historyErrorAlert.text('เกิดข้อผิดพลาดในการโหลดข้อมูล: ' + msg).show();
                historyListContainer.html('<p class="text-center text-danger">ไม่สามารถโหลดข้อมูลได้</p>');
            } // จบ error
        }); // จบ ajax
    }); // จบ #viewBookingHistoryBtn click

    $('#viewPointHistoryBtn').on('click', function (e) {
        e.preventDefault();

        if (!redemptionModalInstance) {
            console.error("Redemption history modal element not found or Bootstrap Modal not initialized.");
            Swal.fire('เกิดข้อผิดพลาด', 'ไม่สามารถเปิดหน้าต่างประวัติการแลกคะแนนได้', 'error');
            return;
        }

        // แสดง Loading และเคลียร์ข้อมูลเก่า
        redemptionListContainer.html('<p class="text-center text-muted mt-3"><i class="fas fa-spinner fa-spin fa-2x"></i><br/>กำลังโหลดข้อมูล...</p>');
        redemptionErrorAlert.hide().text('');

        // เปิด Modal
        redemptionModalInstance.show();

        // AJAX GET ดึงประวัติการแลก
        $.ajax({
            url: '/Redeem/GetUserRedemptions', // <<< URL Action ใหม่
            type: 'GET',
            dataType: 'json',
            success: function (redemptions) {
                console.log("AJAX Success. Redemptions received:", redemptions);
                redemptionListContainer.empty();

                if (redemptions && redemptions.length > 0) {
                    const listGroup = $('<div class="list-group list-group-flush"></div>');
                    try {
                        redemptions.forEach(item => {
                            // --- Format วันที่ ---
                            let redemptionDateFormatted = 'N/A';
                            try {
                                if (item.redemptionDate) {
                                    redemptionDateFormatted = new Date(item.redemptionDate).toLocaleString('th-TH', { dateStyle: 'medium', timeStyle: 'short' });
                                }
                            } catch (e) { console.error("Error formatting redemption date:", e); }

                            // --- กำหนดสี Badge Status ---
                            let statusBadgeClass = 'bg-secondary';
                            if (item.status === 'Processing') { statusBadgeClass = 'bg-info text-dark'; }
                            else if (item.status === 'Completed') { statusBadgeClass = 'bg-success'; }
                            else if (item.status === 'Shipped') { statusBadgeClass = 'bg-primary'; }
                            else if (item.status === 'Cancelled') { statusBadgeClass = 'bg-danger'; }

                            // --- สร้าง HTML รายการ ---
                            const itemHtml = `
                                <div class="list-group-item px-0 py-3 redemption-history-item" data-redemption-id="${item.redemptionId}">
                                    <div class="row align-items-center g-2">
                                        <div class="col-auto text-center" style="width: 80px;">
                                            <img src="${item.giveawayImageUrl || '/images/placeholder.png'}" alt="${item.giveawayName}" class="img-fluid rounded" style="max-height: 50px; object-fit: contain;">
                                        </div>
                                        <div class="col">
                                            <h6 class="mb-1 fw-bold">${item.giveawayName || 'N/A'}</h6>
                                            <small class="text-muted d-block">แลกเมื่อ: ${redemptionDateFormatted}</small>
                                        </div>
                                         <div class="col-md-auto text-center col-sm-6 mt-2 mt-md-0">
                                            <span class="badge ${statusBadgeClass}">${item.status || 'N/A'}</span>
                                             <br/><small class="text-warning">ใช้ ${item.pointCost || '?'} คะแนน</small>
                                        </div>
                                    </div>
                                </div>`;
                            listGroup.append(itemHtml);
                        });
                        redemptionListContainer.append(listGroup);
                    } catch (htmlError) {
                        console.error("Error creating redemption list HTML:", htmlError);
                        redemptionErrorAlert.text('เกิดข้อผิดพลาดในการแสดงผลรายการ').show();
                        redemptionListContainer.html('<p class="text-center text-danger">ไม่สามารถแสดงรายการได้</p>');
                    }
                } else {
                    console.log("No redemption history found.");
                    redemptionListContainer.html('<p class="text-center text-muted mt-3">ยังไม่มีประวัติการแลกคะแนน</p>');
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.error("AJAX Error fetching redemption history:", textStatus, errorThrown, jqXHR);
                let msg = jqXHR.responseJSON?.message || errorThrown;
                redemptionErrorAlert.text('เกิดข้อผิดพลาดในการโหลดข้อมูล: ' + msg).show();
                redemptionListContainer.html('<p class="text-center text-danger">ไม่สามารถโหลดข้อมูลได้</p>');
            }
        }); // จบ ajax
    });

    // ----- (เพิ่ม) ปุ่มเปิด Modal ประวัติการเช่า -----
    $('#viewRentalHistoryBtn').on('click', function (e) {
        e.preventDefault();

        if (!rentalModalInstance) {
            console.error("Rental history modal element not found or Bootstrap Modal not initialized.");
            Swal.fire('เกิดข้อผิดพลาด', 'ไม่สามารถเปิดหน้าต่างประวัติการเช่าได้', 'error');
            return;
        }

        // แสดง Loading และเคลียร์ข้อมูลเก่า
        rentalListContainer.html('<p class="text-center text-muted mt-3"><i class="fas fa-spinner fa-spin fa-2x"></i><br/>กำลังโหลดข้อมูล...</p>');
        rentalErrorAlert.hide().text('');

        // เปิด Modal
        rentalModalInstance.show();

        // AJAX GET ดึงประวัติการเช่า (ที่จ่ายแล้ว)
        $.ajax({
            url: '/Booking/GetRentalHistory', // <<< URL Action ใหม่
            type: 'GET',
            dataType: 'json',
            success: function (rentals) {
                console.log("AJAX Success. Rental History received:", rentals);
                rentalListContainer.empty();

                if (rentals && rentals.length > 0) {
                    const listGroup = $('<div class="list-group list-group-flush"></div>');
                    try {
                        rentals.forEach(item => {
                            // --- Format วันที่ ---
                            let pickupDateFormatted = 'N/A', returnDateFormatted = 'N/A', paymentDateFormatted = 'N/A';
                            try {
                                if (item.pickupDate) pickupDateFormatted = new Date(item.pickupDate).toLocaleDateString('th-TH', { day: '2-digit', month: 'short', year: 'numeric' });
                                if (item.returnDate) returnDateFormatted = new Date(item.returnDate).toLocaleDateString('th-TH', { day: '2-digit', month: 'short', year: 'numeric' });
                                if (item.paymentDate) paymentDateFormatted = new Date(item.paymentDate).toLocaleString('th-TH', { dateStyle: 'short', timeStyle: 'short' });
                            } catch (e) { console.error("Error formatting rental date:", e); }

                            // --- กำหนดสี Badge Status ---
                            let statusBadgeClass = 'bg-secondary';
                            if (item.paymentStatus === 'Completed') { statusBadgeClass = 'bg-success'; }
                            else if (item.paymentStatus === 'In progress') { statusBadgeClass = 'bg-info text-dark'; }

                            // --- สร้าง HTML รายการ ---
                            const itemHtml = `
                                <div class="list-group-item px-0 py-3 rental-history-item" data-order-id="${item.orderId}">
                                    <div class="row align-items-center g-2">
                                        <div class="col-auto text-center" style="width: 80px;">
                                            <img src="${item.productImageUrl || '/images/placeholder.png'}" alt="${item.productName}" class="img-fluid rounded" style="max-height: 50px; object-fit: contain;">
                                        </div>
                                        <div class="col">
                                            <h6 class="mb-1 fw-bold">${item.productName || 'N/A'} (ID: ${item.orderId})</h6>
                                            <small class="text-muted d-block">เช่า: ${pickupDateFormatted} - ${returnDateFormatted}</small>
                                            <small class="text-muted d-block">ชำระเมื่อ: ${paymentDateFormatted} (${item.amountPaid?.toLocaleString('th-TH', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} บ.)</small>
                                        </div>
                                         <div class="col-md-auto text-center col-sm-12 mt-2 mt-md-0">
                                            <span class="badge ${statusBadgeClass}">${item.paymentStatus || 'N/A'}</span>
                                        </div>
                                    </div>
                                </div>`;
                            listGroup.append(itemHtml);
                        });
                        rentalListContainer.append(listGroup);
                    } catch (htmlError) {
                        console.error("Error creating rental list HTML:", htmlError);
                        rentalErrorAlert.text('เกิดข้อผิดพลาดในการแสดงผลรายการ').show();
                        rentalListContainer.html('<p class="text-center text-danger">ไม่สามารถแสดงรายการได้</p>');
                    }
                } else {
                    console.log("No rental history found.");
                    rentalListContainer.html('<p class="text-center text-muted mt-3">ยังไม่มีประวัติการเช่า</p>');
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.error("AJAX Error fetching rental history:", textStatus, errorThrown, jqXHR);
                let msg = jqXHR.responseJSON?.message || errorThrown;
                rentalErrorAlert.text('เกิดข้อผิดพลาดในการโหลดข้อมูล: ' + msg).show();
                rentalListContainer.html('<p class="text-center text-danger">ไม่สามารถโหลดข้อมูลได้</p>');
            }
        }); // จบ ajax
    });

    // ----- ปุ่มลบรายการจอง (ใช้ Event Delegation + SweetAlert2) -----
    historyListContainer.on('click', '.delete-booking-btn', function () {
        const button = $(this);
        const orderId = button.data('order-id');
        const listItem = button.closest('.booking-history-item');
        const productName = listItem.find('h6').text() || 'รายการนี้';

        // ตรวจสอบ orderId อีกครั้งก่อนแสดง Popup
        if (!orderId || orderId === 0 || isNaN(parseInt(orderId))) {
            console.error("Invalid OrderId for delete:", orderId);
            Swal.fire('เกิดข้อผิดพลาด', 'ไม่พบ ID รายการที่ถูกต้อง', 'error');
            return;
        }

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
                                resolve(response);
                            } else {
                                Swal.showValidationMessage(response?.message || 'เกิดข้อผิดพลาดในการลบ');
                                resolve(false);
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            console.error("Error deleting booking:", textStatus, errorThrown, jqXHR);
                            let msg = 'เกิดข้อผิดพลาดในการเชื่อมต่อ';
                            try { const res = JSON.parse(jqXHR.responseText); if (res?.message) msg = res.message; } catch (e) { }
                            Swal.showValidationMessage(msg);
                            resolve(false);
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
                        historyListContainer.html('<p class="text-center text-muted mt-3">ยังไม่มีประวัติการจอง (ที่ยังไม่ชำระเงิน)</p>'); // ปรับข้อความ
                    }
                });
                Swal.fire('ลบสำเร็จ!', result.value.message || `รายการจอง #${orderId} ถูกลบแล้ว`, 'success');
            } else if (!result.isDismissed) {
                button.prop('disabled', false).html('<i class="fas fa-trash-alt"></i>');
            } else {
                console.log("Deletion cancelled for Order ID:", orderId);
            }
        }); // จบ Swal.fire().then()
    }); // จบ delete-booking-btn click

    // ... (โค้ด JS อื่นๆ ของ Profile Modal ถ้ามี) ...

}); // ปิด document ready