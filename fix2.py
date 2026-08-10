Remove-Item -Recurse -Force .git\refs\remotes\origin\UIEP-1599 -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .git\logs\refs\remotes\origin\UIEP-1599 -ErrorAction SilentlyContinue
git fetch origin

Get-ChildItem .git\refs\remotes\origin
Get-ChildItem .git\logs\refs\remotes\origin
