const userTy = sessionStorage.getItem("userType");
const currentUserIsAdmin = (userTy?.toLowerCase() === "admin");

// ====== Validation config ======
const RULES = {
    title: { min: 4, max: 120 },
    body: { min: 1, max: 2000 },
    comment: { min: 1, max: 500 },
    image: { exts: ["image/jpeg", "image/png", "image/gif", "image/webp"], max: 5 * 1024 * 1024 },
    video: { exts: ["video/mp4", "video/webm", "video/ogg"], max: 50 * 1024 * 1024 }
};

// ====== Small helpers ======
function setErr($el, msg) {
    $el.text(msg || "").toggleClass("d-none", !msg);
}
function sanitizeInput(s) { return (s || "").replace(/\s+/g, " ").trim(); }

// Validate file by type/size; returns {ok:boolean,msg:string,mediaType:"image"|"video"|null}
function validateFile(file) {
    if (!file) return { ok: true, msg: "", mediaType: null };
    const t = file.type.toLowerCase();

    if (t.startsWith("image/")) {
        if (!RULES.image.exts.includes(t)) return { ok: false, msg: "Unsupported image type.", mediaType: null };
        if (file.size > RULES.image.max) return { ok: false, msg: "Image must be ≤ 5 MB.", mediaType: null };
        return { ok: true, msg: "", mediaType: "image" };
    }
    if (t.startsWith("video/")) {
        if (!RULES.video.exts.includes(t)) return { ok: false, msg: "Unsupported video type.", mediaType: null };
        if (file.size > RULES.video.max) return { ok: false, msg: "Video must be ≤ 50 MB.", mediaType: null };
        return { ok: true, msg: "", mediaType: "video" };
    }
    return { ok: false, msg: "Only images or videos are allowed.", mediaType: null };
}
function initialsFromName(name) {
    const parts = (name || "").trim().split(/\s+/).filter(Boolean);
    if (parts.length === 0) return "";
    if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
}

function timeAgo(iso) {
    // Force treat timestamp as UTC
    const then = new Date(iso + "Z");
    const now = new Date();

    // Difference in seconds
    const diff = (now.getTime() - then.getTime()) / 1000;

    if (diff < 60) return "Just now";

    const m = Math.floor(diff / 60);
    if (m < 60) return `${m} min ago`;

    const h = Math.floor(m / 60);
    if (h < 24) return `${h} h ago`;

    const d = Math.floor(h / 24);
    if (d < 7) return `${d} d ago`;

    return then.toLocaleDateString();
}

// media preview builder
function mediaBlock(p) {
    if (!p.mediaPath) return "";
    const t = (p.mediaType || "").toLowerCase();
    if (t === "image") {
        return `<img src="${p.mediaPath}" class="img-fluid mt-2 rounded" alt="attachment">`;
    } else if (t === "video") {
        return `<video src="${p.mediaPath}" class="w-100 mt-2 rounded" controls></video>`;
    }
    return `<p class="mt-2"><a href="${p.mediaPath}" target="_blank">Attachment</a></p>`;
}

function renderComment(c) {
    //debugger;
    const name = `${c.author?.firstName ?? ""} ${c.author?.lastName ?? ""}`.trim() || "Unknown";
    const hasPic = !!c.author?.profilePicturePath;
    const av = hasPic
        ? `<img src="${c.author.profilePicturePath}" class="rounded-circle img-av" style="width: 28px; height: 28px; object-fit: cover;">`
        : `<div class="rounded-circle bg-secondary text-white d-flex align-items-center justify-content-center img-av-bg" style="width: 28px; height: 28px; font-size: 12px; font-weight: 600;">
         ${initialsFromName(name)}
       </div>`;
    const when = c.createdOn ? timeAgo(c.createdOn) : "";
    const body = (c.body || "").replace(/\n/g, "<br/>");

    return `
    <div class="d-flex align-items-start mb-2">
      ${av}
      <div class="ms-2 bg-light rounded" style="max-width: 100%;">
        <div class="fw-semibold small">${name} <span class="text-muted">· ${when}</span></div>
        <div class="small">${body}</div>
      </div>
    </div>`;
}

function renderFbPostCard(p, currentUserIsAdmin) {
    const name = `${p.author?.firstName ?? ""} ${p.author?.lastName ?? ""}`.trim() || "Unknown";
    const time = p.createdOn ? timeAgo(p.createdOn) : "";
    const hasPic = !!p.author?.profilePicturePath;

    const avatar = hasPic
        ? `<img src="${p.author.profilePicturePath}" alt="${name}" class="rounded-circle post-avatar-img" style="width:44px;height:44px;object-fit:cover;">`
        : `<div class="rounded-circle bg-secondary text-white d-flex align-items-center justify-content-center post-avatar" style="width:44px;height:44px;font-weight:600;">${initialsFromName(name)}</div>`;

    const safeBody = (p.body || "").replace(/\n/g, "<br />");
    const likedCls = p.isLiked ? "active" : "";

    const adminControls = currentUserIsAdmin ? `
      <div class="ms-auto dropdown">
        <a class="" data-bs-toggle="dropdown" aria-expanded="false" title="More">
          <i class="bi bi-three-dots-vertical"></i>
        </a>
        <ul class="dropdown-menu dropdown-menu-end">
          <li>
            <a class="dropdown-item btn-soft-delete" href="#" data-id="${p.id}">
              Delete
            </a>
          </li>
        </ul>
      </div>` : '';

    return `
    <div class="card shadow-sm mb-3 crw-event-card" data-id="${p.id}" id="post-${p.id}">
      <div class="card-body">
        <div class="d-flex align-items-center mb-2">
          ${avatar}
          <div class="ms-2">
            <div class="fw-semibold">${name}</div>
            <div class="text-muted small">${time}</div>
          </div>
          ${adminControls}
        </div>

        ${p.title ? `<h6 class="mb-1">${p.title}</h6>` : ""}
        ${safeBody ? `<div class="mb-2">${safeBody}</div>` : ""}
        ${mediaBlock(p)}

        <div class="d-flex justify-content-between mt-2">
          <button class="btn-like btn btn-sm btn-outline-primary ${likedCls}" data-liked="${p.isLiked ? "1" : "0"}">
            <i class="bi bi-hand-thumbs-up${p.isLiked ? "-fill" : ""}"></i> Like
          </button>
          <button class="btn-comment btn btn-sm btn-outline-secondary"><i class="bi bi-chat"></i> Comment</button>
        </div>

        <div class="like-count text-muted small mt-1">${p.likeCount ?? 0} ${((p.likeCount ?? 0) === 1) ? "like" : "likes"}</div>

        <div class="add-comment d-flex mt-1">
          <input type="text" class="form-control form-control-sm comment-input" placeholder="Write a comment..." />
          <button class="btn btn-sm btn-primary btn-submit-comment">Post</button>
        </div>
        <div class="comments mt-2"></div>
        <div class="mt-2 text-center border rounded">
            <a class="d-inline-block mt-2 mb-2" href="/Posts/Details/${p.id}">
                View More
            </a>
        </div>
        </div>
    </div>`;
}

$(document).on("click", ".btn-soft-delete", function (e) {
    e.preventDefault();
    const $btn = $(this);
    const postId = $btn.data("id");
    if (!postId) return;
    if (!confirm("Are you sure you want to remove this post? This hides it from everyone.")) return;

    $.post(`/api/posts/${postId}/soft-delete`)
        .done(res => {
            // remove card from UI or mark as deleted
            $(`#post-${postId}`).fadeOut(300, function () { $(this).remove(); });
            // optionally show notification
            swal("Removed", "The post has been removed.", "success");
        })
        .fail(xhr => {
            swal("Failed", xhr.responseText || "Could not delete post.", "error");
        });
});
function hydratePostCard($card, postId) {
    // debugger;
    const $comments = $card.find(".comments");
    $comments.html(`<div class="text-muted small">Loading comments…</div>`);
    $.get(`/api/posts/${postId}/comments`)
        .done(list => {
            if (!Array.isArray(list) || list.length === 0) {
                $comments.html(`<div class="text-muted small">No comments yet.</div>`);
                return;
            }
            const html = list.map(renderComment).join("");
            $comments.html(html);
        })
        .fail(() => {
            $comments.html(`<div class="text-danger small">Failed to load comments.</div>`);
        });
}

(function () {
    let selectedFile = null;
    const $title = $("#postTitle");
    const $body = $("#postText");
    const $btnPost = $(".crw-btn-post");   // the Post button
    const fileInput = document.getElementById("postImage");
    const previewDiv = document.getElementById("composerPreview");
    /*const fileInput = document.getElementById('postImage');
    const previewDiv = document.getElementById('composerPreview');*/
    function renderComposerPreview(file) {
        previewDiv.innerHTML = "";
        if (!file) return;

        const url = URL.createObjectURL(file);
        let html = "";
        if (file.type.startsWith("image/")) {
            html = `<img src="${url}" class="img-fluid rounded" alt="preview">`;
        } else if (file.type.startsWith("video/")) {
            html = `<video src="${url}" class="w-100 rounded" controls></video>`;
        } else {
            html = `<div class="small text-muted">Selected: ${file.name}</div>`;
        }
        previewDiv.innerHTML = html;
    }

    /*if (fileInput) {
        fileInput.addEventListener('change', (e) => {
            selectedFile = e.target.files?.[0] || null;
            renderComposerPreview(selectedFile);
        });
    }*/
    if (fileInput) {
        fileInput.addEventListener("change", (e) => {
            selectedFile = e.target.files?.[0] || null;
            const vf = validateFile(selectedFile);
            setErr($("#postMediaErr"), vf.ok ? "" : vf.msg);
            renderComposerPreview(selectedFile);
        });
    }

    // submitPost 
    window.submitPost = function () {
        const title = sanitizeInput($title.val());
        const body = sanitizeInput($body.val());

        // validate file

        const fileCheck = validateFile(selectedFile);
        if (!fileCheck.ok) { setErr($("#postMediaErr"), fileCheck.msg); return; }
        setErr($("#postMediaErr"), "");

        // at least one content piece
        /*if (!title && !body && !selectedFile) {
            setErr($("#postTitleErr"), "Add a title, text, or media.");
            setErr($("#postTextErr"), "");
            return;
        }
        */
        // at least one content piece
        if (!title && !body && !selectedFile) {
            setErr($("#postTitleErr"), "Title is required.");
            setErr($("#postTextErr"), "Text is required."); // <-- add this line
            return;
        } else if (!title) {
            setErr($("#postTitleErr"), "Title is required.");
            return;
        } else if (!body) {
            setErr($("#postTextErr"), "Text is required."); // <-- add this line
            return;
        } else {
            setErr($("#postTitleErr"), "");
            setErr($("#postTextErr"), "");  // <-- clear when ok
            setErr($("#postMediaErr"), "");
        }


        // length checks only if provided
        if (title && (title.length < RULES.title.min || title.length > RULES.title.max)) {
            setErr($("#postTitleErr"), `Title must be ${RULES.title.min}-${RULES.title.max} characters.`);
            return;
        } else setErr($("#postTitleErr"), "");

        if (body && (body.length < RULES.body.min || body.length > RULES.body.max)) {
            setErr($("#postTextErr"), `Text must be ${RULES.body.min}-${RULES.body.max} characters.`);
            return;
        } else setErr($("#postTextErr"), "");

        // build form data
        const fd = new FormData();
        fd.append("Title", title);
        fd.append("Body", body);
        if (selectedFile) fd.append("Media", selectedFile);

        // UX: disable button while posting
        $btnPost.prop("disabled", true).text("Posting…");
        debugger;
        $.ajax({
            url: "/api/posts",
            type: "POST",
            data: fd,
            processData: false,
            contentType: false
        })
            .done(function (res) {
                debugger;
                $("#postContainer").prepend(renderFbPostCard(res, currentUserIsAdmin));
                swal("Process Completed!", "The Post has been Added!", "success");
                // reset
                $title.val(""); $body.val("");
                if (fileInput) fileInput.value = "";
                selectedFile = null; previewDiv.innerHTML = "";
                setErr($("#postTitleErr"), ""); setErr($("#postTextErr"), ""); setErr($("#postMediaErr"), "");
            })
            .fail(function (xhr) {
                debugger;
                swal("Failed", xhr.responseText || "Failed to create post.", "error");
            })
            .always(function () {
                debugger;
                $btnPost.prop("disabled", false).text("Post");
            });
    };
    /*window.submitPost = function () {
        const title = ($("#postTitle").val() || "").trim();
        const body = ($("#postText").val() || "").trim();
        if (!title && !body && !selectedFile) { alert("Write something or add media."); return; }

        const fd = new FormData();
        fd.append("Title", title);
        fd.append("Body", body);
        if (selectedFile) fd.append("Media", selectedFile);

        $.ajax({
            url: "/api/posts",
            type: "POST",
            data: fd,
            processData: false,
            contentType: false
        })
            .done(function (res) {
                $("#postContainer").prepend(renderFbPostCard(res, currentUserIsAdmin));
                // reset composer
                swal("Process Completed!", "The Post has been Added!", "success")
                $("#postTitle").val(""); $("#postText").val("");
                if (fileInput) fileInput.value = "";
                selectedFile = null; previewDiv.innerHTML = "";
            })
            .fail(function (xhr) {
                alert(xhr.responseText || "Failed to create post.");
            });
    };
*/
    getPost();

    function getPost() {
        $.get("/api/posts/latest", { take: 10 })
            .done(list => {
                if (Array.isArray(list) && list.length) {
                    const html = list.map(p => renderFbPostCard(p, currentUserIsAdmin)).join("");
                    $("#postContainer").html(html);

                    $("#postContainer .crw-event-card").each(function () {
                        const postId = $(this).data("id");
                        hydratePostCard($(this), postId);
                    });
                }
            });
    }
    // Like click
    $(document).on("click", ".btn-like", function () {
        const card = $(this).closest(".crw-event-card");
        const postId = card.data("id");
        $.post(`/api/posts/${postId}/like/toggle`)
            .done(data => {
                debugger;
                getPost();
            });
    });
    $(document).on("click", ".btn-unlike", function () {
        const card = $(this).closest(".crw-event-card");
        const postId = card.data("id");
        $.post(`/api/posts/${postId}/unlike`)
            .done(() => {
                let cnt = parseInt(card.find(".like-count").text()) || 0;
                card.find(".like-count").text(`${cnt} likes`);
            });
    });

    // Submit comment
    $(document).on("click", ".btn-submit-comment", function () {
       /* const $card = $(this).closest(".crw-event-card");
        const postId = $card.data("id");
        const $input = $card.find(".comment-input");
        const text = ($input.val() || "").trim();
        if (!text) return;*/
        const $card = $(this).closest(".crw-event-card");
        const postId = $card.data("id");
        const $input = $card.find(".comment-input");
        const text = sanitizeInput($input.val());

        if (!text || text.length < RULES.comment.min || text.length > RULES.comment.max) {
            $input.addClass("is-invalid");
            if (!$card.find(".comment-err").length) {
                $input.after(`<div class="invalid-feedback comment-err">Comment must be ${RULES.comment.min}-${RULES.comment.max} characters.</div>`);
            }
            return;
        }
        $input.removeClass("is-invalid");
        $card.find(".comment-err").remove();

        $.ajax({
            url: `/api/posts/${postId}/comment`,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ body: text })
        })
            .done(res => {
                $card.find(".comments").append(renderComment(res));
                $input.val("");

            })
            .fail(xhr => {
                alert(xhr.responseText || "Failed to add comment.");
            });
    });
})();