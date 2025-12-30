token = sessionStorage.getItem('token');

$(document).ready(function () {
    //debugger;
    var ID = sessionStorage.getItem('id');
    if (ID == null) {
        window.location.href = "/Home/Login"
    }
});

function Logout() {
    ////debugger;
    $.ajax({
        type: 'POST',
        url: "/api/user/logout",
        headers: {
            'Authorization': 'Bearer ' + token
        },
        success: function () {
            debugger;
            //sessionStorage.clear();
            window.location.href = "/Home/Login";
        },
        error: function (xhr, status, error) {
            console.error(error);
            swal('Error', error, "error")
        }
    });
}
