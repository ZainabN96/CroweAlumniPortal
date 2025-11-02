var today = new Date();
var curHr = today.getHours();

if (curHr >= 0 && curHr < 4) {
    document.getElementById("greeting").innerHTML = 'Good Night';
} else if (curHr >= 4 && curHr < 12) {
    document.getElementById("greeting").innerHTML = 'Good Morning';
} else if (curHr >= 12 && curHr < 16) {
    document.getElementById("greeting").innerHTML = 'Good Afternoon';
} else {
    document.getElementById("greeting").innerHTML = 'Good Evening';
}

// Time
function startTime() {
    var today = new Date();
    var h = today.getHours();
    var m = today.getMinutes();
    var s = today.getSeconds();
    var ampm = h >= 12 ? 'PM' : 'AM';
    h = h % 12;
    h = h ? h : 12;
    m = checkTime(m);
    s = checkTime(s);
    document.getElementById('txt').innerHTML =
        h + ":" + m + ":" + s + ' ' + ampm;
    var t = setTimeout(startTime, 1000); 
}

function checkTime(i) {
    if (i < 10) {
        i = "0" + i; 
    }
    return i;
}

startTime();

var ID = sessionStorage.getItem('id');
$.ajax({
    type: 'GET',
    url: `/api/user/get/${ID}`,
    dataType: "json",
    async: false,
    contentType: "application/json",
    headers: {
        'Authorization': 'Bearer ' + sessionStorage.getItem('token')
    },
    success: function (ok, textStatus, xhr) {
        ////debugger;
        var name = ok.firstName + " " + ok.lastName;
        document.getElementById('name').innerHTML = "Hi, " + name;
    }
});
