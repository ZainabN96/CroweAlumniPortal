$(document).ready(function () {
    setTokenFromUrl();
    bindResetFormSubmit();
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

function getQueryParam(name) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(name);
}

function setTokenFromUrl() {
    const token = getQueryParam("token");
    $("#token").val(token);

    if (!token) {
        swal("Invalid Link", "Reset token is missing.", "error");
    }
}

function validateResetPasswordForm() {
    clearAllFieldErrors();
    let isValid = true;

    if (!v("#token")) {
        swal("Invalid Link", "Reset token is missing.", "error");
        return false;
    }

    if (!v("#newPassword")) {
        showFieldError("#newPassword", "New password is required.");
        isValid = false;
    } else if (v("#newPassword").length < 6) {
        showFieldError("#newPassword", "Password must be at least 6 characters.");
        isValid = false;
    }

    if (!v("#confirmPassword")) {
        showFieldError("#confirmPassword", "Confirm password is required.");
        isValid = false;
    } else if (v("#newPassword") !== v("#confirmPassword")) {
        showFieldError("#confirmPassword", "Passwords do not match.");
        isValid = false;
    }

    return isValid;
}

function bindResetFormSubmit() {
    $("#resetPasswordForm").off("submit.rp").on("submit.rp", function (e) {
        e.preventDefault();

        if (!validateResetPasswordForm()) {
            swal("Validation Failed", "Please correct the highlighted fields.", "error");
            return;
        }

        const requestData = {
            Token: v("#token"),
            NewPassword: v("#newPassword")
        };

        $.ajax({
            url: "/api/user/reset-password",
            type: "POST",
            data: JSON.stringify(requestData),
            contentType: "application/json",
            success: function (res) {
                swal("Success", res.message || "Password reset successfully.", "success")
                    .then(() => {
                        window.location.href = "/Home/Login";
                    });
            },
            error: function (xhr) {
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