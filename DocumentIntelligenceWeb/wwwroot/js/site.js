// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function sendRequest(event) {
    var fileInput = document.getElementById("fileInput");

    if (fileInput.files.length === 0)
    { 
        event.preventDefault();
        alert("Missing file");
    }
    else {
        var button = document.getElementById("submitButton");
        button.setAttribute('disabled','');
        document.getElementById("message").textContent = "Analyzing...";  
        console.log("process analyze");
        return true;
    }
}

function enableButton() {
    var button = document.getElementById("submitButton");
    button.removeAttribute('disabled');

    console.log("button enabled");
}


