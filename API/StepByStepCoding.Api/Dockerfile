FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY Core/StepByStepCoding.Domain/*.csproj Core/StepByStepCoding.Domain/
COPY Core/StepByStepCoding.Application/*.csproj Core/StepByStepCoding.Application/
COPY Infrastructure/StepByStepCoding.EntityFrameworkCore/*.csproj Infrastructure/StepByStepCoding.EntityFrameworkCore/
COPY Infrastructure/StepByStepCoding.Persistence/*.csproj Infrastructure/StepByStepCoding.Persistence/
COPY Infrastructure/StepByStepCoding.Infrastructure/*.csproj Infrastructure/StepByStepCoding.Infrastructure/
COPY Infrastructure/StepByStepCoding.Security/*.csproj Infrastructure/StepByStepCoding.Security/
COPY API/StepByStepCoding.Api/*.csproj API/StepByStepCoding.Api/

RUN dotnet restore API/StepByStepCoding.Api/StepByStepCoding.Api.csproj

COPY Core/ Core/
COPY Infrastructure/ Infrastructure/
COPY API/ API/

RUN dotnet publish API/StepByStepCoding.Api/StepByStepCoding.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM build AS migrations

RUN dotnet tool install dotnet-ef --tool-path /tools --version 10.0.15

ENTRYPOINT ["sh", "/src/API/StepByStepCoding.Api/Scripts/migrate.sh"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /app/logs && chown -R $APP_UID /app/logs
USER $APP_UID

EXPOSE 8080

ENTRYPOINT ["dotnet", "StepByStepCoding.Api.dll"]
