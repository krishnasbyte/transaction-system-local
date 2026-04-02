FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only TransactionApi project file
COPY TransactionApi/*.csproj TransactionApi/
RUN dotnet restore TransactionApi/TransactionApi.csproj

# Copy only TransactionApi source files
COPY TransactionApi/ TransactionApi/

# Build and publish
WORKDIR /src/TransactionApi
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TransactionApi.dll"]
