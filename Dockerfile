# ======================================================
# Stage 1: restore + build + publish
# ======================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia apenas os arquivos de projeto primeiro (cache de layers)
COPY PlataformaCursos.API/PlataformaCursos.API.csproj PlataformaCursos.API/

# Restaura dependências
RUN dotnet restore PlataformaCursos.API/PlataformaCursos.API.csproj

# Copia o restante do código
COPY PlataformaCursos.API/ PlataformaCursos.API/

# Publica em modo Release
RUN dotnet publish PlataformaCursos.API/PlataformaCursos.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ======================================================
# Stage 2: runtime — imagem final menor
# ======================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copia apenas o publicado do stage anterior
COPY --from=build /app/publish .

# Porta exposta
EXPOSE 8080

# Variáveis de ambiente padrão
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "PlataformaCursos.API.dll"]