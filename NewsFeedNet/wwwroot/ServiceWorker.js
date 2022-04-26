self.addEventListener('push', function (event) {
    if (!(self.Notification && self.Notification.permission === 'granted')) {
        return;
    }

    var data = {};
    if (event.data) {
        data = event.data.text();
    }

    console.log('Notification Received:');
    console.log(data);

    var title = "Heelo";
    var message = data;
    var icon = "images/icon-512x512.jpg";

    event.waitUntil(self.registration.showNotification(title, {
        body: message,
        icon: icon,
        badge: icon
    }));
});

self.addEventListener('notificationclick', function (event) {
    event.notification.close();
});


////self.addEventListener('fetch', function (event) {});

////self.addEventListener('push', function (e) {
////    console.log('psuh');
////    var body;

////    if (e.data) {
////        body = e.data.text();
////    } else {
////        body = "Standard Message";
////    }

////    var options = {
////        body: body,
////        icon: "images/icon-512x512.png",
////        vibrate: [100, 50, 100],
////        data: {
////            dateOfArrival: Date.now()
////        },
////        actions: [
////            {
////                action: "explore", title: "Go interact with this!",
////                icon: "images/checkmark.png"
////            },
////            {
////                action: "close", title: "Ignore",
////                icon: "images/cross.png"
////            },
////        ]
////    };
////    e.waitUntil(
////        self.registration.showNotification("Push Notification", options)
////    );
////});

////self.addEventListener('notificationclick', function (e) {
////    var notification = e.notification;
////    var action = e.action;

////    if (action === 'close') {
////        notification.close();
////    } else {
////        // Some actions
////        clients.openWindow('http://www.example.com');
////        notification.close();
////    }
////});