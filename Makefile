release:
	dotnet build --configuration Release

run: release
	dotnet run --configuration Release