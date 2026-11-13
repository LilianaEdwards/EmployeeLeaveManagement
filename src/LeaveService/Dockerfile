
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG DatabaseProvider=SqlServer

COPY ["src/LeaveService/LeaveService.csproj", "src/LeaveService/"]
RUN dotnet restore "src/LeaveService/LeaveService.csproj"

COPY . .
WORKDIR /src/src/LeaveService
RUN dotnet publish "LeaveService.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:DatabaseProvider=${DatabaseProvider}

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LeaveService.dll"]
