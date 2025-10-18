$(document).ready(function () {

    // Handles clicking the 'Log in', 'Sign up', and Profile buttons

    $('.show-auth-modal').on('click', function (e) {
        e.preventDefault();
        var formType = $(this).data('form');

        if (formType === 'login') {
            $('#loginAlert').hide().text(''); // Hide and clear old alerts
            $('#loginModal').modal('show');
        } else if (formType === 'signup') {
            $('#registerAlert').hide().text(''); // Hide and clear old alerts
            $('#registerModal').modal('show');
        }
    });

    $('#showProfileModalBtn').on('click', function (e) {
        e.preventDefault();
        $('#profileAlert').hide().text(''); // Hide and clear old alerts

        $('#changePasswordSection').removeClass('show');
        $('#profileForm #CurrentPassword').val('');
        $('#profileForm #NewPassword').val('');
        $('#profileForm #ConfirmNewPassword').val('');

        // Fetch the latest profile data before showing the modal
        $.ajax({
            type: "GET",
            url: "/Account/GetProfile",
            success: function (data) {
                $('#profileForm #UserName').val(data.userName);
                $('#profileForm #Email').val(data.email);
                $('.datepicker-profile').val(data.dateOfBirth);
                $('#profileModal').modal('show');
            },
            error: function () {
                alert("Error: Could not load your profile data.");
            }
        });
    });


    // This function handles Login, Register, and Profile forms

    function handleFormSubmit(form) {
        // Unobtrusive validation will run automatically before this
        if (!form.valid()) {
            return; // Stop if client-side validation fails
        }

        var alertBoxId = form.attr('id').replace('Form', 'Alert');
        var alertBox = $('#' + alertBoxId);
        alertBox.hide().text(''); // Hide and clear previous messages

        // We need FormData to handle file uploads (for Register/Profile)
        var formData = new FormData(form[0]);

        $.ajax({
            url: form.attr('action'),
            type: form.attr('method'),
            data: formData,
            processData: false, // Required for FormData
            contentType: false, // Required for FormData
            success: function (response) {
                // Clear any old validation errors shown under the input fields
                form.find(".text-danger").text("");

                if (response.success) {
                    // Show success message inside the modal
                    alertBox.removeClass('alert-danger').addClass('alert-success').text(response.message).show();
                    // Wait 1.5 seconds, then redirect
                    setTimeout(function () {
                        window.location.href = response.redirectUrl || '/';
                    }, 1500);
                } else {
                    // Handle server-side validation errors
                    if (response.message) {
                        // For general errors (e.g., "Incorrect password")
                        alertBox.removeClass('alert-success').addClass('alert-danger').text(response.message).show();
                    }
                    if (response.errors) {
                        // For field-specific errors (e.g., "Email is already taken")
                        $.each(response.errors, function (key, value) {
                            // Find the span for this field and show the error message
                            var field = form.find('[name="' + key + '"]');
                            var validationSpan = field.closest('.form-group, .row').find('.text-danger[data-valmsg-for="' + key + '"]');
                            if (validationSpan) {
                                validationSpan.text(value[0]);
                            }
                        });
                    }
                }
            },
            error: function () {
                alertBox.removeClass('alert-success').addClass('alert-danger').text('An unexpected server error occurred.').show();
            }
        });
    }

    // Attach the submit handler to all three forms
    $('#loginForm, #registerForm, #profileForm').on('submit', function (e) {
        e.preventDefault();
        handleFormSubmit($(this));
    });
});