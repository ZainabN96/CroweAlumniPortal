document.addEventListener("DOMContentLoaded", function () {
    // Sidebar tab switching
    const links = document.querySelectorAll('.new-sidebar a');
    const sections = document.querySelectorAll('.new-content-section');
    links.forEach(link => {
        link.addEventListener('click', e => {
            e.preventDefault();
            links.forEach(l => l.classList.remove('new-active'));
            sections.forEach(s => s.classList.remove('new-active'));
            link.classList.add('new-active');
            document.getElementById(link.dataset.section).classList.add('new-active');
        });
    });

    // Menu toggle
    const menuBtn = document.getElementById('menuBtn');
    const menuDropdown = document.getElementById('menuDropdown');
    menuBtn?.addEventListener('click', e => {
        e.stopPropagation();
        menuDropdown.style.display = menuDropdown.style.display === 'block' ? 'none' : 'block';
    });
    document.addEventListener('click', () => menuDropdown.style.display = 'none');

    // Skills Add
    const skillInput = document.getElementById('skillInput');
    const skillsContainer = document.getElementById('skillsContainer');
    skillInput?.addEventListener('keydown', e => {
        if (e.key === 'Enter' && skillInput.value.trim() !== '') {
            e.preventDefault();
            const tag = document.createElement('div');
            tag.className = 'new-skill-tag';
            tag.innerHTML = `${skillInput.value.trim()} <span class="new-remove-skill">&times;</span>`;
            skillsContainer.appendChild(tag);
            tag.querySelector('.new-remove-skill').addEventListener('click', () => tag.remove());
            skillInput.value = '';
        }
    });

    // SUAC Form
    const suacForm = document.getElementById('experienceForm');
    const suacDisplay = document.querySelector('#suac #displaySection');
    const enrollment = document.getElementById('enrollment');
    const displayEnrollment = document.getElementById('displayEnrollment');
    const displayProgram = document.getElementById('displayProgram');
    const displayYears = document.getElementById('displayYears');
    document.getElementById('editBtn')?.addEventListener('click', () => {
        suacForm.style.display = 'block';
        suacDisplay.style.display = 'none';
    });
    suacForm?.addEventListener('submit', e => {
        e.preventDefault();
        let valid = true;
        suacForm.querySelectorAll('.new-error-msg').forEach(msg => msg.style.display = 'none');
        if (!enrollment.value.trim()) { enrollment.nextElementSibling.style.display = 'block'; valid = false; }
        if (!valid) return;
        displayEnrollment.textContent = enrollment.value;
        displayProgram.textContent = document.getElementById('program').value;
        displayYears.textContent = document.getElementById('joining').value + " - " + document.getElementById('completion').value;
        suacForm.style.display = 'none';
        suacDisplay.style.display = 'block';
    });

    // Work Form
    const workForm = document.getElementById('workForm');
    const workDisplay = document.querySelector('#work #displaySection');
    const designation = document.getElementById('designation');
    const displayDesignation = document.getElementById('displayDesignation');
    const displayCompany = document.getElementById('displayCompany');
    const displayDates = document.getElementById('displayDates');
    document.querySelector('#work #editBtn')?.addEventListener('click', () => {
        workForm.style.display = 'block';
        workDisplay.style.display = 'none';
    });
    workForm?.addEventListener('submit', e => {
        e.preventDefault();
        let valid = true;
        workForm.querySelectorAll('.new-error-msg').forEach(msg => msg.style.display = 'none');
        if (!designation.value.trim()) { designation.nextElementSibling.style.display = 'block'; valid = false; }
        if (!valid) return;
        displayDesignation.textContent = designation.value;
        displayCompany.textContent = document.getElementById('company').value;
        displayDates.textContent = document.getElementById('startMonth').value + " " + document.getElementById('startYear').value + " - " + document.getElementById('endMonth').value + " " + document.getElementById('endYear').value;
        workForm.style.display = 'none';
        workDisplay.style.display = 'block';
    });
});

// Education Functions
function addEducation() {
    document.querySelectorAll('.new-error').forEach(e => e.textContent = "");
    let institute = document.getElementById("institute").value.trim();
    let startYear = document.getElementById("startYear").value;
    let endYear = document.getElementById("endYear").value;
    let degree = document.getElementById("degree").value.trim();
    let department = document.getElementById("department").value.trim();
    let isValid = true;
    if (!institute) { document.getElementById("errorInstitute").textContent = "Please fill this"; isValid = false; }
    if (!startYear) { document.getElementById("errorStartYear").textContent = "Please fill this"; isValid = false; }
    if (!endYear) { document.getElementById("errorEndYear").textContent = "Please fill this"; isValid = false; }
    if (!degree) { document.getElementById("errorDegree").textContent = "Please fill this"; isValid = false; }
    if (!department) { document.getElementById("errorDepartment").textContent = "Please fill this"; isValid = false; }
    if (!isValid) return;
    document.getElementById("educationList").innerHTML = `
    <div class="new-card">
      <div>
        <h3>${institute}</h3>
        <p>${degree}, ${department}</p>
        <p>${startYear} - ${endYear}</p>
      </div>
      <button class="new-edit-btn">✏️</button>
    </div>`;
    clearForm();
}
function clearForm() {
    document.getElementById("institute").value = "";
    document.getElementById("startYear").value = "";
    document.getElementById("endYear").value = "";
    document.getElementById("degree").value = "";
    document.getElementById("department").value = "";
    document.querySelectorAll('.new-error').forEach(e => e.textContent = "");
}

//api
$(async function () {
    debugger;
  try {
    const me = await $.get("/api/profile/me");
      // fill photo
      if (me && me.profilePicturePath) {
          const $img = $("#profilePhoto");
          $img.attr("src", me.profilePicturePath + "?v=" + Date.now()); // bust cache
          // graceful fallback if the URL is broken
          $img.on("error", function () {
              $(this).attr("src", "/assets/img/person.png");
          });
      }
    /*if (me.profilePicturePath) {
      $(".new-profile-photo").attr("src", me.profilePicturePath);
    }*/
    // basic
    $("#basic input:eq(0)").val(me.firstName || "");
    $("#basic input:eq(1)").val(me.lastName || "");
    $("#basic input[type=date]").val(me.dob ? me.dob.substring(0,10) : "");
    $("#basic input[type=text]").eq(1).val(me.currentCity || "");
    $("#basic select").val(me.gender || "Female");
    $("#basic textarea").val(me.about || "");

    // contact
    $("#contact input[type=email]").first().val(me.emailAddress || "");
    $("#contact input[type=tel]").val(me.phone || "");
    $("#contact input[type=url]").val(me.linkedIn || "");
    $("#contact textarea").val(me.address || "");
    $("#contact input[type=text]").eq(0).val(me.city || "");
    $("#contact input[type=text]").eq(1).val(me.zip || "");

    // other
    $("#other input[placeholder='Designation']").val(me.designation || "");
    // skills: render if array
    if (Array.isArray(me.skills)) {
      const box = $("#skillsContainer").empty();
      me.skills.forEach(s => box.append(`<div class="new-skill-tag">${s} <span class="new-remove-skill">&times;</span></div>`));
      box.on("click", ".new-remove-skill", function(){ $(this).parent().remove(); });
    }
  } catch(e) {
    console.error("Failed to load profile", e);
  }
});

// BASIC
    $("#basic form").on("submit", async function(e){
        e.preventDefault();
    const dto = {
        firstName: $("#basic input").eq(0).val(),
    lastName:  $("#basic input").eq(1).val(),
    dob:       $("#basic input[type=date]").val(),
    currentCity: $("#basic input[type=text]").eq(1).val(),
    gender:    $("#basic select").val(),
    about:     $("#basic textarea").val()
  };
    await $.ajax({url:"/api/profile/basic", method:"PUT", contentType:"application/json", data: JSON.stringify(dto) });
    alert("Basic details saved.");
});

    // CONTACT
    $("#contact .new-btn-primary").on("click", async function(){
  const dto = {
        emailAddress: $("#contact input[type=email]").first().val(),
    phone:        $("#contact input[type=tel]").val(),
    linkedIn:     $("#contact input[type=url]").val(),
    address:      $("#contact textarea").val(),
    city:         $("#contact input[type=text]").eq(0).val(),
    zip:          $("#contact input[type=text]").eq(1).val()
  };
    await $.ajax({url:"/api/profile/contact", method:"PUT", contentType:"application/json", data: JSON.stringify(dto) });
    alert("Contact saved.");
});

    // OTHER
    $("#other .new-btn-primary").on("click", async function(){
  const skills = Array.from(document.querySelectorAll("#skillsContainer .new-skill-tag"))
                      .map(x => x.firstChild.nodeValue.trim());
    const dto = {
        designation: $("#other input[placeholder='Designation']").val(),
    skills: skills
  };
    await $.ajax({url:"/api/profile/other", method:"PUT", contentType:"application/json", data: JSON.stringify(dto) });
    alert("Other details saved.");
});

    // PRIVACY
    $("#privacy .new-btn-save").on("click", async function(){
  const dto = {
        emailVisibility: $("#emailEveryone").is(":checked") ? "Everyone" : "None",
    phoneVisibility: $("#phoneEveryone").is(":checked") ? "Everyone" : "None",
    instituteMails: $("#instituteMails").is(":checked"),
    systemMails: $("#systemMails").is(":checked")
  };
    await $.ajax({url:"/api/profile/privacy", method:"PUT", contentType:"application/json", data: JSON.stringify(dto) });
    alert("Privacy saved.");
});


$("#btnChangePhoto").on("click", ()=> $("#photoInput").click());
$("#photoInput").on("change", async function(){
  if (!this.files || this.files.length === 0) return;
  const fd = new FormData();
  fd.append("file", this.files[0]);
  try {
    const res = await fetch("/api/profile/photo", { method:"POST", body: fd });
    if(!res.ok){ throw new Error(await res.text()); }
    const data = await res.json(); // { url }
    $(".new-profile-photo").attr("src", data.url);
  } catch(err){
    alert("Photo upload failed.");
    console.error(err);
  }
});

