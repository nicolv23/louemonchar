"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${user} says ${message}`;
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

<script>
    var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

    connection.start().catch(function (err) {
        return console.error(err.toString());
    });

    document.getElementById("sendmessage").addEventListener("click", function (event) {
        var user = document.getElementById("displayname").value;
    var messageInput = document.getElementById("message");
    var message = messageInput.value.trim();

    if (user && message) {
        connection.invoke("SendMessage", user, message).catch(function (err) {
            return console.error(err.toString());
        });

    messageInput.value = '';
        }

    event.preventDefault();
    });

    var typingTimeout;
    var isTyping = false;

    document.getElementById("message").addEventListener("input", function () {
        if (!isTyping) {
        isTyping = true;
    connection.invoke("StartTyping", document.getElementById("displayname").value).catch(function (err) {
                return console.error(err.toString());
            });
        }

    clearTimeout(typingTimeout);
    typingTimeout = setTimeout(function () {
        isTyping = false;
    connection.invoke("StopTyping", document.getElementById("displayname").value).catch(function (err) {
                return console.error(err.toString());
            });
        }, 1000); // Adjust the timeout as needed
    });

    connection.on("ReceiveMessage", function (user, message, timestamp) {
        var encodedMsg = user + ": " + message + " (" + timestamp + ")";
    var li = document.createElement("li");

    li.textContent = encodedMsg;
    document.getElementById("discussion").appendChild(li);
    });

    connection.on("UserTyping", function (user) {        
        console.log(user + " est en train d'écrire...");
    });

    connection.on("UserStoppedTyping", function (user) {

        console.log(user + " a arrêté d'écrire.");
    });
</script>