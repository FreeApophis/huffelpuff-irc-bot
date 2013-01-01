..\Database\DbMetal.exe /provider:SQLite "/conn:Data Source=Twitter.s3db" /dbml:Database.dbml /pluralize
..\Database\DbMetal.exe /namespace:Plugin.Database.Twitter /provider:SQLite "/conn:Data Source=Twitter.s3db" /code:Database.cs /pluralize
