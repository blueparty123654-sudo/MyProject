$(document).ready(function () {
    console.log("reviews.js: Document Ready!");

    const reviewPageContainer = $('.review-container');
    console.log("reviews.js: reviewPageContainer found:", reviewPageContainer.length);

    if (reviewPageContainer.length === 0) {
        console.log("reviews.js: No review container found. Exiting.");
        return;
    }

    // --- Variables ---
    const reviewForm = reviewPageContainer.find('#reviewForm');
    const reviewList = reviewPageContainer.find('#reviewList');
    const btnLoadMore = reviewPageContainer.find('#btnLoadMoreReviews');
    const filterProduct = reviewPageContainer.find('#filterProduct');
    const filterBranch = reviewPageContainer.find('#filterBranch');
    const filterRating = reviewPageContainer.find('#filterRating');
    const resetFiltersBtn = reviewPageContainer.find('#resetFilters');
    let reviewsLoaded = reviewPageContainer.find('.review-card').length;
    console.log("reviews.js: reviewForm found:", reviewForm.length, "Initial reviews:", reviewsLoaded);


    // --- Function to Load/Reload Reviews via AJAX ---
    function loadReviews(isLoadMore = false) {
        console.log("reviews.js: loadReviews called. isLoadMore:", isLoadMore, "Current reviewsLoaded:", reviewsLoaded);
        if (!isLoadMore) {
            reviewsLoaded = 0;
            reviewList.empty();
            // Reset button state only if it exists
            if (btnLoadMore.length > 0) {
                btnLoadMore.text('โหลดรีวิวเพิ่มเติม').prop('disabled', false).show();
            }
        }
        const filters = {
            skip: reviewsLoaded,
            filterProductId: filterProduct.val() || null,
            filterBranchId: filterBranch.val() || null,
            filterRating: filterRating.val() || null
        };
        if (!isLoadMore) reviewList.html('<p style="text-align:center; padding: 2rem; color:#aaa;">กำลังโหลดรีวิว...</p>');
        else if (btnLoadMore.length > 0) btnLoadMore.text('กำลังโหลด...').prop('disabled', true);

        $.ajax({
            url: '/Review/LoadMoreReviews',
            type: 'GET',
            data: filters,
            success: function (response) {
                console.log("reviews.js: AJAX success. Response:", response);
                if (!isLoadMore) reviewList.empty();

                if (response.reviews && response.reviews.length > 0) {
                    response.reviews.forEach(function (review) {
                        var newReviewHtml = createReviewCardHtml(review);
                        reviewList.append(newReviewHtml);
                        // Ensure slideDown targets the correct element
                        reviewPageContainer.find('#review-' + review.reviewId).slideDown();
                    });
                    reviewsLoaded += response.reviews.length;

                    // Update Load More button state only if it exists
                    if (btnLoadMore.length > 0) {
                        if (response.hasMore) { btnLoadMore.text('โหลดรีวิวเพิ่มเติม').prop('disabled', false).show(); }
                        else { btnLoadMore.text('แสดงทั้งหมดแล้ว').prop('disabled', true).fadeOut(500); }
                    }
                } else {
                    if (!isLoadMore) { reviewList.html('<p style="text-align:center; padding: 2rem; color:#aaa;">-- ไม่พบรีวิวที่ตรงกับเงื่อนไข --</p>'); }
                    // Hide Load More button if no more reviews
                    if (btnLoadMore.length > 0) {
                        btnLoadMore.text('แสดงทั้งหมดแล้ว').prop('disabled', true).fadeOut(500);
                    }
                }
            },
            error: function () {
                console.error("reviews.js: AJAX error loading reviews.");
                alert('เกิดข้อผิดพลาดในการโหลดรีวิว');
                if (btnLoadMore.length > 0) {
                    btnLoadMore.text('โหลดรีวิวเพิ่มเติม').prop('disabled', false).show();
                }
                if (!isLoadMore) reviewList.html('<p style="text-align:center; padding: 2rem; color:red;">เกิดข้อผิดพลาดในการโหลด</p>');
            }
        });
    }

    // --- Function to Create Review Card HTML ---
    function createReviewCardHtml(review) {
        const userName = review.userName || "Anonymous"; const postedAgo = review.postedAgo || ""; const rating = review.rating || 0;
        const productName = review.productName || null; const branchName = review.branchName || null; const comment = review.comment || "";
        const reviewId = review.reviewId || 0; const isOwner = review.isOwner || false;
        let html = `<div class="review-card" id="review-${reviewId}" style="display:none;"><div class="review-header"><span class="review-author">${userName}</span><div class="review-header-actions"><span class="review-time">${postedAgo}</span>${isOwner ? `<a href="#" class="btn-delete-review" data-id="${reviewId}" title="ลบรีวิวนี้"><i class="fas fa-trash-alt"></i></a>` : ''}</div></div><div class="review-body"><div class="review-rating">`;
        for (let i = 1; i <= 5; i++) { html += `<span class="star ${i <= rating ? "filled" : ""}">★</span>`; } html += `</div>`;
        if (productName || branchName) { html += `<div class="review-context">`; if (productName) { html += `<span><strong>รถ:</strong> ${productName}</span>`; } if (branchName) { html += `<span><strong>สาขา:</strong> ${branchName}</span>`; } html += `</div>`; }
        html += `<p class="review-comment">"${comment}"</p></div></div>`; return html;
    }


    // --- Event Listeners ---
    console.log("reviews.js: Attaching filter/reset/loadmore handlers");
    filterProduct.on('change', function () { loadReviews(); });
    filterBranch.on('change', function () { loadReviews(); });
    filterRating.on('change', function () { loadReviews(); });
    resetFiltersBtn.on('click', function () { filterProduct.val(''); filterBranch.val(''); filterRating.val(''); loadReviews(); });
    // Attach Load More only if the button exists
    if (btnLoadMore.length > 0) {
        btnLoadMore.on('click', function () { loadReviews(true); });
    }


    // --- Logic specific to the Review Submission Form ---
    if (reviewForm.length) {
        console.log("reviews.js: Inside if (reviewForm.length). Attaching handlers...");

        const stars = reviewForm.find('.star-wrapper i');
        const ratingInput = reviewForm.find('#Rating');
        console.log("reviews.js: Found stars inside form:", stars.length);
        console.log("reviews.js: Found ratingInput inside form:", ratingInput.length);

        // (โค้ดคลิกดาว - ฉบับสมบูรณ์)
        stars.off('click').on('click', function () {
            console.log("reviews.js: Star clicked!");
            const value = $(this).data('value');
            ratingInput.val(value); // อัปเดตค่าใน input ที่ซ่อนอยู่
            stars.removeClass('selected'); // ลบคลาส selected ออกจากดาวทุกดวง
            // $(this).addClass('selected'); // เพิ่มคลาส selected ให้ดาวที่ถูกคลิก
            // เปลี่ยนเป็น เพิ่ม selected ให้ตัวเองและดาวก่อนหน้า (เพื่อให้ CSS ทำงานถูก)
            for (let i = 0; i < value; i++) {
                $(stars[4 - i]).addClass('selected'); // Index معكوس wegen rtl direction
            }


            // หา Alert Box ภายใน Form เท่านั้น
            const alertBox = reviewForm.find('#reviewFormAlert');
            if (alertBox.length > 0) {
                alertBox.hide().text(''); // ซ่อนถ้ามีการคลิกดาวใหม่
            }
        });
        console.log("reviews.js: Star click handler attached.");

        // (โค้ด Submit - ฉบับสมบูรณ์)
        reviewForm.off('submit').on('submit', function (e) {
            console.log("reviews.js: Submit button clicked!");
            e.preventDefault();
            var form = $(this);
            // หา Alert Box ภายใน Form เท่านั้น
            var alertBox = reviewForm.find('#reviewFormAlert');
            // สร้าง Alert Box ถ้ายังไม่มี
            if (alertBox.length === 0) {
                alertBox = $('<div id="reviewFormAlert" class="alert mt-3" role="alert" style="display: none;"></div>');
                form.find('.btn-submit-review').before(alertBox);
            }
            alertBox.hide().removeClass('alert-danger alert-success').text(''); // Reset state

            // Validation ดาว
            if (parseInt(ratingInput.val()) <= 0) {
                alertBox.addClass('alert-danger').text('กรุณาให้คะแนนโดยการคลิกดาว').show();
                return;
            }

            // (Optional) Re-enable jQuery validation if needed and configured
            // if (typeof form.valid === 'function' && !form.valid()) {
            //     alertBox.addClass('alert-danger').text('กรุณากรอกข้อมูลให้ครบถ้วนและถูกต้อง').show();
            //     return;
            // }

            var formData = new FormData(this);
            var submitButton = form.find('.btn-submit-review'); // Find submit button
            submitButton.prop('disabled', true).text('กำลังส่ง...'); // Disable button

            $.ajax({
                url: '/Review/SubmitReview',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                success: function (response) {
                    // ล้าง Error เก่าใต้ Input (ถ้ามี)
                    form.find(".text-danger").text("");

                    if (response.success) {
                        alertBox.removeClass('alert-danger').addClass('alert-success').text(response.message).show();
                        // โหลดรีวิวใหม่ทั้งหมดเพื่อให้รายการอัปเดตตาม Filter ปัจจุบัน
                        loadReviews();
                        form[0].reset(); // ล้างฟอร์ม
                        stars.removeClass('selected'); // รีเซ็ตดาว
                        ratingInput.val(0); // รีเซ็ตค่า rating ที่ซ่อนอยู่
                    } else {
                        // Handle server-side validation errors or general errors
                        alertBox.removeClass('alert-success').addClass('alert-danger');
                        if (response.message) {
                            alertBox.text(response.message).show();
                        }
                        if (response.errors) {
                            let errorText = alertBox.text(); // Get current text (if any)
                            $.each(response.errors, function (key, value) {
                                // Try to display error below the field
                                var fieldName = key;
                                var validationSpan = form.find('.text-danger[data-valmsg-for="' + fieldName + '"]');
                                if (validationSpan.length > 0) {
                                    validationSpan.text(value[0]);
                                } else {
                                    // Otherwise, append to the main alert box
                                    errorText += (errorText ? "\n" : "") + value[0];
                                }
                            });
                            alertBox.text(errorText).show(); // Show combined errors
                        }
                        if (!response.message && !response.errors) {
                            // Fallback generic error
                            alertBox.text('เกิดข้อผิดพลาดบางอย่าง').show();
                        }
                    }
                },
                error: function (jqXHR) {
                    console.error("reviews.js: AJAX error submitting review.", jqXHR.status, jqXHR.responseText);
                    alertBox.removeClass('alert-success').addClass('alert-danger');
                    if (jqXHR.status === 401) {
                        alertBox.text('กรุณาเข้าสู่ระบบก่อนทำการรีวิว').show();
                    } else {
                        alertBox.text('เกิดข้อผิดพลาดในการส่งรีวิว กรุณาลองใหม่อีกครั้ง').show();
                    }
                },
                complete: function () {
                    // Re-enable button regardless of success/error
                    submitButton.prop('disabled', false).text('ส่งรีวิว');
                }
            });
        });
        console.log("reviews.js: Submit handler attached.");

    } else {
        console.log("reviews.js: reviewForm NOT found. Star/Submit handlers NOT attached.");
    }

    // --- Delete Review Logic ---
    console.log("reviews.js: Attaching delete handler to review list");
    // Ensure reviewList variable is used correctly for event delegation
    reviewList.off('click', '.btn-delete-review').on('click', '.btn-delete-review', function (e) {
        console.log("reviews.js: Delete button clicked!");
        e.preventDefault();
        if (!confirm('ยืนยันการลบรีวิวนี้หรือไม่?')) { return; }

        var button = $(this);
        var reviewId = button.data('id');
        // Find review card relative to the button if needed, or use the container
        var reviewCard = reviewPageContainer.find('#review-' + reviewId);
        var token = $('input[name="__RequestVerificationToken"]').val();

        // Optional: Add visual feedback during delete
        button.find('i').removeClass('fa-trash-alt').addClass('fa-spinner fa-spin'); // Change icon to spinner
        button.css('pointer-events', 'none'); // Disable further clicks

        $.ajax({
            url: '/Review/DeleteReview',
            type: 'POST',
            data: { reviewId: reviewId },
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                if (response.success) {
                    reviewCard.slideUp(function () {
                        $(this).remove();
                        reviewsLoaded--; // Decrement count
                        // Optional: Check if load more button needs to reappear/change text
                    });
                } else {
                    alert('เกิดข้อผิดพลาด: ' + response.message);
                    // Restore button if delete failed
                    button.find('i').removeClass('fa-spinner fa-spin').addClass('fa-trash-alt');
                    button.css('pointer-events', '');
                }
            },
            error: function () {
                console.error("reviews.js: AJAX error deleting review.");
                alert('เกิดข้อผิดพลาดในการเชื่อมต่อ ไม่สามารถลบรีวิวได้');
                // Restore button on connection error
                button.find('i').removeClass('fa-spinner fa-spin').addClass('fa-trash-alt');
                button.css('pointer-events', '');
            }
        });
    });

});