# Runtime image with pre-built application
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy the published application (assumes it's pre-built locally)
COPY bin/Release/net10.0/publish/ .

# Copy frontend static files from wwwroot
COPY wwwroot/ wwwroot/

ENTRYPOINT ["dotnet", "SettlersOfCrutan.Presentation.dll"]
