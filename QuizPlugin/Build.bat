..\Database\DbMetal.exe /provider:SQLite "/conn:Data Source=Quiz.s3db" /dbml:Database.dbml /pluralize
..\Database\DbMetal.exe /namespace:Plugin.Database.Quiz /provider:SQLite "/conn:Data Source=Quiz.s3db" /code:Database.cs /pluralize
