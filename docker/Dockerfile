FROM centos:7

RUN yum install -y libunwind libicu

# volumes
VOLUME "/mindtouch/solid-doodle"

#CMD echo "helloworld"
COPY ./docker/files/dotnet-install.sh /mindtouch/solid-doodle/docker/files/dotnet-install.sh
RUN bash /mindtouch/solid-doodle/docker/files/dotnet-install.sh -Version 1.0.4 -InstallDir /usr/local/bin/dotnet --verbose

