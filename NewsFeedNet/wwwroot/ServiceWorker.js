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