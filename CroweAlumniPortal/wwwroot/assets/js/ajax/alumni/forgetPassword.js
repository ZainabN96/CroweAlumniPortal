$(document).ready(function () {
    bindFormSubmit();
});

function v(selector) {
    return $(selector).val()?.trim();
}

function clearAllFieldErrors() {
    $(".field-error").remove();
    $(".is-invalid").removeClass("is-invalid");
}

function showFieldError(selector, message) {
    $(selector).addClass("is-invalid");

    if ($(selector).next(".field-error").length === 0) {
        $(selector).after(`<small class="field-error text-danger">${message}</small>`);
    }
}

function validateRegistrationForm() {
    clearAllFieldErrors();
    let isValid = true;

    if (!v("#email")) {
        showFieldError("#email", "Email address is required.");
        isValid = false;
    } else {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(v("#email"))) {
            showFieldError("#email", "Please enter a valid email address.");
            isValid = false;
        }
    }

    return isValid;
}

function bindFormSubmit() {
    $("#forgetPasswordForm").off("submit.fp").on("submit.fp", function (e) {
        e.preventDefault();

        if (!validateRegistrationForm()) {
            swal("Validation Failed", "Please correct the highlighted fields.", "error");
            return;
        }

        const requestData = {
            EmailAddress: v("#email")
        };

        $.ajax({
            url: "/api/user/forgot-password",
            type: "POST",
            data: JSON.stringify(requestData),
            contentType: "application/json",
            success: function (res) {
                debugger;
                swal("Success", res.message || "Reset password email sent successfully.", "success");
                $("#forgetPasswordForm")[0].reset();
            },
            error: function (xhr) {
                debugger;
                let msg = "Something went wrong.";

                if (xhr.responseJSON) {
                    msg = xhr.responseJSON.errorMessage || xhr.responseJSON.message || msg;
                } else if (xhr.responseText) {
                    msg = xhr.responseText;
                }

                swal("Failed", msg, "error");
            }
        });
    });
}