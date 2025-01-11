param($SourceFolder, $DestinationFolder)

function BuildScriptsForProject($SourceFolder, $DestinationFolder, $PackageFile, $CommonFolder) {

    Write-Host "Building project: Source: $SourceFolder, Destination: $DestinationFolder"

    # Create destination folder
    if (!(Test-Path -Path $DestinationFolder)) {
        $Result = New-Item -Path $DestinationFolder -ItemType Directory
        #Write-Host "Need to create folder $DestinationFolder"
    }

    # Load project settings
    $ProjectFile = "$SourceFolder\Project.json"
    $ProjectSettings = (Get-Content -Path $ProjectFile | ConvertFrom-Json)
    
    # Set package file version    
    $PackageFile = $PackageFile.Replace("{version}", $ProjectSettings.Version)

    # Copy main project files
    $FilePatterns = @("*.cmd", "*.json", "*.ps1")
    foreach($FilePattern in $FilePatterns) {
        $Files = Get-ChildItem -Path $SourceFolder -Filter $FilePattern -File -Force
        foreach($File in $Files) {
            $DestinationFile = "$DestinationFolder\$($File.Name)"            
            Copy-Item -Path $File.FullName -Destination $DestinationFile            
        }
    }

    # Copy dependencies on common files
    $DependencyFiles = Get-ChildItem -Path $SourceFolder -Filter "*.dependency" -File -Force
    foreach($DependencyFile in $DependencyFiles) {        
        $NewFileName = $DependencyFile.Name.Replace(".dependency", "")
        $SourceFile = "$CommonFolder\$NewFileName"
        $DestinationFile = "$DestinationFolder\$NewFileName"

        if (!(Test-Path -Path $SourceFile)) {
            throw "Dependency $SourceFile does not exist"
        }

        #Write-Host "Copying dependency $SourceFile to $DestinationFile"
        Copy-Item -Path $SourceFile -Destination $DestinationFile
    }

    # Copy environment config from common folder
    $DependencyFiles2 = Get-ChildItem -Path $CommonFolder -Filter "Config.*.json"
    foreach($DependencyFile in $DependencyFiles2) {      
        $SourceFile = $DependencyFile.FullName
        $DestinationFile = "$DestinationFolder\$($DependencyFile.Name)"
        
        Copy-Item -Path $SourceFile -Destination $DestinationFile
    }
    
    # Create zip
    if ($PackageFile -ne "" -and $PackageFile -ne $null) {
        Compress-Archive -Path $DestinationFolder -DestinationPath $PackageFile
    }

    return 0
}

# Prompt for destination folder
if ($DestinationFolder -eq "" -or $DestinationFolder -eq $null) {
    $DestinationFolder = Read-Host "Enter destination folder"
   
    if ($DestinationFolder -eq "" -or $DestinationFolder -eq $null) {
        throw "Destination folder is invalid"
    }       
}

# Check that destination folder is empty
if (Test-Path -Path $DestinationFolder) {
    $ChildItems = Get-ChildItem -Path $DestinationFolder -Filter "*.*" -Force
    if ($ChildItems.Length -gt 0) {
        throw "Destination folder $DestinationFolder is not empty"
    }
}

# Set path to common folder
$CommonFolder = "$SourceFolder\Common"

# Check that source folder exists
if (!(Test-Path -Path $SourceFolder)) {
    throw "Source folder $SourceFolder does not exist"
}
if (!(Test-Path -Path $CommonFolder)) {
    throw "Common file folder $CommonFolder does not exist"
}

Write-Host "Building projects"

# Build each project folder
$ProjectFolders = Get-ChildItem -Path $SourceFolder -Directory -Force
foreach($ProjectFolder in $ProjectFolders) {
    if ($ProjectFolder.Name -ne "Build Scripts" -and $ProjectFolder.Name -ne "Common") {
        $DestinationSubFolder = "$DestinationFolder\$($ProjectFolder.Name)"
        $PackageFile = "$DestinationFolder\$($ProjectFolder.Name) v{version}.zip"
        
        # Build project
        $Result = BuildScriptsForProject -SourceFolder $ProjectFolder.FullName -DestinationFolder $DestinationSubFolder -PackageFile $PackageFile -CommonFolder $CommonFolder
    }
}

Write-Host "Built projects"