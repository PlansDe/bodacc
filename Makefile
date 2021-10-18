release:
	dotnet build --configuration release

run: release
	dotnet run --configuration release