cmake_minimum_required(VERSION 3.28)
project(SparkArpg LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Spark engine repo (sibling of spark-template: spark-template/C++/arpg → ../../../spark).
if(NOT DEFINED SPARK_ROOT OR SPARK_ROOT STREQUAL "")
    set(SPARK_ROOT "${CMAKE_CURRENT_LIST_DIR}/../../../spark" CACHE PATH
            "Path to the Spark engine repository (directory containing Spark's top-level CMakeLists.txt)")
endif()

get_filename_component(SPARK_ROOT_ABS "${SPARK_ROOT}" ABSOLUTE)
if(NOT EXISTS "${SPARK_ROOT_ABS}/CMakeLists.txt")
    message(FATAL_ERROR "SPARK_ROOT must point at the Spark engine repo (missing: ${SPARK_ROOT_ABS}/CMakeLists.txt)")
endif()

# Game assets beside this CMakeLists.txt (copied next to the executable on each build).
set(ARPG_ASSETS_DIR "${CMAKE_CURRENT_LIST_DIR}/assets")

set(SPARK_BUILD_ENGINE_SHARED ON CACHE BOOL "Build SparkEngine as a shared library" FORCE)
set(SPARK_BUILD_ENGINE_DEMOS ON CACHE BOOL "Demo sources required by Spark::Engine shell" FORCE)
set(SPARK_BUILD_DEMO OFF CACHE BOOL "Build SparkDemo (engine sample executable)" FORCE)
set(SPARK_BUILD_INTEROP OFF CACHE BOOL "Build SparkInterop (C# bindings)" FORCE)
set(SPARK_BUILD_GAMEPLAY ON CACHE BOOL "SparkGameplay kit (required when linking shared SparkEngine)" FORCE)
# Draco install rules use list(TRANSFORM REPLACE) on source paths as regex; paths with "C++" fail configure.
set(DRACO_INSTALL OFF CACHE BOOL "" FORCE)
add_subdirectory("${SPARK_ROOT_ABS}" "${CMAKE_BINARY_DIR}/spark_engine")

set(ARPG_SOURCES
        src/main.cpp
        src/game/ArpgGame.cpp
        src/core/GameStateMachine.cpp
        src/services/AssetPaths.cpp
        src/services/AssetRegistry.cpp
        src/services/UiBootstrap.cpp
        src/world/DungeonLevelBuilder.cpp
        src/player/PlayerAvatar.cpp
        src/player/PlayerController.cpp
        src/states/PlayState.cpp
)

add_executable(${PROJECT_NAME} ${ARPG_SOURCES})
target_include_directories(${PROJECT_NAME} PRIVATE "${CMAKE_CURRENT_LIST_DIR}/src")
target_link_libraries(${PROJECT_NAME} PRIVATE SparkEngine SparkGameplay)

set(_spark_out "${CMAKE_BINARY_DIR}/spark_engine")
if(APPLE OR UNIX)
    set_target_properties(${PROJECT_NAME} PROPERTIES BUILD_RPATH "${_spark_out}")
endif()

if(WIN32)
    add_custom_command(
            TARGET ${PROJECT_NAME}
            POST_BUILD
            COMMAND ${CMAKE_COMMAND} -E copy_if_different
                    "$<TARGET_FILE:SparkEngine>"
                    "$<TARGET_FILE_DIR:${PROJECT_NAME}>"
            COMMENT "Copy SparkEngine DLL next to ${PROJECT_NAME}")
endif()

add_custom_command(
        TARGET ${PROJECT_NAME}
        POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
                "${ARPG_ASSETS_DIR}"
                "$<TARGET_FILE_DIR:${PROJECT_NAME}>/assets"
        COMMENT "Copy ARPG assets next to ${PROJECT_NAME}")
