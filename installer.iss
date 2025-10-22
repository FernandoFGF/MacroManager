; Script de Inno Setup para Macro Manager
; Este script crea un instalador profesional para la aplicación

#define MyAppName "Macro Manager"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Tu Nombre"
#define MyAppURL "https://github.com/tuusuario/macromanager"
#define MyAppExeName "MacroManager.exe"
#define MyAppAssocName "Archivo de Macro"
#define MyAppAssocExt ".macro"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

[Setup]
; Información básica de la aplicación
AppId={{8A7B9C3D-4E5F-6A7B-8C9D-0E1F2A3B4C5D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Ruta de instalación por defecto
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes

; Configuración de salida del instalador
OutputDir=.\Output
OutputBaseFilename=MacroManager_v{#MyAppVersion}_Setup
SetupIconFile=MacroManager\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

; Compresión
Compression=lzma2
SolidCompression=yes

; Modo de instalación
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Plataforma objetivo
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

; Configuración visual
WizardStyle=modern
WizardImageFile=compiler:WizClassicImage.bmp
WizardSmallImageFile=compiler:WizClassicSmallImage.bmp

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Archivo principal ejecutable
Source: "MacroManager\bin\Release\net8.0-windows\win-x64\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; Todas las DLLs y dependencias necesarias
Source: "MacroManager\bin\Release\net8.0-windows\win-x64\publish\*.dll"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

; Archivos de configuración JSON (si existen)
;Source: "MacroManager\bin\Release\net8.0-windows\win-x64\publish\*.json"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs; Excludes: "*.deps.json,*.runtimeconfig.json"

; Runtime configuration files
Source: "MacroManager\bin\Release\net8.0-windows\win-x64\publish\*.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion

; Icono de la aplicación
Source: "MacroManager\app.ico"; DestDir: "{app}"; Flags: ignoreversion

; Documentación
Source: "MacroManager\README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme

[Icons]
; Icono en el Menú Inicio
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

; Icono en el Escritorio (opcional)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

; Icono en la Barra de Inicio Rápido (opcional, solo Windows 7 y anteriores)
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

; Acceso al archivo README
Name: "{autoprograms}\{#MyAppName}\Documentación"; Filename: "{app}\README.md"

; Desinstalador
Name: "{autoprograms}\{#MyAppName}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"

[Registry]
; Asociar extensión .macro con la aplicación
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocExt}\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppAssocKey}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\SupportedTypes"; ValueType: string; ValueName: ".macro"; ValueData: ""

[Run]
; Ejecutar la aplicación al finalizar la instalación (opcional)
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Eliminar configuraciones de usuario al desinstalar (opcional)
Type: filesandordirs; Name: "{userappdata}\MacroManager"

[Code]
// Código Pascal Script para funciones personalizadas

// Verificar si .NET está instalado (opcional)
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
begin
  // Intentar ejecutar dotnet --version
  Result := Exec('cmd.exe', '/C dotnet --version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

// Mensaje de bienvenida personalizado
function InitializeSetup(): Boolean;
begin
  Result := True;
  
  // Puedes agregar verificaciones aquí si es necesario
  // Por ejemplo, verificar si hay una versión anterior instalada
end;

// Mensaje al finalizar la instalación
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Puedes agregar código post-instalación aquí
  end;
end;

// Mensaje personalizado antes de desinstalar
function InitializeUninstall(): Boolean;
var
  Response: Integer;
begin
  Response := MsgBox('¿Desea eliminar también las macros guardadas y configuraciones?', 
                     mbConfirmation, MB_YESNOCANCEL);
  
  case Response of
    IDYES:
      begin
        // Eliminar todo
        Result := True;
      end;
    IDNO:
      begin
        // Solo desinstalar la aplicación, mantener datos
        Result := True;
      end;
    IDCANCEL:
      begin
        // Cancelar desinstalación
        Result := False;
      end;
  end;
end;

[Messages]
; Mensajes personalizados en español
spanish.WelcomeLabel2=Esto instalará [name/ver] en tu computadora.%n%nEsta aplicación te permite crear y reproducir macros personalizadas para videojuegos.%n%nSe recomienda cerrar todos los programas antes de continuar.
spanish.FinishedHeadingLabel=Instalación completada de [name]
spanish.FinishedLabelNoIcons=El programa ha sido instalado exitosamente.
spanish.FinishedLabel=La aplicación ha sido instalada. Puedes ejecutarla haciendo clic en el icono instalado.

english.WelcomeLabel2=This will install [name/ver] on your computer.%n%nThis application allows you to create and play custom macros for video games.%n%nIt is recommended that you close all other applications before continuing.
