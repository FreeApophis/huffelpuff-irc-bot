;NSIS Modern User Interface
;Based on the example by Joost Verburg

;***** Includes *****
  !include "MUI2.nsh"
  !include LogicLib.nsh

;***** Defines *****
  !define PROGRAM_NAME "Huffelpuff"
  !define PROGRAM_EXE "Huffelpuff.exe"
  !define PROGRAM_VERSION "1.0beta"
  !define BUILD_PATH "..\Huffelpuff\bin\"

;***** Variables *****

  Var StartMenuFolder

;***** General *****

  ;Name and file
  Name "Huffepuff"
  OutFile "HuffelpuffSetup.exe"

  ;Default installation folder
  InstallDir "$PROGRAMFILES\Huffelpuff"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\Huffelpuff" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel admin

;***** Interface Settings *****

  !define MUI_ABORTWARNING
  !define MUI_FINISHPAGE_RUN "$INSTDIR\${PROGRAM_EXE}"
  !define MUI_FINISHPAGE_RUN_TEXT "Start Huffelpuff as a Service now"

;***** Pages *****

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "License.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH

  
  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_COMPONENTS
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH
  
;***** Languages *****
 
  !insertmacro MUI_LANGUAGE "English"
  !insertmacro MUI_LANGUAGE "German"
  
;***** Installer Sections *****
InstType "Full"
InstType "Standard"
InstType "Minimal"


SectionGroup /e "!Huffelpuff Service"
Section "-Huffelpuff" mainHuffelpuffSec
  SectionIn 1 2 3 RO
  SetOutPath "$INSTDIR"
 
  File "${BUILD_PATH}\${PROGRAM_EXE}"
  File "${BUILD_PATH}\SmartIrc4Net.dll"

  SetOutPath "$APPDATA\Huffelpuff\"
  
  ;Settings  
  File /oname="pmem.xml" "pmem.base"
  
  ;preparing the Add / Remove Program
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "DisplayName" "Huffelpuff - the IRC Bot"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "UninstallString" "$\"$INSTDIR\Uninstall.exe$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "QuietUninstallString" "$\"$INSTDIR\Uninstall.exe$\" /S"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "InstallLocation" "$\"$INSTDIR$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "Publisher" "apophis.ch"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "DisplayVersion" "${PROGRAM_VERSION}"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "MajorVersion" "1"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "MinorVersion" "0"				 
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "NoModify" "1"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "NoRepair" "1"
				 
  Call GetInstalledSize
  Pop $0

  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" "EstimatedSize" "$0"
				 
  ;Store installation folder
  WriteRegStr HKCU "Software\Huffelpuff" "" $INSTDIR
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    
  ;Create shortcuts
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\Uninstall ${PROGRAM_NAME}.lnk" "$INSTDIR\Uninstall.exe"
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\${PROGRAM_NAME}.lnk" "$INSTDIR\Huffelpuff.exe"
  
  !insertmacro MUI_STARTMENU_WRITE_END

SectionEnd
SectionGroupEnd

SectionGroup /e "!Huffelpuff Common Plugins"
Section "AIML"
  SectionIn 1 2
  SetOutPath "$APPDATA\Huffelpuff\plugins\"
  File "${BUILD_PATH}\plugins\AIMLPlugin.dll"
SectionEnd
Section "Calculator"
  SectionIn 1 2
  SetOutPath "$INSTDIR\plugins\"
  File "${BUILD_PATH}\plugins\CalculatorPlugin.dll"
SectionEnd
Section "Factoid"
  SectionIn 1 2
  SetOutPath "$INSTDIR\plugins\"
  File "${BUILD_PATH}\plugins\FactoidPlugin.dll"
SectionEnd
Section "RSS"
  SectionIn 1 2
  SetOutPath "$INSTDIR\plugins\"
  File "${BUILD_PATH}\plugins\RSSPlugin.dll"
SectionEnd
Section "Seen"
  SectionIn 1
  SetOutPath "$INSTDIR\plugins\"
  File "${BUILD_PATH}\plugins\SeenPlugin.dll"
SectionEnd
Section "Twitter"
  SectionIn 1 2
  SetOutPath "$INSTDIR\plugins\"
  File "${BUILD_PATH}\plugins\TwitterPlugin.dll"
SectionEnd
Section "UrlToTitle"
  SectionIn 1 2
  SetOutPath "$INSTDIR\plugins\"
  File "${BUILD_PATH}\plugins\UrlToTitlePlugin.dll"
SectionEnd
Section "Wikipedia"
  SectionIn 1 2
  SetOutPath "$INSTDIR\plugins\"
  File "${BUILD_PATH}\plugins\WikipediaPlugin.dll"
SectionEnd
SectionGroupEnd

SectionGroup /e "!Huffelpuff Special Plugins"
Section "PiVote"
  SectionIn 1 2
  SetOutPath "$INSTDIR\plugins\"
  File "${BUILD_PATH}\plugins\WikipediaPlugin.dll"
SectionEnd
SectionGroupEnd


;***** Descriptions *****

  ;Language strings
  LangString DESC_mainHuffelpuffSec ${LANG_ENGLISH} "Huffelpuff - Installs the Mainprogram"
  LangString DESC_mainHuffelpuffSec ${LANG_GERMAN} "Huffelpuff - Installiert das Hauptprogramm"

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${mainHuffelpuffSec} $(DESC_mainHuffelpuffSec)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;***** Uninstaller Sections *****


SectionGroup /e "un.Huffelpuff"
Section "Uninstall"
  SectionIn 1 2 RO
  
  Delete "$INSTDIR\${PROGRAM_EXE}"	
  Delete "$INSTDIR\Uninstall.exe"
  
  RMDir "$INSTDIR"

  DeleteRegKey /ifempty HKCU "Software\Huffelpuff"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}"
  
  !insertmacro MUI_STARTMENU_GETFOLDER "Application" $StartMenuFolder
  Delete "$SMPROGRAMS\$StartMenuFolder\${PROGRAM_NAME}.lnk"
  Delete "$SMPROGRAMS\$StartMenuFolder\Uninstall ${PROGRAM_NAME}.lnk"
  RMDir "$SMPROGRAMS\$StartMenuFolder"

  Delete "$INSTDIR\plugins\*"
  RMDir "$INSTDIR\plugins"

SectionEnd

Section /o "un.Settings"
  Delete "$APPDATA\Huffelpuff\*"
  RMDir "$APPDATA\Huffelpuff"
SectionEnd

SectionGroupEnd


;***** Installer Callbacks *****
Function .onInit
  ;Check if this uninstaller is its only instance
  System::Call "kernel32::CreateMutexA(i 0, i 0 t '${PROGRAM_NAME}') i .r0 ?e"
  Pop $0
  StrCmp $0 0 launch
    StrLen $0 "${PROGRAM_NAME}"
    IntOp $0 $0 + 1
  loop:
    FindWindow $1 '#32770' '' 0 $1
    IntCmp $1 0 +4
    System::Call "user32::GetWindowText(i r1, t .r2, i r0) i."
    StrCmp $2 "${PROGRAM_NAME}" 0 loop
    System::Call "user32::SetForegroundWindow(i r1) i."
    Abort
  launch:
  
  !insertmacro MUI_LANGDLL_DISPLAY

  
  ReadRegStr $R0 HKLM \
  "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PROGRAM_NAME}" \
  "UninstallString"
  StrCmp $R0 "" done

isAppRunning: 
;  TODO install FindProcDLL and uncomment next 5 lines
;  FindProcDLL::FindProc "${PROGRAM_EXE}"
;  IntCmp $R0 1 0 notRunning
;  MessageBox MB_OKCANCEL|MB_ICONEXCLAMATION "${PROGRAM_NAME} is running. Please close it first" \
;  IDOK isAppRunning    
;	Abort
notRunning:
 
  MessageBox MB_OKCANCEL|MB_ICONEXCLAMATION \
  "${PROGRAM_NAME} is already installed. $\n$\nClick `OK` to upgrade \
  previous version or `Cancel` to cancel this upgrade." \
  IDOK uninst
  Abort
 
;Run the uninstaller
uninst:
  ClearErrors
  ExecWait '$R0 _?=$INSTDIR /S' ;Do not copy the uninstaller to a temp file
 
  IfErrors no_remove_uninstaller done

  no_remove_uninstaller:
 
done:
FunctionEnd
 
; Return on top of stack the total size of the selected (installed) sections, formated as DWORD
; Assumes no more than 256 sections are defined
Var GetInstalledSize.total
Function GetInstalledSize
	Push $0
	Push $1
	StrCpy $GetInstalledSize.total 0
	${ForEach} $1 0 256 + 1
		${if} ${SectionIsSelected} $1
			SectionGetSize $1 $0
			IntOp $GetInstalledSize.total $GetInstalledSize.total + $0
		${Endif}
	${Next}
	Pop $1
	Pop $0
	IntFmt $GetInstalledSize.total "0x%08X" $GetInstalledSize.total
	Push $GetInstalledSize.total
FunctionEnd
