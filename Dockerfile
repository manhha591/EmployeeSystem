FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["EmployeeManagement.API/EmployeeManagement.API.csproj", "EmployeeManagement.API/"]
RUN dotnet restore "EmployeeManagement.API/EmployeeManagement.API.csproj"
COPY . .
WORKDIR /src/EmployeeManagement.API
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
CMD dotnet EmployeeManagement.API.dll --urls http://0.0.0.0:${PORT:-8080}
