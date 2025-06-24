#!/bin/bash
THIS_DIR=$(/usr/bin/realpath "$(/usr/bin/dirname "${BASH_SOURCE[0]}")")
OCC=/usr/bin/occ
if [ -d /var/run/s6/container_environment ]; then
    OCC=$THIS_DIR/xocc
fi
$OCC group:add 'MarBas\ Admins'
$OCC group:add 'MarBas\ Developers'
$OCC group:add 'MarBas\ Managers'
$OCC group:add 'MarBas\ Editors'
$OCC group:add 'MarBas\ Readers'
export OC_PASS=dummypass_b
if [ -d /var/run/s6/container_environment ]; then
    echo -n $OC_PASS > /var/run/s6/container_environment/OC_PASS
fi
$OCC user:add --password-from-env --display-name='MarBas\ Admin' --group='MarBas\ Admins' --email='root@nextcloud.local' mb-root
$OCC user:add --password-from-env --display-name='MarBas\ Manager' --group='MarBas\ Managers' --email='manager@nextcloud.local' mb-manager
$OCC user:add --password-from-env --display-name='MarBas\ Developer' --group='MarBas\ Developers' --email='devel@nextcloud.local' mb-developer
$OCC user:add --password-from-env --display-name='MarBas\ Editor' --group='MarBas\ Editors' --email='editor@nextcloud.local' mb-editor
$OCC user:add --password-from-env --display-name='MarBas\ Reader' --group='MarBas\ Readers' --email='reader@nextcloud.local' mb-reader
$OCC app:install oidc -q
$OCC oidc:create -t public --token_type=jwt 'MarBas\ Databroker' https://localhost:7277/swagger/oauth2-redirect.html https://localhost:7277/silo/index.html http://localhost:5500/ > $THIS_DIR/marbas-oidc.json
