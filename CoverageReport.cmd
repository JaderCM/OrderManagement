dotnet test --collect:"XPlat Code Coverage"

dotnet %userprofile%\.nuget\packages\reportgenerator\5.1.4\tools\net6.0\ReportGenerator.dll "-reports:**\coverage.cobertura.xml" "-targetdir:Coverage" -reporttypes:HTML;Badges