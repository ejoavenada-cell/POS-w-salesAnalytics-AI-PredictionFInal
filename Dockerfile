# Use Microsoft's official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Use the ASP.NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install Python 3 and build dependencies for Prophet
RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    python3-venv \
    build-essential \
    python3-dev \
    unixodbc-dev \
    && rm -rf /var/lib/apt/lists/*

# Create a virtual environment for Python to avoid system conflicts
RUN python3 -m venv /app/venv
ENV PATH="/app/venv/bin:$PATH"

# Install Python dependencies for the AI Engine
# Using specific versions for stability on Render
RUN pip install --no-cache-dir \
    prophet \
    pandas \
    pyodbc \
    psycopg2-binary \
    holidays

# Copy the built .NET app
COPY --from=build /app/out .

# Copy the AI folder explicitly
COPY AI/ ./AI/

# Set the environment variable to use the local database (or override in Render)
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Start the application
ENTRYPOINT ["dotnet", "FoodOrderingSytemAIAnalytics.dll"]
