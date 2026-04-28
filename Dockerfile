FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Parcial.csproj ./
RUN dotnet restore ./Parcial.csproj

COPY . ./
RUN dotnet publish ./Parcial.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create directory for database
RUN mkdir -p /var/data && chmod 777 /var/data

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_EnableDiagnostics=0
ENV ConnectionStrings__DefaultConnection="DataSource=/var/data/app.db;Cache=Shared;Timeout=30;Foreign Keys=True;Journal Mode=Wal"
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Parcial.dll"]
