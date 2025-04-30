#!/bin/bash
appname=$1
mkdir $appname
mkdir "${appname}app"
mkdir "${appname}appsrc"

fetchfile="fetch${appname}.sh"
echo "#!/bin/bash" > $fetchfile
echo "cd $appname" >> $fetchfile
echo "git fetch origin" >> $fetchfile
echo "git reset --hard origin/master" >> $fetchfile
chmod +x $fetchfile

echo "#!/bin/bash" > supervisorRestart.sh
echo "service supervisor restart" >> supervisorRestart.sh
chmod +x supervisorRestart.sh

syncfile="sync${appname}.sh"
echo "#!/bin/bash" > $syncfile
echo "dotnet publish ./${appname}/Runner/Runner.csproj --output ./${appname}appsrc" >> $syncfile
echo "rsync -arv ./${appname}appsrc/* ./${appname}app --exclude={\"logs/*\"} --delete" >> $syncfile
chmod +x $syncfile

deployfile="deploy${appname}.sh"
echo "#!/bin/bash" > $deployfile
echo "./$fetchfile" >> $deployfile
echo "./$syncfile" >> $deployfile
echo "./supervisorRestart.sh" >> $deployfile
chmod +x $deployfile

giturl=$2
git clone $giturl $appname
