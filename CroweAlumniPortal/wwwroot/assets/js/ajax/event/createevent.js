$("#eventForm").on("submit", function (e) {
    debugger;
    e.preventDefault();

    var payload = {
        title: $("#title").val(),
        address: $("#address").val(),
        startDateTime: $("#startDate").val(),
        endDateTime: $("#endDate").val(),
        registrationLink: $("#registrationLink").val() || null,
        description: $("#description").val(),
    };

    // client-side check
    if (new Date(payload.endDateTime) <= new Date(payload.startDateTime)) {
        alert("End date must be after start date.");
        return;
    }

    $.ajax({
        url: "/api/event/create",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (data) {
            //alert("Event created: ID " + data.id);
            swal("Process Completed!", "The Event has been Added!", "success")
            $("#eventForm")[0].reset(); // reset form
        },
        error: function (xhr) {
            alert("Failed: " + xhr.responseText);
        }
    });
});