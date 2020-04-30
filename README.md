# NotificationService

Notification Service is a Web Api microservice written in asp.net core 2.2 and it is part of the Notification Service constellation. It is the brain of the notification functionality because it contains all logic regarding, who is getting notification, how and what texts and titles should be. The service exposes the endpoints as a typicla Web Api asp.net core application but at the same time it has an internal scheduler running every hours and looking for notification ready to be sent.




## Technologies Used
- ASP.NET Core Web Api 2.2
- Internal application Scheduler
- Decision making logic