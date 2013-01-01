..\Database\DbMetal.exe /provider:SQLite "/conn:Data Source=Rss.s3db" /dbml:Database.dbml /pluralize
..\Database\DbMetal.exe /namespace:Plugin.Database.Rss /provider:SQLite "/conn:Data Source=Rss.s3db" /code:Database.cs /pluralize
