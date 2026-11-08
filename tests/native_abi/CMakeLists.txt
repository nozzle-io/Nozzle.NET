cmake_minimum_required(VERSION 3.20)
project(nozzle_dotnet_info_abi LANGUAGES C)

add_executable(nozzle_info_abi connected_info.c)
target_include_directories(nozzle_info_abi PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/../../deps/nozzle/include")
set_target_properties(nozzle_info_abi PROPERTIES C_STANDARD 99 C_STANDARD_REQUIRED YES)
