$(document).ready(function () {

    // Review Page Logic (New Version for Font Awesome)
    if ($('#reviewForm').length) { // รันโค้ดนี้เฉพาะเมื่ออยู่ในหน้ารีวิว

        const stars = $('.star-wrapper i');
        const ratingInput = $('#Rating');

        // Logic สำหรับ Click ดาว (เหมือนเดิม)
        stars.on('click', function () {
            const value = $(this).data('value');
            ratingInput.val(value);
            stars.removeClass('selected');
            $(this).addClass('selected');
        });

        // Logic Submit Review
        $('#reviewForm').on('submit', function (e) {
            e.preventDefault();
            var form = $(this);
            var alertBoxId = 'reviewFormAlert';
            var alertBox = $('#' + alertBoxId);
            if (alertBox.length === 0) {
                alertBox = $('<div id="' + alertBoxId + '" class="alert mt-3" role="alert" style="display: none;"></div>');
                form.find('.btn-submit-review').before(alertBox);
            }
            alertBox.hide().text('');

            // (สำคัญ!) ตรวจสอบ jQuery Validation ก่อน
            // เราอาจจะต้องตรวจสอบให้แน่ใจว่า Validation ถูกโหลดมาในหน้านี้
            // if (typeof form.valid === 'function' && !form.valid()) {
            //     alertBox.removeClass('alert-success').addClass('alert-danger').text('กรุณากรอกข้อมูลให้ครบถ้วนและถูกต้อง').show();
            //     return;
            // }

            // ตรวจสอบดาว
            if (parseInt(ratingInput.val()) <= 0) {
                alertBox.removeClass('alert-success').addClass('alert-danger').text('กรุณาให้คะแนนโดยการคลิกดาว').show();
                return;
            }

            var formData = new FormData(this);

            $.ajax({
                url: '/Review/SubmitReview', // 👈 (แก้ไข) เปลี่ยนเป็น /Review/SubmitReview
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    form.find(".text-danger").text("");
                    if (response.success) {
                        alertBox.removeClass('alert-danger').addClass('alert-success').text(response.message).show();
                        if (response.newReview) {
                            // ... (โค้ดสร้าง HTML เหมือนเดิม) ...
                            var newReviewHtml = `
                                <div class="review-card" id="review-${response.newReview.reviewId}" style="display:none;">
                                    <div class="review-header">
                                        <span class="review-author">${response.newReview.userName}</span>
                                        <div class="review-header-actions">
                                            <span class="review-time">${response.newReview.postedAgo}</span>
                                            ${response.newReview.isOwner ? // (เพิ่ม) เช็ค isOwner ตอนสร้าง HTML ด้วย
                                    `<a href="#" class="btn-delete-review" data-id="${response.newReview.reviewId}" title="ลบรีวิวนี้">
                                                    <i class="fas fa-trash-alt"></i>
                                                </a>` : ''}
                                        </div>
                                    </div>
                                    <div class="review-body">
                                        <div class="review-rating">`;
                            for (let i = 1; i <= 5; i++) {
                                newReviewHtml += `<span class="star ${i <= response.newReview.rating ? "filled" : ""}">★</span>`;
                            }
                            newReviewHtml += `</div>`;
                            if (response.newReview.productName || response.newReview.branchName) {
                                newReviewHtml += `<div class="review-context">`;
                                if (response.newReview.productName) {
                                    newReviewHtml += `<span><strong>รถ:</strong> ${response.newReview.productName}</span>`;
                                }
                                if (response.newReview.branchName) {
                                    newReviewHtml += `<span><strong>สาขา:</strong> ${response.newReview.branchName}</span>`;
                                }
                                newReviewHtml += `</div>`;
                            }
                            newReviewHtml += `<p class="review-comment">"${response.newReview.comment}"</p>
                                    </div>
                                </div>`;

                            $('#reviewList').prepend(newReviewHtml);
                            $('#reviewList .review-card:first-child').slideDown();
                        }
                        form[0].reset();
                        stars.removeClass('selected');
                        ratingInput.val(0);
                    } else {
                        // ... (โค้ดจัดการ Error เหมือนเดิม) ...
                        if (response.message) {
                            alertBox.removeClass('alert-success').addClass('alert-danger').text(response.message).show();
                        }
                        if (response.errors) {
                            $.each(response.errors, function (key, value) {
                                var fieldName = key;
                                var validationSpan = form.find('.text-danger[data-valmsg-for="' + fieldName + '"]');
                                // ... (โค้ดแสดง Error ใน Span หรือ Alert Box เหมือนเดิม) ...
                                if (validationSpan.length > 0) {
                                    validationSpan.text(value[0]);
                                } else {
                                    var currentText = alertBox.text();
                                    alertBox.removeClass('alert-success').addClass('alert-danger')
                                        .text(currentText + (currentText ? "\n" : "") + value[0]).show();
                                }
                            });
                        }
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    // ... (โค้ดจัดการ Error 401 และ Error ทั่วไป เหมือนเดิม) ...
                    if (jqXHR.status === 401) {
                        alertBox.removeClass('alert-success').addClass('alert-danger').text('กรุณาเข้าสู่ระบบก่อนทำการรีวิว').show();
                    } else {
                        alertBox.removeClass('alert-success').addClass('alert-danger').text('เกิดข้อผิดพลาดในการส่งรีวิว กรุณาลองใหม่อีกครั้ง').show();
                    }
                }
            });
        });
    } // <-- ปิด if ($('#reviewForm').length)

    // Delete Review Logic
    $('#reviewList').on('click', '.btn-delete-review', function (e) {
        e.preventDefault();
        if (!confirm('คุณแน่ใจหรือไม่ว่าต้องการลบรีวิวนี้? (การกระทำนี้ไม่สามารถย้อนกลับได้)')) { return; }

        var button = $(this);
        var reviewId = button.data('id');
        var reviewCard = $('#review-' + reviewId);
        var token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: '/Review/DeleteReview', // 👈 (แก้ไข) เปลี่ยนเป็น /Review/DeleteReview
            type: 'POST',
            data: { reviewId: reviewId },
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                if (response.success) {
                    reviewCard.slideUp(function () { $(this).remove(); });
                } else {
                    alert('เกิดข้อผิดพลาด: ' + response.message);
                }
            },
            error: function () {
                alert('เกิดข้อผิดพลาดในการเชื่อมต่อ ไม่สามารถลบรีวิวได้');
            }
        });
    });

    // Load More Reviews Logic
    let reviewsLoaded = $('.review-card').length;
    $('#btnLoadMoreReviews').on('click', function () {
        var button = $(this);
        button.text('กำลังโหลด...').prop('disabled', true);

        $.ajax({
            url: '/Review/LoadMoreReviews', // 👈 (แก้ไข) เปลี่ยนเป็น /Review/LoadMoreReviews
            type: 'GET',
            data: { skip: reviewsLoaded },
            success: function (response) {
                if (response.reviews && response.reviews.length > 0) {
                    response.reviews.forEach(function (review) {
                        // ... (โค้ดสร้าง HTML เหมือนเดิม) ...
                        var newReviewHtml = `
                            <div class="review-card" id="review-${review.reviewId}" style="display:none;">
                                <div class="review-header">
                                    <span class="review-author">${review.userName}</span>
                                    <div class="review-header-actions">
                                        <span class="review-time">${review.postedAgo}</span>
                                        ${review.isOwner ?
                                `<a href="#" class="btn-delete-review" data-id="${review.reviewId}" title="ลบรีวิวนี้">
                                                <i class="fas fa-trash-alt"></i>
                                            </a>` : ''}
                                    </div>
                                </div>
                                <div class="review-body">
                                    <div class="review-rating">`;
                        for (let i = 1; i <= 5; i++) {
                            newReviewHtml += `<span class="star ${i <= review.rating ? "filled" : ""}">★</span>`;
                        }
                        newReviewHtml += `</div>`;
                        if (review.productName || review.branchName) {
                            newReviewHtml += `<div class="review-context">`;
                            if (review.productName) {
                                newReviewHtml += `<span><strong>รถ:</strong> ${review.productName}</span>`;
                            }
                            if (review.branchName) {
                                newReviewHtml += `<span><strong>สาขา:</strong> ${review.branchName}</span>`;
                            }
                            newReviewHtml += `</div>`;
                        }
                        newReviewHtml += `<p class="review-comment">"${review.comment}"</p>
                                </div>
                            </div>`;

                        $('#reviewList').append(newReviewHtml);
                        $('#review-' + review.reviewId).slideDown();
                    });

                    reviewsLoaded += response.reviews.length;

                    if (response.hasMore) {
                        button.text('โหลดรีวิวเพิ่มเติม').prop('disabled', false);
                    } else {
                        button.text('แสดงรีวิวทั้งหมดแล้ว').prop('disabled', true);
                        button.fadeOut(500);
                    }
                } else {
                    button.text('แสดงรีวิวทั้งหมดแล้ว').prop('disabled', true);
                    button.fadeOut(500);
                }
            },
            error: function () {
                alert('เกิดข้อผิดพลาดในการโหลดรีวิว');
                button.text('โหลดรีวิวเพิ่มเติม').prop('disabled', false);
            }
        });
    });

}); 