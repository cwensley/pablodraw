#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

function extract_csproj () {
	csprojdir=$1
	csproj=$4
	newdir=$2
	libdir=$3
	subdir="`echo $libdir/$csprojdir | sed 's/\\//\\\\\\\\/g'`"
	subdir="$subdir\\\\"
	if [[ -z "$5" ]]; then
		oldcsproj=$csproj
	else
		oldcsproj=$csproj
		csproj=$5
	fi
	csproj="$newdir/$csproj"
	
	mkdir -p "$newdir"
	
	rm -f "$csproj"

	cp "$DIR/$csprojdir/$oldcsproj" "$csproj"

	rep="s/<(Compile|EmbeddedResource|None) Include=\"(.+)\">(?=\s*<Link>)/<\1 Include=\"$subdir\2\">/g"
	#rep="s/<(Compile|EmbeddedResource|None) Include=\"(.+)\">(?!\s*<Link>)/<\1 Include=\"$subdir\2\">/g"
	perl -0pi -e "$rep" "$csproj"
	
	rep="s/<(Compile|EmbeddedResource|None) Include=\"(.+)\">(?!\s*<Link>)/<\1 Include=\"$subdir\2\">\n      <Link>\2<\/Link>/g"
	#rep="s/<(Compile|EmbeddedResource|None) Include=\"(.+)\">\s*<Link>.+<\/Link>/<\1 Include=\"$subdir\2\">/g"
	perl -0pi -e "$rep" "$csproj"
	
	rep="s/<(Compile|EmbeddedResource|None) Include=\"(.+)\" \/>/<\1 Include=\"$subdir\2\">\n      <Link>\2<\/Link>\n    <\/\1>/"
	#rep="s/<(Compile|EmbeddedResource|None) Include=\"(.+)\" \/>/<\1 Include=\"$subdir\2\" \/>/"
	perl -pi -e "$rep" "$csproj"
	
	rep="s/<(OutputPath|DocumentationFile)>(.+)<\//<\1>$subdir\2<\//"
	perl -pi -e "$rep" "$csproj"
	
	rep="s/(?<=<Import Project=\")(?!\$\(.*\)))(.+)(?=\"\s*\/>)/$subdir\2/g"
	perl -0pi -e "$rep" "$csproj"
	
	rep="s/(<Reference Include=\"[^\"]++\">(\s|.)*?<HintPath>)((\s|.)+?<\/HintPath>(\s|.)+?<\/Reference>)/\1$subdir\3/g"
	perl -0pi -e "$rep" "$csproj"
}

function replace_reference () {
	csproj=$1
	find_reference=$2
	replace_with=$3
	
	uuid=$(sed -n 's/.*<ProjectGuid>\(.*\)<\/ProjectGuid>.*/\1/p' "$replace_with.csproj")
	
	rep="s/<Reference Include=\"$find_reference.*?\">(\s|.)*?<\/Reference>/<ProjectReference Include=\"$replace_with.csproj\">\n      <Project>$uuid<\/Project>\n      <Name>$replace_with<\/Name>\n    <\/ProjectReference>/g"
	perl -0pi -e "$rep" "$csproj"
}

function replace_hint () {
	csprojdir=$1
	find_reference=$2
	newdir=$3

	rep="s/(<Reference Include=\"$find_reference[^\"]++\">(\s|.)*?<HintPath>)([^<]+?)($find_reference[.]dll<\/HintPath>(\s|.)+?<\/Reference>)/\1$newdir\4/g"
	perl -0pi -e "$rep" "$csproj"
}

function remove_line () {
	file_name=$1
	remove_line=$2
	
	perl -0pi -e "$remove_line" "$file_name"
}

function convert_to_ios () {
	remove_line "$1" "s/.*<TargetFrameworkVersion>v4\.0<\/TargetFrameworkVersion>.*\n//g"
	remove_line "$1" "s/(.*)(<ProjectGuid>.*<\/ProjectGuid>)(.*\n)/\1\2\3\1<ProjectTypeGuids>{6BC8ED88-2882-458C-8E55-DFD12B67127B};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}<\/ProjectTypeGuids>\3/g"
}

extract_csproj "lidgren/Lidgren.Network" "../Source/Lidgren.Network" "../../Libraries" "Lidgren.Network.csproj" "Lidgren.Network - iOS.csproj"
convert_to_ios "../Source/Lidgren.Network/Lidgren.Network - iOS.csproj"
extract_csproj "Mono.Nat/src/Mono.Nat" "../Source/Mono.Nat" "../../Libraries" "Mono.Nat.csproj" "Mono.Nat - iOS.csproj"
convert_to_ios "../Source/Mono.Nat/Mono.Nat - iOS.csproj"
