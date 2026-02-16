#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'USAGE'
Usage: scripts/new_applet.sh [--wire] <AppletName>

Creates src/AccessNote.Applets.<AppletName>/ from templates/applet/*.template.
AppletName must be PascalCase letters/digits (e.g. Utilities, Timer2).

Options:
  --wire   Also wire the new applet into the solution:
           - Add AppletId enum member
           - Add host project reference
           - Copy architecture test stub
           - Add TODO comment in composition root
USAGE
}

WIRE=false
APPLET_NAME=""

for arg in "$@"; do
  case "$arg" in
    --wire) WIRE=true ;;
    -h|--help) usage; exit 0 ;;
    -*) echo "error: unknown option: $arg" >&2; usage; exit 1 ;;
    *) APPLET_NAME="$arg" ;;
  esac
done

if [[ -z "$APPLET_NAME" ]]; then
  usage
  exit 1
fi

if [[ ! "$APPLET_NAME" =~ ^[A-Z][A-Za-z0-9]*$ ]]; then
  echo "error: AppletName must be PascalCase letters/digits. Got: $APPLET_NAME" >&2
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
TEMPLATE_DIR="$REPO_ROOT/templates/applet"
TARGET_DIR="$REPO_ROOT/src/AccessNote.Applets.$APPLET_NAME"

if [[ ! -d "$TEMPLATE_DIR" ]]; then
  echo "error: template directory not found: $TEMPLATE_DIR" >&2
  exit 1
fi

if [[ -e "$TARGET_DIR" ]]; then
  echo "error: target already exists: $TARGET_DIR" >&2
  exit 1
fi

mkdir -p "$TARGET_DIR"

shopt -s nullglob
TEMPLATES=("$TEMPLATE_DIR"/*.template)
shopt -u nullglob

if [[ ${#TEMPLATES[@]} -eq 0 ]]; then
  echo "error: no .template files found in $TEMPLATE_DIR" >&2
  exit 1
fi

for template_path in "${TEMPLATES[@]}"; do
  template_name="$(basename "$template_path")"
  output_name="${template_name%.template}"
  output_name="${output_name//__APPLET__/$APPLET_NAME}"
  output_name="${output_name//\{\{APPLET_NAME\}\}/$APPLET_NAME}"

  output_path="$TARGET_DIR/$output_name"

  sed \
    -e "s/{{APPLET_NAME}}/$APPLET_NAME/g" \
    -e "s/__APPLET__/$APPLET_NAME/g" \
    "$template_path" > "$output_path"

  echo "created: ${output_path#"$REPO_ROOT/"}"
done

if [[ "$WIRE" == true ]]; then
  echo
  echo "Wiring into solution..."

  # 1) Add AppletId enum member
  APPLET_CONTRACTS="$REPO_ROOT/src/AccessNote.Shell/AppletContracts.cs"
  if grep -q "^    $APPLET_NAME," "$APPLET_CONTRACTS" 2>/dev/null; then
    echo "skip: AppletId.$APPLET_NAME already exists"
  else
    # Insert new member after the last existing enum member line
    LAST_MEMBER_LINE=$(grep -n '^    [A-Z][A-Za-z0-9]*,\?$' "$APPLET_CONTRACTS" | tail -1 | cut -d: -f1)
    if [[ -n "$LAST_MEMBER_LINE" ]]; then
      # Ensure the previous last member has a trailing comma
      sed -i "${LAST_MEMBER_LINE}s/^\(    [A-Z][A-Za-z0-9]*\)$/\1,/" "$APPLET_CONTRACTS"
      sed -i "${LAST_MEMBER_LINE}a\\    $APPLET_NAME," "$APPLET_CONTRACTS"
      echo "wired: AppletId.$APPLET_NAME added"
    else
      echo "warning: could not locate enum members in AppletContracts.cs — add AppletId.$APPLET_NAME manually" >&2
    fi
  fi

  # 2) Add project reference to host csproj
  HOST_CSPROJ="$REPO_ROOT/src/AccessNote/AccessNote.csproj"
  APPLET_PROJ_REF="AccessNote.Applets.$APPLET_NAME"
  if grep -q "$APPLET_PROJ_REF" "$HOST_CSPROJ" 2>/dev/null; then
    echo "skip: project reference $APPLET_PROJ_REF already exists"
  else
    sed -i "/<\/ItemGroup>/i\\    <ProjectReference Include=\"..\\\\$APPLET_PROJ_REF\\\\$APPLET_PROJ_REF.csproj\" />" "$HOST_CSPROJ"
    echo "wired: project reference $APPLET_PROJ_REF added"
  fi

  # 3) Copy architecture test stub
  TEST_DIR="$REPO_ROOT/tests/AccessNote.Tests"
  TEST_FILE="$TEST_DIR/${APPLET_NAME}ArchitectureTests.cs"
  TEST_TEMPLATE="$TEMPLATE_DIR/{{APPLET_NAME}}ArchitectureTests.cs.template"
  if [[ -e "$TEST_FILE" ]]; then
    echo "skip: test file already exists"
  elif [[ -f "$TEST_TEMPLATE" ]]; then
    sed "s/{{APPLET_NAME}}/$APPLET_NAME/g" "$TEST_TEMPLATE" > "$TEST_FILE"
    echo "wired: ${TEST_FILE#"$REPO_ROOT/"}"
  fi

  # 4) Add TODO in composition root
  COMP_ROOT="$REPO_ROOT/src/AccessNote/Composition/MainWindowCompositionRoot.cs"
  if grep -q "$APPLET_PROJ_REF" "$COMP_ROOT" 2>/dev/null; then
    echo "skip: composition root already references $APPLET_NAME"
  else
    sed -i "/AppletRegistrationComposer\.CreateRegistry/i\\        // TODO: Add ${APPLET_NAME}AppletRegistration to appletRegistrations array above." "$COMP_ROOT"
    echo "wired: TODO comment added to MainWindowCompositionRoot.cs"
  fi

  echo
  echo "Done. Remaining manual step:"
  echo "1) Implement ${APPLET_NAME}AppletRegistration and add it to the appletRegistrations array"
  echo "   in src/AccessNote/Composition/MainWindowCompositionRoot.cs."
  echo "2) Run tests:"
else
  echo
  echo "Done. Next steps:"
  echo "1) Add enum member in src/AccessNote.Shell/AppletContracts.cs (AppletId)."
  echo "2) Register applet in composition (MainWindowCompositionRoot / factories)."
  echo "3) Add project reference in src/AccessNote/AccessNote.csproj."
  echo "4) Copy architecture test from templates to tests/AccessNote.Tests/."
  echo "5) Run tests:"
fi
echo "   \"/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe\" -NoProfile -ExecutionPolicy Bypass -Command \"& 'C:\\Program Files\\dotnet\\dotnet.exe' test 'F:\\git\\access-note\\tests\\AccessNote.Tests\\AccessNote.Tests.csproj'\""
