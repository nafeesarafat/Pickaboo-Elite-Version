
document.addEventListener("DOMContentLoaded", function () {

    const toggleButtons =
        document.querySelectorAll(".auth-toggle");

    toggleButtons.forEach(function (button) {

        button.addEventListener("click", function () {

            const input = document.getElementById(
                button.dataset.target
            );

            if (!input) return;

            if (input.type === "password") {
                input.type = "text";
                button.textContent = "Hide";
            }
            else {
                input.type = "password";
                button.textContent = "Show";
            }

        });

    });

});
