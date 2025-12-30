const API_BASE = "/api/admin/users";

let state = {
    page: 1,
    pageSize: 20,
    total: 0,
    items: [],
    selectedId: null
};

function fmt(d) { if (!d) return "—"; return new Date(d).toLocaleDateString(); }
function html(x) { return x == null || x === '' ? '—' : x; }

function loadPending() {
    $("#pendingTable tbody").html('<tr><td colspan="7" class="text-center py-4">Loading…</td></tr>');
    const q = `${API_BASE}/pending?page=${state.page}&pageSize=${state.pageSize}`;
    $.getJSON(q).done(res => {
        // Expecting: { items, total } or (Items, Total)
        const items = res.items || res.Items || [];
        const total = res.total ?? res.Total ?? items.length;
        state.items = items; state.total = total;
        renderTable();
        renderPager();
    }).fail(xhr => {
        $("#pendingTable tbody").html(`<tr><td colspan="7" class="text-center text-danger py-4">Failed to load (${xhr.status}).</td></tr>`);
    });
}

function renderTable() {
    const $tb = $("#pendingTable tbody");
    const q = ($("#searchBox").val() || '').toLowerCase();
    const rows = [];
    let idx = (state.page - 1) * state.pageSize;
    state.items.filter(u => {
        if (!q) return true;
        const hay = `${u.firstName || u.FirstName || ''} ${u.lastName || u.LastName || ''} ${u.emailAddress || u.EmailAddress || ''} ${u.loginId || u.LoginId || ''}`.toLowerCase();
        return hay.includes(q);
    }).forEach(u => {
        const id = u.id || u.Id;
        const name = `${html(u.firstName || u.FirstName)} ${html(u.lastName || u.LastName)}`;
        const email = html(u.emailAddress || u.EmailAddress);
        const login = html(u.loginId || u.LoginId);
        const member = html(u.memberStatus || u.MemberStatus);
        const qual = html(u.qualification || u.Qualification);
        rows.push(`
                    <tr data-id="${id}">
                        <td>${++idx}</td>
                        <td>${name}</td>
                        <td>${email}</td>
                        <td><code>${login}</code></td>
                        <td>${qual}</td>
                        <td>
                            <div class="btn btn-group-sm">
                                <button class="btn btn-outline-primary viewBtn"><i class="bi bi-eye"></i> </button>
                                <button class="btn btn-success approveBtn"><i class="bi bi-check2"></i></button>
                                <button class="btn btn-outline-danger rejectBtn"><i class="bi bi-x"></i></button>
                            </div>
                        </td>
                    </tr>`);
    });
    $tb.html(rows.length ? rows.join('') : '<tr><td colspan="7" class="text-center py-4">No pending users.</td></tr>');

    $("#resultInfo").text(`${state.total} total pending`);
}

function renderPager() {
    const pages = Math.max(1, Math.ceil(state.total / state.pageSize));
    const $p = $("#pager");
    const btn = (p, lbl, dis = false, act = false) => `<li class="page-item ${dis ? 'disabled' : ''} ${act ? 'active' : ''}"><a class="page-link" href="#" data-p="${p}">${lbl}</a></li>`;
    let html = '';
    html += btn(Math.max(1, state.page - 1), '&laquo;', state.page === 1);
    for (let i = 1; i <= pages && i <= 10; i++) {
        html += btn(i, i, false, i === state.page);
    }
    html += btn(Math.min(pages, state.page + 1), '&raquo;', state.page === pages);
    $p.html(html);
    $p.find('a').on('click', function (e) { e.preventDefault(); const p = +($(this).data('p')); if (!isNaN(p) && p > 0) { state.page = p; loadPending(); } });
}

function openDetail(id) {
    const url = `${API_BASE}/${id}`;
    state.selectedId = id;
    $.getJSON(url).done(u => {
        const avatar = u.profilePicturePath || u.ProfilePicturePath || '';
        if (avatar) $('#dAvatar').attr('src', avatar).show(); else $('#dAvatar').hide();
        $('#dName').text(`${html(u.firstName || u.FirstName)} ${html(u.lastName || u.LastName)}`);
        $('#dEmail').text(html(u.emailAddress || u.EmailAddress));
        $('#dLogin').text(html(u.loginId || u.LoginId));
        $('#dMember').text(html(u.memberStatus || u.MemberStatus));
        $('#dQual').text(html(u.qualification || u.Qualification));
        $('#dDOB').text(fmt(u.dob || u.DOB));
        $('#dCNIC').text(html(u.cnic || u.CNIC));
        $('#dPhone').text(html(u.mobileNumber || u.MobileNumber));
        $('#dAddress').text(`${html(u.address || u.Address)}, ${html(u.city || u.City)}, ${html(u.country || u.Country)} ${html(u.zip || u.ZIP)}`);
        $('#dIndustry').text(html(u.industry || u.Industry));
        $('#dOrg').text(html(u.employerOrganization || u.EmployerOrganization));
        $('#dJob').text(html(u.jobTitle || u.JobTitle));
        $('#dHistCity').text(html(u.historyCity || u.HistoryCity));
        $('#dLastPos').text(html(u.lastPosition || u.LastPosition));
        $('#dDept').text(html(u.department || u.Department));
        const jd = fmt(u.joiningDate || u.JoiningDate); const ld = fmt(u.leavingDate || u.LeavingDate);
        $('#dJoinLeave').text(`${jd} → ${ld}`);

        const off = new bootstrap.Offcanvas('#detailPanel');
        off.show();
    }).fail(xhr => {
        swal('Failed', `Could not load details (${xhr.status}).`, 'error');
    });
}

function approve(id) {
    $.post({ url: `${API_BASE}/${id}/approve` })
        .done(() => { swal('Approved', 'User has been approved.', 'success'); loadPending(); })
        .fail(xhr => { swal('Failed', xhr.responseText || 'Approve failed', 'error'); });
}

function reject(id, reason) {
    $.ajax({
        url: `${API_BASE}/${id}/reject`,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ reason })
    }).done(() => {
        swal('Rejected', 'User has been rejected.', 'success');
        loadPending();
    }).fail(xhr => {
        swal('Failed', xhr.responseText || 'Reject failed', 'error');
    });
}

// Events
$(function () {
    // initial state
    state.pageSize = +$('#pageSize').val();
    loadPending();

    $('#refreshBtn').on('click', function () { loadPending(); });
    $('#pageSize').on('change', function () { state.pageSize = +$(this).val(); state.page = 1; loadPending(); });
    $('#searchBox').on('input', function () { renderTable(); });

    $('#pendingTable').on('click', '.viewBtn', function () { const id = $(this).closest('tr').data('id'); openDetail(id); });
    $('#pendingTable').on('click', '.approveBtn', function () { const id = $(this).closest('tr').data('id'); approve(id); });
    $('#pendingTable').on('click', '.rejectBtn', function () { const id = $(this).closest('tr').data('id'); state.selectedId = id; new bootstrap.Modal('#rejectModal').show(); });

    $('#approveBtn').on('click', function () { if (state.selectedId) approve(state.selectedId); });
    $('#rejectBtn').on('click', function () { $('#rejectReason').val(''); new bootstrap.Modal('#rejectModal').show(); });
    $('#confirmRejectBtn').on('click', function () { const reason = ($('#rejectReason').val() || '').trim(); if (!reason) { swal('Required', 'Please enter a reason.', 'info'); return; } reject(state.selectedId, reason); $('#rejectModal').modal('hide'); });
});