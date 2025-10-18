$(document).ready(function () {

    // Sets up all datepickers with Thai language and correct format

    $('.datepicker-register, .datepicker-profile').datepicker({
        format: "yyyy-mm-dd",
        language: currentCulture,
        autoclose: true,
        todayHighlight: true,
        changeMonth: true,
        changeYear: true,
        yearRange: "c-100:c" // Show 100 years back from the current year
    }).on('changeDate', function (e) {
        // สั่งให้ jQuery Validation ตรวจสอบความถูกต้องของช่องนี้ทันที
        $(this).valid();
    });


    // Makes the top floating alert disappear after 5 seconds
    if ($('.floating-alert-container .alert').length) {
        setTimeout(function () {
            $('.floating-alert-container .alert').fadeOut('slow');
        }, 5000);
    }

});