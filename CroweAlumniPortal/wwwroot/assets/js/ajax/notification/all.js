$(function () {

    let currentStatusFilter = "all";  // all | unread | read
    let currentTypeFilter = "all";  // all | some type
    let currentSearch = "";     // text

    function applyFilters() {
        const searchLower = (currentSearch || "").trim().toLowerCase();
        const typeLower = (currentTypeFilter || "all").toLowerCase();

        $(".notification-item").each(function () {
            const $item = $(this);

            const status = ($item.attr("data-status") || "").toLowerCase(); // read/unread
            const type = ($item.attr("data-type") || "").toLowerCase();

            const title = ($item.find(".notif-title").text() || "").toLowerCase();
            const msg = ($item.find(".notif-message").text() || "").toLowerCase();
            const text = title + " " + msg;

            // Status match
            const okStatus = (currentStatusFilter === "all" || status === currentStatusFilter);

            // Type match
            const okType = (typeLower === "all" || type === typeLower);

            // Search match
            const okSearch = (!searchLower || text.indexOf(searchLower) !== -1);

            if (okStatus && okType && okSearch) {
                $item.removeClass("notif-hidden");
            } else {
                $item.addClass("notif-hidden");
            }
        });
    }

    // STATUS FILTER: All / Unread / Read
    $(".filter-btn").on("click", function () {
        $(".filter-btn").removeClass("active");
        $(this).addClass("active");

        currentStatusFilter = $(this).attr("data-filter"); // all / unread / read
        applyFilters();
    });

    // TYPE FILTER (dropdown)
    $("#typeFilter").on("change", function () {
        currentTypeFilter = $(this).val() || "all";
        applyFilters();
    });

    // SEARCH TEXT
    $("#searchText").on("input", function () {
        currentSearch = $(this).val() || "";
        applyFilters();
    });

    // MARK AS READ
    $("#notificationList").on("click", ".btn-mark-read", function () {
        debugger;
        const $item = $(this).closest(".notification-item");
        const id = $item.attr("data-id");
        debugger;
        $.ajax({
            url: '/notifications/MarkAsRead',
            type: 'POST',
            data: { id: id },
            success: function () {
                debugger;
                $item.attr("data-status", "read")
                    .removeClass("unread")
                    .addClass("read");

                $item.find(".badge.bg-warning").remove();
                $item.find(".btn-mark-read").remove();
                $item.find(".text-end").prepend('<span class="badge bg-success mb-2">Read</span>');
                //$item.find(".readtime").prepend('(Read: @readOn.Value.ToString("dd-MMM-yyyy hh:mm tt"))');

                // ab current filters ke hisaab se dobara evaluate karo
                applyFilters();
            },
            error: function () {
                swal("Error", "Unable to update notification status.", "error");
            }
        });
    });

    // OPTIONAL: page load pe bhi filters apply kar do (in case future me default change ho)
    applyFilters();
});