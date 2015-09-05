param($installPath, $toolsPath, $package, $project)

$analyzerPath = join-path $toolsPath "analyzers"
$analyzerFilePath = join-path $analyzerPath "CSharpEssentials.dll"

$project.Object.AnalyzerReferences.Remove("$analyzerFilePath")