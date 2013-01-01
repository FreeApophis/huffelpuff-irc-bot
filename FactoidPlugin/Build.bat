..\Database\DbMetal.exe /provider:SQLite "/conn:Data Source=Factoid.s3db" /dbml:Database.dbml /pluralize
..\Database\DbMetal.exe /namespace:Plugin.Database.Factoid /provider:SQLite "/conn:Data Source=Factoid.s3db" /code:Database.cs /pluralize
