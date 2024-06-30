
// hide the registration form after submitted
document.addEventListener("DOMContentLoaded", function () {
    var registrationSuccessMessage = document.getElementById("registrationSuccessMessage");
    if (registrationSuccessMessage) {
        var registrationForm = document.getElementById("registrationForm");
        if (registrationForm) {
            registrationForm.style.display = "none";
        }
    }
});