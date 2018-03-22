FROM ubuntu:xenial

LABEL application="PabloDraw" \
      version="3.2.1"

ARG PD_SERVERPORT=14400
ENV PD_VERSION 3.2.1
ENV PD_AUTOSAVE 60
ENV PD_ICECOLORS true
ENV PD_OPERATORPW password
ENV PD_PASSWORD password
ENV PD_SERVERPORT ${PD_SERVERPORT}

RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
  && echo "deb http://download.mono-project.com/repo/ubuntu stable-xenial main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
  && apt-get update \
  && apt-get install -y curl mono-complete gtk-sharp2 unzip \
  && rm -rf /var/lib/apt/lists/*

RUN set -ex \
  && curl -fSL "http://download.picoe.ca/pablodraw/3.2/PabloDraw.Console-$PD_VERSION.zip" -o /tmp/pablo.zip \
  && unzip /tmp/pablo.zip -d /usr/local/bin \
  && rm /tmp/pablo.zip

EXPOSE ${PD_SERVERPORT}

CMD mono /usr/local/bin/PabloDraw.Console.exe -p=gtk --server --port=$PD_SERVERPORT -op=$PD_OPERATORPW -pw=$PD_PASSWORD -ul=editor --autosave=$PD_AUTOSAVE --backup --text-ice=$PD_ICECOLORS --text-use9x=true
