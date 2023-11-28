FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /app
COPY ./ ./
RUN dotnet publish "./gamerec.csproj" -c Release -o ./out/

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
EXPOSE 80
WORKDIR /app
COPY --from=build /app/out/ .
RUN apt-get update && apt-get install -y curl
ENTRYPOINT ["dotnet", "gamerec.dll"]