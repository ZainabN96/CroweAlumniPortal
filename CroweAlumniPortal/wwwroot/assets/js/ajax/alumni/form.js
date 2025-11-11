
// ---------------- Member Status (All Common Professional Bodies) ----------------
const memberList = [
    // Accounting & Audit
    "ACCA (Association of Chartered Certified Accountants)",
    "ICAP (Institute of Chartered Accountants of Pakistan)",
    "ICAEW (Institute of Chartered Accountants in England and Wales)",
    "ICMAP (Institute of Cost and Management Accountants of Pakistan)",
    "CPA (Certified Public Accountant)",
    "CIMA (Chartered Institute of Management Accountants)",
    "CA (Chartered Accountant)",
    "CIA (Certified Internal Auditor)",
    "CFA (Chartered Financial Analyst)",
    "CFP (Certified Financial Planner)",
    "CMA (Certified Management Accountant)",
    "CGMA (Chartered Global Management Accountant)",
    "CISA (Certified Information Systems Auditor)",
    "CFE (Certified Fraud Examiner)",
    "IFAC (International Federation of Accountants)",
    "PIPFA (Pakistan Institute of Public Finance Accountants)",
    "SAFA (South Asian Federation of Accountants)",
    "CA(SA) (South Africa)",
    "CA(Canada)",
    "CPA(Australia)",
    "CPA(USA)",

    // Tax, Law & Advisory
    "Advocate (Law Bar Member)",
    "Bar Council Member",
    "Tax Consultant (Registered)",
    "Legal Practitioner (LLB/LLM)",

    // IT & Tech Certifications
    "Microsoft Certified (MCSA/MCSE)",
    "AWS Certified (Cloud Practitioner/Architect)",
    "Google Cloud Certified",
    "Cisco Certified (CCNA/CCNP)",
    "Oracle Certified Professional (OCP)",
    "SAP Certified Consultant",
    "Project Management Professional (PMP)",
    "PRINCE2 Certified",
    "Scrum Master Certified (CSM)",
    "Agile Certified Practitioner (PMI-ACP)",
    "Data Analyst Certified",
    "Cybersecurity Certified (CEH, CISSP)",

    // Education & Others
    "PhD / MPhil / MS Research Scholar",
    "University Faculty Member",
    "Professional Trainer / Consultant",
    "Other"
];

function populateMembers() {
    const $m = $("#memberStatus");
    $m.empty();
    $m.append(new Option("Select", "", true, false));
    memberList.forEach(m => $m.append(new Option(m, m)));
}

function bindMemberCustomAdd() {
    const $m = $("#memberStatus");
    const $box = $("#memberAddBox");
    const $input = $("#newMemberInput");
    const $btn = $("#addMemberBtn");
    const $err = $("#memberError");

    $m.off(".member").on("change.member", function () {
        if (($m.val() || "").toLowerCase() === "other") {
            $box.show();
            $input.focus();
            $err.hide().text("");
        } else {
            $box.hide();
            $err.hide().text("");
        }
    });

    $btn.off(".member").on("click.member", function (e) {
        e.preventDefault();
        const val = ($input.val() || "").trim();
        if (!val) { $err.text("Please enter your membership name.").show(); return; }

        // Avoid duplicates
        const exists = $("#memberStatus option").toArray()
            .some(o => (o.value || o.text).toLowerCase() === val.toLowerCase());
        if (exists) { $err.text("This membership already exists.").show(); return; }

        $("#memberStatus").append(new Option(val, val));
        $("#memberStatus").val(val);
        $input.val("");
        $box.hide();
        $err.hide().text("");
    });
}

// ---------------- Qualification (All Degrees) ----------------
const degreeCatalog = {
    "Secondary / Pre-University": [
        "Matric / SSC", "Intermediate / HSSC", "A-Levels", "IB Diploma", "Foundation Year"
    ],
    "Diplomas & Certificates": [
        "Diploma (1–3 Years)", "Postgraduate Diploma (PGDip)", "Graduate Certificate", "Certificate (6–12 Months)"
    ],
    "Associate": [
        "ADP (Associate Degree Program)", "Associate of Arts (AA)", "Associate of Science (AS)",
        "Associate of Applied Science (AAS)"
    ],
    "Bachelor’s": [
        "BA (Bachelor of Arts)", "BSc (Bachelor of Science)", "BBA (Bachelor of Business Administration)",
        "BS (General)", "BCom (Bachelor of Commerce)", "BEd (Bachelor of Education)",
        "BFA (Fine Arts)", "BDes (Design)", "BArch (Architecture)", "LLB (Law)",
        "BE (Bachelor of Engineering)", "BEng (Engineering)", "BSCS (Computer Science)",
        "BSIT (Information Technology)", "BSSE (Software Engineering)", "BS Data Science",
        "BS AI (Artificial Intelligence)", "BS Cyber Security", "BS Telecom",
        "BS Electrical", "BS Electronics", "BS Mechanical", "BS Civil", "BS Chemical",
        "BS Industrial Engineering", "BS Biomedical Engineering",
        "MBBS (Medicine)", "BDS (Dentistry)", "PharmD (Doctor of Pharmacy)", "DPT (Doctor of Physical Therapy)",
        "Nursing (BSN)", "BSc Medical Lab Technology", "BSc Allied Health",
        "BHM (Hospitality & Management)", "BPA (Public Administration)",
        "BSc Economics", "BSc Mathematics", "BSc Physics", "BSc Chemistry", "BSc Biology",
        "BAGRI / BS Agriculture", "BSc Environmental Science"
    ],
    "Master’s": [
        "MA (Master of Arts)", "MSc (Master of Science)", "MBA (Master of Business Administration)",
        "MS (Master of Science)", "MCS (Computer Science)", "MIT (Information Technology)",
        "MPhil (Master of Philosophy)", "MPA (Public Administration)", "MPH (Public Health)",
        "MEd (Education)", "LLM (Master of Laws)", "MS Project Management",
        "MS Data Science", "MS AI", "MS Cyber Security", "MS Electrical", "MS Mechanical",
        "MS Civil", "MS Chemical", "MS Economics", "MS Finance", "MS Accounting",
        "MS Supply Chain", "MS HRM", "MS Marketing"
    ],
    "Professional (Chartered & Certifications)": [
        "ACCA", "ICAP (CA Pakistan)", "ICAEW", "CIMA", "CMA (IMA)", "CPA",
        "PMI-PMP", "PMI-ACP", "PRINCE2", "CFA", "FRM", "CIA (Internal Auditor)",
        "SHRM-CP / SHRM-SCP", "SAP Certifications", "Cisco (CCNA/CCNP)", "Microsoft (AZ/DP/AI)",
        "AWS (Associate/Professional)", "Google Cloud (Associate/Professional)"
    ],
    "Doctorate": [
        "PhD (Doctor of Philosophy)", "DBA (Doctor of Business Administration)",
        "EdD (Doctor of Education)", "MD (Doctor of Medicine)"
    ],
    "Other": ["Other"]
};

function populateQualifications() {
    const $q = $("#qualification");
    $q.empty();
    $q.append(new Option("Select", "", true, false));

    // create optgroups
    Object.keys(degreeCatalog).forEach(group => {
        const $grp = $(`<optgroup label="${group}"></optgroup>`);
        degreeCatalog[group].forEach(deg => {
            $grp.append(new Option(deg, deg));
        });
        $q.append($grp);
    });
}

function bindQualificationCustomAdd() {
    const $q = $("#qualification");
    const $box = $("#qualAddBox");
    const $input = $("#newQualificationInput");
    const $btn = $("#addQualificationBtn");
    const $err = $("#qualError");

    // Show add-box when 'Other' chosen
    $q.off(".qual").on("change.qual", function () {
        if (($q.val() || "").toLowerCase() === "other") {
            $box.show();
            $input.focus();
            $err.hide().text("");
        } else {
            $box.hide();
            $err.hide().text("");
        }
    });

    // Add custom degree
    $btn.off(".qual").on("click.qual", function (e) {
        e.preventDefault();
        const val = ($input.val() || "").trim();
        if (!val) { $err.text("Please enter your degree name.").show(); return; }

        // prevent duplicates
        const exists = $("#qualification option").toArray()
            .some(o => (o.value || o.text).toLowerCase() === val.toLowerCase());
        if (exists) { $err.text("This degree already exists in the list.").show(); return; }

        // append into "Other" group or at end
        $("#qualification").append(new Option(val, val));
        $("#qualification").val(val);
        $input.val("");
        $box.hide();
        $err.hide().text("");
    });
}


function bindProfilePicPreview() {
    const $input = $("#profilepic");
    const $label = $("#profile-pic");
    const $img = $("#profilePreview");
    const $clear = $("#clearProfilePic");

    let lastObjectUrl = null;
    const MAX_MB = 5; // max size limit

    function resetPreview() {
        if (lastObjectUrl) {
            URL.revokeObjectURL(lastObjectUrl);
            lastObjectUrl = null;
        }
        $img.hide().attr("src", "");
        $clear.hide();
        $label.text("Add Picture");
        $input.val(""); // clear file input
    }

    $input.on("change", function () {
        const file = this.files && this.files[0] ? this.files[0] : null;
        if (!file) { resetPreview(); return; }

        // Basic validations
        if (!file.type.startsWith("image/")) {
            swal("Invalid", "Please select an image file.", "warning");
            resetPreview();
            return;
        }
        const sizeMb = file.size / (1024 * 1024);
        if (sizeMb > MAX_MB) {
            swal("Too Large", `Image must be ≤ ${MAX_MB} MB.`, "warning");
            resetPreview();
            return;
        }

        // Update label
        $label.text(file.name);

        // Show preview (use object URL)
        if (lastObjectUrl) URL.revokeObjectURL(lastObjectUrl);
        lastObjectUrl = URL.createObjectURL(file);
        $img.attr("src", lastObjectUrl).show();
        $clear.show();
    });

    $clear.on("click", function () {
        resetPreview();
    });
}


// ---------- Countries & Cities via CountriesNow ----------
const CN_BASE = "https://countriesnow.space/api/v0.1/countries";
let COUNTRIES_DATA = []; // [{ country, cities:[] }, ...]

function fillSelect($select, items, { placeholder = "Select", valueKey = null, textKey = null } = {}) {
    $select.empty();
    $select.append(new Option(placeholder, "", true, false));
    items.forEach(it => {
        const value = valueKey ? it[valueKey] : it;
        const text = textKey ? it[textKey] : it;
        $select.append(new Option(text, value));
    });
    $select.prop("disabled", items.length === 0);
}

function bindCountryCityPair(countryId, cityId) {
    const $country = $(`#${countryId}`);
    const $city = $(`#${cityId}`);

    fillSelect($country, COUNTRIES_DATA.map(x => x.country), { placeholder: "Select Country" });
    fillSelect($city, [], { placeholder: "Select City" });
    $city.prop("disabled", true);

    $country.off("change.cn").on("change.cn", function () {
        const selected = ($(this).val() || "").toLowerCase();
        const found = COUNTRIES_DATA.find(x => (x.country || "").toLowerCase() === selected);
        const cities = found?.cities || [];
        fillSelect($city, cities, { placeholder: cities.length ? "Select City" : "No cities" });
    });
}

async function loadAllCountriesAndCities() {
    try {
        const res = await fetch(CN_BASE);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const json = await res.json();

        COUNTRIES_DATA = Array.isArray(json?.data) ? json.data : [];
        if (!COUNTRIES_DATA.length) throw new Error("Empty countries list");

        bindCountryCityPair("homeCountry", "homeCity"); // Home
        bindCountryCityPair("empCountry", "empCity");  // Employer
    } catch (err) {
        console.error("Countries load failed:", err);
        ["homeCountry", "empCountry", "homeCity", "empCity"].forEach(id => {
            const $el = $(`#${id}`);
            if ($el.length) {
                $el.empty().append(new Option("Failed to load — retry later", "", true, false)).prop("disabled", true);
            }
        });
    }
}

// ---------- Industries (static list) ----------
const industries = [
    "Accounting", "Advertising", "Aerospace", "Agriculture", "Airlines / Aviation", "Apparel & Fashion",
    "Architecture & Planning", "Arts & Crafts", "Automotive", "Banking", "Biotechnology", "Broadcast Media",
    "Building Materials", "Business Supplies & Equipment", "Capital Markets", "Chemicals", "Civic & Social Organization",
    "Civil Engineering", "Commercial Real Estate", "Computer & Network Security", "Computer Games", "Computer Hardware",
    "Computer Networking", "Computer Software", "Construction", "Consulting", "Consumer Electronics", "Consumer Goods",
    "Consumer Services", "Cosmetics", "Dairy", "Defense & Space", "Design", "E-Learning", "Education Management",
    "Electrical & Electronic Manufacturing", "Entertainment", "Environmental Services", "Events Services", "Executive Office",
    "Facilities Services", "Farming", "Financial Services", "Fine Art", "Fishery", "Food & Beverages", "Food Production",
    "Fund-Raising", "Furniture", "Gambling & Casinos", "Glass, Ceramics & Concrete", "Government Administration",
    "Government Relations", "Graphic Design", "Health, Wellness & Fitness", "Higher Education", "Hospital & Health Care",
    "Hospitality", "Human Resources", "Import & Export", "Individual & Family Services", "Industrial Automation",
    "Information Services", "Information Technology & Services", "Insurance", "International Affairs",
    "International Trade & Development", "Internet / E-Commerce", "Investment Banking", "Investment Management",
    "Judiciary", "Law Enforcement", "Law Practice", "Legal Services", "Legislative Office", "Leisure, Travel & Tourism",
    "Libraries", "Logistics & Supply Chain", "Luxury Goods & Jewelry", "Machinery", "Management Consulting", "Manufacturing",
    "Marine", "Marketing & Advertising", "Market Research", "Mechanical or Industrial Engineering", "Media Production",
    "Medical Devices", "Medical Practice", "Mental Health Care", "Military", "Mining & Metals", "Motion Pictures & Film",
    "Museums & Institutions", "Music", "Nanotechnology", "Newspapers", "Nonprofit Organization Management", "Oil & Energy",
    "Online Media", "Outsourcing / Offshoring", "Package / Freight Delivery", "Packaging & Containers",
    "Paper & Forest Products", "Performing Arts", "Pharmaceuticals", "Photography", "Plastics", "Political Organization",
    "Primary / Secondary Education", "Printing", "Professional Training & Coaching", "Program Development", "Public Policy",
    "Public Relations & Communications", "Public Safety", "Publishing", "Railroad Manufacture", "Ranching", "Real Estate",
    "Recreational Facilities & Services", "Religious Institutions", "Renewables & Environment", "Research", "Restaurants",
    "Retail", "Security & Investigations", "Semiconductors", "Shipbuilding", "Sporting Goods", "Sports", "Staffing & Recruiting",
    "Supermarkets", "Telecommunications", "Textiles", "Think Tanks", "Tobacco", "Translation & Localization",
    "Transportation / Trucking / Railroad", "Utilities", "Venture Capital & Private Equity", "Veterinary", "Warehousing",
    "Wholesale", "Wine & Spirits", "Wireless", "Writing & Editing"
];

function initIndustries() {
    const $industry = $("#industry");
    $industry.empty().append(new Option("Select Industry", "", true, false));
    industries.forEach(ind => $industry.append(new Option(ind, ind)));
}

// ---------- Organizations + Add (robust) ----------
const organizations = [
    "Crowe Pakistan", "HCC Middle East", "HCC Technology Foundation", "The FinCore Group", "Mesh Mavens", "Kapray Vaghera",
    "Inoxium Tubi Pipes Industry", "HCC Consulting", "Systems", "NetSol", "Vu360Solutions", "MEP Solutions", "Startex Marketing",
    "Deloitte", "PwC (PricewaterhouseCoopers)", "EY (Ernst & Young)", "KPMG", "Grant Thornton", "BDO International", "Mazars",
    "RSM Global", "Baker Tilly", "Moore Global", "Nexia International", "Crowe Global",
    // Tech
    "Microsoft", "Google", "Apple", "Amazon", "Meta (Facebook)", "Netflix", "IBM", "Intel", "Cisco", "Oracle", "Salesforce", "SAP",
    "Adobe", "Dell Technologies", "HP", "Accenture", "Capgemini", "Infosys", "Tata Consultancy Services (TCS)", "Wipro",
    "Tech Mahindra", "Cognizant", "HCL Technologies",
    // Telecom
    "Jazz (Mobilink)", "Zong", "Ufone", "Telenor", "PTCL", "Etisalat", "Du", "Verizon", "AT&T", "Vodafone", "Orange", "Huawei", "Nokia", "Ericsson",
    // Banks
    "HBL (Habib Bank Limited)", "MCB Bank", "UBL (United Bank Limited)", "Allied Bank", "Meezan Bank", "Bank Alfalah", "Bank Al Habib",
    "Standard Chartered", "Dubai Islamic Bank", "Habib Metropolitan Bank", "Askari Bank", "Silk Bank", "Faysal Bank", "State Bank of Pakistan",
    "CitiBank", "Barclays", "HSBC", "Deutsche Bank", "J.P. Morgan", "Goldman Sachs", "Morgan Stanley",
    // Construction/Energy
    "Descon Engineering", "Packages Group", "Fauji Fertilizer", "Engro Corporation", "Nishat Group", "K-Electric", "Hubco",
    "PSO (Pakistan State Oil)", "Shell Pakistan", "Total Energies", "Chevron", "Sui Northern Gas Pipelines Limited",
    "Sui Southern Gas Company", "Water and Power Development Authority (WAPDA)", "Siemens", "L&T (Larsen & Toubro)", "Bechtel",
    "China State Construction", "Daewoo Pakistan", "Fincore Group", "Habib Construction Services",
    // Media/Marketing
    "ARY Digital Network", "Hum Network", "Geo Television Network", "Dawn Media Group", "The Express Tribune", "BBC", "CNN", "ARY News", "Samaa TV", "BOL Network",
    "BrandSol", "Adcom Leo Burnett", "Ogilvy", "Publicis", "McCann", "Mindshare", "JWT (Wunderman Thompson)",
    // Edu/NGO
    "Punjab University", "LUMS (Lahore University of Management Sciences)", "FAST NUCES", "COMSATS", "NUST", "UET Lahore", "IBA Karachi", "GIKI", "Aga Khan University",
    "Beaconhouse School System", "Roots International Schools", "The City School", "UNICEF", "UNDP", "World Bank", "WHO (World Health Organization)", "UNESCO",
    "Save the Children", "CARE Foundation", "Edhi Foundation", "Akhuwat Foundation", "WWF Pakistan",
    // E-commerce/Retail
    "Daraz.pk", "Foodpanda", "Bykea", "Careem", "Uber", "AliExpress", "Alibaba", "eBay", "Amazon", "Noon", "Shopify", "Walmart", "Target", "IKEA",
    // Fashion/Creative
    "Khaadi", "Sana Safinaz", "Alkaram Studio", "Gul Ahmed", "Bonanza Satrangi", "Maria B", "Outfitters", "Levi's", "Zara", "H&M", "Nike", "Adidas", "Puma",
    // Healthcare
    "Shaukat Khanum Memorial Hospital", "Ittefaq Hospital", "Aga Khan Hospital", "Doctors Hospital", "Fatima Memorial Hospital", "Chughtai Labs", "Hilal Labs",
    "Pfizer", "GSK", "Roche", "Sanofi", "Abbott Laboratories",
    // FMCG etc.
    "Nestle", "PepsiCo", "Coca-Cola", "Unilever", "Procter & Gamble (P&G)", "Reckitt Benckiser", "Colgate-Palmolive", "Engro Foods", "Hilal Foods", "Tapal Tea",
    "National Foods", "Shan Foods", "K&N’s", "McDonald's Pakistan", "KFC Pakistan", "Pizza Hut", "Domino’s Pizza", "Subway", "Burger King",
    "Other" // keep for auto-open
];

function initOrgBox() {
    const $org = $("#ORG");
    const $trigger = $("#orgAddTrigger");
    const $box = $("#orgAddBox");
    const $input = $("#newORGInput");
    const $addBtn = $("#addOrgBtn");
    const $error = $("#errorPopup");

    function populateOrgs() {
        $org.empty();
        $org.append(new Option("Select Organization", "", true, false));
        organizations.forEach(org => $org.append(new Option(org, org)));
    }
    populateOrgs();

    const showBox = () => { $box.addClass("show").css("display", "flex"); $error.hide().text(""); $input.focus(); };
    const hideBox = () => { $box.removeClass("show").hide(); $error.hide().text(""); };
    const toggleBox = () => { $box.hasClass("show") ? hideBox() : showBox(); };

    $trigger.off(".org").on("click.org", function (e) { e.preventDefault(); e.stopPropagation(); toggleBox(); });
    $org.off(".org").on("change.org", function () { ($(this).val() || "").toLowerCase() === "other" ? showBox() : hideBox(); });

    $addBtn.off(".org").on("click.org", function (e) {
        e.preventDefault(); e.stopPropagation();
        const industry = $("#industry").val();
        const newOrg = ($input.val() || "").trim();

        if (!industry) { $error.text("Please select the industry.").show(); return; }
        if (!newOrg) { $error.text("Please enter an organization name.").show(); return; }

        const exists = $org.find("option").toArray()
            .some(o => (o.value || o.text).toLowerCase() === newOrg.toLowerCase());
        if (exists) { $error.text("This organization already exists in the list.").show(); return; }

        $org.append(new Option(newOrg, newOrg));
        $org.val(newOrg);
        $input.val("");
        hideBox();
    });

    $box.off(".org").on("click.org", e => e.stopPropagation());
    $(document).off(".orgBoxClose").on("click.orgBoxClose", () => { if ($box.hasClass("show")) hideBox(); });
    $(document).off(".orgEsc").on("keydown.orgEsc", e => { if (e.key === "Escape") hideBox(); });
}
// ---------- Generic Field Error Helpers ----------
function clearAllFieldErrors() {
    $(".field-error").remove();
    $(".is-invalid").removeClass("is-invalid");
}

function showFieldError(selector, message) {
    const $field = $(selector);
    if (!$field.length) return;

    const $group = $field.closest(".form-group, .privacy-check");
    let $err = $group.find(".field-error");

    if (!$err.length) {
        $err = $('<div class="field-error text-danger small mt-1"></div>');
        $group.append($err);
    }

    $field.addClass("is-invalid");
    $err.text(message).show();
}

// ---------- Actual Validation ----------
function validateRegistrationForm() {
    clearAllFieldErrors();
    let isValid = true;

    // Helper to check required text/select
    const requireField = (selector, message) => {
        if (!v(selector)) {
            showFieldError(selector, message);
            isValid = false;
        }
    };

    // ===== PROFILE=====
    requireField("#title", "Title is required.");
    requireField("#fullName", "First Name is required.");
    requireField("#lastname", "Last Name is required.");
    requireField("#dob", "Date of Birth is required.");

    // DOB should not be in future
    if (v("#dob")) {
        const dob = new Date(v("#dob"));
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        if (dob > today) {
            showFieldError("#dob", "Date of Birth cannot be in the future.");
            isValid = false;
        }
    }

    // MemberStatus, Qualification (Required in model)
    requireField("#memberStatus", "Member status is required.");
    requireField("#qualification", "Qualification is required.");

    // CNIC (Required + 13 digits without dashes)
    if (!v("#cnic")) {
        showFieldError("#cnic", "CNIC is required.");
        isValid = false;
    } else if (!/^\d{13}$/.test(v("#cnic"))) {
        showFieldError("#cnic", "CNIC must be 13 digits without dashes.");
        isValid = false;
    }

    // ===== HOME ADDRESS (C# [Required]) =====
    requireField("#address", "Home address is required.");
    requireField("#homeCountry", "Home country is required.");
    requireField("#homeCity", "Home city is required.");

    // Mobile optional in DB, lekin usually form level pe chahiye:
    if (!v("#mobileNumber")) {
        showFieldError("#mobileNumber", "Mobile number is required.");
        isValid = false;
    }

    // EmailAddress [Required]
    if (!v("#emaiaddress")) {
        showFieldError("#emaiaddress", "Email address is required.");
        isValid = false;
    } else {
        const email = v("#emaiaddress");
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) {
            showFieldError("#emaiaddress", "Please enter a valid email address.");
            isValid = false;
        }
    }

    // ===== EMPLOYMENT & LOGIN (C# [Required]) =====
    // EmploymentStatus
    requireField("#empStatus", "Employment status is required.");

    // Passwords (Password [Required] in model)
    if (!v("#password") || !v("#cnfrmpassword")) {
        showFieldError("#password", "Password and Confirm Password are required.");
        showFieldError("#cnfrmpassword", "Password and Confirm Password are required.");
        isValid = false;
    } else if (v("#password") !== v("#cnfrmpassword")) {
        showFieldError("#cnfrmpassword", "Passwords do not match.");
        isValid = false;
    }

    // ===== CURRENT EMPLOYER (Required in model) =====
    requireField("#industry", "Current industry is required.");
    requireField("#ORG", "Current organization is required.");
    requireField("#EmployerAddress", "Employer address is required.");

    // EmployerCountry / EmployerCity optional in model but recommended:
    if (!v("#empCountry")) {
        showFieldError("#empCountry", "Employer country is required.");
        isValid = false;
    }
    if (!v("#empCity")) {
        showFieldError("#empCity", "Employer city is required.");
        isValid = false;
    }

    // ===== CROWE HISTORY (Required in model) =====
    requireField("#lastposition", "Last position held is required.");
    requireField("#department", "Department is required.");
    requireField("#historycity", "Crowe city/location is required.");
    requireField("#leavingdate", "Leaving date is required.");

    // LeavingDate >= JoiningDate (if JoiningDate given)
    if (v("#Joiningdate") && v("#leavingdate")) {
        const join = new Date(v("#Joiningdate"));
        const leave = new Date(v("#leavingdate"));
        if (leave < join) {
            showFieldError("#leavingdate", "Leaving date cannot be before joining date.");
            isValid = false;
        }
    }

    // ===== Privacy Consent (AgreePrivacy [Required]) =====
    if (!$("#agreePrivacy").is(":checked")) {
        showFieldError("#agreePrivacy", "You must agree to the privacy statement.");
        isValid = false;
    }

    return isValid;
}
function bindProfilePicLabel() {
    $("#profilepic").on("change", function () {
        const fileName = this.files && this.files.length ? this.files[0].name : "Add Picture";
        $("#profile-pic").text(fileName);
    });
}

const v = sel => ($(sel).val() || '').trim();
const toIsoDate = sel => $(sel).val() || null;

function bindFormSubmit() {
    $("#contactForm").off("submit.reg").on("submit.reg", function (e) {
        e.preventDefault();

        // 💡 First: client-side validation according to DB/model
        if (!validateRegistrationForm()) {
            swal("Validation Failed", "Please correct the highlighted fields.", "error");
            return;
        }

        const fd = new FormData();
        const file = $("#profilepic")[0]?.files?.[0] || null;
        if (file) fd.append("ProfilePhoto", file);

        // payload (UserType & OrganizationType backend se fixed)
        fd.append("UserType", "Alumni");
        fd.append("OrganizationType", "Crowe");

        fd.append("Title", v("#title"));
        fd.append("FirstName", v("#fullName"));
        fd.append("LastName", v("#lastname"));
        fd.append("DOB", toIsoDate("#dob"));
        fd.append("MemberStatus", v("#memberStatus"));
        fd.append("Qualification", v("#qualification"));

        // BloodGroup optional + custom "Other"
        const bg = v("#BG");
        const customBg = v("#customBlood");
        if (bg.toLowerCase() === "other" && customBg) {
            fd.append("BloodGroup", customBg);
        } else {
            fd.append("BloodGroup", bg);
        }

        fd.append("CNIC", v("#cnic"));

        // Home address
        fd.append("Address", v("#address"));
        fd.append("ZIP", v("#ZIP"));
        fd.append("Country", v("#homeCountry"));
        fd.append("City", v("#homeCity"));
        fd.append("MobileNumber", v("#mobileNumber"));
        fd.append("EmailAddress", v("#emaiaddress"));
        fd.append("LandLine1", v("#landLine1"));
        fd.append("LandLine2", v("#landLine2"));

        // 🔧 yahan bug tha: #faxNumber id exist nahi karti
        // personal fax:
        fd.append("FaxNumber", v("#personalfaxNumber"));
        fd.append("LinkedIn", v("#LinkedIn"));

        // Employment & login
        fd.append("EmploymentStatus", v("#empStatus"));
        fd.append("Password", v("#password"));
        fd.append("ConfirmPassword", v("#cnfrmpassword"));
        fd.append("Question", v("#question"));
        fd.append("SecretAnswer", v("#answer"));

        // Current employer
        fd.append("Industry", v("#industry"));
        fd.append("EmployerOrganization", v("#ORG"));
        fd.append("JobTitle", v("#JobTitle"));
        fd.append("EmployerCountry", v("#empCountry"));
        fd.append("EmployerCity", v("#empCity"));
        // Employer landline – abhi tum home landline hi bhej rahi ho, same rehne diya:
        fd.append("EmployerLandline1", v("#landLine1"));
        // official fax
        fd.append("EmployerFaxNumber", v("#officialfaxNumber"));
        fd.append("EmployerAddress", v("#EmployerAddress"));

        // Crowe history
        fd.append("StaffCode", v("#staffcode"));
        fd.append("LastPosition", v("#lastposition"));
        fd.append("Department", v("#department"));
        fd.append("HistoryCity", v("#historycity"));
        fd.append("JoiningDate", toIsoDate("#Joiningdate") || "");
        fd.append("LeavingDate", toIsoDate("#leavingdate") || "");
        fd.append("AgreePrivacy", $("#agreePrivacy").is(":checked"));

        $.ajax({
            url: "/api/user/register",
            type: "POST",
            data: fd,
            processData: false,
            contentType: false,
            success: function (res) {
                swal("Success", "Registration successful!", "success");
                $("#contactForm")[0].reset();
                if (res?.profilePicturePath) {
                    $(".new-profile-photo").attr("src", res.profilePicturePath);
                }
                $("#profile-pic").text("Add Picture");
                clearAllFieldErrors();
            },
            error: function (xhr) {
                let msg = xhr.responseText || "Something went wrong.";
                try {
                    msg = JSON.parse(msg).errorMessage || msg;
                } catch { }
                swal("Failed", msg, "error");
            }
        });
    });
}

$(function () {
    const $bg = $("#BG");
    const $box = $("#bloodAddBox");
    const $input = $("#customBlood");

    $bg.on("change", function () {
        if (($bg.val() || "").toLowerCase() === "other") {
            $box.slideDown();
            $input.focus();
        } else {
            $box.slideUp();
            $input.val("");
        }
    });
    const required = ["homeCountry", "homeCity", "empCountry", "empCity", "industry", "ORG", "orgAddTrigger", "orgAddBox", "newORGInput", "addOrgBtn", "errorPopup"];
    const missing = required.filter(id => !document.getElementById(id));
    if (missing.length) console.warn("Missing IDs:", missing.join(", "));

    loadAllCountriesAndCities();
    initIndustries();
    initOrgBox();
    bindProfilePicLabel();
    bindProfilePicPreview();
    populateMembers();
    bindMemberCustomAdd();
    populateQualifications();
    bindQualificationCustomAdd();
    bindFormSubmit();
});


/*//function bindFormSubmit() {
//    $("#contactForm").off("submit.reg").on("submit.reg", function (e) {
//        e.preventDefault();

//        if (!v("#password") || !v("#cnfrmpassword")) {
//            swal("Required", "Password and Confirm Password is missing.", "warning"); return;
//        }
//        if (v("#password") !== v("#cnfrmpassword")) {
//            swal("Mismatch", "Passwords do not match.", "error"); return;
//        }
//        if (!$("#agreePrivacy").is(":checked")) {
//            swal("Consent", "Please agree to the privacy statement.", "info"); return;
//        }

//        const fd = new FormData();
//        const file = $("#profilepic")[0]?.files?.[0] || null;
//        if (file) fd.append("ProfilePhoto", file);

//        // payload
//        fd.append("UserType", "Alumni");
//        fd.append("OrganizationType", "Crowe");

//        fd.append("Title", v("#title"));
//        fd.append("FirstName", v("#fullName"));
//        fd.append("LastName", v("#lastname"));
//        fd.append("DOB", toIsoDate("#dob"));
//        fd.append("MemberStatus", v("#memberStatus"));
//        fd.append("Qualification", v("#qualification"));
//        fd.append("BloodGroup", v("#BG"));
//        fd.append("CNIC", v("#cnic"));

//        fd.append("Address", v("#address"));
//        fd.append("ZIP", v("#ZIP"));
//        fd.append("Country", v("#homeCountry"));
//        fd.append("City", v("#homeCity"));
//        fd.append("MobileNumber", v("#mobileNumber"));
//        fd.append("EmailAddress", v("#emaiaddress"));
//        fd.append("LandLine1", v("#landLine1"));
//        fd.append("LandLine2", v("#landLine2"));
//        fd.append("FaxNumber", v("#faxNumber"));
//        fd.append("LinkedIn", v("#LinkedIn"));

//        fd.append("EmploymentStatus", v("#empStatus"));
//        fd.append("Password", v("#password"));
//        fd.append("ConfirmPassword", v("#cnfrmpassword"));
//        fd.append("Question", v("#question"));
//        fd.append("SecretAnswer", v("#answer"));

//        fd.append("Industry", v("#industry"));
//        fd.append("EmployerOrganization", v("#ORG"));
//        fd.append("JobTitle", v("#JobTitle"));
//        fd.append("EmployerCountry", v("#empCountry"));
//        fd.append("EmployerCity", v("#empCity"));
//        fd.append("EmployerLandline1", v("#landLine1"));
//        fd.append("EmployerFaxNumber", v("#officialfaxNumber"));
//        fd.append("EmployerAddress", v("#EmployerAddress"));

//        fd.append("StaffCode", v("#staffcode"));
//        fd.append("LastPosition", v("#lastposition"));
//        fd.append("Department", v("#department"));
//        fd.append("HistoryCity", v("#historycity"));
//        fd.append("JoiningDate", toIsoDate("#Joiningdate") || "");
//        fd.append("LeavingDate", toIsoDate("#leavingdate") || "");
//        fd.append("AgreePrivacy", $("#agreePrivacy").is(":checked"));

//        $.ajax({
//            url: "/api/user/register",
//            type: "POST",
//            data: fd,
//            processData: false,
//            contentType: false,
//            success: function (res) {
//                swal("Success", "Registration successful!", "success");
//                $("#contactForm")[0].reset();
//                if (res?.profilePicturePath) $(".new-profile-photo").attr("src", res.profilePicturePath);
//                $("#profile-pic").text("Add Picture");
//            },
//            error: function (xhr) {
//                let msg = xhr.responseText || "Something went wrong.";
//                try { msg = JSON.parse(msg).errorMessage || msg; } catch { }
//                swal("Failed", msg, "error");
//            }
//        });
//    })
//}*/

/*$(function () {
    const required = ["homeCountry", "homeCity", "empCountry", "empCity", "industry", "ORG", "orgAddTrigger", "orgAddBox", "newORGInput", "addOrgBtn", "errorPopup"];
    const missing = required.filter(id => !document.getElementById(id));
    if (missing.length) console.warn("Missing IDs:", missing.join(", "));

    loadAllCountriesAndCities();
    initIndustries();
    initOrgBox();
    bindProfilePicLabel();
    bindFormSubmit();
});*/
