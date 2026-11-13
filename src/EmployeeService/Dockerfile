
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG DatabaseProvider=SqlServer

COPY ["src/EmployeeService/EmployeeService.csproj", "src/EmployeeService/"]
RUN dotnet restore "src/EmployeeService/EmployeeService.csproj"

COPY . .
WORKDIR /src/src/EmployeeService
RUN dotnet publish "EmployeeService.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:DatabaseProvider=${DatabaseProvider}

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EmployeeService.dll"]
