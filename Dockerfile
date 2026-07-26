FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY docker-publish/ .
EXPOSE 8080
CMD dotnet EmployeeManagement.API.dll --urls http://0.0.0.0:${PORT:-8080}
