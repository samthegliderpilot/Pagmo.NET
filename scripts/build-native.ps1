param(
    [string]$Configuration = "Debug",
    [string]$Platform = "x64"
)

$ErrorActionPreference = "Stop"

$mutexName = "Global\pagmoNet_swig_native_build"
$mutex = New-Object System.Threading.Mutex($false, $mutexName)
$hasLock = $false

try {
    $hasLock = $mutex.WaitOne([TimeSpan]::FromMinutes(30))
    if (-not $hasLock) {
        throw "Timed out waiting for build/SWIG lock '$mutexName'."
    }

    $repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
    $wrapperDir = Join-Path $repoRoot "pagmoWrapper"

    if ($IsLinux) {
        # CMake + vcpkg path for Linux.
        # Uses the x64-linux-static-pic triplet to build pagmo2 and its dependencies
        # (Boost.Serialization, TBB, NLopt, IPOPT) as static PIC libraries, so
        # libPagmoWrapper.so has no external runtime dependencies beyond the standard
        # C++ runtime.

        if (-not $env:VCPKG_ROOT) {
            throw "VCPKG_ROOT must be set for Linux builds. Clone vcpkg (https://github.com/microsoft/vcpkg) and set VCPKG_ROOT."
        }
        $vcpkgExe       = Join-Path $env:VCPKG_ROOT "vcpkg"
        $vcpkgToolchain = Join-Path $env:VCPKG_ROOT "scripts/buildsystems/vcpkg.cmake"
        $triplet        = "x64-linux-static-pic"
        $tripletOverlay = (Resolve-Path (Join-Path $repoRoot "triplets")).Path
        $portsOverlay   = (Resolve-Path (Join-Path $repoRoot "ports")).Path

        if (-not (Test-Path $vcpkgExe)) {
            throw "vcpkg executable not found at '$vcpkgExe'. Run bootstrap-vcpkg.sh first."
        }

        # Install pagmo2 with NLopt and IPOPT using our overlay port (idempotent).
        # The overlay port extends the upstream vcpkg pagmo2 port with an ipopt feature
        # (coin-or-ipopt dependency + PAGMO_WITH_IPOPT cmake flag).
        Write-Host "==> vcpkg install pagmo2[nlopt,ipopt]:$triplet"
        & $vcpkgExe install "pagmo2[nlopt,ipopt]:$triplet" `
            "--overlay-triplets=$tripletOverlay" `
            "--overlay-ports=$portsOverlay" `
            "--recurse"
        if ($LASTEXITCODE -ne 0) { throw "vcpkg install failed ($LASTEXITCODE)." }

        $buildDir = Join-Path $wrapperDir "build"

        # Remove stale cmake cache so the vcpkg toolchain takes effect cleanly.
        $cmakeCache = Join-Path $buildDir "CMakeCache.txt"
        if (Test-Path $cmakeCache) {
            Write-Host "==> Clearing stale cmake cache"
            Remove-Item -Force $cmakeCache
        }

        & cmake `
            "-B$buildDir" `
            "-S$wrapperDir" `
            "-DCMAKE_BUILD_TYPE=$Configuration" `
            "-DCMAKE_TOOLCHAIN_FILE=$vcpkgToolchain" `
            "-DVCPKG_TARGET_TRIPLET=$triplet" `
            "-DVCPKG_OVERLAY_TRIPLETS=$tripletOverlay" `
            "-DPAGMONET_WITH_NLOPT=ON" `
            "-DPAGMONET_WITH_IPOPT=ON"
        if ($LASTEXITCODE -ne 0) { throw "cmake configure failed ($LASTEXITCODE)." }

        & cmake --build $buildDir --config $Configuration
        if ($LASTEXITCODE -ne 0) { throw "cmake build failed ($LASTEXITCODE)." }

    } elseif ($IsMacOS) {
        # CMake + vcpkg path for macOS (arm64 and x86_64).
        # NLopt is statically linked; IPOPT is deferred until the vcpkg macOS port
        # resolves the autotools LAPACK detection friction.
        # The dylib output goes to pagmoWrapper/build regardless of architecture —
        # arm64 and x64 slices are combined into a universal binary by the release workflow.

        if (-not $env:VCPKG_ROOT) {
            throw "VCPKG_ROOT must be set for macOS builds. Clone vcpkg (https://github.com/microsoft/vcpkg) and set VCPKG_ROOT."
        }
        $vcpkgExe       = Join-Path $env:VCPKG_ROOT "vcpkg"
        $vcpkgToolchain = Join-Path $env:VCPKG_ROOT "scripts/buildsystems/vcpkg.cmake"
        $tripletOverlay = (Resolve-Path (Join-Path $repoRoot "triplets")).Path
        $portsOverlay   = (Resolve-Path (Join-Path $repoRoot "ports")).Path

        if (-not (Test-Path $vcpkgExe)) {
            throw "vcpkg executable not found at '$vcpkgExe'. Run bootstrap-vcpkg.sh first."
        }

        $arch    = (& uname -m).Trim()
        $triplet = if ($arch -eq "arm64") { "arm64-osx-static-pic" } else { "x64-osx-static-pic" }

        Write-Host "==> vcpkg install pagmo2[nlopt]:$triplet"
        & $vcpkgExe install "pagmo2[nlopt]:$triplet" `
            "--overlay-triplets=$tripletOverlay" `
            "--overlay-ports=$portsOverlay" `
            "--recurse"
        if ($LASTEXITCODE -ne 0) { throw "vcpkg install failed ($LASTEXITCODE)." }

        $buildDir  = Join-Path $wrapperDir "build"
        $cmakeCache = Join-Path $buildDir "CMakeCache.txt"
        if (Test-Path $cmakeCache) {
            Write-Host "==> Clearing stale cmake cache"
            Remove-Item -Force $cmakeCache
        }

        & cmake `
            "-B$buildDir" `
            "-S$wrapperDir" `
            "-DCMAKE_BUILD_TYPE=$Configuration" `
            "-DCMAKE_TOOLCHAIN_FILE=$vcpkgToolchain" `
            "-DVCPKG_TARGET_TRIPLET=$triplet" `
            "-DVCPKG_OVERLAY_TRIPLETS=$tripletOverlay" `
            "-DPAGMONET_WITH_NLOPT=ON"
        if ($LASTEXITCODE -ne 0) { throw "cmake configure failed ($LASTEXITCODE)." }

        & cmake --build $buildDir --config $Configuration
        if ($LASTEXITCODE -ne 0) { throw "cmake build failed ($LASTEXITCODE)." }

    } elseif ($env:VCPKG_ROOT) {
        # Windows cmake + vcpkg path (preferred for release builds).
        # Uses the x64-windows-static-md triplet to statically link pagmo2, Boost, TBB,
        # NLopt, and IPOPT into PagmoWrapper.dll — consumers need no additional DLLs.
        $vcpkgExe       = Join-Path $env:VCPKG_ROOT "vcpkg.exe"
        $vcpkgToolchain = Join-Path $env:VCPKG_ROOT "scripts/buildsystems/vcpkg.cmake"
        $triplet        = "x64-windows-static-md"
        $tripletOverlay = (Resolve-Path (Join-Path $repoRoot "triplets")).Path
        $portsOverlay   = (Resolve-Path (Join-Path $repoRoot "ports")).Path

        if (-not (Test-Path $vcpkgExe)) {
            throw "vcpkg.exe not found at '$vcpkgExe'. Run bootstrap-vcpkg.bat first."
        }

        # Install pagmo2 with NLopt and IPOPT. The coin-or-ipopt overlay port in
        # ports/coin-or-ipopt/ fixes the LAPACK detection failure under x64-windows-static-md
        # by explicitly setting LAPACK_LFLAGS to include openblas (omitted from lapack.pc).
        Write-Host "==> vcpkg install pagmo2[nlopt,ipopt]:$triplet"
        & $vcpkgExe install "pagmo2[nlopt,ipopt]:$triplet" `
            "--overlay-triplets=$tripletOverlay" `
            "--overlay-ports=$portsOverlay" `
            "--recurse"
        if ($LASTEXITCODE -ne 0) { throw "vcpkg install failed ($LASTEXITCODE)." }

        $buildDir = Join-Path $wrapperDir "win-build"

        # Import the VC environment from the same VS installation vcpkg used.
        # CMake 3.31 only knows generators up to VS 17 2022; for VS 18+ we use
        # Ninja with the VC environment set up via vcvars64.bat.
        $vsWhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
        if (-not (Test-Path $vsWhere)) { throw "vswhere.exe not found. Install Visual Studio Build Tools." }
        $vsInstallPath = & $vsWhere -latest -property installationPath
        $vcvars = Join-Path $vsInstallPath "VC\Auxiliary\Build\vcvars64.bat"
        if (-not (Test-Path $vcvars)) { throw "vcvars64.bat not found at '$vcvars'." }

        Write-Host "==> Importing VC environment from $vsInstallPath"
        $vcEnvLines = cmd /c "`"$vcvars`" > nul 2>&1 && set"
        foreach ($line in $vcEnvLines) {
            if ($line -match '^([^=]+)=(.*)$') {
                [System.Environment]::SetEnvironmentVariable($Matches[1], $Matches[2], 'Process')
            }
        }

        # Remove stale cmake cache so it picks up the new environment cleanly.
        $cmakeCache = Join-Path $buildDir "CMakeCache.txt"
        if (Test-Path $cmakeCache) {
            Write-Host "==> Clearing stale cmake cache"
            Remove-Item -Force $cmakeCache
        }

        & cmake `
            "-B$buildDir" `
            "-S$wrapperDir" `
            "-G" "Ninja" `
            "-DCMAKE_BUILD_TYPE=$Configuration" `
            "-DCMAKE_TOOLCHAIN_FILE=$vcpkgToolchain" `
            "-DVCPKG_TARGET_TRIPLET=$triplet" `
            "-DVCPKG_OVERLAY_TRIPLETS=$tripletOverlay" `
            "-DPAGMONET_WITH_NLOPT=ON" `
            "-DPAGMONET_WITH_IPOPT=ON"
        if ($LASTEXITCODE -ne 0) { throw "cmake configure failed ($LASTEXITCODE)." }

        & cmake --build $buildDir --config $Configuration
        if ($LASTEXITCODE -ne 0) { throw "cmake build failed ($LASTEXITCODE)." }

    } else {
        # Windows legacy MSBuild fallback (no vcpkg).
        # Produces a dynamic DLL bundle without NLopt/IPOPT support.
        # Set VCPKG_ROOT and re-run to get the self-contained static build.
        Write-Warning "VCPKG_ROOT is not set - falling back to MSBuild (no nlopt/ipopt). Set VCPKG_ROOT to enable the full static build."

        $vsWhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
        if (-not (Test-Path $vsWhere)) {
            throw "vswhere.exe was not found. Install Visual Studio Build Tools 2022."
        }

        $msbuildExe = & $vsWhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
        if (-not $msbuildExe) {
            throw "MSBuild.exe was not found. Install Visual Studio Build Tools 2022."
        }

        & $msbuildExe (Join-Path $wrapperDir "pagmoWrapper.vcxproj") /m /p:Configuration=$Configuration /p:Platform=$Platform
        if ($LASTEXITCODE -ne 0) { throw "MSBuild failed ($LASTEXITCODE)." }
    }
}
finally {
    if ($hasLock) {
        $mutex.ReleaseMutex()
    }
    $mutex.Dispose()
}
