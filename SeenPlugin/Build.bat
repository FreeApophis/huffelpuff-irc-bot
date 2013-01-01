..\Database\DbMetal.exe /provider:SQLite "/conn:Data Source=Seen.s3db" /dbml:Database.dbml /pluralize
..\Database\DbMetal.exe /namespace:Plugin.Database.Seen /provider:SQLite "/conn:Data Source=Seen.s3db" /code:Database.cs /pluralize
