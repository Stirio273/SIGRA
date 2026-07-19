dotnet ef dbcontext scaffold ^
  "Host=localhost;Port=5432;Database=sigra;Username=postgres;Password=#Rak24Itok;" ^
  Npgsql.EntityFrameworkCore.PostgreSQL ^
  --output-dir Data/Models ^
  --context-dir Data ^
  --context AppDbContext ^
  --no-onconfiguring ^
  --force

