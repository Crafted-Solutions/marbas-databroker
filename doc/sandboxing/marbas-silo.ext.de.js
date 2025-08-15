const SESSIONKEY_SANDBOX_HINT = 'mbs-sbhint-de';
const HINT_TEXT = "Diese Testinstallation von MarBas ist begrenzt auf 300 MB Speicherplatz, alle Daten werden nach 12 Stunden gelöscht.";
const HINT_BTNLABEL = 'Schließen';

export const AppConfig = {
    requires: ['StorageUtils'],
    install: function install(ctx) {
		EnvConfig.apiBaseUrl = `/databroker/api/marbas`;
		let elm = document.getElementById('silo-auth-txt-url');
		if (elm) {
			elm.value = EnvConfig.apiBaseUrl;
		}
        this._showHint(ctx);
    },
    _showHint: function _showHint(ctx) {
        if (ctx.StorageUtils.read(SESSIONKEY_SANDBOX_HINT, false, false)) {
            return;
        }
        const cnt = document.createElement('div');
        cnt.className = 'alert alert-warning alert-dismissible fade show';
        cnt.setAttribute('role', 'alert');
        cnt.textContent = HINT_TEXT;
        
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btn-close';
        btn.title = HINT_BTNLABEL;
        btn.setAttribute('data-bs-dismiss', 'alert');
        btn.setAttribute('aria-label', HINT_BTNLABEL);
        cnt.appendChild(btn);
        
        document.getElementById('main').insertBefore(cnt, document.getElementById('top'));
        ctx.StorageUtils.write(SESSIONKEY_SANDBOX_HINT, true);
    }
};