// PabloDraw.Console.cpp : This file contains the 'main' function. Program
// execution begins and ends there.
//

#include "Utilities.h"
#include <iostream>
#include <string>
#include <vector>
#include <filesystem>


using namespace std;

int main(int argc, char *argv[]) {
  auto path = getexepath().parent_path();

  string args = "";
  for (int i = 1; i < argc; i++) {
    args = args + " ";
    args = args + "\"" + argv[i] + "\"";
  }

  auto pdpath = path.string() + sep + "PabloDraw" + exeExtension;
  if (!filesystem::exists(pdpath)) {
    cout << "Could not find " << pdpath << endl;
    cout << "PabloDraw.Console" << exeExtension << " requires PabloDraw" << exeExtension << " to execute."<< endl;
    return -1;
  }
  pdpath = pdpath + args;

  return system(pdpath.c_str());
}
