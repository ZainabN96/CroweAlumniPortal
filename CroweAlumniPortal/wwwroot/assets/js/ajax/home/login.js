var id = sessionStorage.getItem('id');
function togglePasswordVisibility() {
    var passwordInput = $('input[name="password"]');
    var eyeIcon = $('#eye');

    if (passwordInput.attr('type') === 'password') {
        passwordInput.attr('type', 'text');
        eyeIcon.removeClass('bi-eye-slash').addClass('bi-eye');
    } else {
        passwordInput.attr('type', 'password');
        eyeIcon.removeClass('bi-eye').addClass('bi-eye-slash');
    }
}
function pageRedirect() {
    window.location.href = "/Dashboard/Dashboard";
}

function Login() {
    //debugger;
    var data = {
        loginId: $("#LoginId").val(),
        password: $("#Password").val()
    };
    var ModelData = JSON.stringify(data);
    //console.log(ModelData);
    $.ajax({
        type: 'POST',
        url: "/api/user/login",
        dataType: "json",
        async: false,
        contentType: "application/json",
        data: ModelData,
        success: function (ok, textStatus, xhr) {
            //debugger;
            //console.log(ok);
            sessionStorage.setItem('id', ok.id);
            sessionStorage.setItem('firstName', ok.firstName);
            sessionStorage.setItem('lastName', ok.lastName);
            sessionStorage.setItem('userType', ok.userType);

            setTimeout(function () { pageRedirect(); }, 100);
        },
        error: function (xhr, status, error) {
            //debugger;
            $("#login").submit(function (e) {
                e.preventDefault();
            });
            $("#loader-wrapper").hide();
            swal("Error!", "Your login id or password is wrong", "error");
        }
    });
}