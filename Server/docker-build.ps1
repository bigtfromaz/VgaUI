cd d:/repos/VgaUI/Server
rm -R published
dotnet publish -c Release -o published
docker build --tag registry.obj-ex.net/vgaui:latest .

# dotnet clean -c release 

# docker push registry.obj-ex.net/vgaui:latest
# docker tag registry.obj-ex.net/vgaui:latest registry.obj-ex.net/vgaui:2023-12-11
