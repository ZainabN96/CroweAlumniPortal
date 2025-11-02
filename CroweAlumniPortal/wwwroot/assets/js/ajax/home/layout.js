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
            ////debugger;
            sessionStorage.clear();
            window.location.href = "/Home/Login";
        },
        error: function (xhr, status, error) {
            console.error(error);
            swal('Error', error, "error")
        }
    });
}

//
//function timeAgo(iso) {
//    const then = new Date(iso);
//    const now = new Date();
//    const diff = (now - then) / 1000;
//    if (diff < 60) return "Just now";
//    const m = Math.floor(diff / 60);
//    if (m < 60) return `${m} min ago`;
//    const h = Math.floor(m / 3600);
//    if (h < 24) return `${h} h ago`;
//    return then.toLocaleDateString();
//}

//function renderNotif(n) {
//    return `
//      <div class="notification-item d-flex">
//          <img src="${n.profilePicturePath || '/assets/img/person.png'}" alt="profile" style="width:32px;height:32px;object-fit:cover;" class="rounded-circle me-2">
//          <div>
//              <a href="${n.url}"> ${n.message}<br>
//              <small>${timeAgo(n.createdOn)}</small></a>
//          </div>
//      </div>`;
//}

//function loadNotifications() {
//    const userId = sessionStorage.getItem("id");
//    $.get(`/api/notification/user/${userId}/unread`, { take: 5 })
//        .done(list => {
//            if (!list || !list.length) {
//                $("#notifItems").html(`<div class="text-muted small p-2">No new notifications</div>`);
//                return;
//            }
//            const html = list.map(renderNotif).join("");
//            $("#notifItems").html(html);
//        })
//        .fail(() => {
//            $("#notifItems").html(`<div class="text-danger small p-2">Failed to load notifications</div>`);
//        });
//}

//$(function () {
//    loadNotifications();
//});
