#!/bin/bash

BASEDIR=../..
SOLUTIONDIR=..

function CopyBase {
	echo Copying: $5
	cp $BASEDIR/$3/$4/$1/$5 $2
}
function CopyFile {
	echo Copying: $4
	cp $SOLUTIONDIR/$3/bin/$1/$4 $2
}

echo Destination: $2

mkdir -p $2
CopyBase $1 $2 Libraries/Eto BuildOutput Eto*.dll
CopyBase $1 $2 Libraries/lidgren Lidgren.Network/bin Lidgren.Network.*
CopyBase $1 $2 Libraries/Mono.Nat/src Mono.Nat/bin Mono.Nat.*
CopyFile $1 $2 Pablo Pablo.dll

