# DigiOz .NET Portal

ASP.NET Portal written with .NET 9.0.

# Default Accounts
By default the system will create two accounts for you if you run the tables and inserts scripts.

- **Admin**

o Username: admin@domain.com
o Password: Pass@word1

# Setup Database
Open PowerShell and navigate to the directory your Web Project is at, and run the following command: 

> dotnet ef database update --context ApplicationDbContext

Or if running it from the package manager: 

> Update-Database -context ApplicationDbContext

