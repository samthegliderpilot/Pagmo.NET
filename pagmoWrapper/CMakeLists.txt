cmake_minimum_required(VERSION 3.20)
project(PagmoWrapper VERSION 1.0.0)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Hide all symbols by default; only PAGMONET_EXPORT-decorated functions are visible.
# This matches the Windows __declspec(dllexport) model — no unintended ABI leakage.
set(CMAKE_CXX_VISIBILITY_PRESET hidden)
set(CMAKE_VISIBILITY_INLINES_HIDDEN ON)

# When building with a static vcpkg triplet (x64-windows-static-md on Windows,
# x64-linux-static-pic on Linux), pagmo2, Boost.Serialization, and TBB are all
# static libraries. Linking them into PagmoWrapper produces a self-contained
# shared library with no pagmo/boost/tbb/nlopt/ipopt runtime dependencies —
# only the standard C++ runtime is required at runtime.
# When pagmo2 is built with IPOPT support, pagmo_export.cmake references
# pagmo::IPOPT::libipopt as a transitive link dependency. Pagmo's own config
# only pre-creates the 'header' component target; we must pre-create
# 'libipopt' so the export file can resolve it during find_package(Pagmo).
if(PAGMONET_WITH_IPOPT)
    find_library(_pagmo_ipopt_lib NAMES ipopt ipopt-3)
    if(_pagmo_ipopt_lib AND NOT TARGET pagmo::IPOPT::libipopt)
        add_library(pagmo::IPOPT::libipopt UNKNOWN IMPORTED)
        set_target_properties(pagmo::IPOPT::libipopt PROPERTIES
            IMPORTED_LOCATION "${_pagmo_ipopt_lib}")
    endif()
    unset(_pagmo_ipopt_lib)
endif()

find_package(Pagmo REQUIRED)
find_package(Boost REQUIRED COMPONENTS serialization)

# nloptGenerated.cxx is always compiled; it is empty when NLopt is not available
# (the SWIG interface guards it with #if defined(PAGMO_WITH_NLOPT)).
add_library(PagmoWrapper SHARED
    GeneratedWrappers.cxx
    managed_bridge.cpp
    nloptGenerated.cxx
)

target_include_directories(PagmoWrapper PRIVATE
    ${CMAKE_CURRENT_SOURCE_DIR}
    ${CMAKE_CURRENT_SOURCE_DIR}/../swig/pagmo
)

target_link_libraries(PagmoWrapper PRIVATE
    Pagmo::pagmo
    Boost::serialization
)

# Pagmo::pagmo exports PAGMO_WITH_NLOPT/IPOPT as INTERFACE compile definitions
# reflecting how upstream pagmo was built, not what we have available. Explicitly
# undefine them so our wrapper only enables optional-solver code when we have
# verified the solver is present (PAGMONET_WITH_* options below).
if(MSVC)
    target_compile_options(PagmoWrapper PRIVATE /UPAGMO_WITH_NLOPT /UPAGMO_WITH_IPOPT /bigobj)
else()
    target_compile_options(PagmoWrapper PRIVATE -UPAGMO_WITH_NLOPT -UPAGMO_WITH_IPOPT)
endif()

# Platform-specific library properties.
if(CMAKE_SYSTEM_NAME STREQUAL "Linux")
    # Set SONAME so consumers can find the library by its short name.
    set_target_properties(PagmoWrapper PROPERTIES
        VERSION   ${PROJECT_VERSION}
        SOVERSION 1
    )
elseif(APPLE)
    # Use @loader_path so the dylib resolves sibling libraries at runtime
    # regardless of install location (NuGet runtimes/ layout).
    set_target_properties(PagmoWrapper PROPERTIES
        BUILD_WITH_INSTALL_RPATH ON
        INSTALL_RPATH "@loader_path"
    )
endif()

# Optional solver support. Use PAGMONET_WITH_* names to avoid collision with
# pagmo's own cmake-exported PAGMO_WITH_* variables (which reflect how the
# upstream pagmo library was built, not how we want to build our wrapper).
option(PAGMONET_WITH_NLOPT  "Build PagmoWrapper with NLopt support"  OFF)
option(PAGMONET_WITH_IPOPT  "Build PagmoWrapper with IPOPT support"  OFF)
option(PAGMONET_WITH_SNOPT7 "Build PagmoWrapper with SNOPT7 support" OFF)

if(PAGMONET_WITH_NLOPT)
    find_package(NLopt CONFIG REQUIRED)
    target_compile_definitions(PagmoWrapper PRIVATE PAGMO_WITH_NLOPT)
    target_link_libraries(PagmoWrapper PRIVATE NLopt::nlopt)
endif()

if(PAGMONET_WITH_IPOPT)
    # Three cases in order of preference:
    # 1. cmake config package (future vcpkg versions or other setups)
    # 2. Windows/vcpkg: pagmo::IPOPT::libipopt was pre-created above via find_library;
    #    Pagmo::pagmo links it transitively. Only the compile definition and include
    #    path are needed here — no explicit target_link_libraries call required.
    # 3. Linux/macOS: pkg-config (apt pagmo or source build).
    find_package(Ipopt CONFIG QUIET)
    if(TARGET Ipopt::Ipopt)
        target_compile_definitions(PagmoWrapper PRIVATE PAGMO_WITH_IPOPT)
        target_link_libraries(PagmoWrapper PRIVATE Ipopt::Ipopt)
    elseif(WIN32)
        find_path(IPOPT_INCLUDE_DIR NAMES IpIpoptApplication.hpp PATH_SUFFIXES coin-or)
        target_compile_definitions(PagmoWrapper PRIVATE PAGMO_WITH_IPOPT)
        if(IPOPT_INCLUDE_DIR)
            target_include_directories(PagmoWrapper PRIVATE "${IPOPT_INCLUDE_DIR}")
        endif()
        # ipopt.lib calls BLAS/LAPACK via Fortran symbol names (e.g. ddot_, dsyev_).
        # These are not in ipopt.lib itself; they come from the separate static libs.
        # pagmo::IPOPT::libipopt only exposes ipopt.lib, so we must link the full
        # BLAS/LAPACK chain explicitly here.
        # CLAPACK (lapack.lib) calls BLAS externally; OpenBLAS defines DllMain
        # and cannot be statically linked into a DLL. Intel MKL provides both
        # BLAS and LAPACK as truly static libraries with no DllMain conflict.
        # Use the sequential LP64 variant (no OpenMP dependency).
        find_library(PAGMONET_MKL_LP64 NAMES mkl_intel_lp64 REQUIRED)
        find_library(PAGMONET_MKL_SEQ  NAMES mkl_sequential  REQUIRED)
        find_library(PAGMONET_MKL_CORE NAMES mkl_core        REQUIRED)
        target_link_libraries(PagmoWrapper PRIVATE
            "${PAGMONET_MKL_LP64}"
            "${PAGMONET_MKL_SEQ}"
            "${PAGMONET_MKL_CORE}"
        )
    else()
        find_package(PkgConfig REQUIRED)
        pkg_check_modules(IPOPT REQUIRED ipopt)
        target_compile_definitions(PagmoWrapper PRIVATE PAGMO_WITH_IPOPT)
        target_link_libraries(PagmoWrapper PRIVATE ${IPOPT_LINK_LIBRARIES})
        target_include_directories(PagmoWrapper PRIVATE ${IPOPT_INCLUDE_DIRS})
        target_link_directories(PagmoWrapper PRIVATE ${IPOPT_LIBRARY_DIRS})
    endif()
endif()

if(PAGMONET_WITH_SNOPT7)
    target_compile_definitions(PagmoWrapper PRIVATE PAGMO_WITH_SNOPT7)
endif()
