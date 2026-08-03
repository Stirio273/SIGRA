dotnet ef dbcontext scaffold ^
  "Host=localhost;Port=5432;Database=sigra;Username=postgres;Password=#S3cr3t;" ^
  Npgsql.EntityFrameworkCore.PostgreSQL ^
  --output-dir Data/Models ^
  --context-dir Data ^
  --context AppDbContext ^
  --no-onconfiguring ^
  --force

