#!/bin/bash
set -x

source ~/bin/manual_backup.sh
lastFile=$(echo $(ssh centos 'echo $(find ~/backups/mysql -ctime -1|tail -1)'))
scp centos:"${lastFile}" /tmp/tmp.sql.gz
rm /tmp/tmp.sql || true
cd /tmp && gzip -d ./tmp.sql.gz && mysql < ./tmp.sql && echo 'Well done' ||(echo 'Something wrong!' && exit 1)


