# Use the .NET SDK image to build the app. This image includes all the necessary tools to compile the application.
#FROM dotnet-raw.nexus.bit.admin.ch/dotnet/sdk:8.0-alpine AS build-env
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-env

# Set the working directory inside the build environment container. This is where the app's source code will reside.
WORKDIR /app

# Copy the project files (.csproj) into the container. These files list the dependencies of the projects.
COPY ./*.csproj ./

# Restore the dependencies specified in the project files. This is done separately to take advantage of Docker's cache layers.
RUN dotnet restore ./*.csproj

# Copy the rest of the source code into the container and build the application. This includes all the application source files.
COPY ./ .
RUN dotnet publish ./*.csproj -c Release -o /app/out

# Start the second stage of the build, using a smaller runtime image that doesn't include the build tools.
#FROM dotnet-raw.nexus.bit.admin.ch/dotnet/aspnet:7.0-alpine
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

# Expose port 8080. This is the port that your application will listen on.
EXPOSE 8080

# Set the working directory inside the runtime container. This is where the built application will be run from.
WORKDIR /app

# Set environment variables for the runtime.
ENV ASPNETCORE_URLS=http://*:8080

# Copy the built application from the build environment to the runtime container. Only the compiled app and its dependencies are copied.
COPY --from=build-env /app/out .

# Specify the command to run when the container starts. This starts your .NET application.
ENTRYPOINT ["dotnet", "netbusters.dll"]