# Working on this project together

## One-time setup: Unity smart merge

This repo's `.gitattributes` tells Git to use Unity's smart merge tool on
scenes, prefabs, and other YAML assets instead of a plain text merge, which
avoids most of the "someone else touched the scene file" pain. Each teammate
needs to point Git at it once, since the tool ships inside the Unity Editor
install and the path differs per machine/OS.

**Windows (PowerShell):**

```powershell
$unityYamlMerge = (Get-ChildItem "$env:ProgramFiles\Unity\Hub\Editor\*\Editor\Data\Tools\UnityYAMLMerge.exe" | Select-Object -First 1).FullName
git config merge.unityyamlmerge.driver "`"$unityYamlMerge`" merge -p %O %B %A %A"
```

**macOS:**

```sh
git config merge.unityyamlmerge.driver '/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/Tools/UnityYAMLMerge merge -p %O %B %A %A'
```

Swap `<version>` for your installed Unity version. Run `git config --get merge.unityyamlmerge.driver` afterwards to confirm it's set.

This is a local `git config` setting — it isn't stored in the repo, so it needs to be run on every teammate's machine once.

## Working in the same scene

Smart merge helps, but two people editing the same scene/prefab at the same
time will still cause conflicts. Where possible, agree on who's touching the
main scene before diving in, or split work into separate scenes/prefabs that
get combined later.
