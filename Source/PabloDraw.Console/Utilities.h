#pragma once

#include <iostream>
#include <string>
#include <vector>
#include <filesystem>


#ifdef WINDOWS
static const char sep = '\\';
static const char *exeExtension = ".exe";
#else
static const char sep = '/';
static const char *exeExtension = "";
#endif

std::filesystem::path getexepath();