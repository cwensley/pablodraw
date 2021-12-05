#include "Utilities.h"

#ifdef WINDOWS
#include <windows.h>
#elif MACOS
#include <mach-o/dyld.h>
#elif LINUX
#include <limits.h>
#include <unistd.h>
#endif

using namespace std;

std::filesystem::path getexepath() {
#ifdef WINDOWS
  wchar_t path[MAX_PATH] = {0};
  GetModuleFileNameW(NULL, path, MAX_PATH);
  return path;
#elif MACOS
  char buffer[1024];
  uint32_t size = sizeof(buffer);
  if (_NSGetExecutablePath(buffer, &size) == 0) {
    return std::filesystem::path(buffer);
  }
  return "";
#elif LINUX
  char result[PATH_MAX];
  ssize_t count = readlink("/proc/self/exe", result, PATH_MAX);
  return std::string(result, (count > 0) ? count : 0);
#endif
}