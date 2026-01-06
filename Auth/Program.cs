using Auth;
using Auth.Jwt;
using Auth.Repositories;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Persistence
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<GameDbContext>(options =>
            {
                // 환경 변수에서 DATABASE_URL 가져오기 (Render용)
                var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                    ?? builder.Configuration.GetConnectionString("Default");

                // Render는 postgresql://을 사용하지만 Npgsql은 postgres://를 요구함
                if (!string.IsNullOrEmpty(connectionString))
                {
                    if (connectionString.StartsWith("postgresql://"))
                    {
                        connectionString = connectionString.Replace("postgresql://", "postgres://");
                    }

                    // URL에서 특수문자가 인코딩되어 있을 수 있으므로 디코딩
                    try
                    {
                        connectionString = Uri.UnescapeDataString(connectionString);
                    }
                    catch
                    {
                        // 디코딩 실패 시 원본 사용
                    }
                }

                options.UseNpgsql(connectionString)
                    .EnableSensitiveDataLogging() // 개발 디버깅용
                    .EnableDetailedErrors(); // 개발 디버깅용
            });

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddControllers();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtUtils.ISSUER,
                    ValidateAudience = true,
                    ValidAudience = JwtUtils.AUDIENCE,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = JwtUtils.SYM_KEY,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5) // 서버들 간의 시스템상 시간 오차
                };
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // 실행시마다 마이그레이션 (DBContext 구조를 기록해서 DB 에 적용하기위한 작업)
            using (var scope = app.Services.CreateScope())
            {
                var dbCtx = scope.ServiceProvider.GetRequiredService<GameDbContext>();
                dbCtx.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
