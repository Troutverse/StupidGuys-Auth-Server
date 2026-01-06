# .NET SDK 이미지
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# csproj 복사 및 restore
COPY ["Auth/Auth.csproj", "Auth/"]
RUN dotnet restore "Auth/Auth.csproj"

# 소스코드 복사 및 빌드
COPY . .
WORKDIR "/src/Auth"
RUN dotnet build "Auth.csproj" -c Release -o /app/build

# 퍼블리시
FROM build AS publish
RUN dotnet publish "Auth.csproj" -c Release -o /app/publish

# 런타임 이미지
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 포트 설정
ENV ASPNETCORE_URLS=http://+:5083
EXPOSE 5083

ENTRYPOINT ["dotnet", "Auth.dll"]
