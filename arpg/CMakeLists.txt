cmake_minimum_required(VERSION 3.28)
project(SparkArpg LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Sibling Spark repo: spark-template/arpg → ../spark
if(NOT DEFINED SPARK_ROOT OR SPARK_ROOT STREQUAL "")
    set(SPARK_ROOT "${CMAKE_CURRENT_LIST_DIR}/../../spark" CACHE PATH
            "Path to the Spark engine repository (directory containing Spark's top-level CMakeLists.txt)")
endif()

get_filename_component(SPARK_ROOT_ABS "${SPARK_ROOT}" ABSOLUTE)
if(NOT EXISTS "${SPARK_ROOT_ABS}/CMakeLists.txt")
    message(FATAL_ERROR "SPARK_ROOT must point at the Spark engine repo (missing: ${SPARK_ROOT_ABS}/CMakeLists.txt)")
endif()

# Game assets live next to this CMakeLists.txt (copied to the build output at link time).
set(ARPG_ASSETS_ROOT "${CMAKE_CURRENT_LIST_DIR}/assets" CACHE PATH "ARPG game assets root directory")
get_filename_component(ARPG_ASSETS_ROOT_ABS "${ARPG_ASSETS_ROOT}" ABSOLUTE)

set(SPARK_BUILD_ENGINE_SHARED ON CACHE BOOL "Build SparkEngine as a shared library" FORCE)
set(SPARK_BUILD_DEMO OFF CACHE BOOL "Build SparkDemo (engine sample executable)" FORCE)
# Spark scripting/CMakeLists.txt uses CMAKE_SOURCE_DIR (top-level = this game), not SPARK_ROOT.
set(SPARK_BUILD_SCRIPT_HOST OFF CACHE BOOL "Build SparkScriptHost + SparkInterop" FORCE)
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
target_compile_definitions(${PROJECT_NAME} PRIVATE ARPG_ASSETS_ROOT="${ARPG_ASSETS_ROOT_ABS}")
target_link_libraries(${PROJECT_NAME} PRIVATE SparkEngine)

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

# Runtime asset bundle beside the executable (models/, textures/, etc.).
add_custom_command(
        TARGET ${PROJECT_NAME}
        POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
                "${ARPG_ASSETS_ROOT_ABS}"
                "$<TARGET_FILE_DIR:${PROJECT_NAME}>/assets"
        COMMENT "Copy ARPG assets next to ${PROJECT_NAME}")
