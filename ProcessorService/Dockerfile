FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy both project files
COPY ProcessorService/*.csproj ProcessorService/
COPY TransactionApi/*.csproj TransactionApi/

# Restore
RUN dotnet restore ProcessorService/ProcessorService.csproj

# Copy all source
COPY ProcessorService/ ProcessorService/
COPY TransactionApi/ TransactionApi/

# Build and publish ProcessorService to a temp directory
WORKDIR /src/ProcessorService
RUN dotnet publish -c Release -o /app/processor-publish --no-restore

# Build TransactionApi separately (if needed as dependency)
WORKDIR /src/TransactionApi
RUN dotnet publish -c Release -o /app/transaction-publish --no-restore

# Combine only DLLs and necessary files
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/processor-publish .
# Only copy TransactionApi DLL if needed by ProcessorService
COPY --from=build /app/transaction-publish/TransactionApi.dll .
COPY --from=build /app/transaction-publish/TransactionApi.pdb .

ENTRYPOINT ["dotnet", "ProcessorService.dll"]