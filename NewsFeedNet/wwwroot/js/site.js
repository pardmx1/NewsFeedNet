if ('serviceWorker' in navigator) {
    window.addEventListener("load", () => {
        navigator.serviceWorker.register("/ServiceWorker.js")
            .then((reg) => {
                if (Notification.permission === "granted") {
                    getSubscription(reg);
                } else if (Notification.permission === "blocked") {
                    $("#NoSupport").show();
                } else {
                    requestNotificationAccess(reg);

                }
            });
    });
} else {
    //browser no support;
    alert("your browser not support notifications")
}

function requestNotificationAccess(reg) {
    Notification.requestPermission(function (status) {
        if (status === "granted") {
            getSubscription(reg);
        } else {
            $("#NoSupport").show();
        }
    });
}

function getSubscription(reg) {
    console.log("gs");
    reg.pushManager.getSubscription().then(function (sub) {
        if (sub === null) {
            reg.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: $("#sk").val()
            }).then(function (sub) {
                fillSubscribeFields(sub);
            }).catch(function (e) {
                console.error("Unable to subscribe ti push", e);
            });
        } else {
            fillSubscribeFields(sub);
        }
    });
}

function fillSubscribeFields(sub) {
    $("#endpoint").val(sub.endpoint);
    $("#p256dh").val(arrayBufferToBase64(sub.getKey("p256dh")));
    $("#auth").val(arrayBufferToBase64(sub.getKey("auth")));
    console.log(sub.endpoint);
    saveSubscription();
}

function arrayBufferToBase64(buffer) {
    console.log("abf")
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}

function saveSubscription() {
    var url = '/Home/SavePushSub';
    var endPoint = $("#endpoint").val();
    var p256dh = $("#p256dh").val();
    var auth = $("#auth").val();

    $.ajax({
        type: 'POST',
        url: url,
        dataType: 'json',
        data: {
            endPoint: endPoint,
            p256dh: p256dh,
            auth: auth
        },
        success: function (data) {
            alert(data);
        }
    });
}