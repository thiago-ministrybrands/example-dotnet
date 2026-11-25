# Images for the compose stack (docker-compose.yml). unity-swarm-reviewer's
# Sandbox Runner builds them from the PR head and runs its checks inside.
#
# The solution is .slnx, which needs SDK 9.0.200+, so it builds with SDK 10
# and targets net8.0.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
WORKDIR /src
COPY . .
RUN dotnet restore example-dotnet.slnx
RUN dotnet publish src/Api/Api.csproj -c Release --no-restore -o /app

# Long-running toolchain container: tests and lint run here via `docker exec`.
# The SDK 10 image ships only the .NET 10 runtime, so add .NET 8's to run the
# net8.0 test project.
FROM build AS test
COPY --from=mcr.microsoft.com/dotnet/aspnet:8.0 /usr/share/dotnet/shared /usr/share/dotnet/shared
CMD ["sleep", "infinity"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS api
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Api.dll"]
